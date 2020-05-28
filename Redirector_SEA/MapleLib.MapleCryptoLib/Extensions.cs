namespace MapleLib.MapleCryptoLib
{
    using System;
    using System.Runtime.CompilerServices;

    public static class Extensions
    {
        public static byte RollLeft(this byte pThis, int pCount)
        {
            uint num = (uint) (pThis << (pCount % 8));
            return (byte) ((num & 0xff) | (num >> 8));
        }

        public static byte RollRight(this byte pThis, int pCount)
        {
            uint num = (uint) ((pThis << 8) >> (pCount % 8));
            return (byte) ((num & 0xff) | (num >> 8));
        }
    }
}

