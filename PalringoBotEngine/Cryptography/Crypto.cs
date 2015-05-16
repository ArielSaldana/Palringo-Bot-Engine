/*
 * 
 * By: Ariel Saldana
 * Released under the MIT License
 * https://github.com/arielsaldana
 * http://ahhriel.com
 * 
 */

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace PalringoBotEngine
{
    internal static class Crypto
    {
        private static byte[] DoubleMd5(byte[] abyte0, byte[] abyte1)
        {
            byte[] sourceArray = Md5(abyte0);
            var destinationArray = new byte[sourceArray.Length + abyte1.Length];
            Array.Copy(sourceArray, 0, destinationArray, 0, sourceArray.Length);
            Array.Copy(abyte1, 0, destinationArray, sourceArray.Length, abyte1.Length);
            return Md5(destinationArray);
        }

        public static byte[] GenerateAuth(byte[] keyAndIv, byte[] password)
        {
            var buffer = new byte[8];
            int index = 0;
            for (index = 16; index < 24; index++)
            {
                buffer[index - 16] = keyAndIv[index];
            }
            byte[] key = DoubleMd5(password, buffer);
            byte[] buffer3 = GenerateRandomBytes(16);
            DoubleMd5(password, buffer3);
            var data = new byte[32];
            for (index = 0; index < 16; index++)
            {
                data[index] = keyAndIv[index];
                data[index + 16] = buffer3[index];
            }
            return Salsa20(buffer, key, data);
        }

        private static byte[] GenerateRandomBytes(int i)
        {
            var data = new byte[i];
            RandomNumberGenerator.Create().GetBytes(data);
            return data;
        }

        private static byte[] Md5(byte[] input)
        {
            return MD5.Create().ComputeHash(input);
        }

        private static byte[] Salsa20(byte[] iv, byte[] key, byte[] data)
        {
            using (ICryptoTransform transform = new Salsa20().CreateEncryptor(key, iv))
            {
                var outputBuffer = new byte[0x20];
                transform.TransformBlock(data, 0, data.Length, outputBuffer, 0);
                return outputBuffer;
            }
        }
    }
}
