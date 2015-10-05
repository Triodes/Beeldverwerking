using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 2048 || InputImage.Size.Width > 2048) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            int[,] Image = new int[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
			progressBar.Maximum = InputImage.Size.Width;// * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = (int)(InputImage.GetPixel(x, y).GetBrightness() * 255);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            double stepSize = 1;

            Image = edgeDetection(Image);
            Image = compute(Image, (v) => Math.Abs(v));
            Image = normalize(Image, 255);
            Image = threshold(Image, 15, 255, false);
            Image = closing(Image, new bool[,] {
                {true, true, true},
                {true, true, true},
                {true, true, true}
            });
            int[,] edges = Image;
            Image = houghLines(Image, stepSize);
            Image = normalize(Image, 255);
            Image = window(Image, 120, 255);

            // Find the first line.
            SortedSet<Tuple<double,double>> lines = findLines(Image, stepSize);
            lines = filterLines(lines);
            Console.WriteLine(lines.Count);
            // Find perpendicular lines.
            List<Tuple<Tuple<double, double>,Tuple<double,double>>> pairs = findPerpendicular(lines);
            Console.WriteLine("Pairs: " + pairs.Count);
            // Find squares.
            lines.Clear();
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
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                int[] lineYs = new int[lines.Count];
                int i = 0;
                foreach(var line in lines) {
                    double angle = line.Item1;
                    double d = line.Item2;
                    lineYs[i] = (int)((d - (double)x * Math.Cos(angle)) / Math.Sin(angle));
                    i++;
                }

                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    bool found = false;
                    foreach(int lineY in lineYs) {
                        if(lineY != y)
                            continue;
                        
                        OutputImage.SetPixel(x, y, Color.Purple);
                        found = true;
                        break;
                    }

                    if(!found)
                    {
                        int v = edges[x, y];
                        OutputImage.SetPixel(x, y, Color.FromArgb(v,v,v));               // Set pixel color in array at (x,y)
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
            
            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
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
                    double d = y - 2 * InputImage.Height;
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

        private int[,] applyKernel(int[,] image, float[,] kernel) 
        {
            // Create a result image the size of the input image.
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];
            // Determine the size and center of the kernel
            int kwidth = kernel.GetLength(0);
            int kheight = kernel.GetLength(1);
            int hw = kwidth / 2;
            int hh = kheight / 2;

            // Loop over the center pixels for the kernel.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Variable holding the new value in the result.
                    int newValue = 0;

                    // Go over the kernel
                    for(int kx = -hw; kx <= hw; kx++) 
                    {
                        for(int ky = -hh; ky <= hh; ky++) 
                        {
                            float weight = kernel[kx + hw, ky + hh];
                            int oldValue;
                            // Check the bounds, outside is black
                            if(x + kx < 0 || x + kx >= width
                               || y + ky < 0 || y + ky >= height) {
                                // Perform not padding, not mirroring, but the other
                                int ix = Math.Min(Math.Max(x + kx, 0), width - 1);
                                int iy = Math.Min(Math.Max(y + ky, 0), height - 1);
                                oldValue = image[ix, iy];
                            } else {
                                oldValue = image[x + kx, y + ky];
                            }

                            newValue += (int)(weight * oldValue);
                        }

                        result[x, y] = newValue; // Note: Not clamped
                    }
                }
            }

            return result;
        }

        private int[,] erosionDilation(int[,] image, bool[,] s, bool erosion) 
        {
            // Create a result image the size of the input image.
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];
            // Determine the size and center of the kernel
            int kwidth = s.GetLength(0);
            int kheight = s.GetLength(1);
            int hw = kwidth / 2;
            int hh = kheight / 2;

            // Loop over the center pixels for the kernel.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Variable holding the new value in the result.
                    int newValue = erosion ? int.MaxValue : int.MinValue;

                    // Go over the kernel
                    for(int kx = -hw; kx <= hw; kx++) 
                    {
                        for(int ky = -hh; ky <= hh; ky++) 
                        {
                            if(!s[kx + hw, ky + hh])
                                continue;

                            int oldValue;
                            // Check the bounds, outside is black
                            if(x + kx < 0 || x + kx >= width
                                || y + ky < 0 || y + ky >= height)
                                oldValue = 0;
                            else
                                oldValue = image[x + kx, y + ky];

                            newValue = erosion ? Math.Min(oldValue, newValue) : Math.Max(oldValue, newValue);
                        }
                    }

                    // Clamp the result to ensure valid gray values.
                    result[x, y] = Math.Min(Math.Max(newValue, 0), 255);
                }
            }

            return result;
        }

        private int[,] erosion(int[,] image, bool[,] s) 
        {
            return erosionDilation(image, s, true);
        }

        private int[,] dilation(int[,] image, bool[,] s)
        {
            return erosionDilation(image, s, false);
        }

        private int[,] opening(int[,] image, bool[,] s)
        {
            return dilation(erosion(image, s), s);
        }

        private int[,] closing(int[,] image, bool[,] s)
        {
            return erosion(dilation(image, s), s);
        }

        private int[,] window(int[,] image, int start, int end) 
        {
            // Create a result image the size of the input image.
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];

            // Loop over the center pixels for the kernel.
            for(int x = 0; x < width; x++) 
            {
                for(int y = 0; y < height; y++) 
                {
                    int value = image[x, y];
                    if(value < start || value > end)
                        value = 0;

                    // Clamp the result to ensure valid gray values.
                    result[x, y] = value;
                }
            }

            return result;
        }

        private int[,] threshold(int[,] image, int thresholdStart, int thresholdEnd, bool keep)
        {
            // Create a result image the size of the input image.
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] result = new int[width, height];

            // Loop over the center pixels for the kernel.
            for(int x = 0; x < width; x++) 
            {
                for(int y = 0; y < height; y++) 
                {
                    int oldValue = image[x, y];
                    if(oldValue > thresholdStart && oldValue < thresholdEnd)
                        result[x, y] = int.MaxValue;
                    else
                        result[x,y] = keep ? oldValue : int.MinValue;

                    // Clamp the result to ensure valid gray values.
                    result[x, y] = Math.Min(Math.Max(result[x,y], 0), 255);
                }
            }

            return result;
        }

        private int[,] edgeDetection(int[,] image)
        {
            int[,] horizontal = applyKernel(image, new float[,] {
                {-1, -2, -1},
                {0, 0, 0},
                {1, 2, 1}
            });
            int[,] vertical = applyKernel(image, new float[,] {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            });

            int[,] result = combine(horizontal, vertical, (x, y) => {
                return (x + y) / 2;
            });
            return result;
        }

        public delegate int PixelArithmetic(int x, int y);
        public delegate int PixelArithmeticOne(int x);

        private int[,] combine(int[,] one, int[,] two, PixelArithmetic f)
        {
            int[,] result = new int[one.GetLength(0), one.GetLength(1)];
            for(int x = 0; x < one.GetLength(0); x++) {
                for(int y = 0; y < one.GetLength(1); y++) {
                    result[x, y] = f(one[x, y], two[x, y]);
                }
            }
            return result;
        }

        private int[,] compute(int[,] image, PixelArithmeticOne f) 
        {
            int[,] result = new int[image.GetLength(0), image.GetLength(1)];
            for(int x = 0; x < image.GetLength(0); x++) {
                for(int y = 0; y < image.GetLength(1); y++) {
                    result[x, y] = f(image[x, y]);
                }
            }
            return result;
        }

        private void findBounds(int[,] image, out int lower, out int upper) 
        {
            lower = 0;
            upper = 0;

            for(int x = 0; x < image.GetLength(0); x++) {
                for(int y = 0; y < image.GetLength(1); y++) {
                    int value = image[x, y];
                    if(value < lower)
                        lower = value;
                    if(value > upper)
                        upper = value;
                }
            }
        }

        private int[,] normalize(int[,] image, int max)
        {
            int lower, upper;
            findBounds(image, out lower, out upper);

            float scale = (float)max / (upper - lower);
            int[,] result = new int[image.GetLength(0), image.GetLength(1)];

            for(int x = 0; x < image.GetLength(0); x++) {
                for(int y = 0; y < image.GetLength(1); y++) {
                    int value = image[x, y];
                    result[x, y] = (int)((value - lower) * scale);
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
                        if(angle == 45 / stepSize) {
                            r = (int)((x + y) / Math.Sqrt(2) + random.NextDouble());
                        } else if(angle == 135 / stepSize) {
                            r = (int)((y - x) / Math.Sqrt(2) + random.NextDouble());
                        } else {
                            r = (int)(
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

