namespace MapleLib.PacketLib
{
    using System;
    using System.IO;
    using System.Text;

    public class PacketReader : AbstractPacket
    {
        private readonly BinaryReader _binReader;

        public PacketReader(byte[] arrayOfBytes)
        {
            base._buffer = new MemoryStream(arrayOfBytes, false);
            this._binReader = new BinaryReader(base._buffer, Encoding.ASCII);
        }

        public bool ReadBool()
        {
            return this._binReader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return this._binReader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return this._binReader.ReadBytes(count);
        }

        public int ReadInt()
        {
            return this._binReader.ReadInt32();
        }

        public long ReadLong()
        {
            return this._binReader.ReadInt64();
        }

        public string ReadMapleString()
        {
            return this.ReadString(this.ReadShort());
        }

        public short ReadShort()
        {
            return this._binReader.ReadInt16();
        }

        public string ReadString(int length)
        {
            return Encoding.ASCII.GetString(this.ReadBytes(length));
        }

        public void Reset(int length)
        {
            base._buffer.Seek((long) length, SeekOrigin.Begin);
        }

        public void Skip(int length)
        {
            base._buffer.Position += length;
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

