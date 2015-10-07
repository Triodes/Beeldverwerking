using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using INFOIBV.ImageOperations;

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

            int[,] image = new Grayscale().Compute(inputImage);

            //==========================================================================================
            double stepSize = 1;

            image = new Sobel().Compute(image);
            image = Defaults.Compute(image, (v) => Math.Abs(v));
            image = Defaults.Normalize(image, 255);
            image = new Threshold(15).Compute(image);
            image = Morphologicals.Closing(image, new bool[,] {
                {true, true, true},
                {true, true, true},
                {true, true, true}
            });
            int[,] edges = image;
            image = houghLines(image, stepSize);
            image = Defaults.Normalize(image, 255);
            image = new Window(120, 255).Compute(image);

            // Find the first line.
            SortedSet<Tuple<double,double>> lines = findLines(image, stepSize);
            lines = filterLines(lines);
            Console.WriteLine(lines.Count);
            // Find perpendicular lines.
            List<Tuple<Tuple<double, double>,Tuple<double,double>>> pairs = findPerpendicular(lines);
            Console.WriteLine("Pairs: " + pairs.Count);
            // Find squares.
            //lines.Clear();
            foreach(var one in pairs) {
                foreach(var two in pairs) {
                    if(one == two)
                        continue;

                    if(Math.Abs(one.Item1.Item1 - two.Item1.Item1) <= 0.1
                       && Math.Abs(one.Item2.Item1 - two.Item2.Item1) <= 0.1
                        && Math.Abs(one.Item1.Item2 - two.Item1.Item2) > 20
                        && Math.Abs(one.Item2.Item2 - two.Item2.Item2) > 20) {
                        lines.Add(one.Item1);
                        lines.Add(one.Item2);
                        lines.Add(two.Item1);
                        lines.Add(two.Item2);

                        double bw = Math.Abs(one.Item1.Item2 - two.Item1.Item2);
                        double bh = Math.Abs(one.Item2.Item2 - two.Item2.Item2);
                        Console.WriteLine("Square: " + bw + "x" + bh + " (" + (bh/bw) + ")");
                        goto done;
                    }
                }
            }
            done:
            Console.WriteLine(lines.Count);
            // Make everything outside square black.
            if(lines.Count == 5)
            {
                Console.WriteLine(lines.First().Item1 + " " + lines.First().Item2);
                int i = 0;
                foreach(var line in lines) {
                    double theta = line.Item1;
                    double lineR = line.Item2;
                    for(int x = 0; x < edges.GetLength(0); x++) {
                        for(int y = 0; y < edges.GetLength(1); y++) {
                            int r = (int)(
                                        x * Math.Cos(theta) +
                                        y * Math.Sin(theta));
                            if(i % 2 == 0) {
                                if(r <= lineR) {
                                    edges[x, y] = 0;
                                }
                            } else {
                                if(r >= lineR) {
                                    edges[x, y] = 0;
                                }
                            }
                                
                        }
                    }
                    i++;
                    if(i == 4)
                        break;
                }
            }

            /**/
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

            // Copy array to output Bitmap
            Image = normalize(Image, 255);
            OutputImage = new Bitmap(Image.GetLength(0), Image.GetLength(1)); // Create new output image
            for (int x = 0; x < Image.GetLength(0); x++)
            {
                for (int y = 0; y < Image.GetLength(1); y++)
                {
                    int value = Image[x, y];
                    OutputImage.SetPixel(x, y, Color.FromArgb(value, value, value));               // Set the pixel color at coordinate (x,y)
                }
            }/**/
            
            pictureBox2.Image = (Image)outputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (outputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                outputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

        private SortedSet<Tuple<double,double>> findLines(int[,] image, double stepSize) 
        {
            SortedSet<Tuple<double,double>> result = new SortedSet<Tuple<double,double>>();
            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    int value = image[x, y];
                    if(value == 0)
                        continue;

                    double angle = ((x * stepSize) * Math.PI/180);
                    double d = y - 2 * inputImage.Height;
                    Console.WriteLine("Theta: " + angle + "   d: " + d);
                    // FIXME: Limit amount of lines found
                    result.Add(Tuple.Create<double,double>(angle, d));
                }
            }

            return result;
        }

        private SortedSet<Tuple<double,double>> filterLines(SortedSet<Tuple<double,double>> lines) 
        {
            SortedSet<Tuple<double,double>> result = new SortedSet<Tuple<double,double>>();
            Tuple<double,double> prevLine = null;
            foreach(var line in lines) {
                if(prevLine != null) {
                    if(Math.Abs(prevLine.Item1 - line.Item1) > 5
                       || Math.Abs(prevLine.Item2 - line.Item2) > 5) {
                        result.Add(line);
                    }
                } else {
                    result.Add(line);
                }
                prevLine = line;
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
        private int[,] houghLines(int[,] image, double stepSize) 
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);

            int[,] result = new int[(int)(180/stepSize), height * 4];

            Random random = new Random();
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    int value = image[x, y];
                    if(value <= 0)
                        continue;

                    // Loop over the angles.
                    for(int angle = 0; angle < (int)(180/stepSize); angle++) {
                        double theta = (angle * stepSize) * Math.PI / 180.0;

                        int r;
                        //if(angle == 45 / stepSize) {
                        //    r = (int)((x + y) / Math.Sqrt(2) + random.NextDouble());
                        //} else if(angle == 135 / stepSize) {
                        //    r = (int)((y - x) / Math.Sqrt(2) + random.NextDouble());
                        //} else 
                        {
                            r = (int)Math.Round(
                                x * Math.Cos(theta) + 
                                y * Math.Sin(theta));
                        }
                        if(r == 0) continue;
                        r += 2 * height;

                        result[angle, r]++;
                    }
                }
            }
            return result;
        }
    }
}

