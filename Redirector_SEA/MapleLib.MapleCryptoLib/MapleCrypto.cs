namespace MapleLib.MapleCryptoLib
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;

    public class MapleCrypto
    {
        private volatile byte[] _IV;
        private short _mapleVersion;
        private RijndaelManaged mAES = new RijndaelManaged();
        private ICryptoTransform mTransformer = null;

        public MapleCrypto(byte[] IV, short mapleVersion)
        {
            this._IV = IV;
            this._mapleVersion = mapleVersion;
            this.mAES.Key = CryptoConstants.UserKey;
            this.mAES.Mode = CipherMode.ECB;
            this.mAES.Padding = PaddingMode.PKCS7;
            this.mTransformer = this.mAES.CreateEncryptor();
        }

        public bool checkPacket(byte[] packet)
        {
            return ((((packet[0] ^ this._IV[2]) & 0xff) == ((this._mapleVersion >> 8) & 0xff)) && (((packet[1] ^ this._IV[3]) & 0xff) == (this._mapleVersion & 0xff)));
        }

        public bool checkPacketToServer(byte[] packet, int offset)
        {
            int num = packet[offset] ^ this._IV[2];
            int num2 = this._mapleVersion;
            int num3 = packet[offset + 1] ^ this._IV[3];
            int num4 = this._mapleVersion >> 8;
            return ((num == num2) && (num3 == num4));
        }

        public void Decrypt(byte[] data)
        {
            this.Transform(data);
            for (int i = 1; i <= 6; i++)
            {
                int num5;
                byte cur;
                byte remember = 0;
                byte nextRemember = 0;
                byte dataLength = (byte) (data.Length & 0xff);
                if ((i % 2) == 0)
                {
                    num5 = 0;
                    while (num5 < data.Length)
                    {
                        cur = data[num5];
                        cur = (byte) (cur - 0x48);
                        cur = ((byte)((~cur) & 0xFF));
                        //cur = ~cur; //Decryption Error?
                        cur = cur.RollLeft(dataLength & 0xff);
                        nextRemember = cur;
                        cur = (byte) (cur ^ remember);
                        remember = nextRemember;
                        cur = (byte) (cur - dataLength);
                        cur = cur.RollRight(3);
                        data[num5] = cur;
                        dataLength = (byte) (dataLength - 1);
                        num5++;
                    }
                }
                else
                {
                    for (num5 = data.Length - 1; num5 >= 0; num5--)
                    {
                        cur = data[num5];
                        cur = (byte) (cur.RollLeft(3) ^ 0x13);
                        nextRemember = cur;
                        cur = (byte) (cur ^ remember);
                        remember = nextRemember;
                        cur = (byte) (cur - dataLength);
                        data[num5] = cur.RollRight(4);
                        dataLength = (byte) (dataLength - 1);
                    }
                }
            }
        }

        public void Encrypt(byte[] data)
        {
            int length = data.Length;
            for (int i = 0; i < 3; i++)
            {
                byte num4;
                byte pThis = 0;
                int pCount = length;
                while (pCount > 0)
                {
                    num4 = data[length - pCount];
                    num4 = (byte) (num4.RollLeft(3) + pCount);
                    num4 = (byte) (num4 ^ pThis);
                    pThis = num4;
                    num4 = (byte) (pThis.RollRight(pCount) ^ 0xff);
                    num4 = (byte) (num4 + 0x48);
                    data[length - pCount] = num4;
                    pCount--;
                }
                pThis = 0;
                for (pCount = data.Length; pCount > 0; pCount--)
                {
                    num4 = data[pCount - 1];
                    num4 = (byte) (num4.RollLeft(4) + pCount);
                    num4 = (byte) (num4 ^ pThis);
                    pThis = num4;
                    num4 = (byte) (num4 ^ 0x13);
                    num4 = num4.RollRight(3);
                    data[pCount - 1] = num4;
                }
            }
            this.Transform(data);
        }

        public byte[] getHeaderToClient(int size)
        {
            byte[] buffer = new byte[4];
            int num = (this._IV[3] * 0x100) + this._IV[2];
            num ^= -(this._mapleVersion + 1);
            int num2 = num ^ size;
            buffer[0] = (byte) (num % 0x100);
            buffer[1] = (byte) ((num - buffer[0]) / 0x100);
            buffer[2] = (byte) (num2 ^ 0x100);
            buffer[3] = (byte) ((num2 - buffer[2]) / 0x100);
            return buffer;
        }

        public byte[] getHeaderToServer(int size)
        {
            byte[] buffer = new byte[4];
            int num = (this.IV[3] * 0x100) + this.IV[2];
            num ^= this._mapleVersion;
            int num2 = num ^ size;
            buffer[0] = Convert.ToByte((int) (num % 0x100));
            buffer[1] = Convert.ToByte((int) (num / 0x100));
            buffer[2] = Convert.ToByte((int) (num2 % 0x100));
            buffer[3] = Convert.ToByte((int) (num2 / 0x100));
            return buffer;
        }

        public static ushort getPacketLength(byte[] pBuffer, int pStart)
        {
            int num = ((pBuffer[pStart] | (pBuffer[pStart + 1] << 8)) | (pBuffer[pStart + 2] << 0x10)) | (pBuffer[pStart + 3] << 0x18);
            num = (num >> 0x10) ^ (num & 0xffff);
            return (ushort) num;
        }

        public static byte[] multiplyBytes(byte[] input, int count, int mult)
        {
            byte[] buffer = new byte[count * mult];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = input[i % count];
            }
            return buffer;
        }

        private void ShiftIV()
        {
            byte[] src = new byte[] { 0xf2, 0x53, 80, 0xc6 };
            for (int i = 0; i < this._IV.Length; i++)
            {
                byte index = src[1];
                byte num3 = CryptoConstants.bShuffle[index];
                byte num4 = this._IV[i];
                num3 = (byte) (num3 - num4);
                src[0] = (byte) (src[0] + num3);
                num3 = src[2];
                num3 = (byte) (num3 ^ CryptoConstants.bShuffle[num4]);
                index = (byte) (index - num3);
                src[1] = index;
                index = src[3];
                num3 = index;
                index = (byte) (index - src[0]);
                num3 = CryptoConstants.bShuffle[num3];
                num3 = (byte) (num3 + num4);
                num3 = (byte) (num3 ^ src[2]);
                src[2] = num3;
                index = (byte) (index + CryptoConstants.bShuffle[num4]);
                src[3] = index;
                uint num5 = (uint) (((src[0] | (src[1] << 8)) | (src[2] << 0x10)) | (src[3] << 0x18));
                uint num6 = num5 >> 0x1d;
                num5 = num5 << 3;
                num6 |= num5;
                src[0] = (byte) (num6 & 0xff);
                src[1] = (byte) ((num6 >> 8) & 0xff);
                src[2] = (byte) ((num6 >> 0x10) & 0xff);
                src[3] = (byte) ((num6 >> 0x18) & 0xff);
            }
            Buffer.BlockCopy(src, 0, this._IV, 0, this._IV.Length);
        }

        public void Transform(byte[] pBuffer)
        {
            int length = pBuffer.Length;
            int num2 = 0x5b0;
            int num3 = 0;
            byte[] inputBuffer = new byte[this._IV.Length * 4];
            while (length > 0)
            {
                int index = 0;
                while (index < inputBuffer.Length)
                {
                    inputBuffer[index] = this._IV[index % 4];
                    index++;
                }
                if (length < num2)
                {
                    num2 = length;
                }
                for (index = num3; index < (num3 + num2); index++)
                {
                    if (((index - num3) % inputBuffer.Length) == 0)
                    {
                        byte[] outputBuffer = new byte[inputBuffer.Length];
                        this.mTransformer.TransformBlock(inputBuffer, 0, inputBuffer.Length, outputBuffer, 0);
                        Buffer.BlockCopy(outputBuffer, 0, inputBuffer, 0, inputBuffer.Length);
                    }
                    pBuffer[index] = (byte) (pBuffer[index] ^ inputBuffer[(index - num3) % inputBuffer.Length]);
                }
                num3 += num2;
                length -= num2;
                num2 = 0x5b4;
            }
            this.ShiftIV();
        }

        public byte[] IV
        {
            get
            {
                return this._IV;
            }
            set
            {
                this._IV = value;
            }
        }
    }
}

