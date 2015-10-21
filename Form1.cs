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
using INFOIBV.ShapeOperations;
using System.Threading;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap inputImage;
        private const double STEP_SIZE = 0.25;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openImageDialog = new OpenFileDialog();
            openImageDialog.Filter = "Bitmap files (*.bmp;*.jpg;*.png;*.tiff;*.jpeg)|*.bmp;*.jpg;*.png;*.tiff;*.jpeg";
            openImageDialog.InitialDirectory = "..\\..\\images";
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
            if (pictureBox2.Image != null) pictureBox2.Image.Dispose();

            Bitmap result = Compute(inputImage);
            pictureBox2.Image = (Image)result;
            
            applyButton.Enabled = true;
        }

        private Bitmap Compute(Bitmap inputImage)
        {
            int[,] output;

            int[,] original = new Grayscale().FromBitmap(inputImage);

            //==========================================================================================
            int[,] edges = new Sobel().Compute(original);
            //edges = Morphologicals.Closing(edges, new bool[,] { { false, true, false }, { true, true, true }, { false, true, false } });
            //edges = new Window(20, int.MaxValue).Compute(edges);

            //int[,] mask = new Threshold(THRESHOLD,true).Compute(original);

            bool[,] strucElem = 
            {
                {false, true, false},
                {true,  true, true},
                {false, true, false}
            };

            //int[,] reconstruction = Morphologicals.GeoDilation(edges, strucElem, mask);

            int[,] wth = Defaults.Combine(edges, Morphologicals.Opening(edges, strucElem), (a, b) => a - b);
            //wth = Defaults.Normalize(wth, 255);
            int[,] wth60 = new Threshold(60).Compute(wth);
            int[,] wth30 = new Threshold(30).Compute(wth);

            #region HOUGH
            int[,] hough = new Hough().Compute(wth60, STEP_SIZE);
            //hough = Defaults.Normalize(hough, 255);
            hough = new Window(20000, int.MaxValue).Compute(hough);
            int maxVal = (int)Math.Ceiling(Math.Sqrt(edges.GetLength(0) * edges.GetLength(0) + edges.GetLength(1) * edges.GetLength(1)));
            SortedSet<Line> lines = Lines.FindLines(hough, STEP_SIZE, maxVal);
            lines = Lines.FilterLines(lines);
            #endregion

            IList<Card> cards = Lines.FindRectangle(lines);

            output = wth30;
            // Copy array to output Bitmap
            Bitmap outputImage = new Grayscale().ToBitmap(output);
            Graphics g = Graphics.FromImage(outputImage);

            DrawLines(lines, outputImage);

            foreach (Card card in cards)
            {
                card.Draw(g, Pens.Orange);
                List<ShapeInfo> shapes = Shapes.Find(wth30, card);
                Suit suit = Shapes.ClassifyShapes(shapes);
                if (suit != Suit.Unknown)
                {
                    Console.WriteLine("Ratio: " + card.ratio);
                    DrawShapes(shapes, g);
                    card.Draw(g);
                    g.DrawString(String.Format("{0} of {1}", shapes.Count == 1 ? "Ace" : shapes.Count.ToString(), suit), new Font("Arial", 14), Brushes.Orange, new PointF(card.bottomLeft.X, card.bottomLeft.Y + 4));
                }
            }



            return outputImage;
        }

        private void DrawLines(SortedSet<Line> lines, Bitmap outputImage)
        {
            Graphics g = Graphics.FromImage(outputImage);
            foreach (Line line in lines)
            {
                if (line.theta == 0)
                {
                    g.DrawLine(Pens.Lime, (float)line.rho, 0, (float)line.rho, outputImage.Height);
                }
                else
                {
                    float y0 = (float)(line.rho / Math.Sin(line.theta));
                    float y1 = (float)((line.rho - outputImage.Width * Math.Cos(line.theta)) / Math.Sin(line.theta));
                    g.DrawLine(Pens.Lime, 0, y0, outputImage.Width, y1);
                }
            }
        }

        private void DrawShapes(List<ShapeInfo> shapes, Graphics g)
        {            
            foreach (ShapeInfo shape in shapes)
            {
                g.DrawRectangle(Pens.Purple, shape.X, shape.Y, shape.Width, shape.Height);
            }
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null) return;

            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Filter = "Bitmap file (*.bmp)|*.bmp";
            saveImageDialog.InitialDirectory = "..\\..\\images";
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                pictureBox2.Image.Save(saveImageDialog.FileName);
        }

        private void Batch_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Bitmap files (*.bmp;*.jpg;*.png;*.tiff;*.jpeg)|*.bmp;*.jpg;*.png;*.tiff;*.jpeg";
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
                    Console.WriteLine("\nProcessing: " + progressBar1.Value + "\n");
                    Bitmap image = new Bitmap(fileName);
                    Compute(image).Save(outputDir + "\\" + progressBar1.Value + ".bmp");
                    progressBar1.PerformStep();
                }

                progressBar1.Visible = false;
            }
        }
    }
}

