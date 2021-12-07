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
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;

namespace lab1_Gen
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0-9]+");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void fast_Button_Click(object sender, RoutedEventArgs e)
        {
            ulong x0;
            ulong a;
            ulong c;
            ulong m;
            Result.Clear();

            try
            {
                x0 = ulong.Parse(X.Text.Split(" ")[0]);
                a = ulong.Parse(A.Text.Split(" ")[0]);

                c = ulong.Parse(C.Text.Split(" ")[0]);
                if (int.Parse(M.Text.Split(" ")[0]) > 31) M.Text = "31";
                m = (ulong)Math.Pow(2, ulong.Parse(M.Text.Split(" ")[0])) - ulong.Parse(N.Text.Split(" ")[0]);

            }
            catch
            {
                return;
            }

            ulong x = x0;
            ulong x_start = (a * x + c) % m;
            x = x_start;
            ulong temp;
            FileStream trunc = File.Open("FileOutput.txt", FileMode.Truncate);
            trunc.Close();
            StreamWriter file = new("FileOutput.txt", append: true);
            file.WriteLine(ulong.Parse(X.Text.Split(" ")[0]));
            file.WriteLine(x_start);
            int count = int.Parse(Count.Text);
            file.WriteLine("----------------------------------Stepan Khymera");
            for (int i = 1; (file.BaseStream.CanWrite); ++i)
            {
                if(i % 100000 == 0) { file.WriteLine("----------------------------------Stepan Khymera"); }
                x = (a * x + c) % m;
                file.WriteLine(x);
                if (x == 31 )
                {
                    U_Count.Text = (i + 1).ToString();
                    file.WriteLine("----------------------------------Stepan Khymera");
                    file.Flush();
                    file.Close();
                }
            }
            if (file.BaseStream.CanWrite)
            {
                file.Flush();
                file.Close();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ulong x0;
            ulong a;
            ulong c;
            ulong m;
            Result.Clear();

            try
            {
                 x0 = ulong.Parse(X.Text.Split(" ")[0]);
                 a = ulong.Parse(A.Text.Split(" ")[0]);
                 
                 c = ulong.Parse(C.Text.Split(" ")[0]);
                if (int.Parse(M.Text.Split(" ")[0]) > 31) M.Text = "31";
                m = (ulong) Math.Pow(2, ulong.Parse(M.Text.Split(" ")[0])) - ulong.Parse(N.Text.Split(" ")[0]);

            } catch
            {
                return;
            }

            ulong x = x0;
            ulong x_start = (a * x + c) % m;
            x = x_start;
            ulong temp;
            FileStream trunc = File.Open("Test.txt", FileMode.Truncate);
            trunc.Close();
            StreamWriter file = new("Test.txt", append: true);
            file.WriteLine(ulong.Parse(X.Text.Split(" ")[0]));
            file. WriteLine(x_start);
            for (int i = 1; (i < int.Parse(Count.Text) || file.BaseStream.CanWrite) ; ++i)
            {
                temp = (a * x + c) % m;
                if(x == temp && file.BaseStream.CanWrite)
                {
                    U_Count.Text = (i+1).ToString();
                    file.Flush();
                    file.Close();
                }
                x = temp;
                if(file.BaseStream.CanWrite) file.WriteLine(x);
                if((x == x_start || x == x0) && file.BaseStream.CanWrite)
                {
                    U_Count.Text = (i+1).ToString();
                    file.Flush();
                    file.Close();
                }
                Result.Text += x.ToString() + " ";
            }
            if (file.BaseStream.CanWrite)
            {
                file.Flush();
                file.Close();
            }
        }

    }
}
