using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TcpConnection : MonoBehaviour
{
    public GameObject PlayerLocal;
    public GameObject PlayerRemote;
    public GameObject TextConnected;
    public GameObject TextConnectFailed;
    public string RemoteHost = "127.0.0.1";
    public int RemotePort = 1337;
    bool mConnected = false;
    bool mConnectionError = false;
    float mTargetX = 0;
    float mTargetY = 0;

    Socket mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
    // Start is called before the first frame update
    void Start()
    {
        TextConnected.SetActive(false);
        TextConnectFailed.SetActive(false);
        SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();
        socketArgs.Completed += new System.EventHandler<SocketAsyncEventArgs>(SocketConnected);
        socketArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(RemoteHost), RemotePort);
        socketArgs.SetBuffer(new byte[8], 0, 8);
        mSocket.ConnectAsync(socketArgs);
    }

    // Update is called once per frame
    void Update()
    {
        TextConnected.SetActive(mConnected);
        TextConnectFailed.SetActive(mConnectionError);

        SendLocation();
        UpdatePlayerRemote();
    }

    void OnDestroy()
    {
        mSocket.Close();
    }

    void SocketConnected(object o, SocketAsyncEventArgs args)
    {
        Socket s = (Socket)o;
        mConnected = s.Connected;
        mConnectionError = !mConnected;

        if (args.LastOperation == SocketAsyncOperation.Receive)
        {
            mTargetX = BitConverter.ToSingle(args.Buffer, 0);
            mTargetY = BitConverter.ToSingle(args.Buffer, 4);
        }

        if (s.Connected)
        {
            s.ReceiveAsync(args);
        }
    }

    void SendLocation()
    {
        if (!mSocket.Connected)
        {
            return;
        }
        byte[] buffer = new byte[8];
        float x = PlayerLocal.transform.position.x;
        float y = PlayerLocal.transform.position.y;
        BitConverter.GetBytes(x).CopyTo(buffer, 0);
        BitConverter.GetBytes(y).CopyTo(buffer, 4);
        mSocket.BeginSend(buffer, 0, 8, SocketFlags.None, null, null);
    }

    void UpdatePlayerRemote()
    {
        PlayerRemote.transform.position = new Vector3(mTargetX, mTargetY);
    }
}
