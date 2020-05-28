namespace CrypticSEA
{
    using MapleLib.PacketLib;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;

    public class Listener
    {
        private readonly Socket _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private ushort Port;

        public event ClientConnectedHandler OnClientConnected;

        public void Close()
        {
            this._listener.Close();
        }

        public void Listen(ushort port)
        {
            this.Port = port;
            this._listener.Bind(new IPEndPoint(IPAddress.Any, port));
            this._listener.Listen(15);
            this._listener.BeginAccept(new AsyncCallback(this.OnClientConnect), null);
        }

        private void OnClientConnect(IAsyncResult async)
        {
            Session session = new Session(this._listener.EndAccept(async), SessionType.SERVER_TO_CLIENT);
            if (this.OnClientConnected != null)
            {
                this.OnClientConnected(session, this.Port);
            }
            session.WaitForData();
            this._listener.BeginAccept(new AsyncCallback(this.OnClientConnect), null);
        }

        public void Release(Session session)
        {
            session.Socket.Close();
        }

        public bool Running
        {
            get
            {
                return this._listener.IsBound;
            }
        }

        public delegate void ClientConnectedHandler(Session session, ushort port);
    }
}

