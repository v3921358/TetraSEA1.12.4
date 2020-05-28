namespace MapleLib.PacketLib
{
    using MapleLib.MapleCryptoLib;
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;

    public class Session
    {
        private MapleCrypto _RIV;
        private MapleCrypto _SIV;
        private readonly System.Net.Sockets.Socket _socket;
        private SessionType _type;
        public bool Connected = true;
        private const int DEFAULT_SIZE = 0x3e80;
        private byte[] mBuffer = new byte[0x3e80];
        private int mCursor = 0;
        private byte[] mSharedBuffer = new byte[0x3e80];

        public event ClientDisconnectedHandler OnClientDisconnected;

        public event InitPacketReceived OnInitPacketReceived;

        public event PacketReceivedHandler OnPacketReceived;

        public Session(System.Net.Sockets.Socket socket, SessionType type)
        {
            this._socket = socket;
            this._type = type;
        }

        public void Append(byte[] pBuffer)
        {
            this.Append(pBuffer, 0, pBuffer.Length);
        }

        public void Append(byte[] pBuffer, int pStart, int pLength)
        {
            if ((this.mBuffer.Length - this.mCursor) < pLength)
            {
                int newSize = this.mBuffer.Length * 2;
                while (newSize < (this.mCursor + pLength))
                {
                    newSize *= 2;
                }
                Array.Resize<byte>(ref this.mBuffer, newSize);
            }
            Buffer.BlockCopy(pBuffer, pStart, this.mBuffer, this.mCursor, pLength);
            this.mCursor += pLength;
        }

        private void BeginInSend(byte[] data)
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(data, 0, data.Length);
            this._socket.SendAsync(e);
        }

        private void BeginReceive()
        {
            if (!(this.Connected && this._socket.Connected))
            {
                this.ForceDisconnect();
            }
            else
            {
                this._socket.BeginReceive(this.mSharedBuffer, 0, 0x3e80, SocketFlags.None, new AsyncCallback(this.EndReceive), this._socket);
            }
        }

        private void EndReceive(IAsyncResult ar)
        {
            if (this.Connected)
            {
                int pLength = 0;
                try
                {
                    pLength = this._socket.EndReceive(ar);
                }
                catch
                {
                    this.ForceDisconnect();
                    return;
                }
                if (pLength <= 0)
                {
                    this.ForceDisconnect();
                }
                else
                {
                    this.Append(this.mSharedBuffer, 0, pLength);
                    while (true)
                    {
                        if (this.mCursor < 4)
                        {
                            break;
                        }
                        ushort count = MapleCrypto.getPacketLength(this.mBuffer, 0);
                        if (this.mCursor < (count + 4))
                        {
                            break;
                        }
                        byte[] dst = new byte[count];
                        Buffer.BlockCopy(this.mBuffer, 4, dst, 0, count);
                        this.RIV.Decrypt(dst);
                        this.mCursor -= count + 4;
                        if (this.mCursor > 0)
                        {
                            Buffer.BlockCopy(this.mBuffer, count + 4, this.mBuffer, 0, this.mCursor);
                        }
                        if (this.OnPacketReceived != null)
                        {
                            if (!this.Connected)
                            {
                                return;
                            }
                            this.OnPacketReceived(dst);
                        }
                    }
                    this.BeginReceive();
                }
            }
        }

        private void ForceDisconnect()
        {
            if (this.Connected)
            {
                if (this.OnClientDisconnected != null)
                {
                    this.OnClientDisconnected(this);
                }
                this.Connected = false;
            }
        }

        private void OnInitPacketRecv(IAsyncResult ar)
        {
            if (this.Connected)
            {
                byte[] asyncState = (byte[]) ar.AsyncState;
                if (this._socket.EndReceive(ar) < 15)
                {
                    if (this.OnClientDisconnected != null)
                    {
                        this.OnClientDisconnected(this);
                    }
                    this.Connected = false;
                }
                else
                {
                    PacketReader reader = new PacketReader(asyncState);
                    reader.ReadShort();
                    short mapleVersion = reader.ReadShort();
                    string str = reader.ReadMapleString();
		    byte[] serverRecv = reader.ReadBytes(4);
		    byte[] serverSend = reader.ReadBytes(4);
                    byte serverIdentifier = (byte)7;
		    if (reader.ReadByte() == (byte)7) 
		    {
			serverRecv = new byte[] {0x74, 0x65, 0x74, 0x72};
			serverSend = new byte[] {0x61, 0x53, 0x45, 0x41};
		    }
			
                    this._SIV = new MapleCrypto(serverRecv, mapleVersion);
                    this._RIV = new MapleCrypto(serverSend, mapleVersion);
                    if (this._type == SessionType.CLIENT_TO_SERVER)
                    {
                        this.OnInitPacketReceived(mapleVersion, serverIdentifier, str);
                    }
                    this.WaitForData();
                }
            }
        }

        public void SendPacket(byte[] input)
        {
            if (this.Connected && this._socket.Connected)
            {
                byte[] data = input;
                byte[] dst = new byte[data.Length + 4];
                byte[] src = (this._type == SessionType.SERVER_TO_CLIENT) ? this._SIV.getHeaderToClient(data.Length) : this._SIV.getHeaderToServer(data.Length);
                this.SIV.Encrypt(data);
                Buffer.BlockCopy(src, 0, dst, 0, 4);
                Buffer.BlockCopy(data, 0, dst, 4, data.Length);
                this.SendRawPacket(dst);
            }
        }

        public void SendRawPacket(byte[] buffer)
        {
            if (this.Connected)
            {
                this.BeginInSend(buffer);
            }
        }

        public void WaitForData()
        {
            this.BeginReceive();
        }

        public void WaitForDataNoEncryption()
        {
            if (!this._socket.Connected)
            {
                this.ForceDisconnect();
            }
            else
            {
                byte[] buffer = new byte[0x10];
                this._socket.BeginReceive(buffer, 0, 0x10, SocketFlags.None, new AsyncCallback(this.OnInitPacketRecv), buffer);
            }
        }

        public MapleCrypto RIV
        {
            get
            {
                return this._RIV;
            }
            set
            {
                this._RIV = value;
            }
        }

        public MapleCrypto SIV
        {
            get
            {
                return this._SIV;
            }
            set
            {
                this._SIV = value;
            }
        }

        public System.Net.Sockets.Socket Socket
        {
            get
            {
                return this._socket;
            }
        }

        public SessionType Type
        {
            get
            {
                return this._type;
            }
        }

        public delegate void ClientDisconnectedHandler(Session session);

        public delegate void InitPacketReceived(short version, byte serverIdentifier, string str);

        public delegate void PacketReceivedHandler(byte[] packet);
    }
}

