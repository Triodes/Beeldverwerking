using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using INFOIBV.ImageOperations;
using INFOIBV.LineOperations;
using System.Threading;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap inputImage;
        private Bitmap outputImage;
        private const int THRESHOLD = 158;
        private const double STEP_SIZE = 0.25;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)
            {
                imageFileName.Text = openImageDialog.FileName;
                if (inputImage != null) inputImage.Dispose();
                inputImage = new Bitmap(imageFileName.Text);
                pictureBox1.Image = (Image) inputImage;
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (inputImage == null) return;
            applyButton.Enabled = false;
            if (outputImage != null) outputImage.Dispose();
            int[,] output;

            // Setup progress bar
            //progressBar.Visible = true;
            //progressBar.Minimum = 1;
            //progressBar.Maximum = inputImage.Size.Width;
            //progressBar.Value = 1;
            //progressBar.Step = 1;

            int[,] original = new Grayscale().FromBitmap(inputImage);

            //==========================================================================================

            int[,] edges = new Sobel().Compute(original);
            //edges = Morphologicals.Closing(edges, new bool[,] { { false, true, false }, { true, true, true }, { false, true, false } });
            edges = new Window(20, int.MaxValue).Compute(edges);

            //int[,] mask = new Threshold(THRESHOLD,true).Compute(original);

            //bool[,] strucElem = 
            //{
            //    {true, true, true, true, true, true, true},
            //    {true, true, true, true, true, true, true},
            //    {true, true, true, true, true, true, true},
            //    {true, true, true, true, true, true, true},
            //    {true, true, true, true, true, true, true},
            //    {true, true, true, true, true, true, true},
            //    {true, true, true, true, true, true, true}
            //};

            //int[,] reconstruction = Morphologicals.GeoDilation(edges, strucElem, mask);

            #region HOUGH
            int[,] hough = new Hough().Compute(edges, STEP_SIZE);
            hough = Defaults.Normalize(hough, 255);
            hough = new Window(75, 255).Compute(hough);
            int maxVal = (int)Math.Ceiling(Math.Sqrt(edges.GetLength(0) * edges.GetLength(0) + edges.GetLength(1) * edges.GetLength(1)));
            SortedSet<Line> lines = Lines.FindLines(hough, STEP_SIZE, maxVal);
            lines = Lines.FilterLines(lines);
            Console.WriteLine("\nFILTERED:");
            foreach (Line line in lines)
            {
                Console.WriteLine("Theta: {0}, rho: {1}, value: {2}", line.theta, line.rho, line.value);
            }
            #endregion

            output = Defaults.Normalize(edges,255);

            // Copy array to output Bitmap
            outputImage = new Grayscale().ToBitmap(output);
            pictureBox2.Image = (Image)outputImage;

            Graphics g = Graphics.FromImage(outputImage);

            foreach (Line line in lines)
            {
                if (line.theta == 0)
                {
                    g.DrawLine(Pens.Cyan, (float)line.rho, 0, (float)line.rho, outputImage.Height);
                }
                else
                {
                    float y0 = (float)(line.rho / Math.Sin(line.theta));
                    float y1 = (float)((line.rho - outputImage.Width * Math.Cos(line.theta)) / Math.Sin(line.theta));
                    g.DrawLine(Pens.Cyan, 0, y0, outputImage.Width, y1);
                }
            }
            
            applyButton.Enabled = true;
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (outputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                outputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

        private SortedSet<Tuple<double,double, int>> findLines(int[,] image, double stepSize, int maxVal) 
        {
            SortedSet<Tuple<double,double,int>> result = new SortedSet<Tuple<double,double,int>>();

            int width = image.GetLength(0);
            int height = image.GetLength(1);

            for (int step = 0; step < width; step++)
            {
                for (int r = 0; r < height; r++)
                {
                    int value = image[step, r];
                    if(value == 0)
                        continue;

                    double angle = ((step * stepSize) * Math.PI/180.0);
                    double d = r - maxVal;
                    Console.WriteLine("Angle: {0}, r: {1}, value: {2}", angle, d, value);
                    result.Add(new Tuple<double,double,int>(angle, d, value));
                }
            }

            return result;
        }

        private List<Tuple<Tuple<double, double>,Tuple<double,double>>> findPerpendicular(SortedSet<Tuple<double,double>> lines) 
        {
            List<Tuple<Tuple<double, double>,Tuple<double,double>>> result = new List<Tuple<Tuple<double, double>,Tuple<double,double>>>();
            foreach(var one in lines) {
                foreach(var two in lines) {
                    if(one == two)
                        continue;

                    if(Math.Abs(Math.Abs(one.Item1 - two.Item1) - Math.PI / 2) <= 0.2) {
                        // Ensure first one has the lowest angle.
                        if(one.Item1 < two.Item1)
                            result.Add(Tuple.Create(one, two));
                        else
                            result.Add(Tuple.Create(two, one));
                    }
                }   
            }
            return result;
        }
    }
}

