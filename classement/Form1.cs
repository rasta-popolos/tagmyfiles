using TagMyFiles.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagMyFiles.Component.FileInfo;
using TagMyFiles;
using DatabaseManager;
using MyUtils;


namespace TagMyFiles
{

    public partial class Form1 : Form
    {
        MyDatabase Db;
        
        TreeGroup TreeGroup;
        GridFile GridFile;
        TagContainer TagContainer;
        FileTag FileTag;

        public event StatusEventHandler statusEvent;

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.BackColor = System.Drawing.Color.MistyRose;
            //this.StartPosition = FormStartPosition.CenterScreen;
        }

        public delegate void FileTagLoadList(List < Hashtable > tags, Hashtable info);
        public FileTagLoadList DelegateFileTag;

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            this.statusEvent += h_statusInfo;

            #region mulilanguage

            /*
             * Set multilanguage
             */
            // left menu
            this.ApplyMultiLangueToToolbar(this.left_toolStrip1);
            // Top menu
            this.ApplyMultiLangueToToolbar(this.top_toolstrip);
            // center menu
            this.ApplyMultiLangueToToolbar(this.center_toolstrip);
            // left tree contextmenu
            this.ApplyMultiLangueToDDMenu(this.left_treectxmenu);
            // center grid contextmenu
            this.ApplyMultiLangueToDDMenu(this.center_grid_contextmenu);
            // center bottom menu (selected tag liste)
            this.ApplyMultiLangueToToolbar(this.center_bottom_TagtoolStrip1);
            
            #endregion

            this.Db = new MyDatabase();

            // Init tree group
            Hashtable treeOption = new Hashtable();
            treeOption["Db"] = this.Db;
            treeOption["tree"] = this.left_treeview;
            treeOption["contextmenu"] = this.left_treectxmenu;
            treeOption["autoLoad"] = true;
            this.TreeGroup = new TreeGroup(treeOption);
            this.TreeGroup.statusEvent += h_statusInfo;

            // Init center listview
            Hashtable listviewOption = new Hashtable();
            listviewOption["Db"] = this.Db;
            listviewOption["Form"] = this;
            listviewOption["listview"] = this.center_listview;
            listviewOption["contextmenu"] = this.center_grid_contextmenu;
            this.GridFile = new GridFile(listviewOption);
            this.GridFile.statusEvent += h_statusInfo;
            
            // Init center bottom Tag Container
            Hashtable tagContainerOption = new Hashtable();
            tagContainerOption["Db"] = this.Db;
            tagContainerOption["listview"] = this.center_bottomLIstView;
            tagContainerOption["GridFile"] = this.GridFile;
            this.TagContainer = new TagContainer(tagContainerOption);
            this.TagContainer.statusEvent += h_statusInfo;
            
            // Init right File Info
            Hashtable fileTagOption = new Hashtable();
            fileTagOption["Db"] = this.Db;
            fileTagOption["Form"] = this;
            fileTagOption["listview"] = this.right_listviewTag;
            fileTagOption["infopanel"] = this.right_bottom_FileInfoContainer;            
            fileTagOption["GridFile"] = this.GridFile;
            this.FileTag = new FileTag(fileTagOption);
            this.FileTag.statusEvent += h_statusInfo;
            DelegateFileTag = new FileTagLoadList(LoadTagWithList);

            // load lisy type fichier (center toolbar)
            this.ReloadComboTypeFile();
        }

        /**
         * Loop sur les toolbar pour appliquer les langues
         */
        private void ApplyMultiLangueToToolbar(ToolStrip toolstrip) {
            foreach (object tsic in toolstrip.Items)
            {
                if (tsic is ToolStripItemCollection)
                {
                    foreach (ToolStripItem tsi in (ToolStripItemCollection)tsic)
                    {
                        tsi.Text = Globalisation.GetString(tsi.Text);
                    }
                }
                else if (tsic is ToolStripDropDownItem)
                {
                    ToolStripDropDownItem ts = (ToolStripDropDownItem)tsic;
                    ts.Text = Globalisation.GetString(ts.Text);
                    foreach (ToolStripMenuItem menu in ts.DropDownItems)
                    {
                        menu.Text = Globalisation.GetString(menu.Text);
                    }
                }
                else if (tsic is ToolStripItem)
                {
                    ((ToolStripItem)tsic).ToolTipText = Globalisation.GetString(((ToolStripItem)tsic).ToolTipText);
                    ((ToolStripItem)tsic).Text = Globalisation.GetString(((ToolStripItem)tsic).Text);
                }
            }
        }
        private void ApplyMultiLangueToDDMenu(ToolStripDropDownMenu dropDMenu)
        {
            // Set global langage for menu
            foreach (ToolStripItem menu in dropDMenu.Items)
            {
                menu.Text = Globalisation.GetString(menu.Text.ToString());
                if (menu is ToolStripMenuItem)
                {
                    ToolStripMenuItem m = (ToolStripMenuItem)menu;
                    if (m.DropDownItems.Count > 0)
                    {
                        foreach (ToolStripItem tsi in m.DropDownItems)
                        {
                            tsi.Text = Globalisation.GetString(tsi.Text);
                        }
                    }
                }
            }
        }

        /**
         * Uncheck all menu items in this menu except checked_item.
         * 
         * */
        private void CheckMenuItem(ToolStripDropDownButton mnu, ToolStripMenuItem checked_item)
        {
            // Uncheck all of the menu items.
            foreach (ToolStripItem item in mnu.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                {
                    ToolStripMenuItem menu_item = item as ToolStripMenuItem;
                    menu_item.Checked = false;
                }
            }

            // Check the one that should be checked.
            checked_item.Checked = true;
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        #region Statusbar management
        /**
         * Efface le status barre tous les N sec
        */
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Db.InTransaction)
            {
                this.status_event_label.ForeColor = System.Drawing.Color.Blue;
                this.status_event_label.Text = Globalisation.GetString("DB_Intransaction");
            }
            else
            {
                if (this.status_event_label.Text == Globalisation.GetString("DB_Intransaction"))
                {
                    this.status_event_label.Text = "";
                }
            }
            // stop le timer pour effacer le status
            //this.timer1.Enabled = false;
        }

        // Affiche les events dans le status barre
        void h_statusInfo(object sender, StatusEventArgs e)
        {
            this.status_event_label.Text = e.Text.ToString();
            this.status_event_label.ForeColor = e.TextColor;

            // active le timer pour effacer le status
            //this.timer1.Enabled = false;
        }

        #endregion

        #region Left Tool bar Event

        /**
         * Refresh List
        */
        private void left_refreshGroup_Click(object sender, EventArgs e)
        {
            this.Db.SyncGroupFiles();
            this.TreeGroup.Load(true);
        }

        /**
         * Add Group
        */
        private void toolStrip_addGroup_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.TreeGroup.addGroup(this.left_inputaddgroup.Text))
                {
                    this.left_inputaddgroup.Text = "";
                    this.TreeGroup.Load();
                }
            }
            catch (Exception ex)
            {
                StatusEvent.FireStatusError(this, statusEvent, ex.Message);
            }
        }

        #endregion

        #region Tree (+ctx menu) Event

        /**
         * Ctx Rename Group
        */
        private void renameGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TreeGroup.StartEdit();
        }


        /**
         * Delete Group 
        */
        private void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TreeGroup.deleteGroup();
        }
        #endregion

        // scan le repertoire et maj bdd
        private void scandirectoryToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.GridFile.ReloadFile();
        }

        // sacn les fichiers vidéos et ajoute les tags correspondantes
        private void scanvideosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.ReloadVideoFile();
        }

        // rafraichi le grid
        private void center_toolstrip_refresh_Click(object sender, EventArgs e)
        {
            this.Db.SyncGroupFiles();
            this.TagContainer.Clear();
            Hashtable aFilters = new Hashtable();
            aFilters = this.GetGridFIlters();
            this.GridFile.Load(aFilters);
        }

        #region Center menu choix afficahge GridFile
        private void detailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.SetView(System.Windows.Forms.View.Details);
            this.CheckMenuItem(this.center_menu_choixaffichage, this.center_toolstrip_detail);
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.SetView(System.Windows.Forms.View.List);
            this.CheckMenuItem(this.center_menu_choixaffichage, this.center_toolstrip_list);
        }
        private void largeiconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.SetView(System.Windows.Forms.View.LargeIcon);
            this.CheckMenuItem(this.center_menu_choixaffichage, this.center_toolstrip_largeicon);
        }
        private void center_toolstrip_smallicon_Click(object sender, EventArgs e)
        {
            this.GridFile.SetView(System.Windows.Forms.View.SmallIcon);
            this.CheckMenuItem(this.center_menu_choixaffichage, this.center_toolstrip_smallicon);
        }
        private void center_toolstrip_tile_Click(object sender, EventArgs e)
        {
            this.GridFile.SetView(System.Windows.Forms.View.Tile);
            this.CheckMenuItem(this.center_menu_choixaffichage, this.center_toolstrip_tile);
        }
        #endregion

        /**
         * OUvrir le dossier
         * */
        private void openfoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.OpenFolder();
        }

        /**
         * Reload specific files
         * */
        private void reloaddefaulttagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.ReloadSpecificFile();
        }

        /**
         * Ajoute les tag selectionés
         * */
        private void left_treeview_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.TagContainer.AddTag(this.TreeGroup.Tree.SelectedNode, this.TreeGroup.Tree.SelectedNode.Name.ToInt());
        }


        #region Delegate
        /*
         * 
         * */
        public void LoadTagWithList(List < Hashtable > aTags, Hashtable aInfo)
        {
            this.FileTag.LoadTagWithList(aTags, aInfo);
        }
        #endregion


        /*
         * Toggle type jointure des tag
         * */
        private void left_toogleContainsGroup_Click(object sender, EventArgs e)
        {
            if (this.left_toogleContainsGroup.Checked)
            {
                this.GridFile.SetTagJoin("INTERSECT");
            }
            else
            {
                this.GridFile.SetTagJoin("UNION");
            }
            this.TagContainer.ReloadGrid();
        }

        private void center_toolstripClearTags_Click(object sender, EventArgs e)
        {
            this.TagContainer.Clear();
        }

        private void opendirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Db.InTransaction)
            {
                MessageBox.Show(this, Globalisation.GetString("DB_Intransaction"));
            }
            else
            {
                DirectorySelection f = new DirectorySelection(Db);
                f.FormClosed += new FormClosedEventHandler(f_FormClosed);
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
            }
        }

        /**
         * Event Close du form choix directory => charge le repertoire
         * */
        void f_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.Db.IsDefined && (sender as DirectorySelection).IsSelected)
            {
                this.Db.InitDb();
                this.TreeGroup.Load(true);
                this.GridFile.Clear();
            }
        }

        /**
         * Load combo list des type fichier
         * 
         * 
         * */
        protected void ReloadComboTypeFile()
        {
            this.center_toolStrip_FileTypeComboBox.ComboBox.SelectedIndex = -1;
            this.center_toolStrip_FileTypeComboBox.ComboBox.DataSource = null;
            this.center_toolStrip_FileTypeComboBox.Items.Clear();

            var dataSource = new List<ComboItem>();
            dataSource.Add(new ComboItem() { Name = "", Value = "" });
            foreach (Hashtable dir in this.Db.GetFileType())
            {
                dataSource.Add(new ComboItem() { Name = Globalisation.GetString("Typefile_" + dir["name"].ToString()), Value = dir["id"].ToString() });
            }
            this.center_toolStrip_FileTypeComboBox.ComboBox.DataSource = dataSource;
            this.center_toolStrip_FileTypeComboBox.ComboBox.DisplayMember = "Name";
            this.center_toolStrip_FileTypeComboBox.ComboBox.ValueMember = "Value";
            this.center_toolStrip_FileTypeComboBox.ComboBox.SelectedIndex = 0;
        }

        /*
         * Reccup les paramètre du filtre
         * */
        public Hashtable GetGridFIlters()
        {
            Hashtable res = new Hashtable();

            res["filetype"] = this.center_toolStrip_FileTypeComboBox.ComboBox.SelectedValue.ToString().ToInt(); // int
            res["tags"] = this.TagContainer.GetTags(); // List<String>
            res["textfilter"] = this.toolStripTextBox1.Text; // text

            return res;
        }

        /**
         * Filtre le grid à partir du combo type
         * 
         * */
        private void center_toolStrip_FileTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Hashtable aFilters = new Hashtable();
            aFilters = this.GetGridFIlters();
            this.GridFile.Load(aFilters);
        }

        /**
         * Filtre le grid à partir du textbox
         * */
        private void center_toolStrip_FileTypeComboBox_TextUpdate(object sender, EventArgs e)
        {
            //string text = ((ToolStripTextBox) sender).Text;
            //List<String> aTags = this.TagContainer.GetTags();
            Hashtable aFilters = new Hashtable();
            aFilters = this.GetGridFIlters();
            this.GridFile.Load(aFilters);
        }

        private void left_treectxmenu_Opening(object sender, CancelEventArgs e)
        {
            //Point p = new Point(e.X, e.Y);
        }

        /**
         * Open Options Windows
         * */
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolsOptions f = new ToolsOptions(Db);
            //f.FormClosed += new FormClosedEventHandler(f_FormClosed);
            f.StartPosition = FormStartPosition.CenterParent;
            f.ShowDialog(this);
        }

        /***
         * Suppr deleted files
         * */
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.DeleteSelectedFiles();
            Hashtable aFilters = new Hashtable();
            aFilters = this.GetGridFIlters();
            this.GridFile.Load(aFilters);
        }

        /**
         * Fusionner des vidéos
         * */
        private void joinvideoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.GridFile.MergeSelected();
        }

        // Rotate left
        private void rotateleftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.RotateSelected(2);
        }

        // Rotate right
        private void rotaterightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GridFile.RotateSelected(1);
        }
    }
}
