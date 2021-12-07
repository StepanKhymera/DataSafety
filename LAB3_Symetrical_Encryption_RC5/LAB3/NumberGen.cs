using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB3
{
    class NumberGen
    {
        public Byte[] GenereateNumbers(int bytes_num)
        {
            int x = 31;
            int a = (int)Math.Pow(7, 5);
            int c = 17711;
            int m = (int)Math.Pow(2, 31) - 1;

            List<Byte> da_data = new List<byte>(bytes_num);

            for (int i = 1; i <= bytes_num/4; ++i)
            {
                da_data = da_data.Concat(BitConverter.GetBytes(x)).ToList();
                x = (a * x + c) % m;
            }

            return da_data.ToArray();
        }
       
    }
}
