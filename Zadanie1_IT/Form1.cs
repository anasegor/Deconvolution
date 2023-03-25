using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Numerics;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using Timer = System.Threading.Timer;

namespace Zadanie1_IT
{
    public partial class Form1 : Form
    {
        private double A1, A2, A3, A4;
        private double mu1, mu2, mu3;
        private double sigma1, sigma2, sigma3,sigma4;
        private double f_d,step,E;
        private int N;
        static public int padding = 10;
        static public int left_keys_padding = 20;
        static public int actual_left = 30;
        static public int actual_top = 10;

        //private void pictureBox1_Resize(object sender, EventArgs e)
        //{
        //    pictureBox1.Invalidate();
        //}
        private double[] sign;
        private double[] sign_new;
        private double[] sign_h;
        private double[] sign_y;
        private double[] lambda;
        private PointF[] x_points;
        private PointF[] x_points_new;
        private PointF[] h_points;
        private PointF[] y_points;
        public Graphics graphics1;
        public Graphics graphics2;
        public Graphics graphics3;
        Pen pen1 = new Pen(Color.DarkRed, 2f);
        Pen pen2 = new Pen(Color.Black, 2f);
        Pen pen3= new Pen(Color.Blue, 2f);

        private void Form1_Load(object sender, EventArgs e)
        {
            for_A1.Text = "1";
            for_A2.Text = "0,7";
            for_A3.Text = "0,9";
            for_A4.Text = "1";
            for_mu1.Text = "1";
            for_mu2.Text = "3";
            for_mu3.Text = "4";
            for_sigma1.Text = "0,25";
            for_sigma2.Text = "0,25";
            for_sigma3.Text = "0,25";
            for_sigma4.Text = "0,1";
            for_f_d.Text = "10";
            for_N.Text = "50";
            for_step.Text = "0,1";

        }
        public Form1()
        {
            InitializeComponent();

        }
        private void timer1_Tick(object sender, EventArgs e)//функция таймера/двойная буферизация
        {
            // создание буфера для нового кадра
            Bitmap Image = new Bitmap(Width, Height);
            Graphics gbuf = Graphics.FromImage(Image);
            // (создание фона)
            gbuf.Clear(Color.White);
            // тут должна идти ваша графика
            PainNet(gbuf, pictureBox1, pen2, x_points, N / f_d, N);
            PaintGraph(gbuf, pictureBox1, pen1, x_points, N / f_d, N);
            DeconvSvertka(lambda);
            PaintGraph(gbuf, pictureBox1, pen3, x_points_new, N / f_d, N);
            // теперь нужно скопировать кадр на канвас формы
            graphics1.DrawImageUnscaled(Image, 0, 0);
            // освобождаем задействованные в операции ресурсы
            gbuf.Dispose();
            Image.Dispose();
        }
        private void button1_Click(object sender, EventArgs e)//прямая задача
        {
            //pictureBox1.Image = null;
            //pictureBox1.Update();
            //pictureBox2.Image = null;
            //pictureBox2.Update();
            //pictureBox3.Image = null;
            //pictureBox3.Update();

            graphics1 = pictureBox1.CreateGraphics();
            graphics2 = pictureBox2.CreateGraphics();
            graphics3 = pictureBox3.CreateGraphics();
            LSistema();
            CreateXandH();
            sign_y = new double[N];
            y_points = new PointF[N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                    sign_y[i] += sign[j] * sign_h[Math.Abs(j - i)];
                y_points[i] = new PointF((float)(i * (1 / f_d)), (float)sign_y[i]);
            }
            PainNet(graphics1, pictureBox1, pen2, x_points, N / f_d, N);
            PainNet(graphics2, pictureBox2, pen2, h_points, N / f_d, N);
            PainNet(graphics3, pictureBox3, pen2, y_points, N / f_d, N);
            PaintGraph(graphics1, pictureBox1, pen1, x_points, N / f_d, N);
            PaintGraph(graphics2, pictureBox2, pen1, h_points, N / f_d, N);
            PaintGraph(graphics3, pictureBox3, pen1, y_points, N / f_d, N);
        }

        Thread th;
        private void button2_Click(object sender, EventArgs e)//обратная задача
        {
            //pictureBox1.Image = null;
            //pictureBox1.Update();
            timer1.Enabled = true;
            lambda = new double[N];
            Random rnd = new Random();
            //начальное приближение
            th = new Thread(() =>
            {
                double fb = MHJ(N, ref lambda, step);
                Thread.Sleep(1000);
                timer1.Enabled = false;
                double energy = 0;
                for (int i = 0; i < N; i++)
                {
                    energy += (sign[i] - sign_new[i]) * (sign[i] - sign_new[i]);
                }
                textBox1.Text = energy.ToString();
            });
            th.Start();
            //DeconvSvertka(lambda);
            //double energy = 0;
            //for (int i = 0; i < N; i++)
            //{
            //    energy += (sign[i] - sign_new[i]) * (sign[i] - sign_new[i]);
            //}
            //textBox1.Text=energy.ToString();
            
        }

        //гауссов купол
        double Gauss(double A, double mu, double sigma, double i, double fd)
        {
            return A /(Math.Sqrt(2*Math.PI)*sigma)*Math.Exp(-Math.Pow((i / fd - mu), 2) / (sigma * sigma));
        }
        public void LSistema()
        {

            if (for_A1.Text != "" || for_A2.Text != "" || for_A3.Text != "")
            {
                A1 = Convert.ToDouble(for_A1.Text);
                A2 = Convert.ToDouble(for_A2.Text);
                A3 = Convert.ToDouble(for_A3.Text);
                A4 = Convert.ToDouble(for_A4.Text);
            } 
            else { MessageBox.Show("параметры A по умолчанию", "Внимание!"); }
            if (for_mu1.Text != "" || for_mu2.Text != "" || for_mu3.Text != "")
            {
                mu1 = Convert.ToDouble(for_mu1.Text);
                mu2 = Convert.ToDouble(for_mu2.Text);
                mu3 = Convert.ToDouble(for_mu3.Text);
            }
            else { MessageBox.Show("параметры v по умолчанию", "Внимание!"); }
            if (for_sigma1.Text != "" || for_sigma2.Text != "" || for_sigma3.Text != "")
            {
                sigma1 = Convert.ToDouble(for_sigma1.Text);
                sigma2 = Convert.ToDouble(for_sigma2.Text);
                sigma3 = Convert.ToDouble(for_sigma3.Text);
                sigma4 = Convert.ToDouble(for_sigma4.Text);
            }
            else { MessageBox.Show("параметры f по умолчанию", "Внимание!"); }
            if (for_f_d.Text != "" || for_N.Text != "")
            {
                f_d = Convert.ToDouble(for_f_d.Text);
                N = Convert.ToInt32(for_N.Text);
                step=Convert.ToDouble(for_step.Text);
            }
            else { MessageBox.Show("параметры f_d,N,a по умолчанию", "Внимание!"); }  
            

        }

        

        public void CreateXandH()
        {
            sign = new double[N];
            for (int i = 0; i < N; i++)
            {
                sign[i] = (float)(Gauss(A1, mu1, sigma1, i, f_d) + Gauss(A2, mu2, sigma2, i, f_d) + Gauss(A3, mu3, sigma3, i, f_d));
            }
            x_points = new PointF[N];
            float dt = (float)(1 / f_d);
            for (int i = 0; i < N; i++)
            {
                x_points[i] = new PointF(i * dt, (float)sign[i]);
            }
            sign_h = new double[N];
            for (int i = 0; i < N; i++)
            {
                if (i < N / 2)
                    sign_h[i] = (float)Gauss(A4, 0, sigma4, i, f_d);
                else sign_h[i] = (float)Gauss(A4, 0, sigma4, N - i - 1, f_d);
            }
            h_points = new PointF[N];
            for (int i = 0; i < N; i++)
            {
                h_points[i] = new PointF(i * dt, (float)sign_h[i]);
            }
        }
        public void DeconvSvertka(double []l)
        {
            sign_new = new double[N];
            x_points_new = new PointF[N];
            double sum = 0;
            for (int j = 0; j < N; j++)
            {
                sum = 0;
                for (int k = 0; k < N; k++)
                {
                    sum += sign_h[Math.Abs(k - j)] * l[k];
                }
                sign_new[j]=Math.Exp(-1 - sum);
                x_points_new[j] = new PointF((float)(j * (1 / f_d)), (float)sign_new[j]);
            }
        }

        public double functional(double [] x)//функционал
        {
            double[] new_x = new double[N];
            double func=0;
            double sum;
            for (int m = 0; m < N; m++)
            {
                sum = 0;
                for (int j = 0; j < N; j++)
                {
                    sum += sign_h[Math.Abs(j - m)] * lambda[j];
                }
                new_x[m] = Math.Exp(-1 - sum);
            }
            
            for (int m = 0; m < N; m++)
            {
                sum = 0;
                for (int j = 0; j < N; j++)
                {
                     sum += new_x[j] * sign_h[Math.Abs(m - j)];
                }
                func += (sign[m] - sum) * (sign[m] - sum);
            }
            return func;
        }
        //метод Хука-Дживса(Морозов)
        public double MHJ(int kk, ref double []x, double h)//N and lambda
        {

            // kk - количество параметров; x - массив параметров
            double TAU = 0.000001; // Точность вычислений 
            int i, j, bs, ps, calc=0;
            double z,  k, fi, fb;
            double[] b = new double[kk];
            double[] y = new double[kk];
            double[] p = new double[kk];
            Random rnd = new Random();
            x[0] = 1;
            for (i = 1; i < kk; i++)
            {
                x[i] = rnd.NextDouble();  // Задается начальное приближение lambda
            }
            k = h;
            for (i = 0; i < kk; i++)
            {
                y[i] = p[i] = b[i] = x[i];
            }
            fi = functional(x);
            ps = 0;
            bs = 1;
            fb = fi;
            j = 0;
            while (true)
            {
                calc++; // Счетчик итераций. Можно игнорировать

                //if (calc % 100 == 0)
                //{
                //    graphics1.Clear(Color.White);
                //    // тут должна идти ваша графика
                //    PainNet(graphics1, pictureBox1, pen2, x_points, N / f_d, N);
                //    PainGraph(graphics1, pictureBox1, pen1, x_points, N / f_d, N);
                //    DeconvSvertka(x);
                //    PainGraph(graphics1, pictureBox1, pen3, x_points_new, N / f_d, N);
                //}
                x[j] = y[j] + k;
                z = functional(x);
                if (z >= fi)
                {
                    x[j] = y[j] - k;
                    z = functional(x);
                    if (z < fi)
                    {
                        y[j] = x[j];
                    }
                    else x[j] = y[j];
                }
                else y[j] = x[j];
                fi = functional(x);

                if (j < kk - 1)
                {
                    j++;
                    continue;
                }
                if (fi + 1e-8 >= fb)
                {
                    if (ps == 1 && bs == 0)
                    {
                        for (i = 0; i < kk; i++)
                        {
                            p[i] = y[i] = x[i] = b[i];
                        }
                        z = functional(x);
                        bs = 1;
                        ps = 0;
                        fi = z;
                        fb = z;
                        j = 0;
                        continue;
                    }
                    k /= 10.0;
                    if (k < TAU)
                    {
                        break;
                    }
                    j = 0;
                    continue;
                   
                }
                for (i = 0; i < kk; i++)
                {
                    p[i] = 2 * y[i] - b[i];
                    b[i] = y[i];
                    x[i] = p[i];
                    y[i] = x[i];
                }
                z = functional(x);
                fb = fi;
                ps = 1;
                bs = 0;
                fi = z;
                j = 0;
                //Invalidate(0);
            } //  end of while(1)
            for (i = 0; i < kk; i++)
            {
                x[i] = p[i];
            }
            return fb;

        }
        


        //РИСОВАЛКА!!!
        public void PainNet(Graphics gr, PictureBox pictureBox, Pen penG, PointF[] points, double toX, int n)//Отрисовка сетки с подписями
        {
            PointF[] copy_points = new PointF[n];
            copy_points = (PointF[])points.Clone();


            int wX, hX;
            wX = pictureBox.Width;
            hX = pictureBox.Height;
            Point KX1 = new Point(30, hX - 10);
            Point KX2 = new Point(wX - 10, hX - 10);
            gr.DrawLine(penG, KX1, KX2);
            Point KY1 = new Point(30, 10);
            Point KY2 = new Point(30, hX - 10);
            gr.DrawLine(penG, KY1, KY2);
            int actual_width = wX - 2 * padding - left_keys_padding;
            int actual_height = hX - 2 * padding;
            int actual_bottom = actual_top + actual_height;
            int actual_right = actual_left + actual_width;
            float maxY = GetMaxY(copy_points, n);
            int grid_size = 11;
            Pen GridPen = new Pen(Color.Gray, 1f);
            PointF K1, K2, K3, K4;
            for (double i = 0.5; i < grid_size; i += 1.0)
            {
                //вертикальная
                K1 = new PointF((float)(actual_left + i * actual_width / grid_size), actual_top);
                K2 = new PointF((float)(actual_left + i * actual_width / grid_size), actual_bottom);
                gr.DrawLine(GridPen, K1, K2);
                double v = 0 + i * (toX - 0) / grid_size;
                string s1 = v.ToString("0.00");
                gr.DrawString(s1, new Font("Arial", 7), Brushes.Green, actual_left + (float)i * actual_width / grid_size, actual_bottom + 0);


                K3 = new PointF(actual_left, (float)(actual_top + i * actual_height / grid_size));
                K4 = new PointF(actual_right, (float)(actual_top + i * actual_height / grid_size));
                gr.DrawLine(GridPen, K3, K4);
                double g = 0 + i * (double)(maxY / grid_size);
                string s2 = g.ToString("0.00");
                gr.DrawString(s2, new Font("Arial", 7), Brushes.Green, actual_left - left_keys_padding, actual_bottom - (float)i * actual_height / grid_size);
            }

        }
        static public void PaintGraph(Graphics gr, PictureBox pictureBox, Pen penG, PointF[] points, double toX, int n)//Отрисовка графика
        {

             
            PointF[] copy_points = new PointF[n];
            copy_points = (PointF[])points.Clone();

            int wX, hX;
            wX = pictureBox.Width;
            hX = pictureBox.Height;
            int actual_width = wX - 2 * padding - left_keys_padding;
            int actual_height = hX - 2 * padding;
            int actual_bottom = actual_top + actual_height;
            int actual_right = actual_left + actual_width;
            float maxY = GetMaxY(copy_points, n); ;
            PointF actual_tb = new PointF(actual_top, actual_bottom);//для y
            PointF actual_rl = new PointF(actual_right, actual_left);//для x
            PointF from_toX = new PointF(0, (float)(toX));
            PointF from_toY = new PointF(0, maxY * (float)1.2);
            convert_range_graph(copy_points, actual_rl, actual_tb, from_toX, from_toY);
            gr.DrawLines(penG, copy_points);
        }
        static public float GetMaxY(PointF[] points, int n)
        {
            float m = 0;
            for (int i = 0; i < n; i++)
            {
                if (m < Math.Abs(points[i].Y)) m = Math.Abs(points[i].Y);//макс значение Y

            }
            return m;
        }
        static public void convert_range_graph(PointF[] data, PointF actual_rl, PointF actual_tb, PointF from_toX, PointF from_toY )
        {
           //actual-размер:X-top/right Y-right,left
           //from_to: X-мин, Y-макс
            float kx = (actual_rl.X - actual_rl.Y) / (from_toX.Y - from_toX.X);
            float ky = (actual_tb.X - actual_tb.Y) / (from_toY.Y - from_toY.X);
            for (int i=0; i< data.Length;i++)
           {
               data[i].X = (data[i].X - from_toX.X) * kx + actual_rl.Y;
               data[i].Y = (data[i].Y - from_toY.X) * ky + actual_tb.Y;
           }
        }
        

    }

}
    
    

