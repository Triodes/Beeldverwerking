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
                    InputImage.Size.Height > 1024 || InputImage.Size.Width > 1024) // Dimension check
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
            // TODO: include here your own code
            // example: create a negative image

            // Sharpening
            /*
            Image = applyKernel(Image, new float[,] {
                {-1, -1, -1},
                {-1,  9, -1},
                {-1, -1, -1}
            });*/
            // Derivative
            /*
            Image = applyKernel(Image, new float[,] {
                {1/12f, -8/12f, 0, 8/12f, -1/12f},
            });*/
            /*
            Image = opening(Image, new bool[,] { 
                {true, true, true, true, true}, 
                {true, true, true, true, true}, 
                {true, true, true, true, true}, 
                {true, true, true, true, true}, 
                {true, true, true, true, true}
            });*/
            Image = edgeDetection(Image);
            Image = compute(Image, (v) => Math.Abs(v));
            Image = normalize(Image, 255);
            Image = threshold(Image, 30, 255, false);
            Image = houghLines(Image, 1);
            Image = normalize(Image, 255);
            Image = threshold(Image, 128, 255, false);

            /*
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B); // Negative image
                    Image[x, y] = updatedColor;                             // Set the new pixel color at coordinate (x,y)
                }
				progressBar.PerformStep();                                  // Increment progress bar
            }*/
            //==========================================================================================

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
            }
            
            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
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
                               || y + ky < 0 || y + ky >= height)
                                oldValue = 0;
                            else
                                oldValue = image[x + kx, y + ky];

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
            upper = 255;

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

        private int[,] houghLines(int[,] image, int stepSize) 
        {
            int[,] result = new int[360, image.GetLength(1) * 2];

            for(int x = 0; x < image.GetLength(0); x++) {
                for(int y = 0; y < image.GetLength(1); y++) {
                    int value = image[x, y];
                    if(value < 128)
                        continue;

                    // Loop over the angles.
                    for(int angle = 0; angle < 360; angle += stepSize) {
                        int d = (int)((x - image.GetLength(0) / 2) * Math.Cos(angle * 0.017) + (y - image.GetLength(1)/2) * Math.Sin(angle * 0.017));
                        d += image.GetLength(1);
                        result[angle, d]++;
                    }
                }
            }
            return result;
        }
    }
}
