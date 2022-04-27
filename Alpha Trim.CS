using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZGraphTools;

namespace ImageFilters
{
    public partial class Alpha_Trim : Form
    {


        int filter_size, trimming_value;

        static byte[,] imgMatrix;
        static byte[,] outputImg;
        int G_max;
        public Alpha_Trim(byte[,] images)
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


        public static int left(int i)
        {
            return 2 * i + 1;
        }
        public static int right(int i)
        {
            return 2 * i + 2;
        }
        public static void maxHeapify(byte[] Array, int ArrayLength, int i)
        {
            int Left = left(i);
            int Right = right(i);
            int Largest;
            if (Left < ArrayLength && Array[Left] > Array[i])
                Largest = Left;
            else
                Largest = i;
            if (Right < ArrayLength && Array[Right] > Array[Largest])
                Largest = Right;
            if (Largest != i)
            {
                byte Temp = Array[i];
                Array[i] = Array[Largest];
                Array[Largest] = Temp;
                maxHeapify(Array, ArrayLength, Largest);
            }
        }
        public static void buildingMaxHeapifying(byte[] Array, int ArrayLength)
        {
            for (int i = ArrayLength / 2 - 1; i >= 0; i--)
                maxHeapify(Array, ArrayLength, i);
        }
        public static byte[] sortingHeap(byte[] Array, int ArrayLength)
        {
            int HeapSize = ArrayLength;
            buildingMaxHeapifying(Array, ArrayLength);
            for (int i = ArrayLength - 1; i > 0; i--)
            {
                byte Temp = Array[0];
                Array[0] = Array[i];
                Array[i] = Temp;
                HeapSize--;
                maxHeapify(Array, HeapSize, 0);
            }
            return Array;
        }

        public void imgLoop(int sorting_Method)
        {
            for (int i = filter_size/2; i < imgMatrix.GetLength(0)-(filter_size/2); i++)
            {
                for (int j = filter_size/2; j < imgMatrix.GetLength(1)-(filter_size/2); j++)
                {
                    applyFilter(i, j, sorting_Method);

                }
            }
        }

        public void applyFilter(int height_pos, int width_pos, int sorting_Method)
        {
            byte[] pixelArray = new byte[filter_size * filter_size];
            int index=0;

            for (int i = height_pos- (filter_size/2); i <= height_pos+(filter_size/2); i++)
            {

                for (int j = width_pos-(filter_size/2); j <= width_pos+(filter_size/2); j++)
                {

                    pixelArray[index] = imgMatrix[i, j];
                    index++;


                }

            }

            if (sorting_Method == 1)
            {
                pixelArray = countingSort(pixelArray);
            }
            else if (sorting_Method == 2) 
            {
                pixelArray = sortingHeap(pixelArray, pixelArray.Length);           
            }

            int sum = 0;
            int count = 0;


            for(int i = trimming_value; i <pixelArray.Length-trimming_value; i++)
            {
                sum += pixelArray[i];
                count++;
            }

            int avg = sum / count;

            outputImg[height_pos - (filter_size / 2), width_pos - (filter_size / 2)] = (byte)avg;


        }

        private void Alpha_Trim_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Graph_Button_Click(object sender, EventArgs e)
        {
            G_max = Decimal.ToInt32(Graph_Input.Value);
            int N = G_max;
            double[] x_values = new double[N+1];
            double[] y_values_N = new double[N+1];
            double[] y_values_NT = new double[N+1];
            Stopwatch stopwatch = new Stopwatch();
            TimeSpan ts;
            int xIndex = 0;

            if (object.Equals(outputImg, null))
            {
                outputImg = new byte[imgMatrix.GetLength(0) - (filter_size - 1), imgMatrix.GetLength(1) - (filter_size - 1)];
            }

            for(int i = 0; i <= G_max; i++)
            {


                x_values[xIndex++] = i;


            }


            for (int j = 3; j <= G_max; j += 2)
            {
                stopwatch.Reset();
                stopwatch.Start();
                filter_size = j;
                imgLoop(1);
                stopwatch.Stop();
                ts = stopwatch.Elapsed;

                y_values_N[j] = ts.TotalMilliseconds;
                y_values_N[j-1] = ts.TotalMilliseconds;


            }

            
            for (int j = 3; j <= G_max; j += 2)
            {
                stopwatch.Reset();
                stopwatch.Start();
                filter_size = j;
                imgLoop(2);
                stopwatch.Stop();
                ts = stopwatch.Elapsed;

                y_values_NT[j] = ts.TotalMilliseconds;
                y_values_NT[j - 1] = ts.TotalMilliseconds;

            }

            ZGraphForm ZGF = new ZGraphForm("Alpha Trim Graph", "N", "f(N)");
            ZGF.add_curve("Counting Sort: f(N) = N + K", x_values, y_values_N, Color.Red); 
            ZGF.add_curve("Kth Element: f(N) = (K + (N-K)*Log K", x_values, y_values_NT, Color.Blue); 
            ZGF.Show();

        }

        

        private void Start_Click(object sender, EventArgs e)
        {
            
            int outputHeight = imgMatrix.GetLength(0) - (filter_size - 1);
            int outputWidth = imgMatrix.GetLength(1) - (filter_size - 1);
            outputImg = new byte[outputHeight, outputWidth];

            filter_size = Decimal.ToInt32(numericUpDown_N.Value);
            trimming_value = Decimal.ToInt32(numericUpDown_T.Value);
            imgLoop(2);
            ImageOperations.DisplayImage(outputImg, pictureBox1);
        }
    }
}
