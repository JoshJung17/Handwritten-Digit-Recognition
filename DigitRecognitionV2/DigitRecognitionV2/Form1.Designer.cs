namespace DigitRecognitionV2
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnGuess = new System.Windows.Forms.Button();
            this.txtIteration = new System.Windows.Forms.TextBox();
            this.btnIterate = new System.Windows.Forms.Button();
            this.txtConfidence3 = new System.Windows.Forms.TextBox();
            this.txtGuess3 = new System.Windows.Forms.TextBox();
            this.txtConfidence2 = new System.Windows.Forms.TextBox();
            this.txtGuess2 = new System.Windows.Forms.TextBox();
            this.txtConfidence = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtGuess = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.prgIteration = new System.Windows.Forms.ProgressBar();
            this.btnLoad = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(36, 356);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(147, 39);
            this.button1.TabIndex = 0;
            this.button1.Text = "Clear";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnGuess
            // 
            this.btnGuess.Location = new System.Drawing.Point(265, 12);
            this.btnGuess.Name = "btnGuess";
            this.btnGuess.Size = new System.Drawing.Size(98, 104);
            this.btnGuess.TabIndex = 1;
            this.btnGuess.Text = "Guess";
            this.btnGuess.UseVisualStyleBackColor = true;
            this.btnGuess.Click += new System.EventHandler(this.btnGuess_Click);
            // 
            // txtIteration
            // 
            this.txtIteration.Location = new System.Drawing.Point(449, 12);
            this.txtIteration.Name = "txtIteration";
            this.txtIteration.Size = new System.Drawing.Size(122, 20);
            this.txtIteration.TabIndex = 2;
            // 
            // btnIterate
            // 
            this.btnIterate.Location = new System.Drawing.Point(449, 38);
            this.btnIterate.Name = "btnIterate";
            this.btnIterate.Size = new System.Drawing.Size(122, 39);
            this.btnIterate.TabIndex = 3;
            this.btnIterate.Text = "Iterate";
            this.btnIterate.UseVisualStyleBackColor = true;
            this.btnIterate.Click += new System.EventHandler(this.btnIterate_Click);
            // 
            // txtConfidence3
            // 
            this.txtConfidence3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConfidence3.Location = new System.Drawing.Point(415, 343);
            this.txtConfidence3.Name = "txtConfidence3";
            this.txtConfidence3.Size = new System.Drawing.Size(39, 38);
            this.txtConfidence3.TabIndex = 38;
            this.txtConfidence3.Text = "11";
            // 
            // txtGuess3
            // 
            this.txtGuess3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGuess3.Location = new System.Drawing.Point(371, 343);
            this.txtGuess3.Name = "txtGuess3";
            this.txtGuess3.Size = new System.Drawing.Size(23, 38);
            this.txtGuess3.TabIndex = 37;
            this.txtGuess3.Text = "1";
            // 
            // txtConfidence2
            // 
            this.txtConfidence2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConfidence2.Location = new System.Drawing.Point(415, 299);
            this.txtConfidence2.Name = "txtConfidence2";
            this.txtConfidence2.Size = new System.Drawing.Size(39, 38);
            this.txtConfidence2.TabIndex = 36;
            this.txtConfidence2.Text = "11";
            // 
            // txtGuess2
            // 
            this.txtGuess2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGuess2.Location = new System.Drawing.Point(371, 299);
            this.txtGuess2.Name = "txtGuess2";
            this.txtGuess2.Size = new System.Drawing.Size(23, 38);
            this.txtGuess2.TabIndex = 35;
            this.txtGuess2.Text = "1";
            // 
            // txtConfidence
            // 
            this.txtConfidence.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConfidence.Location = new System.Drawing.Point(415, 228);
            this.txtConfidence.Name = "txtConfidence";
            this.txtConfidence.Size = new System.Drawing.Size(50, 49);
            this.txtConfidence.TabIndex = 34;
            this.txtConfidence.Text = "11";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(411, 171);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(160, 31);
            this.label12.TabIndex = 33;
            this.label12.Text = "Confidence:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(298, 171);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(101, 31);
            this.label11.TabIndex = 32;
            this.label11.Text = "Guess:";
            // 
            // txtGuess
            // 
            this.txtGuess.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGuess.Location = new System.Drawing.Point(328, 215);
            this.txtGuess.Name = "txtGuess";
            this.txtGuess.Size = new System.Drawing.Size(35, 62);
            this.txtGuess.TabIndex = 31;
            this.txtGuess.Text = "1";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(449, 83);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(122, 33);
            this.btnSave.TabIndex = 39;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // prgIteration
            // 
            this.prgIteration.Location = new System.Drawing.Point(265, 122);
            this.prgIteration.Name = "prgIteration";
            this.prgIteration.Size = new System.Drawing.Size(306, 46);
            this.prgIteration.TabIndex = 40;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(371, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(72, 104);
            this.btnLoad.TabIndex = 41;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 413);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.prgIteration);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtConfidence3);
            this.Controls.Add(this.txtGuess3);
            this.Controls.Add(this.txtConfidence2);
            this.Controls.Add(this.txtGuess2);
            this.Controls.Add(this.txtConfidence);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtGuess);
            this.Controls.Add(this.btnIterate);
            this.Controls.Add(this.txtIteration);
            this.Controls.Add(this.btnGuess);
            this.Controls.Add(this.button1);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnGuess;
        private System.Windows.Forms.TextBox txtIteration;
        private System.Windows.Forms.Button btnIterate;
        private System.Windows.Forms.TextBox txtConfidence3;
        private System.Windows.Forms.TextBox txtGuess3;
        private System.Windows.Forms.TextBox txtConfidence2;
        private System.Windows.Forms.TextBox txtGuess2;
        private System.Windows.Forms.TextBox txtConfidence;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtGuess;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ProgressBar prgIteration;
        private System.Windows.Forms.Button btnLoad;
    }
}

