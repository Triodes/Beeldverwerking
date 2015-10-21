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
            int[,] wth60 = new Threshold(30).Compute(wth);
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

            // ---
            List<ShapeInfo> shapes = new List<ShapeInfo>();
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                card = Rectangles.Shrink(card, 0.75f, 0.9f);
                int[,] cardContent = Rectangles.CreateMask(wth30, card);

                int objects = 0;
                for (int y = 0; y < cardContent.GetLength(1); y++)
                {
                    for (int x = 0; x < cardContent.GetLength(0); x++)
                    {
                        if (cardContent[x, y] > objects)
                        {
                            objects++;
                            IList<int> path = Perimeter.WalkPerimeter(cardContent, x, y);
                            double length = Perimeter.ComputeLength(path);
                            double area = Perimeter.ComputeArea(path);
                            double compactness = area / length;
                            int[] bounding = Perimeter.BoundingBox(path, x, y);
                            ShapeInfo info = new ShapeInfo(path, bounding);

                            if (length <= 50 || compactness <= 0.6 || length > 500 || info.Ratio > 2.5)
                            {
                                objects--;
                                Perimeter.Colour(cardContent, x, y, 0);
                                continue;
                            }
                            Perimeter.Colour(cardContent, x, y, objects);

                            shapes.Add(info);
                            // DEBUG
                        }
                    }
                }

                // 
                String suit = "?";
                double avgSolidity = 0;
                int count = 0;
                foreach (ShapeInfo shape in shapes)
                {
                    // Classify the shape.
                    double area = Perimeter.ComputeArea(shape.Perimeter);
                    double solidity = (shape.Width * shape.Height) / area;
                    if (solidity >= 2)
                    {
                        // Something very wrong.
                        continue;
                    }
                    count++;
                    avgSolidity += solidity;
                }
                avgSolidity /= count;

                if (avgSolidity >= 1.72)
                {
                    suit = "Diamonds";
                }
                else if (avgSolidity >= 1.59)
                {
                    suit = "Clubs";
                }
                else if (avgSolidity >= 1.42)
                {
                    suit = "Spades";
                }
                else if (avgSolidity >= 1.25)
                {
                    suit = "Hearts";
                }

                Console.WriteLine("*** It's a card of {0} #{1}", suit, shapes.Count);
            }

            // Copy array to output Bitmap
            Bitmap outputImage = inputImage;//new Grayscale().ToBitmap(output);
            Graphics g = Graphics.FromImage(outputImage);
            foreach (Card card in cards)
            {
                card.Draw(g);
            }
            DrawShapes(shapes, g);

            return outputImage;
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

