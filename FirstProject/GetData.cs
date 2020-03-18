using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FirstProject
{
   public class GetData
   {
        //Считывает массив данных из файла
        public static int[] ReadFile(string filename)
        {
            List<int> arr = new List<int>();
            StreamReader file = new StreamReader(filename);
            string line;
            while (!file.EndOfStream)
            {
                line = file.ReadLine();
                arr.Add(Convert.ToInt32(line));
            }
            file.Close();
            return arr.ToArray();
        }

        //Подсчет СКО
        public static double Count (int left, int n, int[] mass)
        {
            double sum = 0;
            double x = 0;
            double sd = 0;
            for (int j = left; j < left + n; j++)
            {
                sum += mass[j];
            }
            x = sum / 10;
            for (int j = left; j < left + n; j++)
            {
                sd += (mass[j] - x)* (mass[j] - x);
            }
            sd /= 9;
            return Math.Sqrt(sd);
        }

        //Подсчет СКО для массива данных
        public static double [] RMSDeviation(int [] arr, int n)
        {          
            double[] RMS = new double[arr.Length - n];

            for (int i = 0; i < arr.Length - n; i++)
            {
                RMS[i] = Count(i, n, arr);
            }
            return RMS;
        }
   }
}
