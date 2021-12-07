using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace PDS_LAB2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
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

            if (tail == 448){ paddingsize = 64; }
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

        public void HashFile(String filePath)
        {
            Hash = new MD();
            using (var fs = File.OpenRead(filePath))
            {
                UInt64 totalBytesRead = 0;
                Int32 currentBytesRead = 0;
                bool isFileEnd = false;

                do
                {
                    var chunk = new Byte[chunk_size];
                    currentBytesRead = fs.ReadAsync(chunk, 0, chunk.Length).Result;
                    totalBytesRead += (UInt64)currentBytesRead;

                    if (currentBytesRead < chunk.Length)
                    {
                        Byte[] lastChunk = new Byte[currentBytesRead];

                        if (currentBytesRead != 0)
                        {
                            Array.Copy(chunk, lastChunk, currentBytesRead);
                        }
                        uint paddingsize = 0;
                        uint tail = (UInt32)(totalBytesRead * 8 % 512);

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

                        var m_length = totalBytesRead * (UInt32)8;
                        for (var i = 0; i < 8; ++i)
                        {
                            padding[paddingsize + i] = (Byte)(m_length >> (Int32)(i * 8));
                        }
                        chunk = lastChunk.Concat(padding).ToArray();
                        isFileEnd = true;
                    }

                    HashByteArray(chunk);
                    
                }
                while (isFileEnd == false);
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hash = new MD();
            var message = Encoding.ASCII.GetBytes(StringINPUT.Text);
            HashByteArray(Padd(message));          
            RESULT.Text = Hash.ToString();
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            String filePath = "";
            var openFileDialog = new OpenFileDialog();

            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }
            HashFile(filePath);
            RESULT.Text = Hash.ToString();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FILE_CHECK.Content = "";
            String filePath = "";
            var openFileDialog = new OpenFileDialog();

            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }
            HashFile(filePath);
            MY_HASH.Text = Hash.ToString();
            if(MY_HASH.Text != INPUT_HASH.Text)
            {
                FILE_CHECK.Content = "Hash codes are different";
                FILE_CHECK.Foreground = Brushes.Red;
            }
            else
            {
                FILE_CHECK.Content = "Hash codes are the same";
                FILE_CHECK.Foreground = Brushes.Green;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            FileStream trunc = File.Open("FileOutput.txt", FileMode.OpenOrCreate | FileMode.Truncate);
            trunc.Close();
            File.WriteAllTextAsync("FileOutput.txt", RESULT.Text);
        }
    }
}
