using TagMyFiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager;
using MyUtils;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TagMyFiles.Component.FileInfo
{
    class FileTag
    {

        public event StatusEventHandler statusEvent;
        GridFile GridFile;
        ListView ListView;
        ListView GridFileListView;
        Form1 Form;
        RichTextBox Infopanel;

        MyDatabase Db;

        #region Contruct

        public FileTag(Hashtable options)
        {
            StatusEvent.FireStatusError(this, statusEvent, Globalisation.GetString(""));
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
                this.ListView = (ListView)options["listview"];
                this.ListView.MouseDown += new MouseEventHandler(mouseDown_ListView);

                this.ListView.OwnerDraw = true;
                this.ListView.DrawItem += new DrawListViewItemEventHandler(ListView_DrawItem);
            }

            if (options.ContainsKey("infopanel"))
            {
                this.Infopanel = (RichTextBox)options["infopanel"];
            }            

            if (options.ContainsKey("GridFile"))
            {
                this.GridFile = (GridFile)options["GridFile"];
                this.GridFileListView = this.GridFile.GetListView();
                //this.GridFileListView.MouseDown += new MouseEventHandler(mouseDown_LoadTag);
                this.GridFileListView.SelectedIndexChanged += new System.EventHandler(this.center_listview_SelectedIndexChanged);
            }
            
        }

        #endregion


        /// <summary>
        /// Draws the backgrounds for entire ListView items. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {

            if ((e.State & ListViewItemStates.Selected) != 0)
            {
                // Draw the background and focus rectangle for a selected item.
                e.Graphics.FillRectangle(Brushes.Aqua, e.Bounds);
                e.DrawFocusRectangle();
            }
            else
            {
                // Draw the background for an unselected item. 
                using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, Color.White, Color.Aqua, LinearGradientMode.BackwardDiagonal))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            // Draw the item text for views other than the Details view. 
            //if (listView1.View != View.Details)
            //{
                e.DrawText();
            //}
        }

        /// <summary>
        /// Affiche les info du fichier au click du grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseDown_LoadTag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.GridFileListView.GetItemAt(e.X, e.Y) != null)
            {
                if (e.Clicks == 1)
                {
                    ListViewItem lvi = this.GridFileListView.GetItemAt(e.X, e.Y);
                    // si l'item n'est pas selectionné => drag single item
                    if (lvi != null)
                    {
                        int fileId = lvi.Tag.ToString().ToInt();
                        Task task = new Task(() => this.GetFileInfo(fileId));
                        task.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Affiche les info du fichier au changement de la sélection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void center_listview_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ListView.Items.Clear();

            if (this.GridFileListView.SelectedItems.Count > 0)
            {
                ListViewItem lvi = this.GridFileListView.SelectedItems[0];
                // si l'item n'est pas selectionné => drag single item
                if (lvi != null)
                {
                    int fileId = lvi.Tag.ToString().ToInt();
                    Task task = new Task(() => this.GetFileInfo(fileId));
                    task.Start();
                }
            }
        }

        /// <summary>
        /// Appel le delegate du Form qui appelle à son tour LoadTagWithList
        /// </summary>
        /// <param name="fileId"></param>
        protected void GetFileInfo(int fileId)
        {
            List < Hashtable >  aTags = this.Db.GetGroupFile(fileId);
            Hashtable aInfo = new Hashtable();
            Hashtable file = this.Db.getFile(fileId);
            if (file.Count > 0)
            {
                MyVideos.Ffmpeg.VIDEO_EXTENSIONS = this.Db.GetMainExtensions("Videos").Union(this.Db.GetMainExtensions("Images")).Union(this.Db.GetMainExtensions("Audios")).ToList();
                aInfo = MyVideos.Ffmpeg.getVideoInformation(file["path"].ToString());
            }
            this.Form.Invoke(this.Form.DelegateFileTag, aTags, aInfo);
            //this.LoadTagWithList(res);
        }


        /// <summary>
        /// Affiche les infos du fichier dans la listeview
        /// </summary>
        /// <param name="tags"></param>
        public void LoadTagWithList(List<Hashtable> aTags, Hashtable aInfo)
        {
            ListViewItem newItem;
            foreach (Hashtable tag in aTags) 
            {
                newItem = this.ListView.Items.Add(new ListViewItem(new string[] {
                    Globalisation.GetString(tag["name"].ToString())
                }));
                newItem.Tag = tag["id"].ToString() + "/" + tag["files_id"].ToString();
            }

            List<string> aText = new List<string>();
            if (aInfo.Contains("width") && aInfo.Contains("height"))
            {
                aInfo["resolution"] = aInfo["width"].ToString() + " x " + aInfo["height"].ToString();
            }

            string val;
            string key;
            foreach (DictionaryEntry pair in aInfo)
            {
                key = pair.Key.ToString();
                val = pair.Value.ToString();
                if (val != string.Empty && val != "N/A")
                {
                    if (pair.Key.ToString() == "format_duration")
                    {
                        val = float.Parse(pair.Value.ToString()).FormatSecond();
                    }
                    aText.Add(key.RegReplace("^format_", "").RegReplace("^tags_", "").UcFirst() + " : " + val);
                }
            }

            aText.Sort();
            this.Infopanel.Text = String.Join("\n", aText);
        }

        /// <summary>
        /// Liste view : Remove tag on double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseDown_ListView(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.ListView.GetItemAt(e.X, e.Y) != null)
            {
                if (e.Clicks == 1)
                {

                }
                else if (e.Clicks == 2)
                {
                    ListViewItem item = this.ListView.SelectedItems[0];

                    List<string> tag = item.Tag.ToString().RegSplit("/");
                    int groupId = tag[0].ToInt();
                    int fileId = tag[1].ToInt();
                    if (this.Db.RemoveGroupFile(groupId, fileId))
                    {
                        this.ListView.Items.Remove(item);
                    }
                    //this.ReloadGrid();
                }
            }
        }
    }
}
