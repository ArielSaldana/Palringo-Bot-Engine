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
    internal sealed class Salsa20 : SymmetricAlgorithm
    {
        private int m_rounds;

        public Salsa20()
        {
            base.LegalBlockSizesValue = new[] { new KeySizes(0x200, 0x200, 0) };
            base.LegalKeySizesValue = new[] { new KeySizes(0x80, 0x100, 0x80) };
            base.BlockSizeValue = 0x200;
            base.KeySizeValue = 0x100;
            this.m_rounds = 20;
        }

        public override byte[] IV
        {
            get { return base.IV; }
            set
            {
                CheckValidIV(value, "value");
                base.IVValue = (byte[])value.Clone();
            }
        }

        public int Rounds
        {
            get { return this.m_rounds; }
            set
            {
                if (((value != 8) && (value != 12)) && (value != 20))
                {
                    throw new ArgumentOutOfRangeException("value", "The number of rounds must be 8, 12, or 20.");
                }
                this.m_rounds = value;
            }
        }

        private static void CheckValidIV(byte[] iv, string paramName)
        {
            if (iv == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (iv.Length != 8)
            {
                throw new CryptographicException("Invalid IV size; it must be 8 bytes.");
            }
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return this.CreateEncryptor(rgbKey, rgbIV);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (rgbKey == null)
            {
                throw new ArgumentNullException("rgbKey");
            }
            if (!base.ValidKeySize(rgbKey.Length * 8))
            {
                throw new CryptographicException("Invalid key size; it must be 128 or 256 bits.");
            }
            CheckValidIV(rgbIV, "rgbIV");
            return new Salsa20CryptoTransform(rgbKey, rgbIV, this.m_rounds);
        }

        public override void GenerateIV()
        {
            base.IVValue = GetRandomBytes(8);
        }

        public override void GenerateKey()
        {
            base.KeyValue = GetRandomBytes(this.KeySize / 8);
        }

        private static byte[] GetRandomBytes(int byteCount)
        {
            RandomNumberGenerator generator = new RNGCryptoServiceProvider();
            var data = new byte[byteCount];
            generator.GetBytes(data);
            return data;
        }

        #region Nested type: Salsa20CryptoTransform

        private sealed class Salsa20CryptoTransform : IDisposable, ICryptoTransform
        {
            private static readonly byte[] c_sigma = Encoding.ASCII.GetBytes("expand 32-byte k");
            private static readonly byte[] c_tau = Encoding.ASCII.GetBytes("expand 16-byte k");
            private readonly int m_rounds;
            private uint[] m_state;

            public Salsa20CryptoTransform(byte[] key, byte[] iv, int rounds)
            {
                Debug.Assert((key.Length == 0x10) || (key.Length == 0x20), "abyKey.Length == 16 || abyKey.Length == 32",
                             "Invalid key size.");
                Debug.Assert(iv.Length == 8, "abyIV.Length == 8", "Invalid IV size.");
                Debug.Assert(((rounds == 8) || (rounds == 12)) || (rounds == 20),
                             "rounds == 8 || rounds == 12 || rounds == 20",
                             "Invalid number of rounds.");
                this.Initialize(key, iv);
                this.m_rounds = rounds;
            }

            #region ICryptoTransform Members

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                if (inputBuffer == null)
                {
                    throw new ArgumentNullException("inputBuffer");
                }
                if ((inputOffset < 0) || (inputOffset >= inputBuffer.Length))
                {
                    throw new ArgumentOutOfRangeException("inputOffset");
                }
                if ((inputCount < 0) || ((inputOffset + inputCount) > inputBuffer.Length))
                {
                    throw new ArgumentOutOfRangeException("inputCount");
                }
                if (outputBuffer == null)
                {
                    throw new ArgumentNullException("outputBuffer");
                }
                if ((outputOffset < 0) || ((outputOffset + inputCount) > outputBuffer.Length))
                {
                    throw new ArgumentOutOfRangeException("outputOffset");
                }
                if (this.m_state == null)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                var output = new byte[0x40];
                int num = 0;
                while (inputCount > 0)
                {
                    this.Hash(output, this.m_state);
                    this.m_state[8] = AddOne(this.m_state[8]);
                    if (this.m_state[8] == 0)
                    {
                        this.m_state[9] = AddOne(this.m_state[9]);
                    }
                    int num2 = Math.Min(0x40, inputCount);
                    for (int i = 0; i < num2; i++)
                    {
                        outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ output[i]);
                    }
                    num += num2;
                    inputCount -= 0x40;
                    outputOffset += 0x40;
                    inputOffset += 0x40;
                }
                return num;
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (inputCount < 0)
                {
                    throw new ArgumentOutOfRangeException("inputCount");
                }
                var outputBuffer = new byte[inputCount];
                this.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
                return outputBuffer;
            }

            public bool CanReuseTransform
            {
                get { return false; }
            }

            public bool CanTransformMultipleBlocks
            {
                get { return true; }
            }

            public int InputBlockSize
            {
                get { return 0x40; }
            }

            public int OutputBlockSize
            {
                get { return 0x40; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                if (this.m_state != null)
                {
                    Array.Clear(this.m_state, 0, this.m_state.Length);
                }
                this.m_state = null;
            }

            #endregion

            private static uint Add(uint v, uint w)
            {
                return (v + w);
            }

            private static uint AddOne(uint v)
            {
                return (v + 1);
            }

            private void Hash(byte[] output, uint[] input)
            {
                var numArray = (uint[])input.Clone();
                for (int i = this.m_rounds; i > 0; i -= 2)
                {
                    numArray[4] ^= Rotate(Add(numArray[0], numArray[12]), 7);
                    numArray[8] ^= Rotate(Add(numArray[4], numArray[0]), 9);
                    numArray[12] ^= Rotate(Add(numArray[8], numArray[4]), 13);
                    numArray[0] ^= Rotate(Add(numArray[12], numArray[8]), 0x12);
                    numArray[9] ^= Rotate(Add(numArray[5], numArray[1]), 7);
                    numArray[13] ^= Rotate(Add(numArray[9], numArray[5]), 9);
                    numArray[1] ^= Rotate(Add(numArray[13], numArray[9]), 13);
                    numArray[5] ^= Rotate(Add(numArray[1], numArray[13]), 0x12);
                    numArray[14] ^= Rotate(Add(numArray[10], numArray[6]), 7);
                    numArray[2] ^= Rotate(Add(numArray[14], numArray[10]), 9);
                    numArray[6] ^= Rotate(Add(numArray[2], numArray[14]), 13);
                    numArray[10] ^= Rotate(Add(numArray[6], numArray[2]), 0x12);
                    numArray[3] ^= Rotate(Add(numArray[15], numArray[11]), 7);
                    numArray[7] ^= Rotate(Add(numArray[3], numArray[15]), 9);
                    numArray[11] ^= Rotate(Add(numArray[7], numArray[3]), 13);
                    numArray[15] ^= Rotate(Add(numArray[11], numArray[7]), 0x12);
                    numArray[1] ^= Rotate(Add(numArray[0], numArray[3]), 7);
                    numArray[2] ^= Rotate(Add(numArray[1], numArray[0]), 9);
                    numArray[3] ^= Rotate(Add(numArray[2], numArray[1]), 13);
                    numArray[0] ^= Rotate(Add(numArray[3], numArray[2]), 0x12);
                    numArray[6] ^= Rotate(Add(numArray[5], numArray[4]), 7);
                    numArray[7] ^= Rotate(Add(numArray[6], numArray[5]), 9);
                    numArray[4] ^= Rotate(Add(numArray[7], numArray[6]), 13);
                    numArray[5] ^= Rotate(Add(numArray[4], numArray[7]), 0x12);
                    numArray[11] ^= Rotate(Add(numArray[10], numArray[9]), 7);
                    numArray[8] ^= Rotate(Add(numArray[11], numArray[10]), 9);
                    numArray[9] ^= Rotate(Add(numArray[8], numArray[11]), 13);
                    numArray[10] ^= Rotate(Add(numArray[9], numArray[8]), 0x12);
                    numArray[12] ^= Rotate(Add(numArray[15], numArray[14]), 7);
                    numArray[13] ^= Rotate(Add(numArray[12], numArray[15]), 9);
                    numArray[14] ^= Rotate(Add(numArray[13], numArray[12]), 13);
                    numArray[15] ^= Rotate(Add(numArray[14], numArray[13]), 0x12);
                }
                for (int j = 0; j < 0x10; j++)
                {
                    ToBytes(Add(numArray[j], input[j]), output, 4 * j);
                }
            }

            private void Initialize(byte[] key, byte[] iv)
            {
                this.m_state = new uint[0x10];
                this.m_state[1] = ToUInt32(key, 0);
                this.m_state[2] = ToUInt32(key, 4);
                this.m_state[3] = ToUInt32(key, 8);
                this.m_state[4] = ToUInt32(key, 12);
                byte[] input = (key.Length == 0x20) ? c_sigma : c_tau;
                int inputOffset = key.Length - 0x10;
                this.m_state[11] = ToUInt32(key, inputOffset);
                this.m_state[12] = ToUInt32(key, inputOffset + 4);
                this.m_state[13] = ToUInt32(key, inputOffset + 8);
                this.m_state[14] = ToUInt32(key, inputOffset + 12);
                this.m_state[0] = ToUInt32(input, 0);
                this.m_state[5] = ToUInt32(input, 4);
                this.m_state[10] = ToUInt32(input, 8);
                this.m_state[15] = ToUInt32(input, 12);
                this.m_state[6] = ToUInt32(iv, 0);
                this.m_state[7] = ToUInt32(iv, 4);
                this.m_state[8] = 0;
                this.m_state[9] = 0;
            }

            private static uint Rotate(uint v, int c)
            {
                return ((v << c) | (v >> (0x20 - c)));
            }

            private static void ToBytes(uint input, byte[] output, int outputOffset)
            {
                output[outputOffset] = (byte)input;
                output[outputOffset + 1] = (byte)(input >> 8);
                output[outputOffset + 2] = (byte)(input >> 0x10);
                output[outputOffset + 3] = (byte)(input >> 0x18);
            }

            private static uint ToUInt32(byte[] input, int inputOffset)
            {
                return
                    (uint)
                    (((input[inputOffset] | (input[inputOffset + 1] << 8)) | (input[inputOffset + 2] << 0x10)) |
                     (input[inputOffset + 3] << 0x18));
            }
        }

        #endregion
    }
}
