using System;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Threading;
using NLog;
using ProtoBuf;
using Protocol;
using Server.Utility;

namespace Server
{
    public class NetPeerStats
    {
        public long BytesSent;
        public long BytesReceived;
        public long MessagedSent;
        public long MessagedReceived;
    }

    public abstract class NetPeer : IDisposable
    {
        private const int BUFFER_SIZE = 2048;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static BufferManager s_buffers;

        private Fiber m_fiber = new Fiber();
        private MemoryStream m_receiveBuffer;
        private Socket m_socket;

        private long m_continueReadFrom = 0;
        private long m_lastReceiveBufferCapacity = 0;

        public NetPeerStats Stats
        {
            get;
            private set;
        }

        public bool IsConnected 
        {
            get { return m_socket.Connected; }
        }

        static NetPeer()
        {
            s_buffers = BufferManager.CreateBufferManager(1024 * 1024 * 1024 * 2L, 2048);
        }

        public NetPeer(Socket socket)
        {
            Stats = new NetPeerStats();

            m_receiveBuffer = new MemoryStream();
            m_socket = socket;
            StartReceiving();
        }

        public void EnqueueWork(Action a)
        {
            m_fiber.Enqueue(a);
        }

        public void Disconnect()
        {
            if (m_socket.Connected)
            {
                s_log.Debug("Disconnecting");
                m_socket.Disconnect(false);
            }
        }

        public void Send(object o)
        {
            int? packetCode = ProtocolUtility.GetPacketTypeCode(o.GetType());

            if (packetCode != null)
            {
                try
                {
                    if (m_socket.Connected)
                    {
                        byte[] buffer = s_buffers.TakeBuffer(BUFFER_SIZE);
                        long size = 0;
                        using (MemoryStream memoryStream = new MemoryStream(buffer))
                        {
                            Serializer.NonGeneric.SerializeWithLengthPrefix(memoryStream, o, PrefixStyle.Base128, packetCode.Value);
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
                    s_log.Warn("Exception on Send: " + ex);
                    Disconnect();
                }
            }
            else
            {
                s_log.Warn("Tried to send a type that isn't part of the protocol: " + o.GetType());
            }
        }

        private void SendCompleted(object o, SocketAsyncEventArgs eventArgs)
        {
            Interlocked.Add(ref Stats.BytesSent, eventArgs.BytesTransferred);

            s_buffers.ReturnBuffer(eventArgs.Buffer);
            eventArgs.Dispose();

            Interlocked.Increment(ref Stats.MessagedSent);
        }

        private void StartReceiving()
        {
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.SetBuffer(s_buffers.TakeBuffer(BUFFER_SIZE), 0, BUFFER_SIZE);
            eventArgs.Completed += ReceiveCompleted;
            Receive(eventArgs);
        }

        private void Receive(SocketAsyncEventArgs eventArgs)
        {
            try
            {
                if (!m_socket.ReceiveAsync(eventArgs))
                {
                    ReceiveCompleted(null, eventArgs);
                }
            }
            catch (Exception ex)
            {
                s_log.Warn("Exception on Receive: " + ex);
                Disconnect();
            }
        }

        private void ReceiveCompleted(object o, SocketAsyncEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.SocketError == SocketError.Success)
                {
                    //Write the data we received to the buffer
                    m_receiveBuffer.Write(eventArgs.Buffer, 0, eventArgs.BytesTransferred);

                    //Rewind to where we should continue reading from, to attempt deserialization
                    m_receiveBuffer.Seek(m_continueReadFrom, SeekOrigin.Begin);

                    object obj = null;
                    int len = 0;

                    //Until we reach the end of the buffer...
                    while (m_receiveBuffer.Position < m_receiveBuffer.Length)
                    {
                        //Find out how much data we need before we can deserialize
                        bool gotLength = false;

                        try
                        {
                            gotLength = Serializer.TryReadLengthPrefix(m_receiveBuffer, PrefixStyle.Base128, true, out len);
                        }
                        catch (EndOfStreamException)
                        {
                            //Not enough data to determine the packet length.
                            //Skip to the end of the buffer and wait for more
                            s_log.Trace("Reached end of stream while reading length prefix. Waiting for more data");
                        }

                        if (gotLength)
                        {
                            //If we have enough data to deserialize...
                            if (m_receiveBuffer.Length - m_receiveBuffer.Position >= len)
                            {
                                //Rewind back to the start of the packet
                                m_receiveBuffer.Seek(m_continueReadFrom, SeekOrigin.Begin);
                                if (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(m_receiveBuffer, PrefixStyle.Base128, ProtocolUtility.GetPacketType, out obj))
                                {
                                    //Deserialize one packet
                                    object packet = obj;
                                    m_fiber.Enqueue(() => DispatchPacket(packet));
                                    Interlocked.Increment(ref Stats.MessagedReceived);

                                    //Update pointer to the beginning of the next packet
                                    m_continueReadFrom = m_receiveBuffer.Position;
                                }
                                else
                                {
                                    s_log.Warn("Error deserializing! Disconnecting...");
                                    s_buffers.ReturnBuffer(eventArgs.Buffer);
                                    eventArgs.Dispose();
                                    Disconnect();
                                }
                            }
                            else
                            {
                                //Not enough data for a whole packet.
                                //Skip to the end of the buffer and wait for more
                                s_log.Trace("Partial packet. Waiting for more data.");
                                m_receiveBuffer.Seek(0, SeekOrigin.End);
                                break;
                            }
                        }
                        else
                        {
                            //Not enough data to determine the packet length.
                            //Skip to the end of the buffer and wait for more
                            s_log.Trace("Partial length prefix. Waiting for more data");
                            m_receiveBuffer.Seek(0, SeekOrigin.End);
                            break;
                        }
                    }

                    if (m_receiveBuffer.Position == m_continueReadFrom)
                    {
                        //If the buffer position and the start of the next packet are aligned,
                        //reset the buffer and start writing from the beginning again.
                        m_receiveBuffer.SetLength(0);
                        m_continueReadFrom = 0;
                    }

                    if (m_lastReceiveBufferCapacity != m_receiveBuffer.Capacity)
                    {
                        //s_log.Trace("Buffer grew to " + m_receiveBuffer.Capacity + " received bytes was " + eventArgs.BytesTransferred);
                        m_lastReceiveBufferCapacity = m_receiveBuffer.Capacity;
                    }

                    Interlocked.Add(ref Stats.BytesReceived, eventArgs.BytesTransferred);

                    Receive(eventArgs);
                }
                else
                {
                    if (eventArgs.SocketError != SocketError.ConnectionReset)
                    {
                        s_log.Warn("Socket error on receive: " + eventArgs.SocketError);
                    }

                    s_buffers.ReturnBuffer(eventArgs.Buffer);
                    eventArgs.Dispose();
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                s_log.Warn("Exception on deserialize: " + ex);
                s_buffers.ReturnBuffer(eventArgs.Buffer);
                eventArgs.Dispose();
                Disconnect();
            }
        }

        public void Dispose()
        {
            m_socket.Dispose();
        }

        protected abstract void DispatchPacket(object packet);
    }
}
