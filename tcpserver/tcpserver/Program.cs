using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace tcpserver
{
    class Program
    {
        static System.Collections.Generic.List<Socket> mClients = new System.Collections.Generic.List<Socket>();

        static void Main(string[] unused)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(new IPEndPoint(IPAddress.Any, 1337));
            s.Listen(5);
            while (true)
            {
                Socket client = s.Accept();
                Console.WriteLine("Client connected");
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                mClients.Add(client);
                args.Completed += new EventHandler<SocketAsyncEventArgs>(DataReceived);
                args.SetBuffer(new byte[8], 0, 8);
                client.ReceiveAsync(args);
            }
        }

        static void DataReceived(object o, SocketAsyncEventArgs args)
        {
            Console.WriteLine();
            Socket s = (Socket)o;
            byte[] buffer = args.Buffer;
        again:
            //Console.WriteLine("Received: " + args.BytesTransferred);
            //Console.WriteLine("Offset: " + args.Offset);
            //Console.WriteLine("Available: " + s.Available);
            int receivedSize = args.BytesTransferred + args.Offset;
            if (receivedSize == 8)
            {
                Console.WriteLine("Received data");
                SendToOtherClients(s, (byte[])buffer.Clone());
                if (s.Available >= 8)
                {
                    s.Receive(buffer);
                    goto again;
                }
                if (s.Available > 0)
                {
                    int received = s.Receive(buffer);
                    args.SetBuffer(received, 8 - received);
                }
                else
                {
                    args.SetBuffer(0, 8);
                }
            }
            else
            {
                args.SetBuffer(receivedSize, 8 - receivedSize);
            }
            
            s.ReceiveAsync(args);
        }

        static void SendToOtherClients(Socket sender, byte[] msg)
        {
            for (int i = 0; i < mClients.Count; i++)
            {
                Socket client = mClients[i];
                if (client != sender && client.Connected)
                {
                    try
                    {
                        client.BeginSend(msg, 0, 8, SocketFlags.None, null, null);
                    } catch (Exception) {
                        Console.WriteLine("Removing dead client");
                        mClients.RemoveAt(i--);
                    }
                }
            }
        }
    }
}
