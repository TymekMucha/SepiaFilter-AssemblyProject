using Grayscale;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Collections.Specialized;

namespace GrayscaleConv
{
    public partial class Form1 : Form
    {
        [DllImport(@"D:\1. My Stuff D\Documents\polsl\SEM 5\JA\SEPIA\x64\Debug\JAAsm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ApplySepiaFilterAsm(IntPtr ptr, int width, int bytesPerPixel, int stride, int intensity, int startRow, int endRow);

        [DllImport(@"D:\1. My Stuff D\Documents\polsl\SEM 5\JA\SEPIA\x64\Debug\ImageProcessingLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertToSepia(IntPtr pixelData, int width, int height, int stride, int numThreads, int depth);


        private Bitmap source;

        private int numThreads = 1;
        private string filePath = string.Empty;

        private int intensity = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = fileDialog.FileName;
                    source = new Bitmap(filePath);
                    pictureBox1.Image = source;
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (selectedAsm.Checked)
            {
                if (source != null)
                {

                    int width = source.Width;
                    int height = source.Height;

                    int bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;

                    Bitmap tmp = new Bitmap(source);
                    BitmapData bitmapData = tmp.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.ReadWrite,
                        PixelFormat.Format24bppRgb);

                    IntPtr ptr = bitmapData.Scan0;


                    int stride = bitmapData.Stride;

                    Stopwatch sw = Stopwatch.StartNew();

                    int numThreads = (int)numericUpDown1.Value;

                    Task[] tasks = new Task[numThreads];
                    for (int i = 0; i < numThreads; i++)
                    {
                        int startRow = (height / numThreads) * i;
                        int endRow = (i == numThreads - 1) ? height : startRow + (height / numThreads);

                        tasks[i] = Task.Run(() =>
                        {
                            ApplySepiaFilterAsm(ptr, width, bytesPerPixel, stride, intensity, startRow, endRow);
                        });
                    }

                    await Task.WhenAll(tasks);

                    sw.Stop();

                    tmp.UnlockBits(bitmapData);
                    pictureBox2.Image = new Bitmap(tmp);
                    label5.Text = $"Time: {sw.Elapsed.TotalMilliseconds} ms";
                }
                else
                {
                    MessageBox.Show("Nie wybrano pliku", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (selectedC.Checked)
            {
                if (source != null)
                {
                    int width = source.Width;
                    int height = source.Height;

                    Bitmap tmp = new Bitmap(source);
                    BitmapData bitmapData = tmp.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.ReadWrite,
                        PixelFormat.Format32bppArgb);

                    IntPtr ptr = bitmapData.Scan0;
                    int stride = bitmapData.Stride;

                    int threadCount = (int)numericUpDown1.Value; // Liczba wątków z suwaka

                    Stopwatch sw = Stopwatch.StartNew();

                    // Wywołanie funkcji z DLL
                    ConvertToSepia(ptr, width, height, stride, threadCount, intensity);

                    sw.Stop();

                    tmp.UnlockBits(bitmapData);
                    pictureBox2.Image = new Bitmap(tmp);
                    label5.Text = $"Czas konwersji: {sw.Elapsed.TotalMilliseconds} ms";
                }
                else
                {
                    MessageBox.Show("Nie wybrano pliku", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void numThreadsValue(object sender, EventArgs e)
        {
            numThreads = (int)numericUpDown1.Value;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numThreads = (int)numericUpDown1.Value;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            intensity = (int)trackBar1.Value;
        }
    }
}
