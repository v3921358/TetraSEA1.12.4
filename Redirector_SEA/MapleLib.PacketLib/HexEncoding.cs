namespace MapleLib.PacketLib
{
    using System;
    using System.Globalization;

    public class HexEncoding
    {
        public static byte[] GetBytes(string hexString)
        {
            int num;
            string str = string.Empty;
            for (num = 0; num < hexString.Length; num++)
            {
                char c = hexString[num];
                if (IsHexDigit(c))
                {
                    str = str + c;
                }
            }
            if ((str.Length % 2) != 0)
            {
                str = str.Substring(0, str.Length - 1);
            }
            int num2 = str.Length / 2;
            byte[] buffer = new byte[num2];
            int num3 = 0;
            for (num = 0; num < buffer.Length; num++)
            {
                string hex = new string(new char[] { str[num3], str[num3 + 1] });
                buffer[num] = HexToByte(hex);
                num3 += 2;
            }
            return buffer;
        }

        private static byte HexToByte(string hex)
        {
            if ((hex.Length > 2) || (hex.Length <= 0))
            {
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            }
            return byte.Parse(hex, NumberStyles.HexNumber);
        }

        public static bool IsHexDigit(char c)
        {
            int num2 = Convert.ToInt32('A');
            int num3 = Convert.ToInt32('0');
            c = char.ToUpper(c);
            int num = Convert.ToInt32(c);
            return (((num >= num2) && (num < (num2 + 6))) || ((num >= num3) && (num < (num3 + 10))));
        }

        public static string ToStringFromAscii(byte[] bytes)
        {
            char[] chArray = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                if ((bytes[i] < 0x20) && (bytes[i] >= 0))
                {
                    chArray[i] = '.';
                }
                else
                {
                    int num2 = bytes[i] & 0xff;
                    chArray[i] = (char) num2;
                }
            }
            return new string(chArray);
        }
    }
}

