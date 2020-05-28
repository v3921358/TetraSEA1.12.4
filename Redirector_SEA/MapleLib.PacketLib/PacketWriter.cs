namespace MapleLib.PacketLib
{
    using System;
    using System.IO;
    using System.Text;

    public class PacketWriter : AbstractPacket
    {
        private readonly BinaryWriter _binWriter;

        public PacketWriter() : this(0)
        {
        }

        public PacketWriter(int size)
        {
            base._buffer = new MemoryStream(size);
            this._binWriter = new BinaryWriter(base._buffer, Encoding.ASCII);
        }

        public PacketWriter(byte[] data)
        {
            base._buffer = new MemoryStream(data);
            this._binWriter = new BinaryWriter(base._buffer, Encoding.ASCII);
        }

        public void Reset(int length)
        {
            base._buffer.Seek((long) length, SeekOrigin.Begin);
        }

        public void SetBool(long index, bool @bool)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteBool(@bool);
            base._buffer.Position = position;
        }

        public void SetByte(long index, int @byte)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteByte((byte) @byte);
            base._buffer.Position = position;
        }

        public void SetBytes(long index, byte[] bytes)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteBytes(bytes);
            base._buffer.Position = position;
        }

        public void SetHexString(long index, string @string)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteHexString(@string);
            base._buffer.Position = position;
        }

        public void SetInt(long index, int @int)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteInt(@int);
            base._buffer.Position = position;
        }

        public void SetLong(long index, long @long)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteLong(@long);
            base._buffer.Position = position;
        }

        public void SetMapleString(long index, string @string)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteMapleString(@string);
            base._buffer.Position = position;
        }

        public void SetShort(long index, int @short)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteShort((short) @short);
            base._buffer.Position = position;
        }

        public void SetString(long index, string @string)
        {
            long position = base._buffer.Position;
            base._buffer.Position = index;
            this.WriteString(@string);
            base._buffer.Position = position;
        }

        public void WriteBool(bool @bool)
        {
            this._binWriter.Write(@bool);
        }

        public void WriteByte(int @byte)
        {
            this._binWriter.Write((byte) @byte);
        }

        public void WriteBytes(byte[] bytes)
        {
            this._binWriter.Write(bytes);
        }

        public void WriteHexString(string hexString)
        {
            this.WriteBytes(HexEncoding.GetBytes(hexString));
        }

        public void WriteInt(int @int)
        {
            this._binWriter.Write(@int);
        }

        public void WriteLong(long @long)
        {
            this._binWriter.Write(@long);
        }

        public void WriteMapleString(string @string)
        {
            this.WriteShort((short) @string.Length);
            this.WriteString(@string);
        }

        public void WriteShort(int @short)
        {
            this._binWriter.Write((short) @short);
        }

        public void WriteString(string @string)
        {
            this._binWriter.Write(@string.ToCharArray());
        }

        public short Length
        {
            get
            {
                return (short) base._buffer.Length;
            }
        }
    }
}

