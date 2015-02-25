namespace FlatPiler
{
    partial class FormFlatPiler
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
            this.taInput = new System.Windows.Forms.TextBox();
            this.buttonCompile = new System.Windows.Forms.Button();
            this.taOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // taInput
            // 
            this.taInput.Location = new System.Drawing.Point(12, 35);
            this.taInput.Multiline = true;
            this.taInput.Name = "taInput";
            this.taInput.Size = new System.Drawing.Size(205, 164);
            this.taInput.TabIndex = 0;
            this.taInput.TextChanged += new System.EventHandler(this.taInput_TextChanged);
            // 
            // buttonCompile
            // 
            this.buttonCompile.Location = new System.Drawing.Point(12, 205);
            this.buttonCompile.Name = "buttonCompile";
            this.buttonCompile.Size = new System.Drawing.Size(75, 23);
            this.buttonCompile.TabIndex = 1;
            this.buttonCompile.Text = "FlatPile!";
            this.buttonCompile.UseVisualStyleBackColor = true;
            this.buttonCompile.Click += new System.EventHandler(this.buttonCompile_Click);
            // 
            // taOutput
            // 
            this.taOutput.Location = new System.Drawing.Point(384, 35);
            this.taOutput.Multiline = true;
            this.taOutput.Name = "taOutput";
            this.taOutput.Size = new System.Drawing.Size(264, 334);
            this.taOutput.TabIndex = 2;
            this.taOutput.TextChanged += new System.EventHandler(this.taOutput_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Input Area";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(381, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output Area";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 381);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.taOutput);
            this.Controls.Add(this.buttonCompile);
            this.Controls.Add(this.taInput);
            this.Name = "Form1";
            this.Text = "FlatPiler";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox taInput;
        private System.Windows.Forms.Button buttonCompile;
        private System.Windows.Forms.TextBox taOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

