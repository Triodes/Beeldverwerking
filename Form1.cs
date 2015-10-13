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
            if (outputImage != null) outputImage.Dispose();

            // Setup progress bar
            //progressBar.Visible = true;
            //progressBar.Minimum = 1;
            //progressBar.Maximum = inputImage.Size.Width;
            //progressBar.Value = 1;
            //progressBar.Step = 1;

            int[,] image = new Grayscale().FromBitmap(inputImage);

            //==========================================================================================
            double stepSize = 0.25;

            image = new Sobel().Compute(image);
            image = Morphologicals.Closing(image, new bool[,] {{false, true, false}, {true, true, true}, {false, true, false}});
            image = Defaults.Compute(image, (v) => v < 20 ? 0 : v);
            int[,] edges = image;
            image = new Hough().Compute(image, stepSize);
            image = Defaults.Normalize(image, 255);
            image = new Window(75, 255).Compute(image);
            int maxVal = (int)Math.Ceiling(Math.Sqrt(edges.GetLength(0) * edges.GetLength(0) + edges.GetLength(1) * edges.GetLength(1)));
            SortedSet<Line> lines = Lines.FindLines(image, stepSize, maxVal);
            lines = Lines.FilterLines(lines);
            Console.WriteLine("\nFILTERED:");
            foreach(Line line in lines) 
            {
                Console.WriteLine("Theta: {0}, rho: {1}, value: {2}", line.theta, line.rho, line.value);
            }
            image = edges;

            //// Find the first line.
            //SortedSet<Tuple<double,double>> lines = findLines(image, stepSize);
            ////lines = filterLines(lines);
            //Console.WriteLine(lines.Count);
            //// Find perpendicular lines.
            //List<Tuple<Tuple<double, double>,Tuple<double,double>>> pairs = findPerpendicular(lines);
            //Console.WriteLine("Pairs: " + pairs.Count);

            // Find squares.
            //lines.Clear();
            //foreach(var one in pairs) {
            //    foreach(var two in pairs) {
            //        if(one == two)
            //            continue;

            //        if(Math.Abs(one.Item1.Item1 - two.Item1.Item1) <= 0.1
            //           && Math.Abs(one.Item2.Item1 - two.Item2.Item1) <= 0.1
            //            && Math.Abs(one.Item1.Item2 - two.Item1.Item2) > 20
            //            && Math.Abs(one.Item2.Item2 - two.Item2.Item2) > 20) {
            //            lines.Add(one.Item1);
            //            lines.Add(one.Item2);
            //            lines.Add(two.Item1);
            //            lines.Add(two.Item2);

            //            double bw = Math.Abs(one.Item1.Item2 - two.Item1.Item2);
            //            double bh = Math.Abs(one.Item2.Item2 - two.Item2.Item2);
            //            Console.WriteLine("Square: " + bw + "x" + bh + " (" + (bh/bw) + ")");
            //            goto done;
            //        }
            //    }
            //}
            //done:
            //Console.WriteLine(lines.Count);
            //// Make everything outside square black.
            //if(lines.Count == 5)
            //{
            //    Console.WriteLine(lines.First().Item1 + " " + lines.First().Item2);
            //    int i = 0;
            //    foreach(var line in lines) {
            //        double theta = line.Item1;
            //        double lineR = line.Item2;
            //        for(int x = 0; x < edges.GetLength(0); x++) {
            //            for(int y = 0; y < edges.GetLength(1); y++) {
            //                int r = (int)(
            //                            x * Math.Cos(theta) +
            //                            y * Math.Sin(theta));
            //                if(i % 2 == 0) {
            //                    if(r <= lineR) {
            //                        edges[x, y] = 0;
            //                    }
            //                } else {
            //                    if(r >= lineR) {
            //                        edges[x, y] = 0;
            //                    }
            //                }
                                
            //            }
            //        }
            //        i++;
            //        if(i == 4)
            //            break;
            //    }
            //}

            /** /
            outputImage = new Bitmap(inputImage.Size.Width, inputImage.Size.Height); // Create new output image
            for (int x = 0; x < inputImage.Size.Width; x++)
            {
                int[] lineYs = new int[lines.Count];
                int i = 0;
                foreach(var line in lines) {
                    double angle = line.Item1;
                    double d = line.Item2;
                    lineYs[i] = (int)((d - (double)x * Math.Cos(angle)) / Math.Sin(angle));
                    i++;
                }

                for (int y = 0; y < inputImage.Size.Height; y++)
                {
                    bool found = false;
                    foreach(int lineY in lineYs) {
                        if(lineY != y)
                            continue;
                        
                        outputImage.SetPixel(x, y, Color.Purple);
                        found = true;
                        break;
                    }

                    if(!found)
                    {
                        int v = edges[x, y];
                        outputImage.SetPixel(x, y, Color.FromArgb(v,v,v));               // Set pixel color in array at (x,y)
                    }
                }
            }
            /** /
            //===========================================================================================

            */
            
            // Copy array to output Bitmap
            outputImage = new Grayscale().ToBitmap(image);
            pictureBox2.Image = (Image)outputImage;
            progressBar.Visible = false;
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

