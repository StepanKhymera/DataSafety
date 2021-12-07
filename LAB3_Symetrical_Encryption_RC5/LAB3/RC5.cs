using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB3
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
}
