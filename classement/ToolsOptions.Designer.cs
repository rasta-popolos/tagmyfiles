namespace TagMyFiles
{
    partial class ToolsOptions
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolsOptions));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.etiquette_GridView = new System.Windows.Forms.DataGridView();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.gridFile_ListSize = new System.Windows.Forms.NumericUpDown();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.stat_comboCHart = new System.Windows.Forms.ComboBox();
            this.label_taillefichier = new System.Windows.Forms.Label();
            this.label_nombrefichier = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.statChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.etiquette_GridView)).BeginInit();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFile_ListSize)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statChart)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(259, 410);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Ok";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(340, 410);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Annuler";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.etiquette_GridView);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(419, 371);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Etiquettes";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // etiquette_GridView
            // 
            this.etiquette_GridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.etiquette_GridView.Location = new System.Drawing.Point(0, 0);
            this.etiquette_GridView.Name = "etiquette_GridView";
            this.etiquette_GridView.Size = new System.Drawing.Size(419, 375);
            this.etiquette_GridView.TabIndex = 0;
            this.etiquette_GridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.etiquette_GridView_RowPostPaint);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Window;
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.gridFile_ListSize);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(419, 371);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Liste fichier";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Item max";
            // 
            // gridFile_ListSize
            // 
            this.gridFile_ListSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.gridFile_ListSize.Location = new System.Drawing.Point(61, 32);
            this.gridFile_ListSize.Name = "gridFile_ListSize";
            this.gridFile_ListSize.Size = new System.Drawing.Size(247, 20);
            this.gridFile_ListSize.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(0, 1);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(427, 397);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.stat_comboCHart);
            this.tabPage4.Controls.Add(this.label_taillefichier);
            this.tabPage4.Controls.Add(this.label_nombrefichier);
            this.tabPage4.Controls.Add(this.label3);
            this.tabPage4.Controls.Add(this.label2);
            this.tabPage4.Controls.Add(this.statChart);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(419, 371);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Statistiques";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // stat_comboCHart
            // 
            this.stat_comboCHart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.stat_comboCHart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stat_comboCHart.FormattingEnabled = true;
            this.stat_comboCHart.Location = new System.Drawing.Point(0, 39);
            this.stat_comboCHart.Name = "stat_comboCHart";
            this.stat_comboCHart.Size = new System.Drawing.Size(419, 21);
            this.stat_comboCHart.TabIndex = 6;
            this.stat_comboCHart.SelectedIndexChanged += new System.EventHandler(this.stat_comboCHart_SelectedIndexChanged);
            // 
            // label_taillefichier
            // 
            this.label_taillefichier.AutoSize = true;
            this.label_taillefichier.Location = new System.Drawing.Point(121, 13);
            this.label_taillefichier.Name = "label_taillefichier";
            this.label_taillefichier.Size = new System.Drawing.Size(19, 13);
            this.label_taillefichier.TabIndex = 5;
            this.label_taillefichier.Text = "    ";
            // 
            // label_nombrefichier
            // 
            this.label_nombrefichier.AutoSize = true;
            this.label_nombrefichier.Location = new System.Drawing.Point(121, 0);
            this.label_nombrefichier.Name = "label_nombrefichier";
            this.label_nombrefichier.Size = new System.Drawing.Size(19, 13);
            this.label_nombrefichier.TabIndex = 5;
            this.label_nombrefichier.Text = "    ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Taille total fichier";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Nombre total fichier";
            // 
            // statChart
            // 
            this.statChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.statChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.statChart.Legends.Add(legend1);
            this.statChart.Location = new System.Drawing.Point(3, 66);
            this.statChart.Name = "statChart";
            series1.ChartArea = "ChartArea1";
            series1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.statChart.Series.Add(series1);
            this.statChart.Size = new System.Drawing.Size(413, 305);
            this.statChart.TabIndex = 0;
            this.statChart.Text = "chart1";
            // 
            // ToolsOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 445);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ToolsOptions";
            this.Text = "Options";
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.etiquette_GridView)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFile_ListSize)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView etiquette_GridView;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown gridFile_ListSize;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.DataVisualization.Charting.Chart statChart;
        private System.Windows.Forms.Label label_nombrefichier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox stat_comboCHart;
        private System.Windows.Forms.Label label_taillefichier;
        private System.Windows.Forms.Label label3;
    }
}