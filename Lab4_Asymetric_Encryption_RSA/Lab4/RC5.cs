using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Lab4
{
    class RC5
    {
        private readonly NumberGen numberGenerator = new NumberGen();
        private readonly WordTools words = new WordTools();
        private readonly Int32 rounds = 20;
        public RC5()
        {

        }

        public Byte[] EncipherCBCPAD(Byte[] input, Byte[] key)
        {
            var paddedBytes = input.Concat(GetPadding(input)).ToArray();
            var bytesPerBlock = words.BytesPerBlock;
            var s = BuildExpandedKeyTable(key);
            var cnPrev = numberGenerator.GenereateNumbers(bytesPerBlock);
            var encodedFileContent = new Byte[cnPrev.Length + paddedBytes.Length];

            EncipherECB(cnPrev, encodedFileContent, inStart: 0, outStart: 0, s);

            for (int i = 0; i < paddedBytes.Length; i += bytesPerBlock)
            {
                var current_block = new Byte[bytesPerBlock];
                Array.Copy(paddedBytes, i, current_block, 0, current_block.Length);

                for (int x = 0; x < current_block.Length; ++x)
                {
                    current_block[x] ^= cnPrev[x];
                }
                var a = words.CreateFromBytes(current_block, 0);
                var b = words.CreateFromBytes(current_block, 0 + words.BytesPerWord);
                a.Add(s[0]);
                b.Add(s[1]);
                for (var x = 1; x < rounds + 1; ++x)
                {
                    a.XorWith(b).ROL(b.ToInt32()).Add(s[2 * x]);
                    b.XorWith(a).ROL(a.ToInt32()).Add(s[2 * x + 1]);
                }
                a.FillBytesArray(encodedFileContent, i + bytesPerBlock);
                b.FillBytesArray(encodedFileContent, i + bytesPerBlock + words.BytesPerWord);
                Array.Copy(encodedFileContent, i + bytesPerBlock, cnPrev, 0, current_block.Length);
            }

            return encodedFileContent;
        }

        public Byte[] DecipherCBCPAD(Byte[] input, Byte[] key)
        {
            var bytesPerBlock = words.BytesPerBlock;
            var s = BuildExpandedKeyTable(key);
            var cnPrev = new Byte[bytesPerBlock];
            var decodedFileContent = new Byte[input.Length - cnPrev.Length];

            DecipherECB(
                inBuf: input,
                outBuf: cnPrev,
                inStart: 0,
                outStart: 0,
                s: s);

            for (int i = bytesPerBlock; i < input.Length; i += bytesPerBlock)
            {
                var cn = new Byte[bytesPerBlock];
                Array.Copy(input, i, cn, 0, cn.Length);

                DecipherECB(
                    inBuf: cn,
                    outBuf: decodedFileContent,
                    inStart: 0,
                    outStart: i - bytesPerBlock,
                    s: s);

                for (int x = 0; x < cn.Length; ++x)
                {
                    decodedFileContent[x + i - bytesPerBlock] ^= cnPrev[x];
                }

                Array.Copy(input, i, cnPrev, 0, cnPrev.Length);
            }

            var decodedWithoutPadding = new Byte[decodedFileContent.Length - decodedFileContent.Last()];
            Array.Copy(decodedFileContent, decodedWithoutPadding, decodedWithoutPadding.Length);

            return decodedWithoutPadding;
        }

        private void EncipherECB(Byte[] inBytes, Byte[] outBytes, Int32 inStart, Int32 outStart, Word[] s)
        {
            var a = words.CreateFromBytes(inBytes, inStart);
            var b = words.CreateFromBytes(inBytes, inStart + words.BytesPerWord);

            a.Add(s[0]);
            b.Add(s[1]);

            for (var i = 1; i < rounds + 1; ++i)
            {
                a.XorWith(b).ROL(b.ToInt32()).Add(s[2 * i]);
                b.XorWith(a).ROL(a.ToInt32()).Add(s[2 * i + 1]);
            }

            a.FillBytesArray(outBytes, outStart);
            b.FillBytesArray(outBytes, outStart + words.BytesPerWord);
        }

        private void DecipherECB(Byte[] inBuf, Byte[] outBuf, Int32 inStart, Int32 outStart, Word[] s)
        {
            var a = words.CreateFromBytes(inBuf, inStart);
            var b = words.CreateFromBytes(inBuf, inStart + words.BytesPerWord);

            for (var i = rounds; i > 0; --i)
            {
                b = b.Sub(s[2 * i + 1]).ROR(a.ToInt32()).XorWith(a);
                a = a.Sub(s[2 * i]).ROR(b.ToInt32()).XorWith(b);
            }

            a.Sub(s[0]);
            b.Sub(s[1]);

            a.FillBytesArray(outBuf, outStart);
            b.FillBytesArray(outBuf, outStart + words.BytesPerWord);
        }

        private Byte[] GetPadding(Byte[] inBytes)
        {
            var paddingLength = words.BytesPerBlock - inBytes.Length % (words.BytesPerBlock);

            var padding = new Byte[paddingLength];

            for (int i = 0; i < padding.Length; ++i)
            {
                padding[i] = (Byte)paddingLength;
            }

            return padding;
        }

        private Word[] BuildExpandedKeyTable(Byte[] key)
        {
            var keysWordArrLength = key.Length % words.BytesPerWord > 0
                ? key.Length / words.BytesPerWord + 1
                : key.Length / words.BytesPerWord;

            var lArr = new Word[keysWordArrLength];

            for (int i = 0; i < lArr.Length; i++)
            {
                lArr[i] = words.Create();
            }

            for (var i = key.Length - 1; i >= 0; i--)
            {
                lArr[i / words.BytesPerWord].ROL(RC5Constants.BitsPerByte).Add(key[i]);
            }

            var sArray = new Word[2 * (rounds + 1)];
            sArray[0] = words.CreateP();
            var q = words.CreateQ();

            for (var i = 1; i < sArray.Length; i++)
            {
                sArray[i] = sArray[i - 1].Clone();
                sArray[i].Add(q);
            }

            var x = words.Create();
            var y = words.Create();

            var n = 3 * Math.Max(sArray.Length, lArr.Length);

            for (Int32 k = 0, i = 0, j = 0; k < n; ++k)
            {
                sArray[i].Add(x).Add(y).ROL(3);
                x = sArray[i].Clone();

                lArr[j].Add(x).Add(y).ROL(x.ToInt32() + y.ToInt32());
                y = lArr[j].Clone();

                i = (i + 1) % sArray.Length;
                j = (j + 1) % lArr.Length;
            }

            return sArray;
        }
    }

    class NumberGen
    {
        public Byte[] GenereateNumbers(int bytes_num)
        {
            int x = 31;
            int a = (int)Math.Pow(7, 5);
            int c = 17711;
            int m = (int)Math.Pow(2, 31) - 1;

            List<Byte> da_data = new List<byte>(bytes_num);

            for (int i = 1; i <= bytes_num / 4; ++i)
            {
                da_data = da_data.Concat(BitConverter.GetBytes(x)).ToList();
                x = (a * x + c) % m;
            }

            return da_data.ToArray();
        }

    }

    class MD5
    {
        MD Hash;
        uint chunk_size = 2097152;
        public static UInt32[] FetchBlock(Byte[] message, UInt32 iteration)
        {
            var block = new UInt32[16];
            for (UInt32 i = 0; i < 64; i += 4)
            {
                var j = iteration * 64 + i;
                block[i / 4] = message[j] | (((UInt32)message[j + 1]) << ((Int32)8 * 1)) | (((UInt32)message[j + 2]) << ((Int32)8 * 2)) | (((UInt32)message[j + 3]) << ((Int32)8 * 3));
            }
            return block;
        }
        private static Byte[] Padd(Byte[] message)
        {
            uint paddingsize = 0;
            uint tail = (UInt32)(message.Length * 8 % 512);

            if (tail == 448) { paddingsize = 64; }
            if (tail > 448)
            {
                paddingsize = (512 - tail + 448) / 8;
            }
            if (tail < 448)
            {
                paddingsize = (448 - tail) / 8;
            }

            var padding = new Byte[paddingsize + 8];
            padding[0] = 128;

            var m_length = message.Length * (UInt32)8;
            for (var i = 0; i < 8; ++i)
            {
                padding[paddingsize + i] = (Byte)(m_length >> (Int32)(i * 8));
            }

            //padding[paddingLengthInBytes + i] = (Byte)((ulong)m_length << (Int32)(m_length_bitsize - (8 * (8 - i))));
            return message.Concat(padding).ToArray();
        }

        public Byte[] HashString(String m)
        {
            Hash = new MD();
            var message = Encoding.ASCII.GetBytes(m);
            HashByteArray(Padd(message));
            return Hash.GetBytes();
        }
        public Byte[] HashArray(Byte[] m)
        {
            Hash = new MD();
            HashByteArray(Padd(m));
            return Hash.GetBytes();
        }

        public static UInt32 F(UInt32 B, UInt32 C, UInt32 D) => (B & C) | (~B & D);
        public static UInt32 G(UInt32 B, UInt32 C, UInt32 D) => (D & B) | (C & ~D);
        public static UInt32 H(UInt32 B, UInt32 C, UInt32 D) => B ^ C ^ D;
        public static UInt32 I(UInt32 B, UInt32 C, UInt32 D) => C ^ (B | ~D);

        private void HashByteArray(Byte[] message)
        {
            for (UInt32 block = 0; block < message.Length / 64; ++block)
            {
                var X = FetchBlock(message, block);
                UInt32 f, i;
                var temp_MD = Hash.Clone();
                for (i = 0; i < 16; ++i)
                {
                    f = F(temp_MD.B, temp_MD.C, temp_MD.D);
                    temp_MD.EOI(f, X, i, i);
                }
                for (; i < 32; ++i)
                {
                    f = G(temp_MD.B, temp_MD.C, temp_MD.D);
                    temp_MD.EOI(f, X, i, (1 + (5 * i)) % 16);
                }
                for (; i < 48; ++i)
                {
                    f = H(temp_MD.B, temp_MD.C, temp_MD.D);
                    temp_MD.EOI(f, X, i, (5 + (3 * i)) % 16);
                }
                for (; i < 64; ++i)
                {
                    f = I(temp_MD.B, temp_MD.C, temp_MD.D);
                    temp_MD.EOI(f, X, i, 7 * i % (16));
                }

                Hash.addCurrentBlockHash(temp_MD);
            }
        }
    }

    class MD
    {
        public UInt32 A { get; set; }

        public UInt32 B { get; set; }

        public UInt32 C { get; set; }

        public UInt32 D { get; set; }

        internal void EOI(UInt32 F, UInt32[] X, UInt32 i, UInt32 k)
        {
            var tempD = D;
            D = C;
            C = B;
            B += LeftRotate(A + F + X[k] + T[i], S[i]);
            A = tempD;
        }
        public Byte[] GetBytes()
        {
            Byte[] byte_array = new byte[16];
            byte_array = byte_array.Concat(BitConverter.GetBytes(A)).ToArray();
            byte_array = byte_array.Concat(BitConverter.GetBytes(B)).ToArray();
            byte_array = byte_array.Concat(BitConverter.GetBytes(C)).ToArray();
            byte_array = byte_array.Concat(BitConverter.GetBytes(D)).ToArray();
            return byte_array;
        }
        public override String ToString()
        {
            string hashString = "";
            for (int i = 0; i < 4; ++i)
            {
                hashString += BitConverter.GetBytes(A)[i].ToString("x2");
            }
            for (int i = 0; i < 4; ++i)
            {
                hashString += BitConverter.GetBytes(B)[i].ToString("x2");
            }
            for (int i = 0; i < 4; ++i)
            {
                hashString += BitConverter.GetBytes(C)[i].ToString("x2");
            }
            for (int i = 0; i < 4; ++i)
            {
                hashString += BitConverter.GetBytes(D)[i].ToString("x2");
            }
            return hashString;
        }

        public static UInt32 LeftRotate(UInt32 value, Int32 shiftValue)
        {
            return (value << shiftValue) | (value >> (Int32)(32 - shiftValue));
        }
        public Int32[] S = new Int32[] {
            7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,
            5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,
            4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,
            6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21
        };
        public UInt32[] T = new UInt32[64]
       {
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
            0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
            0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
            0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
            0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
            0xd62f105d, 0x2441453,  0xd8a1e681, 0xe7d3fbc8,
            0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
            0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
            0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
            0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
            0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x4881d05,
            0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
            0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
            0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
            0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
            0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
       };
        public MD()
        {
            A = 0x67452301;
            B = 0xefcdab89;
            C = 0x98badcfe;
            D = 0x10325476;
        }
        public MD Clone()
        {
            return MemberwiseClone() as MD;
        }
        public void addCurrentBlockHash(MD blockHash)
        {
            A += blockHash.A;
            B += blockHash.B;
            C += blockHash.C;
            D += blockHash.D;
        }
    }

    public static class RC5Constants
    {
        public const UInt32 P32 = 0xB7E15162;
        public const UInt32 Q32 = 0x9E3779B9;
        public const Int32 BitsPerByte = 8;
        public const Int32 ByteMask = 0b11111111;
    }

    class Word
    {
        public const Int32 WordSizeInBits = BytesPerWord * RC5Constants.BitsPerByte;
        public const Int32 BytesPerWord = sizeof(UInt32);

        public UInt32 WordValue { get; set; }

        public void CreateFromBytes(Byte[] bytes, Int32 startFrom)
        {
            WordValue = 0;

            for (var i = startFrom + BytesPerWord - 1; i > startFrom; --i)
            {
                WordValue |= bytes[i];
                WordValue <<= RC5Constants.BitsPerByte;
            }

            WordValue |= bytes[startFrom];
        }

        public Byte[] FillBytesArray(Byte[] bytesToFill, Int32 startFrom)
        {
            var i = 0;
            for (; i < BytesPerWord - 1; ++i)
            {
                bytesToFill[startFrom + i] = (Byte)(WordValue & RC5Constants.ByteMask);
                WordValue >>= RC5Constants.BitsPerByte;
            }

            bytesToFill[startFrom + i] = (Byte)(WordValue & RC5Constants.ByteMask);

            return bytesToFill;
        }

        public Word ROL(Int32 offset)
        {
            WordValue = (WordValue << offset) | (WordValue >> (WordSizeInBits - offset));

            return this;
        }

        public Word ROR(Int32 offset)
        {
            WordValue = (WordValue >> offset) | (WordValue << (WordSizeInBits - offset));

            return this;
        }

        public Word Add(Word word)
        {
            WordValue += (word as Word).WordValue;

            return this;
        }

        public Word Add(Byte value)
        {
            WordValue += value;

            return this;
        }

        public Word Sub(Word word)
        {
            WordValue -= (word as Word).WordValue;

            return this;
        }

        public Word XorWith(Word word)
        {
            WordValue ^= (word as Word).WordValue;

            return this;
        }

        public Word Clone()
        {
            return (Word)MemberwiseClone();
        }

        public Int32 ToInt32()
        {
            return (Int32)WordValue;
        }
    }
    class WordTools
    {
        public Int32 BytesPerWord => Word.BytesPerWord;

        public Int32 BytesPerBlock => BytesPerWord * 2;

        public Word Create()
        {
            return CreateConcrete();
        }

        public Word CreateP()
        {
            return CreateConcrete(RC5Constants.P32);
        }

        public Word CreateQ()
        {
            return CreateConcrete(RC5Constants.Q32);
        }

        public Word CreateFromBytes(Byte[] bytes, Int32 startFromIndex)
        {
            var word = Create();
            word.CreateFromBytes(bytes, startFromIndex);

            return word;
        }

        public Word CreateConcrete(UInt32 value = 0)
        {
            return new Word
            {
                WordValue = value
            };
        }
    }

    public static class RSAPrivateEncryption
    {
        public static byte[] PrivateEncipher(this RSACryptoServiceProvider rsa, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (rsa.PublicOnly)
                throw new InvalidOperationException("Private key is not loaded");

            int maxDataLength = (rsa.KeySize / 8) - 6;
            if (data.Length > maxDataLength)
                throw new ArgumentOutOfRangeException("data", string.Format(
                    "Maximum data length for the current key size ({0} bits) is {1} bytes (current length: {2} bytes)",
                    rsa.KeySize, maxDataLength, data.Length));

            BigInteger numData = GetBig(AddPadding(data));

            RSAParameters rsaParams = rsa.ExportParameters(true);
            BigInteger D = GetBig(rsaParams.D);
            BigInteger Modulus = GetBig(rsaParams.Modulus);
            BigInteger encData = BigInteger.ModPow(numData, D, Modulus);

            return encData.ToByteArray();
        }

        public static byte[] PublicDecipher(this RSACryptoServiceProvider rsa, byte[] cipherData)
        {
            if (cipherData == null)
                throw new ArgumentNullException("cipherData");

            BigInteger numEncData = new BigInteger(cipherData);

            RSAParameters rsaParams = rsa.ExportParameters(false);
            BigInteger Exponent = GetBig(rsaParams.Exponent);
            BigInteger Modulus = GetBig(rsaParams.Modulus);

            BigInteger decData = BigInteger.ModPow(numEncData, Exponent, Modulus);

            byte[] data = decData.ToByteArray();
            byte[] result = new byte[data.Length - 1];
            Array.Copy(data, result, result.Length);
            result = RemovePadding(result);

            Array.Reverse(result);
            return result;
        }

        private static BigInteger GetBig(byte[] data)
        {
            byte[] inArr = (byte[])data.Clone();
            Array.Reverse(inArr);
            byte[] final = new byte[inArr.Length + 1];
            Array.Copy(inArr, final, inArr.Length);

            return new BigInteger(final);
        }

        private static byte[] AddPadding(byte[] data)
        {
            Random rnd = new Random();
            byte[] paddings = new byte[4];
            rnd.NextBytes(paddings);
            paddings[0] = (byte)(paddings[0] | 128);

            byte[] results = new byte[data.Length + 4];

            Array.Copy(paddings, results, 4);
            Array.Copy(data, 0, results, 4, data.Length);
            return results;
        }

        private static byte[] RemovePadding(byte[] data)
        {
            byte[] results = new byte[data.Length - 4];
            Array.Copy(data, results, results.Length);
            return results;
        }
    }
    

}
