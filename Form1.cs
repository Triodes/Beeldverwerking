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

            if(1 - 1 == 0) {
                int x = 2;
                int y = 1;
                IList<int> perimeter = Perimeter.WalkPerimeter(original, x, y);
                IList<int> four = Perimeter.To4Connected(perimeter);
                Console.WriteLine(Perimeter.ComputeArea(four));
                return new Grayscale().ToBitmap(original);
            } 

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
            wth = new Threshold(30).Compute(wth);

            #region HOUGH
            int[,] hough = new Hough().Compute(wth, STEP_SIZE);
            //hough = Defaults.Normalize(hough, 255);
            hough = new Window(20000, int.MaxValue).Compute(hough);
            int maxVal = (int)Math.Ceiling(Math.Sqrt(edges.GetLength(0) * edges.GetLength(0) + edges.GetLength(1) * edges.GetLength(1)));
            SortedSet<Line> lines = Lines.FindLines(hough, STEP_SIZE, maxVal);
            lines = Lines.FilterLines(lines);
            Console.WriteLine("\nFILTERED:");
            foreach (Line line in lines)
            {
                Console.WriteLine("Theta: {0}, rho: {1}, value: {2}", line.theta, line.rho, line.value);
            }
            #endregion

            IList<Card> cards = Lines.FindRectangle(lines);
            Console.WriteLine("\nCARDS: {0}", cards.Count);

            // ---
            List<ShapeInfo> shapes = new List<ShapeInfo>();
            if(cards.Count >= 1)
            {
                Card card = cards[0];
                card = Rectangles.Shrink(card, 0.75f, 0.9f);
                int[,] cardContent = Rectangles.CreateMask(wth, card);

                int objects = 0;
                for(int y = 0; y < cardContent.GetLength(1); y++)
                {
                    for(int x = 0; x < cardContent.GetLength(0); x++)
                    {
                        if(cardContent[x,y] > objects) {
                            objects++;
                            IList<int> path = Perimeter.WalkPerimeter(cardContent, x, y);
                            double length = Perimeter.ComputeLength(path);
                            double area = Perimeter.ComputeArea(path);
                            double ratio = area / length;
                            int[] bounding = Perimeter.BoundingBox(path, x, y);
                            ShapeInfo info = new ShapeInfo(path, bounding);

                            if(length <= 50 || ratio <= 0.6 || length > 500 || info.Ratio > 2.5)
                            {
                                //Console.WriteLine("\tInsignificant object");
                                objects--;
                                Perimeter.remove(ref cardContent, x, y, 0);
                                continue;
                            }
                            Console.WriteLine("#{0}\t{1}/{2} = {3}", objects, length, area, ratio);
                            Perimeter.remove(ref cardContent, x, y, objects);

                            shapes.Add(info);
                            // DEBUG
                        }
                    }
                }
                Console.WriteLine("Objects: {0}", objects);
                output = Defaults.Normalize(cardContent, 255);

                // 
                String suit = "?";
                double avgSolidity = 0;
                int count = 0;
                foreach(ShapeInfo shape in shapes)
                {
                    // Classify the shape.
                    int[,] shapeImage = Perimeter.FillArea(original, shape.Perimeter, shape.X, shape.Y);
                    double area = Perimeter.ComputeArea(shape.Perimeter);
                    double solidity = (shape.Width * shape.Height) / area;
                    if(solidity >= 2) {
                        // Something very wrong.
                        continue;
                    }
                    count++;
                    avgSolidity += solidity;
                    Console.WriteLine("Solidity: {0}", solidity);
                }
                avgSolidity /= count;
                Console.WriteLine("\t\tAVG Solidity: {0}", avgSolidity);

                if(avgSolidity >= 1.72) {
                    suit = "Diamonds";
                } else if(avgSolidity >= 1.59) {
                    suit = "Clubs";
                } else if(avgSolidity >= 1.42) {
                    suit = "Spades";
                } else if(avgSolidity >= 1.25) {
                    suit = "Hearts";
                }

                Console.WriteLine("*** It's a card of {0} #{1}", suit, shapes.Count);
                //DialogResult result = MessageBox.Show(String.Format());
            }
            else
            {
                output = Defaults.Normalize(wth, 255);
            }

            // Copy array to output Bitmap
            Bitmap outputImage = new Grayscale().ToBitmap(output);
            DrawLines(lines, outputImage);
            DrawShapes(shapes, outputImage);

            return outputImage;
        }

        private void DrawLines(SortedSet<Line> lines, Bitmap outputImage)
        {
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
        }

        private void DrawShapes(List<ShapeInfo> shapes, Bitmap outputImage)
        {
            Graphics g = Graphics.FromImage(outputImage);
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

