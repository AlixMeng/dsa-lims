﻿namespace DSA_lims
{
    partial class FormPrintSampleLabel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrintSampleLabel));
            this.cboxPrinters = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.cboxPaperSizes = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbLandscape = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbReplications = new System.Windows.Forms.TextBox();
            this.tbCopies = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboxPrinters
            // 
            this.cboxPrinters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxPrinters.FormattingEnabled = true;
            this.cboxPrinters.Location = new System.Drawing.Point(26, 37);
            this.cboxPrinters.Name = "cboxPrinters";
            this.cboxPrinters.Size = new System.Drawing.Size(288, 21);
            this.cboxPrinters.TabIndex = 0;
            this.cboxPrinters.SelectedIndexChanged += new System.EventHandler(this.cboxPrinters_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select printer";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 221);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(338, 28);
            this.panel1.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(138, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(238, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cboxPaperSizes
            // 
            this.cboxPaperSizes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxPaperSizes.FormattingEnabled = true;
            this.cboxPaperSizes.Location = new System.Drawing.Point(26, 88);
            this.cboxPaperSizes.Name = "cboxPaperSizes";
            this.cboxPaperSizes.Size = new System.Drawing.Size(288, 21);
            this.cboxPaperSizes.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Paper sizes";
            // 
            // cbLandscape
            // 
            this.cbLandscape.AutoSize = true;
            this.cbLandscape.Checked = true;
            this.cbLandscape.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLandscape.Location = new System.Drawing.Point(26, 125);
            this.cbLandscape.Name = "cbLandscape";
            this.cbLandscape.Size = new System.Drawing.Size(79, 17);
            this.cbLandscape.TabIndex = 2;
            this.cbLandscape.Text = "Landscape";
            this.cbLandscape.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(176, 156);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Batch count";
            // 
            // tbReplications
            // 
            this.tbReplications.Location = new System.Drawing.Point(177, 172);
            this.tbReplications.MaxLength = 3;
            this.tbReplications.Name = "tbReplications";
            this.tbReplications.Size = new System.Drawing.Size(137, 20);
            this.tbReplications.TabIndex = 4;
            // 
            // tbCopies
            // 
            this.tbCopies.Location = new System.Drawing.Point(26, 172);
            this.tbCopies.MaxLength = 3;
            this.tbCopies.Name = "tbCopies";
            this.tbCopies.Size = new System.Drawing.Size(144, 20);
            this.tbCopies.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Number of copies";
            // 
            // FormPrintSampleLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 249);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbCopies);
            this.Controls.Add(this.tbReplications);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbLandscape);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboxPaperSizes);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboxPrinters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPrintSampleLabel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSALims- Print sample label";
            this.Load += new System.EventHandler(this.FormPrintSampleLabel_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboxPrinters;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ComboBox cboxPaperSizes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbLandscape;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbReplications;
        private System.Windows.Forms.TextBox tbCopies;
        private System.Windows.Forms.Label label4;
    }
}