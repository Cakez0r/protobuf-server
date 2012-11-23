using UnityEngine;
using System.Net.Sockets;
using System;
using ProtoBuf;
using Protocol;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

public enum NetworkState
{
    Disconnected,
    Connecting,
    Connected
}

public class NetworkController : MonoBehaviour
{
    [SerializeField]
    private UnityScheduler m_scheduler;

    public NetworkState State
    {
        get;
        private set;
    }

    private TcpClient m_tcpClient = new TcpClient();
    private NetworkStream m_networkStream;
    private Thread m_receiveThread;

    Dictionary<Type, Delegate> m_handlers = new Dictionary<Type, Delegate>();

    private bool m_runReceiveThread = true;

    public NetworkController()
    {
        State = NetworkState.Disconnected;
    }

    private void Start()
    {
        Application.runInBackground = true;
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
            m_tcpClient.NoDelay = true;
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

    public void Send(object o)
    {
        if (State != NetworkState.Connected)
        {
            Debug.LogWarning("Tried to send an object while not connected");
            return;
        }

        int? packetCode = ProtocolUtility.GetPacketTypeCode(o.GetType());

        if (packetCode == null)
        {
            Debug.LogWarning("Tried to send a type that is not part of the protocol.");
            return;
        }

        Serializer.NonGeneric.SerializeWithLengthPrefix(m_networkStream, o, PrefixStyle.Base128, packetCode.Value);
    }

    private void Update()
    {
        if (State == NetworkState.Connected && !m_tcpClient.Connected)
        {
            DisconnectCleanup();
        }
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

    private void DispatchPacket(object o)
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
            if (m_tcpClient.Connected)
            {
                try
                {
                    object packet = null;

                    if (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(m_networkStream, PrefixStyle.Base128, ProtocolUtility.GetPacketType, out packet))
                    {
                        m_scheduler.Schedule(() => DispatchPacket(packet));
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
