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

namespace TagMyFiles
{
    public partial class DirectorySelection : Form
    {
        MyDatabase Db;

        public bool IsSelected = false;

        public DirectorySelection(MyDatabase db)
        {
            this.Db = db;
            
            InitializeComponent();

            // Load Combo repertoire
            this.ReloadComboDirectory();
            
        }

        protected void ReloadComboDirectory() {
            this.comboBox_ChoixRepertoire.SelectedIndex = -1;
            this.comboBox_ChoixRepertoire.DataSource = null;
            this.comboBox_ChoixRepertoire.Items.Clear();

            var dataSource = new List<ComboItem>();
            int selectedIndex = -1;
            int i = 0;
            foreach (Hashtable dir in this.Db.GetDirectories())
            {
                dataSource.Add(new ComboItem() { Name = dir["name"].ToString(), Value = dir["id"].ToString() });
                // defaul selected => active directory
                if (dir["active"].ToString() == "1")
                {
                    selectedIndex = i;
                }
                i++;
            }
            this.comboBox_ChoixRepertoire.DataSource = dataSource;
            this.comboBox_ChoixRepertoire.DisplayMember = "Name";
            this.comboBox_ChoixRepertoire.ValueMember = "Value";
            this.comboBox_ChoixRepertoire.SelectedIndex = selectedIndex;

            this.LoadDetails();
        }

        private int getSelectedDir() {
            return (this.comboBox_ChoixRepertoire.SelectedItem as ComboItem).Value.ToString().ToInt();
        }


        /**
         * Affiche les détail dans le form
         * */
        private void setFormDetails(Hashtable res)
        {
            this.name_textBox.Text = "";
            this.directory_textBox.Text = "";
            this.description_TextBox.Text = "";
            this.source_textBox.Text = "";
            if (res.Count > 0)
            {
                this.name_textBox.Text = res["name"].ToString();
                this.directory_textBox.Text = res["path"].ToString();
                this.description_TextBox.Text = res["description"].ToString();
                this.source_textBox.Text = res["db"].ToString();
            }
        }

        /**
         * Charge le détail du repertoire
         * */
        private void LoadDetails()
        {
            if (this.comboBox_ChoixRepertoire.SelectedIndex == -1) return;
            if (this.comboBox_ChoixRepertoire.SelectedItem == null) return;

            int dirId = this.getSelectedDir();
            Hashtable res = this.Db.GetDirectory(dirId);
            if (res.Count > 0)
            {
                this.setFormDetails(res);
            }
            return;
        }
        /**
         * On change event du combo pour Détail repertoire
         * */
        private void comboBox_ChoixRepertoire_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadDetails();
        }

        // Cancel
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // Save
        private void button3_Click_1(object sender, EventArgs e)
        {
            if (this.comboBox_ChoixRepertoire.SelectedIndex == -1) return;
            if (this.comboBox_ChoixRepertoire.SelectedItem == null) return;

            int dirId = this.getSelectedDir();
            string name = this.name_textBox.Text;
            string path = this.directory_textBox.Text;
            string desc = this.description_TextBox.Text;
            this.Db.SaveDirectoryMainDb(dirId, name, path, desc);

        }
        // Delete
        private void button5_Click(object sender, EventArgs e)
        {
            if (this.comboBox_ChoixRepertoire.SelectedIndex == -1) return;
            if (this.comboBox_ChoixRepertoire.SelectedItem == null) return;

            if (MessageBox.Show(Globalisation.GetString("Prompt_delete_directory"), Globalisation.GetString("Delete_directory"), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int dirId = this.getSelectedDir();
                if (this.Db.DeleteDirectoryMainDb(dirId))
                {
                    this.ReloadComboDirectory();
                }
            }
        }


        // Select et Open
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.comboBox_ChoixRepertoire.SelectedIndex == -1) return;
            if (this.comboBox_ChoixRepertoire.SelectedItem == null) return;

            int dirId = this.getSelectedDir();
            Hashtable res = this.Db.GetDirectory(dirId);
            if (res.Count > 0)
            {
                this.Db.SetDefaultMainDirectory(res["id"].ToString().ToInt());
                this.IsSelected = true;
                this.Close();
            }
        }

        /**
         * Open directory dialog & Add Directory
         * */
        private void button3_Click(object sender, EventArgs e)
        {
            // Show the FolderBrowserDialog.
            DialogResult result = this.folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folderName = this.folderBrowserDialog.SelectedPath;
                this.Db.InsertDirectoryMainDb(folderName);
                this.ReloadComboDirectory();
            }
        }

        /**
         * Change directory path
         * */
        private void button4_Click(object sender, EventArgs e)
        {
            // Show the FolderBrowserDialog.
            DialogResult result = this.folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folderName = this.folderBrowserDialog.SelectedPath;
                if (this.name_textBox.Text == this.directory_textBox.Text)
                {
                    this.name_textBox.Text = folderName;
                }
                this.directory_textBox.Text = folderName;
            }
        }


    }
}
