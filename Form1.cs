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
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
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
            Image = opening(Image, new bool[,] { 
                {true, true, true, true, true}, 
                {true, true, true, true, true}, 
                {true, true, true, true, true}, 
                {true, true, true, true, true}, 
                {true, true, true, true, true}
            });

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
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
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
            int[,] result = new int[image.GetLength(0), image.GetLength(1)];
            // Determine the size and center of the kernel
            int kwidth = kernel.GetLength(0);
            int kheight = kernel.GetLength(1);
            int hw = kwidth / 2;
            int hh = kheight / 2;

            // Loop over the center pixels for the kernel.
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
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
                            if(x + kx < 0 || x + kx >= image.GetLength(0)
                               || y + ky < 0 || y + ky >= image.GetLength(1))
                                oldValue = 0;
                            else
                                oldValue = image[x + kx, y + ky];

                            newValue += (int)(weight * oldValue);
                        }
                    }

                    // Clamp the result to ensure valid gray values.
                    result[x, y] = Math.Min(Math.Max(newValue, 0), 255);
                }
            }

            return result;
        }

        private int[,] erosionDilation(int[,] image, bool[,] s, bool erosion) 
        {
            // Create a result image the size of the input image.
            int[,] result = new int[image.GetLength(0), image.GetLength(1)];
            // Determine the size and center of the kernel
            int kwidth = s.GetLength(0);
            int kheight = s.GetLength(1);
            int hw = kwidth / 2;
            int hh = kheight / 2;

            // Loop over the center pixels for the kernel.
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
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
                            if(x + kx < 0 || x + kx >= image.GetLength(0)
                                || y + ky < 0 || y + ky >= image.GetLength(1))
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
    }
}
