using System;
using System.Text;
namespace Xxtea
{
    public sealed class XXTEA
    {
        private const uint delta = 2654435769u;
        private static readonly UTF8Encoding utf8 = new UTF8Encoding();

        private XXTEA()
        {
        }

        private static uint MX(uint sum, uint y, uint z, int p, uint e, uint[] k)
        {
            return (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[(int)checked((IntPtr)unchecked((long)(p & 3) ^ (long)((ulong)e)))] ^ z);
        }

        /// <summary>
        /// 数据加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data.Length == 0)
            {
                return data;
            }
            return XXTEA.ToByteArray(XXTEA.Encrypt(XXTEA.ToUInt32Array(data, true), XXTEA.ToUInt32Array(XXTEA.FixKey(key), false)), false);
        }

        public static byte[] Encrypt(string data, byte[] key)
        {
            return XXTEA.Encrypt(XXTEA.utf8.GetBytes(data), key);
        }

        public static byte[] Encrypt(byte[] data, string key)
        {
            return XXTEA.Encrypt(data, XXTEA.utf8.GetBytes(key));
        }

        public static byte[] Encrypt(string data, string key)
        {
            return XXTEA.Encrypt(XXTEA.utf8.GetBytes(data), XXTEA.utf8.GetBytes(key));
        }

        /// <summary>
        /// 数据解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data.Length == 0)
            {
                return data;
            }
            return XXTEA.ToByteArray(XXTEA.Decrypt(XXTEA.ToUInt32Array(data, false), XXTEA.ToUInt32Array(XXTEA.FixKey(key), false)), true);
        }

        public static byte[] Decrypt(byte[] data, string key)
        {
            return XXTEA.Decrypt(data, XXTEA.utf8.GetBytes(key));
        }

        private static uint[] Encrypt(uint[] v, uint[] k)
        {
            int num = v.Length - 1;
            if (num < 1)
            {
                return v;
            }
            uint z = v[num];
            uint num2 = 0u;
            int num3 = 6 + 52 / (num + 1);
            while (0 < num3--)
            {
                num2 += 2654435769u;
                uint e = num2 >> 2 & 3u;
                int i;
                uint y;
                for (i = 0; i < num; i++)
                {
                    y = v[i + 1];
                    z = (v[i] += XXTEA.MX(num2, y, z, i, e, k));
                }
                y = v[0];
                z = (v[num] += XXTEA.MX(num2, y, z, i, e, k));
            }
            return v;
        }
        private static uint[] Decrypt(uint[] v, uint[] k)
        {
            int num = v.Length - 1;
            if (num < 1)
            {
                return v;
            }
            uint y = v[0];
            int num2 = 6 + 52 / (num + 1);
            for (uint num3 = (uint)((long)num2 * (long)(-1640531527)); num3 != 0u; num3 -= 2654435769u)
            {
                uint e = num3 >> 2 & 3u;
                int i;
                uint z;
                for (i = num; i > 0; i--)
                {
                    z = v[i - 1];
                    y = (v[i] -= XXTEA.MX(num3, y, z, i, e, k));
                }
                z = v[num];
                y = (v[0] -= XXTEA.MX(num3, y, z, i, e, k));
            }
            return v;
        }
        private static byte[] FixKey(byte[] key)
        {
            if (key.Length == 16)
            {
                return key;
            }
            byte[] array = new byte[16];
            if (key.Length < 16)
            {
                key.CopyTo(array, 0);
            }
            else
            {
                Array.Copy(key, 0, array, 0, 16);
            }
            return array;
        }
        private static uint[] ToUInt32Array(byte[] data, bool includeLength)
        {
            int num = data.Length;
            int num2 = ((num & 3) != 0) ? ((num >> 2) + 1) : (num >> 2);
            uint[] array;
            if (includeLength)
            {
                array = new uint[num2 + 1];
                array[num2] = (uint)num;
            }
            else
            {
                array = new uint[num2];
            }
            for (int i = 0; i < num; i++)
            {
                array[i >> 2] |= (uint)((uint)data[i] << ((i & 3) << 3));
            }
            return array;
        }
        private static byte[] ToByteArray(uint[] data, bool includeLength)
        {
            int num = data.Length << 2;
            if (includeLength)
            {
                int num2 = (int)data[data.Length - 1];
                num -= 4;
                if (num2 < num - 3 || num2 > num)
                {
                    return null;
                }
                num = num2;
            }
            byte[] array = new byte[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = (byte)(data[i >> 2] >> ((i & 3) << 3));
            }
            return array;
        }
    }
}
