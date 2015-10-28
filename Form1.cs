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
using INFOIBV.CardOperations;
using INFOIBV.ImageOperations;
using INFOIBV.LineOperations;
using INFOIBV.ShapeOperations;
using System.Threading;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap inputImage;

        public INFOIBV()
        {
            InitializeComponent();
            outputSelector.SelectedIndex = 0;
        }

        private Bitmap Compute(Bitmap inputImage)
        {
            // Define output array
            int[,] output;

            // Get the grayscale image from src
            int[,] grayscale = Grayscale.FromBitmap(inputImage);
            int width = grayscale.GetLength(0);
            int height = grayscale.GetLength(1);

            // Detect edges using sobel
            int[,] edges = Sobel.Compute(grayscale);


            bool[,] strucElem = 
            {
                {false, true, false},
                {true,  true, true},
                {false, true, false}
            };

            // Calculate white top-hat (WTH) with plus-shaped structuring element
            int[,] wth = Defaults.Combine(edges, Morphologicals.Opening(edges, strucElem), (a, b) => a - b);

            // Create thresholds from the WTH
            int[,] wth60 = Threshold.Compute(wth, 60);
            int[,] wth25 = Threshold.Compute(wth, 25);

            // Calculate Hough transform
            const double STEP_SIZE = 0.25;
            int[,] hough = Hough.Compute(wth60, STEP_SIZE);
            int[,] houghWindow = Window.Compute(hough,20000,int.MaxValue);

            // Find and filter lines from hough transform
            SortedSet<Line> lines = Lines.FindLines(houghWindow, STEP_SIZE);
            lines = Lines.FilterLines(lines);

            strucElem = new bool[,]
            {
                {false, false, true, false, false},
                {false,  true, true,  true, false},
                { true,  true, true,  true,  true},
                {false,  true, true,  true, false},
                {false, false, true, false, false}
            };

            // Find all rectangles that somewhat resemble a card shape
            IList<Card> cards = Lines.FindCardShapedRectangles(lines, width, height);

            // Filter all card shaped rectangles with enough line support
            int[,] dilation = Morphologicals.Dilation(wth60, strucElem);
            IList<Card> filteredCards = Lines.FilterCardShapedRectangles(dilation, cards);

            // Set the output image, convert it to a bitmap and create a graphics object so we can draw on it
            switch (outputSelector.SelectedIndex)
            {
                default:
                case 0:
                    output = grayscale;
                    break;
                case 1:
                    output = edges;
                    break;
                case 2:
                    output = wth;
                    break;
                case 3:
                    output = wth60;
                    break;
                case 4:
                    output = wth25;
                    break;
                case 5:
                    output = dilation;
                    break;
                case 6:
                    output = hough;
                    break;
            }
            Bitmap outputImage = Grayscale.ToBitmap(Defaults.Normalize(output));

            if (output == hough)
                return outputImage;

            Graphics g = Graphics.FromImage(outputImage);

            // Draw the filtered lines
            if (drawFilteredCheckbox.Checked)
                DrawLines(lines, g, outputImage.Width, outputImage.Height);

            foreach (Card card in cards)
            {
                card.Draw(g,Pens.Orange);
            }

            foreach (Card card in filteredCards)
            {
                List<ShapeInfo> shapes = Shapes.Find(wth25, card);

                if (shapes.Count > 10) continue;

                Suit suit = Shapes.ClassifyShapes(shapes);
                if (suit != Suit.Unknown)
                {
                    // Draw the bboxes of the shapes
                    if (drawBBCheckbox.Checked)
                        DrawShapes(shapes, g);

                    // Draw the card outline
                    if (drawFoundCheckbox.Checked)
                        card.Draw(g);

                    // Format the card name and print it on the card
                    string cardName = String.Format("{0} of {1}", shapes.Count == 1 ? "Ace" : shapes.Count.ToString(), suit);
                    g.DrawString(cardName, new Font("Arial", 14), Brushes.Orange, new PointF(card.bottomLeft.X, card.bottomLeft.Y + 4));

                    Console.WriteLine(cardName);
                }
            }
            return outputImage;
        }

        private void DrawLines(SortedSet<Line> lines, Graphics g, int width, int height)
        {
            // Draw all the lines
            foreach (Line line in lines)
            {
                // Check for corner case
                if (line.theta == 0)
                {
                    // If we have a perfect vertical line we have to use a different calculation.
                    g.DrawLine(Pens.Lime, (float)line.rho, 0, (float)line.rho, height);
                }
                else
                {
                    // Calculate y-values at x=0 and x=width and draw a line between these points.
                    float y0 = (float)(line.rho / Math.Sin(line.theta));
                    float y1 = (float)((line.rho - width * Math.Cos(line.theta)) / Math.Sin(line.theta));
                    g.DrawLine(Pens.Lime, 0, y0, width, y1);
                }
            }
        }

        private void DrawShapes(List<ShapeInfo> shapes, Graphics g)
        {           
            // Draw all the shape boundingboxes
            foreach (ShapeInfo shape in shapes)
            {
                g.DrawRectangle(Pens.Purple, shape.X, shape.Y, shape.Width, shape.Height);
            }
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            // Load an image
            OpenFileDialog openImageDialog = new OpenFileDialog();
            openImageDialog.Filter = "Bitmap files (*.bmp;*.jpg;*.png;*.jpeg)|*.bmp;*.jpg;*.png;*.jpeg";
            openImageDialog.InitialDirectory = "..\\..\\images";
            if (openImageDialog.ShowDialog() == DialogResult.OK)
            {
                imageFileName.Text = openImageDialog.FileName;
                if (inputImage != null) inputImage.Dispose();
                inputImage = new Bitmap(imageFileName.Text);
                pictureBox1.Image = (Image)inputImage;
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            // Run the algorithm on the loaded image.
            if (inputImage == null) return;

            EnableDisableElements(false);

            if (pictureBox2.Image != null) pictureBox2.Image.Dispose();

            Bitmap result = Compute(inputImage);
            pictureBox2.Image = (Image)result;

            EnableDisableElements(true);
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null) return;

            // Save outputimage to file.
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Filter = "Bitmap file (*.bmp)|*.bmp";
            saveImageDialog.InitialDirectory = "..\\..\\images";
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                pictureBox2.Image.Save(saveImageDialog.FileName);
        }

        private void Batch_Click(object sender, EventArgs e)
        {
            EnableDisableElements(false);

            // Run the algorithm on multiple images.
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Bitmap files (*.bmp;*.jpg;*.png;*.jpeg)|*.bmp;*.jpg;*.png;*.jpeg";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string[] files = dialog.FileNames;

                progressBar1.Step = 1;
                progressBar1.Value = 0;
                progressBar1.Maximum = files.Length;

                progressBar1.Visible = true;

                string outputDir = Directory.GetParent(files[0]).FullName + "\\output";

                Directory.CreateDirectory(outputDir);

                foreach (string fileName in files)
                {
                    Bitmap image = new Bitmap(fileName);
                    Compute(image).Save(outputDir + "\\" + progressBar1.Value + ".bmp");
                    progressBar1.PerformStep();
                }

                progressBar1.Visible = false;
            }

            EnableDisableElements(true);
        }

        private void outputSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = outputSelector.SelectedIndex != 6;

            drawBBCheckbox.Enabled = enabled;
            drawFilteredCheckbox.Enabled = enabled;
            drawFoundCheckbox.Enabled = enabled;
            drawPotentialCheckbox.Enabled = enabled;
        }

        private void EnableDisableElements(bool enable)
        {
            drawBBCheckbox.Enabled = enable;
            drawFilteredCheckbox.Enabled = enable;
            drawFoundCheckbox.Enabled = enable;
            drawPotentialCheckbox.Enabled = enable;
            outputSelector.Enabled = enable;
            batchButton.Enabled = enable;
            applyButton.Enabled = enable;
            loadButton.Enabled = enable;
            saveButton.Enabled = enable;
        }
    }
}

