using NLog;
using ProtoBuf;
using Protocol;
using Server.Utility;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Threading;

namespace Server
{
    public abstract class NetPeer : IDisposable
    {
        private const int BUFFER_SIZE = 8192;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static BufferManager s_buffers;

        private Fiber m_fiber = new Fiber();
        private MemoryStream m_receiveBuffer;
        private Socket m_socket;

        private long m_continueReadFrom = 0;
        private long m_lastBufferSize;

        private ConcurrentStack<byte[]> m_bufferPool = new ConcurrentStack<byte[]>();

        public int ID
        {
            get;
            private set;
        }

        public bool Disposed
        {
            get;
            private set;
        }

        public bool IsConnected 
        {
            get { return m_socket.Connected; }
        }

        protected Fiber Fiber
        {
            get { return m_fiber; }
        }

        #region Stats
        private static long s_totalBytesIn;
        public static long TotalBytesIn
        {
            get { return s_totalBytesIn;  }
        }

        private static long s_totalBytesOut;
        public static long TotalBytesOut
        {
            get { return s_totalBytesOut; }
        }

        private static long s_totalPacketsIn;
        public static long TotalPacketsIn
        {
            get { return s_totalPacketsIn; }
        }

        private static long s_totalPacketsOut;
        public static long TotalPacketsOut
        {
            get { return s_totalPacketsOut; }
        }
        #endregion

        static NetPeer()
        {
            s_buffers = BufferManager.CreateBufferManager(1024 * 1024 * 1024 * 2L, BUFFER_SIZE);
        }

        public NetPeer(Socket socket)
        {
            ID = IDGenerator.GetNextID();
            m_receiveBuffer = new MemoryStream();
            m_socket = socket;
            StartReceiving();
        }

        public void Disconnect()
        {
            if (!Disposed && m_socket.Connected)
            {
                s_log.Debug("[{0}] Disconnecting", ID);
                m_socket.Disconnect(false);
            }
        }

        public void Respond(Packet p, Packet response)
        {
            response.ID = p.ID;
            Send(response);
        }

        public void Send(Packet p)
        {
            int? packetCode = ProtocolUtility.GetPacketTypeCode(p.GetType());

            if (packetCode != null)
            {
                try
                {
                    if (m_socket.Connected)
                    {
                        byte[] buffer = GetBuffer();

                        long size = 0;
                        using (MemoryStream memoryStream = new MemoryStream(buffer))
                        {
                            Serializer.NonGeneric.SerializeWithLengthPrefix(memoryStream, p, PrefixStyle.Base128, packetCode.Value);
                            size = memoryStream.Position;
                        }

                        SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                        eventArgs.SetBuffer(buffer, 0, (int)size);
                        eventArgs.Completed += SendCompleted;

                        if (!m_socket.SendAsync(eventArgs))
                        {
                            SendCompleted(null, eventArgs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    s_log.Warn("[{0}] Exception on Send: " + ex, ID);
                    Disconnect();
                }
            }
            else
            {
                s_log.Warn("Tried to send a type that isn't part of the protocol: " + p.GetType());
            }
        }

        private void SendCompleted(object o, SocketAsyncEventArgs eventArgs)
        {
            ReturnBuffer(eventArgs.Buffer);

            Interlocked.Increment(ref s_totalPacketsOut);
            Interlocked.Add(ref s_totalBytesOut, eventArgs.BytesTransferred);

            eventArgs.Dispose();
        }

        private void StartReceiving()
        {
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.SetBuffer(GetBuffer(), 0, BUFFER_SIZE);
            eventArgs.Completed += ReceiveCompleted;
            Receive(eventArgs);
        }

        private void Receive(SocketAsyncEventArgs eventArgs)
        {
            try
            {
                if (!Disposed && !m_socket.ReceiveAsync(eventArgs))
                {
                    ReceiveCompleted(null, eventArgs);
                }
            }
            catch (Exception ex)
            {
                s_log.Warn("[{0}] Exception on Receive: " + ex, ID);
                Disconnect();
            }
        }

        private void ReceiveCompleted(object o, SocketAsyncEventArgs eventArgs)
        {
            bool disconnect = false;
            try
            {
                if (eventArgs.SocketError == SocketError.Success)
                {
                    m_receiveBuffer.Write(eventArgs.Buffer, 0, eventArgs.BytesTransferred);

                    m_receiveBuffer.Position = m_continueReadFrom;

                    object obj = default(object);
                    try
                    {
                        long prev = m_receiveBuffer.Position;
                        while (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(m_receiveBuffer, PrefixStyle.Base128, ProtocolUtility.GetPacketType, out obj))
                        {
                            m_continueReadFrom = m_receiveBuffer.Position;

                            Packet packet = (Packet)obj;
                            m_fiber.Enqueue(() => DispatchPacket(packet));

                            Interlocked.Increment(ref s_totalPacketsIn);

                            prev = m_continueReadFrom;
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        //Partial packet
                    }

                    if (m_continueReadFrom == m_receiveBuffer.Length)
                    {
                        m_continueReadFrom = 0;
                        m_receiveBuffer.SetLength(0);
                    }
                    else
                    {
                        s_log.Trace("[{0}] Partial packet. Continuing from {1}", ID, m_continueReadFrom);
                    }

                    if (m_lastBufferSize != m_receiveBuffer.Length)
                    {
                        s_log.Trace("[{0}] Receive buffer size changed {1} -> {2}", ID, m_lastBufferSize, m_receiveBuffer.Length);
                    }

                    m_lastBufferSize = m_receiveBuffer.Length;

                    Interlocked.Add(ref s_totalBytesIn, eventArgs.BytesTransferred);

                    Receive(eventArgs);
                }
                else
                {
                    s_log.Trace("[{0}] Socket error: " + eventArgs.SocketError.ToString(), ID);
                    disconnect = true;
                }
            }
            catch (Exception ex)
            {
                s_log.Trace("[{0}] Exception on receive: " + ex, ID);
                disconnect = true;
            }
            finally
            {
                if (disconnect)
                {
                    s_log.Trace("[{0}] Something went wrong in receive. Disconnecting.", ID);
                    Disconnect();
                    eventArgs.Dispose();
                    ReturnBuffer(eventArgs.Buffer);
                }
            }
        }

        public virtual void Dispose()
        {
            m_socket.Dispose();
            byte[] buffer = default(byte[]);
            while (m_bufferPool.TryPop(out buffer))
            {
                s_buffers.ReturnBuffer(buffer);
            }
            Disposed = true;
        }

        private byte[] GetBuffer()
        {
            byte[] buffer = default(byte[]);
            if (!m_bufferPool.TryPop(out buffer))
            {
                buffer = s_buffers.TakeBuffer(BUFFER_SIZE);
            }
            return buffer;
        }

        private void ReturnBuffer(byte[] buffer)
        {
            m_bufferPool.Push(buffer);
        }

        protected abstract void DispatchPacket(Packet packet);
    }
}
