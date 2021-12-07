using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB3
{
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
}
