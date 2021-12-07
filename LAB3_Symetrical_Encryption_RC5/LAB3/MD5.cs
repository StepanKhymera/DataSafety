using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB3
{
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
}
