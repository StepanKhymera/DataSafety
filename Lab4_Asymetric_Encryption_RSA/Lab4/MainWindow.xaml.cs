using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Lab4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private const int KeyLength = 16;

        private const int EncipherBlockSizeRSA = 64;
        private const int DecipherBlockSizeRSA = 128;
        private MD5 _MD5 = new MD5();

        private readonly RC5 _rc5 = new RC5();
        private readonly RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();

        private Byte[] HashKey(string message)
        {
            var password_hash = _MD5.HashString(message);
            password_hash = password_hash.Concat(_MD5.HashArray(password_hash)).ToArray();
            return password_hash;
        }

        private async void ProcessEncipher(object sender, RoutedEventArgs e)
        {
            var stopWatch = new Stopwatch();
            string filePath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }
            var hashedKey = HashKey(PASSWORD.Text);

            var inputBytes = File.ReadAllBytes(filePath);

            
            stopWatch.Start();
            var encodedFileContent = _rc5.EncipherCBCPAD(
                inputBytes,
                hashedKey);
            stopWatch.Stop();


            RC5.Content = $"Час кодування RC5: {stopWatch.ElapsedMilliseconds} ms";

            File.WriteAllBytes(addNewFile(filePath, "EN_RC5_"), encodedFileContent);

            var rsaEncipheredBytes = await EncipherRSAAsync(inputBytes);
            if (RSA_ENCODE_PRIVATE.IsChecked.Value)
            {
                File.WriteAllBytes(addNewFile(filePath, "EN_RSA_PR_"), rsaEncipheredBytes);

            }
            else
            {
                File.WriteAllBytes(addNewFile(filePath, "EN_RSA"), rsaEncipheredBytes);

            }

        }

        private async void ProcessDecipherRC5(object sender, RoutedEventArgs e)
        {
            var stopWatch = new Stopwatch();
            string filePath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }
            var hashedKey = HashKey(PASSWORD.Text);

            var inputBytes = File.ReadAllBytes(filePath);


            stopWatch.Start();
            var decodedFileContent = _rc5.DecipherCBCPAD(
                inputBytes,
                hashedKey);
            stopWatch.Stop();

            File.WriteAllBytes(addNewFile(filePath, "DE_RC5_"), decodedFileContent);
        }

        private async void ProcessDecipherRSA(object sender, RoutedEventArgs e)
        {
            string filePath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }

            var inputBytes = File.ReadAllBytes(filePath);

            var rsaDecipheredBytes = await DecipherRSAAsync(inputBytes);
            if (RSA_DECODE_PUBLIC.IsChecked.Value)
            {
                File.WriteAllBytes(addNewFile(filePath, "DE_RSA_PU_"), rsaDecipheredBytes);

            }
            else
            {
                File.WriteAllBytes(addNewFile(filePath, "DE_RSA"), rsaDecipheredBytes);

            }
        }
        
        private static String addNewFile(string filePath, string padding)
        {
            var fi = new FileInfo(filePath);
            var fn = System.IO.Path.GetFileNameWithoutExtension(filePath);

            return System.IO.Path.Combine(fi.DirectoryName, fn + padding + fi.Extension);
        }

        private async Task<Byte[]> EncipherRSAAsync(Byte[] inputBytes)
        {
            var stopWatch = new Stopwatch();
            var encipheredBytes = new List<byte>
            {
                Capacity = inputBytes.Length * 2
            };

            if (RSA_ENCODE_PRIVATE.IsChecked.Value)
            {
                stopWatch.Start();

                await Task.Run(() =>
                {
                    for (int i = 0; i < inputBytes.Length; i += EncipherBlockSizeRSA)
                    {
                        var inputBlock = inputBytes
                            .Skip(i)
                            .Take(EncipherBlockSizeRSA)
                            .ToArray();

                        encipheredBytes.AddRange(_rsa.PrivateEncipher(inputBlock));
                    }
                });

                stopWatch.Stop();
            } else
            {
                stopWatch.Start();
                await Task.Run(() =>
                {
                    for (int i = 0; i < inputBytes.Length; i += EncipherBlockSizeRSA)
                    {
                        var inputBlock = inputBytes
                            .Skip(i)
                            .Take(EncipherBlockSizeRSA)
                            .ToArray();

                        encipheredBytes.AddRange(_rsa.Encrypt(
                            inputBlock,
                            fOAEP: false));
                    }
                });
                stopWatch.Stop();
            }
            RSA.Content = $"Час кодування RSA: {stopWatch.ElapsedMilliseconds} ms";
            return encipheredBytes.ToArray();
        }

        private async Task<Byte[]> DecipherRSAAsync(Byte[] inputBytes)
        {
            var stopWatch = new Stopwatch();
            var decipheredBytes = new List<byte>
            {
                Capacity = inputBytes.Length / 2
            };
            if (RSA_DECODE_PUBLIC.IsChecked.Value)
            {
                stopWatch.Start();

                await Task.Run(() =>
                {
                    for (int i = 0; i < inputBytes.Length; i += DecipherBlockSizeRSA)
                    {
                        var inputBlock = inputBytes
                            .Skip(i)
                            .Take(DecipherBlockSizeRSA)
                            .ToArray();

                        decipheredBytes.AddRange(_rsa.PublicDecipher(inputBlock));
                    }
                });

                stopWatch.Stop();
            } else
            {
                stopWatch.Start();
                await Task.Run(() =>
                {
                    for (int i = 0; i < inputBytes.Length; i += DecipherBlockSizeRSA)
                    {
                        var inputBlock = inputBytes
                            .Skip(i)
                            .Take(DecipherBlockSizeRSA)
                            .ToArray();

                        decipheredBytes.AddRange(_rsa.Decrypt(
                            inputBlock,
                            fOAEP: false));
                    }
                });
                stopWatch.Stop();
            }
           

            RSA.Content = $"Час декодування RSA: {stopWatch.ElapsedMilliseconds} ms";
            return decipheredBytes.ToArray();
        }
    }
}
