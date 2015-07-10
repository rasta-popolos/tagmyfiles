using DatabaseManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyUtils;
using System.Windows.Forms.DataVisualization.Charting;

namespace TagMyFiles
{
    public partial class ToolsOptions : Form
    {
        MyDatabase Db;

        public ToolsOptions(MyDatabase db)
        {
            this.Db = db;

            InitializeComponent();

            this.gridFile_ListSize.Maximum = int.MaxValue;

            this.LoadPanelConfig(this.tabPage1);

            // etiquettes grid
            this.etiquette_GridView.ColumnCount = 2;
            this.etiquette_GridView.Columns[0].Name = "Tag";
            this.etiquette_GridView.Columns[1].Name = "Nombre de fichiers";
            List<Hashtable> aExt = this.Db.getTagCount();
            int i = 0;
            foreach (Hashtable ext in aExt)
            {
                this.etiquette_GridView.Rows.Add();
                this.etiquette_GridView.Rows[i].Cells[0].Value = ext["name"].ToString();
                this.etiquette_GridView.Rows[i].Cells[1].Value = ext["ct"].ToString().ToInt();
                i++;
            }

            this.label_nombrefichier.Text = this.Db.getFileCount().ToString();
            this.label_taillefichier.Text = this.Db.getFileSize().ToFileSize();
            
            // Combo chart
            //stat_comboCHart
            var dataSource = new List<ComboItem>();
            dataSource.Add(new ComboItem() { Value = "1", Name = "Taille fichier par extension" });
            dataSource.Add(new ComboItem() { Value = "2", Name = "Taille fichier par type" });

            dataSource.Add(new ComboItem() { Value = "3", Name = "Nombre fichier par extension" });
            dataSource.Add(new ComboItem() { Value = "4", Name = "Nombre fichier par type" });

            dataSource.Add(new ComboItem() { Value = "5", Name = "Nombre fichier par taille" });

            this.stat_comboCHart.DataSource = dataSource;
            this.stat_comboCHart.DisplayMember = "Name";
            this.stat_comboCHart.ValueMember = "Value";
            this.stat_comboCHart.SelectedIndex = 0;

        }

        /// <summary>
        /// Maj chart suivant le choix
        /// </summary>
        /// <param name="type"></param>
        protected void updateChart(string type)
        {
            ///// CHart
            // http://stackoverflow.com/questions/20606838/view-val-label-on-pie-chart-and-not-inside-the-series-when-slice-selected-tru

            //this.statChart.Series[0].Points.AddXY("2", "40");

            //this.statChart.ChartAreas[0].AxisX.LabelStyle.Format = "C";

            this.statChart.AntiAliasing = AntiAliasingStyles.All;
            this.statChart.Series[0].Points.Clear();
            this.statChart.Series[0].IsVisibleInLegend = true;
            this.statChart.Series[0].SmartLabelStyle.Enabled = false;

            var pieData = new List<KeyValuePair<string, long>>();
            switch (type)
            {
                case "1":
                    pieData = this.Db.GetFileXXXByExtension("size");
                    this.statChart.Series[0].ChartType = SeriesChartType.Pie;
                    break;
                case "2":
                    pieData = this.Db.GetFileXXXByType("size");
                    this.statChart.Series[0].ChartType = SeriesChartType.Pie;
                    break;
                case "3":
                    pieData = this.Db.GetFileXXXByExtension("count");
                    this.statChart.Series[0].ChartType = SeriesChartType.Pie;
                    break;
                case "4":
                    pieData = this.Db.GetFileXXXByType("count");
                    this.statChart.Series[0].ChartType = SeriesChartType.Pie;
                    break;
                case "5":
                    pieData = this.Db.GetFileXXXByInterval("count");
                    this.statChart.Series[0].ChartType = SeriesChartType.Column;
                    this.statChart.Series[0].IsVisibleInLegend = false;
                    break;
            }

            // calcul total pour avoir %
            double total = 0;
            foreach (KeyValuePair<string, long> pair in pieData)
            {
                total += pair.Value;
            }

            foreach (KeyValuePair<string, long> pair in pieData)
            {
                double avg = long.Parse(pair.Value.ToString()) * 100 / total;
                
                // Data arrays.
                DataPoint point = new DataPoint();
                point.SetValueXY(pair.Key, pair.Value);

                point.LegendText = pair.Key.ToString(); // "#VAL";
                
                //point.LabelBackColor = Color.Bisque;
                //point.LabelBorderColor = Color.Black;

                point.Font = new Font("Calibri Light", 8);
                if (avg >= 5)
                {
                    point.Label = pair.Key.ToString() + "\n" + "(" + Math.Round(avg).ToString() + "%)";
                    point.CustomProperties = "PieLabelStyle = Inside, Exploded = False";
                }
                else
                {
                    point.CustomProperties = "PieLabelStyle = Disabled, Exploded = True";
                }

                if (type == "1" || type == "2")
                {
                    point.ToolTip = string.Format("{0}, {1} ({2}%)", pair.Key, pair.Value.ToFileSize(), Math.Round(avg, 2));
                }
                else if (type == "5")
                {
                    point.LabelAngle = 90;
                    point.Label = pair.Key.ToString();
                    point.ToolTip = string.Format("{0}", pair.Value.ToString(), Math.Round(avg, 2));
                    point.CustomProperties = "PieLabelStyle = Inside, Exploded = True";
                    point.LabelAngle = -90;
                }
                else
                {
                    point.ToolTip = string.Format("{0}, {1} ({2}%)", pair.Key.ToString(), pair.Value.ToString(), Math.Round(avg, 2));
                }

                this.statChart.Series[0].Points.Add(point);

            }
        }

        protected string GetConfValue(string name)
        {
            return this.Db.GetConfValue(name);
        }



        protected void LoadPanelConfig(TabPage tabpage)
        {
            //tabpage.Container.Components.
            foreach (ContainerControl item in tabpage.Controls.OfType<ContainerControl>())
            {
                item.Text = this.GetConfValue(item.Name);
            }
        }
        protected void SavePanelConfig(TabPage tabpage)
        {
            //tabpage.Container.Components.
            foreach (ContainerControl item in tabpage.Controls.OfType<ContainerControl>())
            {
                this.Db.SetConfValue(item.Name.ToString(), item.Text.ToString());
            }
        }


        /***
         * Ok / Cancel
         * */
        private void button1_Click(object sender, EventArgs e)
        {
            this.SavePanelConfig(this.tabPage1);
            this.Close();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /***
         * Grid tag row number
         * 
         * */
        private void etiquette_GridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(this.etiquette_GridView.RowHeadersDefaultCellStyle.ForeColor))
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 10, e.RowBounds.Location.Y + 4);
            }
        }

        private void stat_comboCHart_SelectedIndexChanged(object sender, EventArgs e)
        {
            string type = ((ComboBox) sender).SelectedValue.ToString();            
            this.updateChart(type);
        }
    }
}
