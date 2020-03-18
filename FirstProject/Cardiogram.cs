using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirstProject
{
    public partial class Cardiogram : Form
    {
        public Cardiogram()
        {
            InitializeComponent();
            //chart2.Series.Add("point");
            //chart2.Series["point"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart2.Series.Add("best");
            chart2.Series["best"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            this.WindowState = FormWindowState.Maximized;
            g = this.CreateGraphics();
        }

        private void Cardiogram_Load(object sender, EventArgs e)
        {
            
        }
        Graphics g;
        int s = 0, r, l, n;
        List<PointF> list, list2;
        double[] RMS1, RMS2;
        int[] mass1, mass2;

        //Кнопка загрузки примера
        private void button1_Click(object sender, EventArgs e)
        {            
            l = (int)(numericUpDown1.Value);
            r = (int)(numericUpDown2.Value);
            //1 ЭКГ
            mass1 = GetData.ReadFile(@"test1.rr");
            n = (int)(numericUpDown3.Value);

            //СКО 1 ЭКГ
            RMS1 = GetData.RMSDeviation(mass1, n);

            chart1.Series["Data"].Points.Clear();
            //Рисуем СКО ЭКГ
            for (int i = l; i < RMS1.Length - r; i++)
                    chart1.Series["Data"].Points.AddY(RMS1[i]);
            s++;
            chart2.Series.Add(Convert.ToString(s));
            chart2.Series[Convert.ToString(s)].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            //Получаем чувствительность и специфичность для 1 ЭКГ
            list = SensAndSpec(mass1, GetBordersIndexes(GetTimeInterval(mass1)), l, r);
            //Рисуем ROC-диаграмму
            for (int i = 0; i < list.Count(); i++)
                chart2.Series[Convert.ToString(s)].Points.AddXY(list[i].X, list[i].Y);          
        }

        //Получение лучшей точки на ROC-диаграмме
        public PointF GetBest(List<PointF> list)
        {
            //Делаем выборку
            List<float> newList = new List<float>();
            for (int i = 0; i < list.Count; i++)
                newList.Add(list[i].Y%list[i].X);  
            //Упорядочиваем по убыванию
            newList.Sort();

            PointF res = new PointF();
            for (int i = 0; i < list.Count; i++)
                if (list[i].Y % list[i].X == newList[newList.Count - 1])
                {
                    res = list[i];
                    break;
                }

            return res;
        }

        //Получение уровня отсечки, соответствующего лучшей точке
        public double GetBestLevel(PointF best, double[] data, List<PointF> list)
        {
            double lev = 0;
            for (int i = 0; i < list.Count; i++)
                if (list[i].X == best.X && list[i].Y == best.Y)
                    lev = data[i];
            return lev;
        }

        private int[] GetTimeInterval(int[] data)
        {
            int[] time = new int[data.Length];
            time[0] = data[0];
            for (int i = 1; i < data.Length; i++)
                time[i] += data[i] + time[i - 1];

            return time;
        }

        //Чекбокс точки
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBox1.Checked)
            //    chart2.Series["point"].Points.AddXY(list[(int)nud1.Value].X, list[(int)nud1.Value].Y);
            //else
            //{
                chart2.Series["point"].Points.Clear();
                chart1.Series["Level"].Points.Clear();
            //}
        }

        //Уровень отсечки
        private void nud1_ValueChanged(object sender, EventArgs e)
        {
            chart1.Series["Level"].Points.Clear();
            for (int i = l; i < RMS1.Length - r; i++)
                chart1.Series["Level"].Points.AddY(nud1.Value);
        }

        //Получение индексов граничных элементов
        private int[] GetBordersIndexes(int[] time)
        {
            int[] borders = new int[2];
            borders[0] = time.ToList().FindIndex((v) => v >= 150000);
            borders[1] = time.ToList().FindLastIndex((v) => v <= 180000);
            return borders;
        }

        //Вычисление чувствительности и специфичности
        private List<PointF> SensAndSpec(int[] data, int[] borders, int left, int right)
        {
            int min = data.ToList().Min(); // границы уровней
            int max = data.ToList().Max();
            List<PointF> res = new List<PointF>();
            float spec, sens;
            int t, f;
            float perfSens = borders[1] - borders[0];
            float perfSpec = borders[0] - 1 + (data.Length -1 - borders[1]);

            for (int y = min; y <= max; y++)
            {
                spec = 0;
                sens = 0;
                //для специфичности
                t = 0;
                f = 0;
                for(int l = left; l < borders[0]; l++)
                    if (data[l] > y)
                        t++;
                for (int l = borders[1] + 1; l < data.Length - right; l++)
                    if (data[l] > y)
                        t++;
                spec = t / perfSpec;

                //для чувствительности
                for (int l = borders[0]; l <= borders[1]; l++)
                    if (data[l] < y)
                        f++;
                sens = f / perfSens;

                res.Add(new PointF(spec, sens));
            }
            return res;
        }

        //Получение чувствительности и специфичности, соответствующих уровню отсечки (для 2 ЭКГ)
        private PointF SensAndSpecOnLevel(int[] data, int[] borders, int left, int right, double lev)
        { 
            float spec, sens;
            int t, f;
            float perfSens = borders[1] - borders[0];
            float perfSpec = borders[0] - 1 + (data.Length - 1 - borders[1]);

            spec = 0;
            sens = 0;
            //для специфичности
            t = 0;
            f = 0;
            for (int l = left; l < borders[0]; l++)
                if (data[l] > lev)
                    t++;
            for (int l = borders[1] + 1; l < data.Length - right; l++)
                if (data[l] > lev)
                    t++;
            spec = t / perfSpec;

            //для чувствительности
            for (int l = borders[0]; l < borders[1] - 1; l++)
                if (data[l] > lev)
                    f++;
            sens = f / perfSens;

            return new PointF(spec, sens);
        }

        //Кнопка для результата
        private void button2_Click(object sender, EventArgs e)
        {
            chart3.Series["Data"].Points.Clear();
            chart3.Series["Level"].Points.Clear();
            PointF point = GetBest(list);
            //2 ЭКГ
            mass2 = GetData.ReadFile(@"test2.rr");
            chart2.Series["best"].Points.AddXY(point.X, point.Y);
            n = (int)(numericUpDown3.Value);

            //СКО 2 ЭКГ
            RMS2 = GetData.RMSDeviation(mass2, n);
   
            //Рисуем СКО 2 ЭКГ
            for (int i = l; i < RMS2.Length; i++)
            {               
                chart3.Series["Data"].Points.AddY(RMS2[i]);
                chart3.Series["Level"].Points.AddY(nud1.Value);
            }
           
            //Координаты лучшей точки
            textBox1.Text = point.X.ToString();
            textBox2.Text = point.Y.ToString();
          
            list2 = SensAndSpec(mass2, GetBordersIndexes(GetTimeInterval(mass1)), l, r);

            //Координаты практического значения
            PointF res = SensAndSpecOnLevel(mass2, GetBordersIndexes(GetTimeInterval(mass2)), l, r, Convert.ToInt32(nud1.Value));
            textBox4.Text = res.X.ToString();
            textBox3.Text = res.Y.ToString();
        }
    }
}
