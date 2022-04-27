using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ZGraphTools;

namespace ImageFilters
{
    public partial class Adaptive_Median : Form
    {
        int maskSize;
        int W_max;
        int G_max;
        static byte[,] imgMatrix;
        static byte[,] outputImg;

        private void Adaptive_Median_Load(object sender, EventArgs e)
        {


        }

        public Adaptive_Median(byte[,] images)
        {
            imgMatrix = images;
            InitializeComponent();
        }

        private byte[] countingSort(byte[] arr)
        {
            int size = arr.Length;
            int maxValue = -1000000;
            for (int i = 0; i < size; i++)
            {
                if (arr[i] > maxValue)
                {
                    maxValue = arr[i];
                }
            }
            int[] count = new int[maxValue + 1];

            for (int i = 0; i < size; i++)
            {
                count[arr[i]]++;
            }

            for (int i = 1; i <= maxValue; i++)
            {
                count[i] = count[i] + count[i - 1];
            }

            byte[] sortedArr = new byte[size];

            for (int i = size - 1; i >= 0; i--)
            {
                sortedArr[--count[arr[i]]] = arr[i];
            }

            return sortedArr;

        }


        static public int Partition(byte[] arr, int left, int right)
        {
            int i = left; byte pivot = arr[right];
            byte temp;

            for (int j = left; j < right; j++)
            {
                if (arr[j] < pivot)
                {
                    temp = arr[j];
                    arr[j] = arr[i];
                    arr[i] = temp;
                    i++;
                }
            }

            temp = arr[right];
            arr[right] = arr[i];
            arr[i] = temp;
            return i;
        }


        static public byte[] quickSort(byte[] arr, int left, int right)
        {
            if (left < right) // not (high == low || high < 0)
            {
                int pivot = Partition(arr, left, right);

                quickSort(arr, left, pivot - 1);
                quickSort(arr, pivot + 1, right);
            }
            return arr;
        }

        public void imgLoop(int sorting_Method)
        {
            maskSize = 3;
            for (int i = maskSize / 2; i < imgMatrix.GetLength(0) - (maskSize / 2); i++)
            {
                for (int j = maskSize / 2; j < imgMatrix.GetLength(1) - (maskSize / 2); j++)
                {
                    maskSize = 3;
                    applyFilter(i, j, sorting_Method);
                }
            }
        }


        public void applyFilter(int height_pos, int width_pos, int sorting_Method)
        {
            byte[] pixelArray = new byte[maskSize * maskSize];
            int index = 0;

            for (int i = height_pos - (maskSize / 2); i <= height_pos + (maskSize / 2); i++)
            {

                for (int j = width_pos - (maskSize / 2); j <= width_pos + (maskSize / 2); j++)
                {
                    if (i >= 0 && j >= 0 && i < imgMatrix.GetLength(0) && j < imgMatrix.GetLength(1))
                        pixelArray[index] = imgMatrix[i, j];

                    else if (i >= imgMatrix.GetLength(0) && j < imgMatrix.GetLength(1))
                        pixelArray[index] = imgMatrix[imgMatrix.GetLength(0) - 1, j];

                    else if (i < imgMatrix.GetLength(0) && j >= imgMatrix.GetLength(1))
                        pixelArray[index] = imgMatrix[i, imgMatrix.GetLength(1) - 1];

                    else if (i >= imgMatrix.GetLength(0) && j >= imgMatrix.GetLength(1))
                        pixelArray[index] = imgMatrix[imgMatrix.GetLength(0) - 1, imgMatrix.GetLength(1) - 1];

                    else if (i < 0 && j >= 0)
                        pixelArray[index] = imgMatrix[0, j];

                    else if (j < 0 && i >= 0)
                        pixelArray[index] = imgMatrix[i, 0];

                    else
                        pixelArray[index] = imgMatrix[0, 0];

                    index++;
                }

            }

            if (sorting_Method == 1)
            {
                pixelArray = countingSort(pixelArray);
            }
            else if (sorting_Method == 2)
            {
                pixelArray = quickSort(pixelArray, 0, pixelArray.Length - 1);
            }

            int median = pixelArray[pixelArray.Length / 2];
            int min = pixelArray[0];
            int max = pixelArray[pixelArray.Length - 1];
            // check is the median is corrupted 
            int a1 = median - min;
            int a2 = max - median;
            //if the median is corrupted , inc the size of the mask
            if (a1 > 0 && a2 > 0) //Input Max Size 
            {

                int b1 = imgMatrix[height_pos, width_pos] - pixelArray[0];
                int b2 = pixelArray[pixelArray.Length - 1] - imgMatrix[height_pos, width_pos];
                if (b1 > 0 && b2 > 0)
                {
                    outputImg[height_pos - 1, width_pos - 1] = imgMatrix[height_pos, width_pos];
                }
                else
                {
                    outputImg[height_pos - 1, width_pos - 1] = (byte)median;
                }

            }
            else
            {
                if (maskSize < W_max)
                {
                    maskSize += 2;
                    applyFilter(height_pos, width_pos, sorting_Method);


                }
                else
                {
                    outputImg[height_pos - 1, width_pos - 1] = (byte)median;
                }
            }


        }



        private void Graph_Button_Click(object sender, EventArgs e)
        {
            G_max = Decimal.ToInt32(Graph_Input.Value);
            int N = G_max;
            double[] x_values = new double[N + 1];
            double[] y_values_N = new double[N + 1];
            double[] y_values_NT = new double[N + 1];
            Stopwatch stopwatch = new Stopwatch();
            TimeSpan ts;
            int xIndex = 0;

            if (object.Equals(outputImg, null))
            {
                outputImg = new byte[imgMatrix.GetLength(0) - 1, imgMatrix.GetLength(1) - 1];
            }

            for (int i = 0; i <= G_max; i++)
            {
                x_values[xIndex++] = i;
            }


            for (int j = 3; j <= G_max; j += 2)
            {
                stopwatch.Reset();
                stopwatch.Start();
                W_max = j;
                imgLoop(1);
                stopwatch.Stop();
                ts = stopwatch.Elapsed;

                y_values_N[j] = ts.TotalMilliseconds;
                y_values_N[j - 1] = ts.TotalMilliseconds;


            }


            for (int j = 3; j <= G_max; j += 2)
            {
                stopwatch.Reset();
                stopwatch.Start();
                W_max = j;
                imgLoop(2);
                stopwatch.Stop();
                ts = stopwatch.Elapsed;

                y_values_NT[j] = ts.TotalMilliseconds;
                y_values_NT[j - 1] = ts.TotalMilliseconds;

            }

            ZGraphForm ZGF = new ZGraphForm("Adaptive Median Graph", "N", "f(N)");
            ZGF.add_curve("Counting Sort: f(N) = N + K", x_values, y_values_N, Color.Red);
            ZGF.add_curve("Quick Sort: f(N) = (K + (N-K)*Log K", x_values, y_values_NT, Color.Blue);
            ZGF.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            maskSize = 3;
            int outputHeight = imgMatrix.GetLength(0) - (maskSize - 1);
            int outputWidth = imgMatrix.GetLength(1) - (maskSize - 1);
            outputImg = new byte[outputHeight, outputWidth];
            W_max = Decimal.ToInt32(Wmax.Value);

            imgLoop(2);
            ImageOperations.DisplayImage(outputImg, pictureBox1);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            G_max = Decimal.ToInt32(Graph_Input.Value);
            int N = G_max;
            double[] x_values = new double[N + 1];
            double[] y_values_N = new double[N + 1];
            double[] y_values_NT = new double[N + 1];
            Stopwatch stopwatch = new Stopwatch();
            TimeSpan ts;
            int xIndex = 0;

            if (object.Equals(outputImg, null))
            {
                outputImg = new byte[imgMatrix.GetLength(0) - 1, imgMatrix.GetLength(1) - 1];
            }

            for (int i = 0; i <= G_max; i++)
            {
                x_values[xIndex++] = i;
            }


            for (int j = 3; j <= G_max; j += 2)
            {
                stopwatch.Reset();
                stopwatch.Start();
                W_max = j;
                imgLoop(1);
                stopwatch.Stop();
                ts = stopwatch.Elapsed;

                y_values_N[j] = ts.TotalMilliseconds;
                y_values_N[j - 1] = ts.TotalMilliseconds;


            }


            for (int j = 3; j <= G_max; j += 2)
            {
                stopwatch.Reset();
                stopwatch.Start();
                W_max = j;
                imgLoop(2);
                stopwatch.Stop();
                ts = stopwatch.Elapsed;

                y_values_NT[j] = ts.TotalMilliseconds;
                y_values_NT[j - 1] = ts.TotalMilliseconds;

            }

            ZGraphForm ZGF = new ZGraphForm("Adaptive Median Graph", "N", "f(N)");
            ZGF.add_curve("Counting Sort: f(N) = N + K", x_values, y_values_N, Color.Red);
            ZGF.add_curve("Quick Sort: f(N) = (K + (N-K)*Log K", x_values, y_values_NT, Color.Blue);
            ZGF.Show();
        }

        private void Graph_Input_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
