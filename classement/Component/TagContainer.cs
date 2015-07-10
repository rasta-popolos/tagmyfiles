using TagMyFiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyUtils;
using DatabaseManager;

namespace TagMyFiles.Component
{
    class TagContainer
    {

        public event StatusEventHandler statusEvent;
        ListView ListView;
        GridFile GridFile;

        MyDatabase Db;

        #region Contruct

        public TagContainer(Hashtable options)
        {
            StatusEvent.FireStatusError(this, statusEvent, Globalisation.GetString(""));
            if (options.ContainsKey("Db"))
            {
                this.Db = (MyDatabase)options["Db"];
            }

            if (options.ContainsKey("listview"))
            {
                this.ListView = (ListView)options["listview"];
                this.ListView.View = System.Windows.Forms.View.LargeIcon;
                this.ListView.MouseDown += new MouseEventHandler(mouseDown);
            }

            if (options.ContainsKey("GridFile"))
            {
                this.GridFile = (GridFile)options["GridFile"];
            }
            
        }

        #endregion

        public List<string> GetTags()
        {
            List<String> aTags = new List<String>();

            foreach (ListViewItem lvi in this.ListView.Items)
            {
                aTags.Add(lvi.Tag.ToString());
            }
            return aTags;
        }

        /**
         * Ajoute un tag dans las liste
         * 
         * */
        public void AddTag(TreeNode node, int groupId)
        {
            //Check si le tag est déjà selectionné
            // => remove le tag
            List<String> aTags = this.GetTags();
            if (aTags.Contains(groupId.ToString()))
            {
                foreach (ListViewItem lvi in this.ListView.Items)
                {
                    if (lvi.Tag.ToString().ToInt() == groupId)
                    {
                        this.ListView.Items.Remove(lvi);
                        break;
                    }
                }
            }
            else
            {
                Hashtable group = this.Db.GetGroup(groupId);

                this.ListView.SmallImageList = node.TreeView.ImageList;
                this.ListView.LargeImageList = node.TreeView.ImageList;

                ListViewItem newItem = this.ListView.Items.Add(new ListViewItem(new string[] {
                    Globalisation.GetString(group["name"].ToString())
                }));
                newItem.ImageIndex = (node.ImageIndex == -1) ? 0 : node.ImageIndex;
                newItem.Tag = group["id"].ToString();
            }
            this.ReloadGrid();
        }

        public void ReloadGrid()
        {
            List<String> aTags = this.GetTags();

            Hashtable aFilters = new Hashtable();

            //aFilters["filetype"] = this.center_toolStrip_FileTypeComboBox.ComboBox.SelectedValue.ToString().ToInt(); // int
            aFilters["tags"] = this.GetTags(); // List<String>
            aFilters["textfilter"] = ""; // text

            this.GridFile.Load(aFilters);
        }

        public void Clear()
        {
            this.ListView.Items.Clear();
            this.ReloadGrid();
        }
        
        /**
         * Remove itme on dblclick
         * 
         * */
        private void mouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.ListView.GetItemAt(e.X, e.Y) != null)
            {
                if (e.Clicks == 1)
                {
                    
                }
                else if (e.Clicks == 2)
                {
                    ListViewItem item = this.ListView.GetItemAt(e.X, e.Y);
                    this.ListView.Items.Remove(item);

                    this.ReloadGrid();
                }
            }
        }

    }
}
