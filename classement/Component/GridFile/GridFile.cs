using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagMyFiles;
using MyUtils;
using DatabaseManager;
using System.Text.RegularExpressions;
using System.Drawing;

namespace TagMyFiles.Component
{
    class GridFile
    {
        ListView ListView;
        MyDatabase Db;
        ContextMenuStrip ContextMenu;
        Form1 Form;

        //private ListViewColumnSorter lvwColumnSorter;
        private Hashtable currentFIlter = new Hashtable();

        public event StatusEventHandler statusEvent;

        public ListView GetListView()
        {
            return this.ListView;
        }

        public GridFile(Hashtable options)
        {

            if (options.ContainsKey("Db"))
            {
                this.Db = (MyDatabase)options["Db"];
            }

            if (options.ContainsKey("Form"))
            {
                this.Form = (Form1)options["Form"];
            }
            
            if (options.ContainsKey("listview"))
            {
                this.ListView = (ListView) options["listview"];
                this.ListView.AllowColumnReorder = false;
                
                this.SetView(System.Windows.Forms.View.Details);

                this.ListView.FullRowSelect = true;
                this.ListView.GridLines = true;
                this.ListView.AllowDrop = false;
                this.ListView.HideSelection = false;
                //this.ListView.ShowItemToolTips = true;

                // D&D
                this.ListView.MouseDown += new MouseEventHandler(mouseDown);
                this.ListView.DragOver += new DragEventHandler(dragOver);

                // event
                this.ListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.center_listview_ColumnClick);
                this.ListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.center_listview_MouseClick);
                this.ListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.center_listview_KeyDown);

                ColumnHeader column1 = new ColumnHeader();
                column1.Text = Globalisation.GetString("File_name");
                column1.TextAlign = HorizontalAlignment.Left;
                column1.Width = 300;

                ColumnHeader column2 = new ColumnHeader();
                column2.Text = Globalisation.GetString("Extension");
                column2.TextAlign = HorizontalAlignment.Left;
                column2.Width = 50;
                
                /*
                ColumnHeader column3 = new ColumnHeader();
                column3.Text = Globalisation.GetString("File_size")+ " (o)";
                column3.TextAlign = HorizontalAlignment.Right;
                column3.Width = 90;
                */

                ColumnHeader column4 = new ColumnHeader();
                column4.Text = Globalisation.GetString("File_size");
                column4.TextAlign = HorizontalAlignment.Right;
                column4.Width = 40;

                ColumnHeader column5 = new ColumnHeader();
                column5.Text = Globalisation.GetString("Date");
                column5.TextAlign = HorizontalAlignment.Left;
                column5.Width = 80;

                this.ListView.Columns.AddRange(new ColumnHeader[] {
                    column1, column2, column4, column5
                });

                // Add sorter
                //lvwColumnSorter = new ListViewColumnSorter();
                //this.ListView.ListViewItemSorter = lvwColumnSorter;
            }
            if (options.ContainsKey("contextmenu"))
            {
                this.setContextMenu((ContextMenuStrip)options["contextmenu"]);
            }

            if (options.ContainsKey("autoLoad") && (bool)options["autoLoad"])
            {
                this.Load();
            }
        }

        public void SetView(System.Windows.Forms.View v)
        {
            this.ListView.View = v;
            //this.ListView.TileSize = new Size(400, 100);
        }

        /**
         * Add context menu
        */
        public void setContextMenu(ContextMenuStrip ctxMenu)
        {
            this.ContextMenu = ctxMenu;
        }

        #region Grid event

        //  Double click
        // => mousedown

        // Sort column
        private void center_listview_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.Sort(e);
        }

        private Hashtable storeOrder = new Hashtable();

        public void Sort(ColumnClickEventArgs e)
        {
            /*
            // Déterminer si la colonne sélectionnée est déjà la colonne triée.
            if (e.Column == this.lvwColumnSorter.SortColumn)
            {
                // Inverser le sens de tri en cours pour cette colonne.
                if (this.lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    this.lvwColumnSorter.Order = SortOrder.Descending;
                    orientation = "DESC";
                }
                else
                {
                    this.lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Définir le numéro de colonne à trier ; par défaut sur croissant.
                //this.lvwColumnSorter.SortColumn = e.Column;
                this.lvwColumnSorter.Order = SortOrder.Ascending;
            }
            // Procéder au tri avec les nouvelles options.
            //this.ListView.Sort();
            */
            if (this.currentFIlter == null) this.currentFIlter = new Hashtable();

            string orderBy = "LOWER(name)";
            switch (e.Column)
            {
                case 1:
                    orderBy = "LOWER(extension)";
                    break;
                case 2:
                    orderBy = "size";
                    break;
                case 3:
                    orderBy = "date_create";
                    break;
                default:
                    break;
            }

            // si la colonne est déjà trié
            if(this.storeOrder.ContainsKey(orderBy)) {
                if(this.storeOrder[orderBy].ToString() == "ASC") {
                    this.storeOrder[orderBy] = "DESC";
                }else 
                {
                    this.storeOrder[orderBy] = "ASC";
                }
            }else {
                this.storeOrder[orderBy] = "ASC";
            }
            this.currentFIlter["order"] = orderBy + " " + this.storeOrder[orderBy].ToString();

            this.Load(this.currentFIlter);

        }

        // show contextmenu
        private void center_listview_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.ContextMenu != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (this.ListView.FocusedItem.Bounds.Contains(e.Location) == true)
                    {

                        // Vidéo fusion
                        ((ToolStripMenuItem)this.ContextMenu.Items[2]).DropDownItems[0].Enabled = false;

                        // Vidéo rotate
                        ((ToolStripMenuItem)this.ContextMenu.Items[2]).DropDownItems[2].Enabled = false;
                        ((ToolStripMenuItem)this.ContextMenu.Items[2]).DropDownItems[3].Enabled = false;

                        this.ContextMenu.Items[4].Enabled = false; // delete

                        // enable fusion option
                        if (this.IsSelectedVideoFiles())
                        {
                            ((ToolStripMenuItem)this.ContextMenu.Items[2]).DropDownItems[0].Enabled = true;
                        }

                        // enable vidéo rotate
                        if (this.ListView.SelectedItems.Count == 1)
                        {
                            ((ToolStripMenuItem)this.ContextMenu.Items[2]).DropDownItems[2].Enabled = true;
                            ((ToolStripMenuItem)this.ContextMenu.Items[2]).DropDownItems[3].Enabled = true;
                        }

                        // enable delete option
                        if (this.IsSelectedDeleted())
                        {
                            this.ContextMenu.Items[4].Enabled = true;
                        }

                        this.ContextMenu.Show(Cursor.Position);
                    }
                }
            }
        }

        #endregion

        public void DeleteSelectedFiles()
        {
            int fileId;
            Hashtable aFile;
            string path;
            if (this.ListView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in this.ListView.SelectedItems)
                {
                    fileId = item.Tag.ToString().ToInt();
                    if (this.Db.GetFileStatus(fileId).Contains(1))
                    {
                        aFile = this.Db.getFile(fileId);
                        path = aFile["path"].ToString();
                        if(File.Exists(path))
                        {
                            try
                            {
                                File.Delete(path);
                            }
                            catch { }
                        }
                    }
                }


            }
        }

        /// <summary>
        /// Test si les fichiers selectionné sont tagé delete
        /// </summary>
        protected bool IsSelectedDeleted()
        {
            if (this.ListView.SelectedItems.Count > 0)
            {
                int fileId;
                foreach (ListViewItem item in this.ListView.SelectedItems)
                {
                    fileId = item.Tag.ToString().ToInt();
                    if (! this.Db.GetFileStatus(fileId).Contains(1))
                    {
                        return false;
                    }

                }
                return true;
            }
            return false;
        }

        #region Drag and drop

        private void mouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.ListView.GetItemAt(e.X, e.Y) != null)
            {
                if (e.Clicks == 1)
                {
                    bool s = this.ListView.GetItemAt(e.X, e.Y).Selected;
                    // si l'item n'est pas selectionné => drag single item
                    if (!s)
                    {
                        this.ListView.DoDragDrop(this.ListView.GetItemAt(e.X, e.Y), DragDropEffects.Move);
                    }
                    else
                    {
                        this.ListView.DoDragDrop(this.ListView.SelectedItems, DragDropEffects.Move);
                    }
                }
                else if (e.Clicks == 2)
                {
                    int fileId = this.ListView.GetItemAt(e.X, e.Y).Tag.ToString().ToInt();
                    this.OpenItem(fileId);
                }
            }
        }

        private void dragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        #endregion


        /// <summary>
        /// Test si les fichier sélectionné sont des fichiers vidéos
        /// </summary>
        /// <returns></returns>
        private bool IsSelectedVideoFiles()
        {
            if (this.ListView.SelectedItems.Count > 1)
            {
                int fileId;
                Hashtable aFile;
                foreach (ListViewItem item in this.ListView.SelectedItems)
                {
                    fileId = item.Tag.ToString().ToInt();
                    aFile = this.Db.getFile(fileId);
                    if (!MyVideos.Ffmpeg.IsVideoFile(aFile["path"].ToString()))
                    {
                        return false;
                    }

                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fusionner les vidéos selectionnés
        /// </summary>
        public void MergeSelected()
        {
            int fileId;
            Hashtable aFile;
            List<string> Files = new List<string>();
            List<int> FilesId = new List<int>();

            foreach (ListViewItem item in this.ListView.SelectedItems)
            {
                fileId = item.Tag.ToString().ToInt();
                aFile = this.Db.getFile(fileId);
                if (MyVideos.Ffmpeg.IsVideoFile(aFile["path"].ToString()))
                {
                    Files.Add(aFile["path"].ToString());
                    FilesId.Add(fileId);
                }
            }
            if (Files.Count > 0)
            {
                Files.Sort();

                // place le fichier outpu dans le repertoire du premier fichier
                string d = Path.GetDirectoryName(Files[0]);
                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }

                string fileName;
                string tmpF = "";
                string output;

                System.IO.FileInfo f = new System.IO.FileInfo(Files[0]);
                fileName = f.Name.RegReplace(Regex.Escape(f.Extension) + "$", "");

                tmpF = fileName.RegReplace("[0-9]{1,}$", "").Trim();

                output = d + "\\" + tmpF + f.Extension;

                if (!string.IsNullOrEmpty(tmpF) && !File.Exists(output))
                {
                    output = d + "\\" + tmpF + f.Extension;
                }
                else
                {
                    tmpF = tmpF + " Merge";
                    output = d + "\\" + tmpF + f.Extension;

                }

                Process process = MyVideos.Ffmpeg.MergeVideoFiles(Files, output);
                //process.WaitForExit();
                process.Exited += (sender, eventArgs) => {
                    this.MergeSelectedFinish(FilesId, output);
                };

            }
        }

        /// <summary>
        /// Rotate selected files
        /// 0 = 90CounterCLockwise and Vertical Flip (default)
        /// 1 = 90Clockwise
        /// 2 = 90CounterClockwise
        /// 3 = 90Clockwise and Vertical Flip
        /// </summary>
        /// <param name="direction"></param>
        public void RotateSelected(int direction)
        {
            if (this.ListView.SelectedItems.Count == 1)
            {
                int fileId = this.ListView.SelectedItems[0].Tag.ToString().ToInt();
                Hashtable aFile = this.Db.getFile(fileId);

                System.IO.FileInfo f = new System.IO.FileInfo(aFile["path"].ToString());
                string fileName = f.Name.RegReplace(Regex.Escape(f.Extension) + "$", "");
                string output = Path.GetDirectoryName(aFile["path"].ToString()) + "\\" + fileName + "_rotate" + f.Extension;

                Process process = MyVideos.Ffmpeg.RotateVideoFile(aFile["path"].ToString(), output, direction);
                //process.WaitForExit();
                process.Exited += (sender, eventArgs) =>
                {
                    this.RotateSelectedFinish(aFile["path"].ToString().ToInt(), output);
                };
            }
        }
        
        /// <summary>
        /// Ajout les tags du fichier mergé
        /// </summary>
        /// <param name="Files"></param>
        /// <param name="output"></param>
        private void MergeSelectedFinish(List<int> FilesId, string output)
        {
            Hashtable FileInfo = MyVideos.Ffmpeg.getVideoInformation(output);
            if (FileInfo.Count > 0)
            {
                int fid = this.Db.addFile(output);
                if (fid > 0)
                {
                    this.Db.addTagForVideoFiles(output, fid);
                    this.Db.MergeGroupFile(fid, FilesId);

                    // delete merged files
                    this.Db.UpdateStatusFile(FilesId, 1);
                    // update new file status
                    this.Db.UpdateStatusFile(fid, 2, string.Join(",", FilesId));
                }
            }
        }

        /// <summary>
        /// Ajout des tags du fichier rotaté
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="output"></param>
        private void RotateSelectedFinish(int fileId, string output)
        {
            Hashtable FileInfo = MyVideos.Ffmpeg.getVideoInformation(output);
            if (FileInfo.Count > 0)
            {
                // TODO erreur
                int fid = this.Db.addFile(output);
                if (fid > 0)
                {
                    this.Db.addTagForVideoFiles(output, fid);
                    this.Db.MergeGroupFile(fid, fileId);
                    // update new file status
                    this.Db.UpdateStatusFile(fid, 3, fileId.ToString());
                }
            }
        }

        // Open file on enter press
        private void center_listview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (this.ListView.SelectedItems.Count == 1)
                {
                    int fileId = this.ListView.SelectedItems[0].Tag.ToString().ToInt();
                    this.OpenItem(fileId);
                }
            }
        }

        public void Clear()
        {
            this.ListView.Items.Clear();
        }

        /// <summary>
        /// Charge le contenu du grid avec les filtres en options
        /// </summary>
        /// <param name="aFilters"></param>
        public void Load(Hashtable aFilters=null)
        {
            if (!this.Db.IsDefined) return;

            int maxSize = this.Db.GetConfValue("gridFile_ListSize").ToInt();

            this.Clear();
            
            this.currentFIlter = aFilters;

            List<Hashtable> aFile = this.Db.getFiles(aFilters);

            string sFile;

            ListViewItem newItem;
            List<ListViewItem> items = new List<ListViewItem>();

            this.Db.Begin();
            int i = 0;
            foreach (Hashtable file in aFile)
            {
                i++;
                if (i > maxSize) break;

                sFile = file["path"].ToString();
                if (File.Exists(sFile))
                {
                    newItem = new ListViewItem(new string[] {
                        file["name"].ToString(),
                        file["extension"].ToString(),                        
                        long.Parse(file["size"].ToString()).ToFileSize(),
                        file["date_create"].ToString()      
                    });
                    newItem.Tag = file["id"].ToString();
                    newItem.ToolTipText = sFile;

                    items.Add(newItem);
                }
                else
                {
                    // TODO delete in bdd
                    this.Db.deleteFiles(int.Parse(file["id"].ToString()));
                }
            }
            this.Db.End();

            this.ListView.BeginUpdate();
            this.ListView.Items.AddRange(items.ToArray());
            this.ListView.EndUpdate();

            if (aFile.Count > maxSize)
            {
                StatusEvent.FireStatusInfo(this, statusEvent, aFile.Count.ToString() + " items (" + maxSize + " " + Globalisation.GetString("Max_number") + ")");
            }
            else
            {
                StatusEvent.FireStatusInfo(this, statusEvent, aFile.Count.ToString() + " items");
            }
        }

        public void ResetStatus()
        {
            StatusEvent.FireStatusInfo(this, statusEvent, "");
        }

        public bool ReloadFile()
        {
            if (!this.Db.IsDefined) return false;
            if (this.Db.InTransaction)
            {
                MessageBox.Show(this.Form, Globalisation.GetString("DB_Intransaction"));
                return false;
            }

            StatusEvent.FireStatusInfo(this, statusEvent, Globalisation.GetString("Loading"));
            this.Db.ReloadFile();
            return true;
        }

        /// <summary>
        /// Recharghe les tag vidéos
        /// </summary>
        /// <returns></returns>
        public bool ReloadVideoFile()
        {
            if (!this.Db.IsDefined) return false;
            if (this.Db.InTransaction)
            {
                MessageBox.Show(this.Form, Globalisation.GetString("DB_Intransaction"));
                return false;
            }

            StatusEvent.FireStatusInfo(this, statusEvent, Globalisation.GetString("Loading"));
            this.Db.ReloadVideoFile();
            return true;
        }

        /// <summary>
        /// Recharge les tag spécific d'une fichier
        /// </summary>
        public void ReloadSpecificFile()
        {
            if (this.ListView.SelectedItems.Count > 0)
            {
                int fileId;
                Hashtable aFile;
                foreach(ListViewItem item in this.ListView.SelectedItems) {
                    fileId = item.Tag.ToString().ToInt();
                    aFile = this.Db.getFile(fileId);

                    // load extension tag
                    this.Db.AddExtensionTag(aFile["path"].ToString(), fileId);

                    // Load vidéos tag
                    this.Db.ReloadSpecificFile(aFile["id"].ToString().ToInt());

                }
            }
        }

        /*
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        SetForegroundWindow(process.MainWindowHandle);
        */

        public void OpenItem(int fileId)
        {
            // TODO : reccup dans bdd des files
            Hashtable aFile = this.Db.getFile(fileId);
            if (aFile != null)
            {
                string file = aFile["path"].ToString();
                if (File.Exists(file))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = file;
                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();
                }
            }
        }

        public void OpenFolder()
        {
            if (this.ListView.SelectedItems.Count > 0) 
            {
                int fileId = this.ListView.SelectedItems[0].Tag.ToString().ToInt();
                Hashtable aFile = this.Db.getFile(fileId);
                string filePath = aFile["path"].ToString();
                if (!File.Exists(filePath))
                {
                    return;
                }
                // combine the arguments together
                // it doesn't matter if there is a space after ','
                string argument = "/select, \"" + filePath + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        public void SetTagJoin(string jointype) {
            MyDatabase.TAG_JOIN = jointype;
        }
    }
}
