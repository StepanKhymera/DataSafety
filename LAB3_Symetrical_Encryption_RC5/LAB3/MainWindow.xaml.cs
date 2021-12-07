using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace LAB3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RC5 _rc5 = new RC5();
        private MD5 _MD5 = new MD5();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(Object sender, EventArgs e)
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
            var hashedKey = HashKey(PASSWORD.Text);

            var encodedFileContent = _rc5.EncipherCBCPAD(
                File.ReadAllBytes(filePath),
                hashedKey);

            File.WriteAllBytes(addNewFile(filePath, $"(E_PSW({PASSWORD.Text}))"), encodedFileContent);
        }

        private void Button_Click_1(Object sender, EventArgs e)
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
            var hashedKey = HashKey(PASSWORD.Text);

            var decodedFileContent = _rc5.DecipherCBCPAD(
                File.ReadAllBytes(filePath),
                hashedKey);

            File.WriteAllBytes(addNewFile(filePath, $"(D_PSW({PASSWORD.Text}))"), decodedFileContent);
        }

        private Byte[] HashKey(string message)
        {
            var password_hash = _MD5.HashString(message);
            password_hash = password_hash.Concat(_MD5.HashArray(password_hash)).ToArray();
            return password_hash;
        }

        private static String addNewFile(string filePath, string padding)
        {
            var fi = new FileInfo(filePath);
            var fn = System.IO.Path.GetFileNameWithoutExtension(filePath);

            return System.IO.Path.Combine(fi.DirectoryName,fn + padding + fi.Extension);
        }

    }
}
