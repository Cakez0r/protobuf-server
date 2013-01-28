using UnityEngine;
using System.Net.Sockets;
using System;
using ProtoBuf;
using Protocol;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using Assets.Network;
using System.Linq;

public enum NetworkState
{
    Disconnected,
    Connecting,
    Connected
}

public class ConnectionLostException : Exception
{
    public ConnectionLostException() : base("Connection to the server was lost before a response was received") { } 
}

public class UnexpectedResponseTypeException : Exception
{
    public UnexpectedResponseTypeException() : base("Got an unexpected response type for the request.") { }
}

public class NetworkController : MonoBehaviour
{
    [SerializeField]
    private UnityScheduler m_scheduler;

    [SerializeField]
    private int m_responseTimeoutMilliseconds = 5000;


    private class PendingResponse
    {
        public Action<Packet> OnCompletion { get; private set; }
        public Action<Exception> OnException { get; private set; }
        public DateTime StartTime { get; private set; }
        public TimeSpan Timeout { get; private set; }

        public PendingResponse(Action<Packet> completionHandler, Action<Exception> exceptionHandler, TimeSpan timeout)
        {
            OnCompletion = completionHandler;
            OnException = exceptionHandler;
            Timeout = timeout;
            StartTime = DateTime.Now;
        }
    }


    public NetworkState State
    {
        get;
        private set;
    }

    private TcpClient m_tcpClient = new TcpClient();
    private NetworkStream m_networkStream;
    private Thread m_receiveThread;

    private Dictionary<Type, Delegate> m_handlers = new Dictionary<Type, Delegate>();

    private Dictionary<ushort, PendingResponse> m_pendingResponses = new Dictionary<ushort, PendingResponse>();

    private List<ushort> m_timedOutResponses = new List<ushort>();

    private bool m_runReceiveThread = true;

    private ushort m_nextRequestID = 1;

    public NetworkController()
    {
        State = NetworkState.Disconnected;
    }

    private void Start()
    {
        Application.runInBackground = true;

        ProtocolUtility.InitialiseSerializer();

        m_tcpClient.NoDelay = true;
    }

    public void Connect(string hostname, int port)
    {
        if (State == NetworkState.Connected || State == NetworkState.Connecting)
        {
            Debug.LogWarning("Already connected or connecting. Disconnect before calling Connect again.");
            return;
        }

        m_tcpClient.BeginConnect(hostname, port, OnConnected, null);
        Debug.Log("Connecting");
    }

    private void OnConnected(IAsyncResult result)
    {
        try
        {
            m_tcpClient.EndConnect(result);
            m_networkStream = m_tcpClient.GetStream();
            State = m_tcpClient.Connected ? NetworkState.Connected : NetworkState.Disconnected;
            m_receiveThread = new Thread(ReceiveThread);
            m_receiveThread.Start();
            m_scheduler.Schedule(() => Debug.Log("Connected"));
        }
        catch (Exception ex)
        {
            m_scheduler.Schedule(() => Debug.Log("Exception on connect: " + ex));
        }
    }

    public void Send(Packet packet)
    {
        if (State != NetworkState.Connected)
        {
            Debug.LogWarning("Tried to send an object while not connected");
            return;
        }

        int? packetCode = ProtocolUtility.GetPacketTypeCode(packet.GetType());

        if (packetCode == null)
        {
            Debug.LogWarning("Tried to send a type that is not part of the protocol.");
            return;
        }

        Debug.Log("Sending with id: " + packet.ID);

        Serializer.NonGeneric.SerializeWithLengthPrefix(m_networkStream, packet, PrefixStyle.Base128, packetCode.Value);
    }

    public Future<T> SendWithResponse<T>(Packet request) where T : Packet
    {
        Future<T> future = new Future<T>();

        PendingResponse pendingResponse = new PendingResponse
        (
            (response) => { RequestCompletionHandler<T>(future, response); },
            (exception) => { future.SetException(exception); },
            TimeSpan.FromMilliseconds(m_responseTimeoutMilliseconds)
        );

        request.ID = m_nextRequestID++;

        m_pendingResponses.Add(request.ID, pendingResponse);

        Send(request);

        return future;
    }

    private void RequestCompletionHandler<T>(Future<T> future, Packet response) where T : Packet
    {
        try
        {
            future.SetResult((T)response);
        }
        catch (InvalidCastException)
        {
            future.SetException(new UnexpectedResponseTypeException());
        }
        catch (Exception ex)
        {
            future.SetException(ex);
        }
    }

    private void Update()
    {
        if (State == NetworkState.Connected && !m_tcpClient.Connected)
        {
            DisconnectCleanup();
        }

        foreach (KeyValuePair<ushort, PendingResponse> pendingResponse in m_pendingResponses.Where(pr => DateTime.Now - pr.Value.StartTime > pr.Value.Timeout))
        {
            pendingResponse.Value.OnException(new TimeoutException());
            m_timedOutResponses.Add(pendingResponse.Key);
        }

        foreach (ushort timedOutResponse in m_timedOutResponses)
        {
            m_pendingResponses.Remove(timedOutResponse);
        }

        m_timedOutResponses.Clear();
    }

    public void RegisterHandler<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (!m_handlers.ContainsKey(type))
        {
            m_handlers.Add(type, handler);
        }
        else
        {
            m_handlers[type] = Delegate.Combine(m_handlers[type], handler);
        }
    }

    public void UnregisterHandler<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (m_handlers.ContainsKey(type))
        {
            m_handlers[type] = Delegate.Remove(m_handlers[type], handler);
        }
    }

    private void DispatchPacket(Packet o)
    {
        Delegate handler = null;

        m_handlers.TryGetValue(o.GetType(), out handler);

        if (handler != null)
        {
            handler.DynamicInvoke(o);
        }
    }

    private void DisconnectCleanup()
    {
        State = NetworkState.Disconnected;
        Debug.Log("Disconnected");

        foreach (PendingResponse pendingResponse in m_pendingResponses.Values)
        {
            pendingResponse.OnException(new ConnectionLostException());
        }

        m_pendingResponses.Clear();
    }

    private void OnApplicationQuit()
    {
        m_tcpClient.Close();
        m_runReceiveThread = false;
        Debug.Log("Waiting for receive thread to join...");
        m_receiveThread.Join();
        Debug.Log("Exiting");
    }

    private void ReceiveThread()
    {
        while (m_runReceiveThread)
        {
            if (m_tcpClient.Connected && m_networkStream.DataAvailable)
            {
                try
                {
                    object obj = null;

                    if (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(m_networkStream, PrefixStyle.Base128, ProtocolUtility.GetPacketType, out obj))
                    {
                        Packet packet = (Packet)obj;
                        if (m_pendingResponses.ContainsKey(packet.ID))
                        {
                            m_pendingResponses[packet.ID].OnCompletion(packet);
                        }
                        else
                        {
                            m_scheduler.Schedule(() => DispatchPacket(packet));
                        }
                    }
                    else
                    {
                        m_scheduler.Schedule(() => Debug.LogWarning("Failed to receive."));
                    }
                }
                catch (Exception ex)
                {
                    m_scheduler.Schedule(() => Debug.LogWarning("Exception on receive: " + ex));
                }
            }

            Thread.Sleep(1);
        }
    }
}
