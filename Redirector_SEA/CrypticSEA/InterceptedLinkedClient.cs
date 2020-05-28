namespace CrypticSEA
{
    using MapleLib.MapleCryptoLib;
    using MapleLib.PacketLib;
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class InterceptedLinkedClient
    {
        private bool block = false;
        private int charID = -1;
        private bool connected = true;
        private bool gotEnc = false;
        private Session inSession;
        private volatile Mutex mutex = new Mutex();
        private volatile Mutex mutex2 = new Mutex();
        private Session outSession;
        private ushort Port;

        public InterceptedLinkedClient(Session inside, string toIP, ushort toPort)
        {
            this.Port = toPort;
            Debug.WriteLine("New linkclient to " + toIP);
            this.inSession = inside;
            inside.OnPacketReceived += new Session.PacketReceivedHandler(this.inside_OnPacketReceived);
            inside.OnClientDisconnected += new Session.ClientDisconnectedHandler(this.inside_OnClientDisconnected);
            if (Program.checkIP(toIP))
            {
                this.ConnectOut(toIP, toPort);
                Debug.WriteLine("Connecting out to port " + toPort);
            }
        }

        private void ChannelCompleteLogin()
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteShort(12);
            writer.WriteInt(this.charID);
            this.outSession.SendPacket(writer.ToArray());
            this.block = false;
            Debug.WriteLine("change channel complete.");
        }

        private void ConnectOut(string ip, int port)
        {
            try
            {
                Socket state = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                state.BeginConnect(ip, port, new AsyncCallback(this.OnOutConnectCallback), state);
            }
            catch
            {
                this.outSession_OnClientDisconnected(null);
            }
        }

        private void inside_OnClientDisconnected(Session session)
        {
            if (this.outSession != null)
            {
                this.outSession.Socket.Shutdown(SocketShutdown.Both);
            }
            this.connected = false;
        }

        private void inside_OnPacketReceived(byte[] packet)
        {
            if (this.connected && !this.block)
            {
                this.mutex.WaitOne();
                try
                {
                    short num = BitConverter.ToInt16(packet, 0);
                    switch (num)
                    {
                        case 12:
                            this.charID = BitConverter.ToInt32(packet, 2);
                            goto Label_0075;
                    }
                Label_0075:
                    this.outSession.SendPacket(packet);
                }
                finally
                {
                    this.mutex.ReleaseMutex();
                }
            }
        }

        private void OnOutConnectCallback(IAsyncResult ar)
        {
            Socket asyncState = (Socket) ar.AsyncState;
            try
            {
                asyncState.EndConnect(ar);
            }
            catch
            {
                this.connected = false;
                this.inSession.Socket.Shutdown(SocketShutdown.Both);
                return;
            }
            if (this.outSession != null)
            {
                this.outSession.Socket.Close();
                this.outSession.Connected = false;
            }
            Session session = new Session(asyncState, SessionType.CLIENT_TO_SERVER);
            this.outSession = session;
            this.outSession.OnInitPacketReceived += new Session.InitPacketReceived(this.outSession_OnInitPacketReceived);
            this.outSession.OnPacketReceived += new Session.PacketReceivedHandler(this.outSession_OnPacketReceived);
            this.outSession.OnClientDisconnected += new Session.ClientDisconnectedHandler(this.outSession_OnClientDisconnected);
            session.WaitForDataNoEncryption();
        }

        private void outSession_OnClientDisconnected(Session session)
        {
            if (!this.block)
            {
                this.inSession.Socket.Shutdown(SocketShutdown.Both);
                Debug.WriteLine("out disconnected (" + this.Port + ")");
                this.connected = false;
            }
        }

        private void outSession_OnInitPacketReceived(short version, byte serverIdentifier, string str)
        {
            Debug.WriteLine(string.Concat(new object[] { "Init packet: v", version, "ident: ", serverIdentifier }));
            if (this.block)
            {
                this.connected = true;
                this.ChannelCompleteLogin();
            }
            else
            {
                this.SendHandShake(version, serverIdentifier, str);
            }
        }

        private void outSession_OnPacketReceived(byte[] packet)
        {
            if (this.gotEnc && this.connected)
            {
                this.mutex2.WaitOne();
                try
                {
                    this.inSession.SendPacket(packet);
                }
                finally
                {
                    this.mutex2.ReleaseMutex();
                }
            }
        }

        private void SendHandShake(short version, byte serverident, string str)
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteShort(13 + str.Length);
            writer.WriteShort(version);
            writer.WriteMapleString(str);
            byte[] buffer = new byte[4];
            byte[] buffer2 = new byte[4];
            Random random = new Random();
            random.NextBytes(buffer);
            random.NextBytes(buffer2);
            this.inSession.RIV = new MapleCrypto(buffer, version);
            this.inSession.SIV = new MapleCrypto(buffer2, version);
            writer.WriteBytes(buffer);
            writer.WriteBytes(buffer2);
            writer.WriteByte(serverident);
            this.gotEnc = true;
            this.inSession.SendRawPacket(writer.ToArray());
        }
    }
}

