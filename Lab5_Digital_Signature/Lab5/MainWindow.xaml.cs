using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace Lab5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly DSACryptoServiceProvider _dsa = new DSACryptoServiceProvider();
        private readonly SHA1 _sha1 = SHA1.Create();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Of_String(object sender, RoutedEventArgs e)
        {
            ProcessSignature(Encoding.Default.GetBytes(STRING.Text));
        }

        private void Of_File(object sender, RoutedEventArgs e)
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
            ProcessSignature(File.ReadAllBytes(filePath));
        }

        private void ProcessSignature(Byte[] message)
        {
            byte[] hash = _sha1.ComputeHash(message);
            string result = Convert.ToBase64String(_dsa.CreateSignature(hash));
            check_result.Content = "";
            SIGNA.Text = result.Trim();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SIGNA.Text))
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                RestoreDirectory = true,
                FileName = "Signature.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, SIGNA.Text);
            }
        }

        private bool VerifySignature(byte[] message, string sign)
        {
            try
            {
                byte[] hash = _sha1.ComputeHash(message);
                bool verified = _dsa.VerifySignature(hash, Convert.FromBase64String(sign));
                return verified;
            }
            catch
            {
                return false;
            }
        }

        private void Verify(object sender, RoutedEventArgs e)
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
            byte[] message = File.ReadAllBytes(filePath);
            string sign = SIGNA.Text;

            var result = VerifySignature(message, sign)
                ? "Signature is correct"
                : "Signature is incorrect";
            check_result.Content = result;
           
        }
    }
}
