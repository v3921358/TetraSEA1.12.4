namespace MapleLib.PacketLib
{
    using System;
    using System.IO;

    public abstract class AbstractPacket
    {
        protected MemoryStream _buffer;

        protected AbstractPacket()
        {
        }

        public byte[] ToArray()
        {
            return this._buffer.ToArray();
        }
    }
}

