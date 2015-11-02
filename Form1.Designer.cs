namespace INFOIBV
{
    partial class INFOIBV
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.loadButton = new System.Windows.Forms.Button();
            this.imageFileName = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.batchButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.drawFilteredLinesCheckbox = new System.Windows.Forms.CheckBox();
            this.drawFoundRectanglesCheckbox = new System.Windows.Forms.CheckBox();
            this.drawFoundCardsCheckbox = new System.Windows.Forms.CheckBox();
            this.drawBBCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.drawFilteredRectanglesCheckbox = new System.Windows.Forms.CheckBox();
            this.outputSelector = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(113, 11);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(98, 23);
            this.loadButton.TabIndex = 0;
            this.loadButton.Text = "Load image...";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.LoadImageButton_Click);
            // 
            // imageFileName
            // 
            this.imageFileName.Location = new System.Drawing.Point(217, 13);
            this.imageFileName.Name = "imageFileName";
            this.imageFileName.ReadOnly = true;
            this.imageFileName.Size = new System.Drawing.Size(325, 20);
            this.imageFileName.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 94);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 512);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(548, 11);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(103, 23);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(12, 11);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(95, 23);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save as BMP...";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(531, 94);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(512, 512);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // batchButton
            // 
            this.batchButton.Location = new System.Drawing.Point(657, 11);
            this.batchButton.Name = "batchButton";
            this.batchButton.Size = new System.Drawing.Size(91, 23);
            this.batchButton.TabIndex = 6;
            this.batchButton.Text = "Batch";
            this.batchButton.UseVisualStyleBackColor = true;
            this.batchButton.Click += new System.EventHandler(this.Batch_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(754, 11);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(286, 23);
            this.progressBar1.TabIndex = 7;
            this.progressBar1.Visible = false;
            // 
            // drawFilteredLinesCheckbox
            // 
            this.drawFilteredLinesCheckbox.AutoSize = true;
            this.drawFilteredLinesCheckbox.Location = new System.Drawing.Point(6, 19);
            this.drawFilteredLinesCheckbox.Name = "drawFilteredLinesCheckbox";
            this.drawFilteredLinesCheckbox.Size = new System.Drawing.Size(120, 17);
            this.drawFilteredLinesCheckbox.TabIndex = 8;
            this.drawFilteredLinesCheckbox.Text = "Filtered lines (green)";
            this.drawFilteredLinesCheckbox.UseVisualStyleBackColor = true;
            // 
            // drawFoundRectanglesCheckbox
            // 
            this.drawFoundRectanglesCheckbox.AutoSize = true;
            this.drawFoundRectanglesCheckbox.Location = new System.Drawing.Point(132, 19);
            this.drawFoundRectanglesCheckbox.Name = "drawFoundRectanglesCheckbox";
            this.drawFoundRectanglesCheckbox.Size = new System.Drawing.Size(146, 17);
            this.drawFoundRectanglesCheckbox.TabIndex = 9;
            this.drawFoundRectanglesCheckbox.Text = "Found rectangles (yellow)";
            this.drawFoundRectanglesCheckbox.UseVisualStyleBackColor = true;
            // 
            // drawFoundCardsCheckbox
            // 
            this.drawFoundCardsCheckbox.AutoSize = true;
            this.drawFoundCardsCheckbox.Location = new System.Drawing.Point(426, 19);
            this.drawFoundCardsCheckbox.Name = "drawFoundCardsCheckbox";
            this.drawFoundCardsCheckbox.Size = new System.Drawing.Size(117, 17);
            this.drawFoundCardsCheckbox.TabIndex = 10;
            this.drawFoundCardsCheckbox.Text = "Found cards (cyan)";
            this.drawFoundCardsCheckbox.UseVisualStyleBackColor = true;
            // 
            // drawBBCheckbox
            // 
            this.drawBBCheckbox.AutoSize = true;
            this.drawBBCheckbox.Location = new System.Drawing.Point(549, 19);
            this.drawBBCheckbox.Name = "drawBBCheckbox";
            this.drawBBCheckbox.Size = new System.Drawing.Size(173, 17);
            this.drawBBCheckbox.TabIndex = 11;
            this.drawBBCheckbox.Text = "Shape bounding boxes (purple)";
            this.drawBBCheckbox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.drawFilteredRectanglesCheckbox);
            this.groupBox1.Controls.Add(this.drawFilteredLinesCheckbox);
            this.groupBox1.Controls.Add(this.drawBBCheckbox);
            this.groupBox1.Controls.Add(this.drawFoundRectanglesCheckbox);
            this.groupBox1.Controls.Add(this.drawFoundCardsCheckbox);
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(736, 47);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Draw options";
            // 
            // drawFilteredRectanglesCheckbox
            // 
            this.drawFilteredRectanglesCheckbox.AutoSize = true;
            this.drawFilteredRectanglesCheckbox.Location = new System.Drawing.Point(284, 19);
            this.drawFilteredRectanglesCheckbox.Name = "drawFilteredRectanglesCheckbox";
            this.drawFilteredRectanglesCheckbox.Size = new System.Drawing.Size(136, 17);
            this.drawFilteredRectanglesCheckbox.TabIndex = 12;
            this.drawFilteredRectanglesCheckbox.Text = "Filtered rectangles (red)";
            this.drawFilteredRectanglesCheckbox.UseVisualStyleBackColor = true;
            // 
            // outputSelector
            // 
            this.outputSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.outputSelector.FormattingEnabled = true;
            this.outputSelector.Items.AddRange(new object[] {
            "Grayscale",
            "Edges (Sobel)",
            "White top hat",
            "White top hat (threshold 60)",
            "White top hat (threshold 30)",
            "Line support dilation",
            "Hough"});
            this.outputSelector.Location = new System.Drawing.Point(6, 15);
            this.outputSelector.Name = "outputSelector";
            this.outputSelector.Size = new System.Drawing.Size(274, 21);
            this.outputSelector.TabIndex = 14;
            this.outputSelector.SelectedIndexChanged += new System.EventHandler(this.outputSelector_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.outputSelector);
            this.groupBox2.Location = new System.Drawing.Point(754, 41);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(286, 47);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output image";
            // 
            // INFOIBV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 619);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.batchButton);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.imageFileName);
            this.Controls.Add(this.loadButton);
            this.Location = new System.Drawing.Point(10, 10);
            this.Name = "INFOIBV";
            this.ShowIcon = false;
            this.Text = "INFOIBV";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.TextBox imageFileName;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button batchButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox drawFilteredLinesCheckbox;
        private System.Windows.Forms.CheckBox drawFoundRectanglesCheckbox;
        private System.Windows.Forms.CheckBox drawFoundCardsCheckbox;
        private System.Windows.Forms.CheckBox drawBBCheckbox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox outputSelector;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox drawFilteredRectanglesCheckbox;

    }
}

