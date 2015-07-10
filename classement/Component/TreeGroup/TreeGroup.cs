using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagMyFiles;
using System.IO;
using MyUtils;
using DatabaseManager;

namespace TagMyFiles.Component
{
    partial class TreeGroup
    {
        public TreeView Tree;
        MyDatabase Db;
        ContextMenuStrip ContextMenu;
        ImageList ImageList = new ImageList();
        Hashtable ImageListIndex = new Hashtable();

        public event StatusEventHandler statusEvent;

        public TreeNode SelectedNode
        {
            get
            {
                return this.Tree.SelectedNode;
            }
            set
            {
                this.Tree.SelectedNode = value;
            }
        }

        #region Contruct
        
        public TreeGroup(Hashtable options)
        {
            if (options.ContainsKey("Db"))
            {
                this.Db = (MyDatabase)options["Db"];
            }
            
            if (options.ContainsKey("tree"))
            {
                this.Tree = (TreeView)options["tree"];
                this.Tree.HideSelection = false;
                this.Tree.CheckBoxes = false;
                this.Tree.AllowDrop = true;
                this.Tree.ShowNodeToolTips = true;
                //this.Tree.ShowRootLines = false;
                this.Tree.ShowLines = false;
                this.Tree.ShowPlusMinus = true;
                //Enable custom drawing
                //this.Tree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
                //this.Tree.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView_DrawNode);

                // event
                this.Tree.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.left_treeview_AfterLabelEdit);
                this.Tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.left_treeview_AfterSelect);
                this.Tree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.left_treeview_MouseDown);

                // Drag & drop
                this.Tree.DragEnter += new DragEventHandler(dragEnter);
                this.Tree.DragDrop += new DragEventHandler(dragDrop);
                this.Tree.DragOver += new DragEventHandler(treeView_DragOver);
                 
                this.Tree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
            }

            if(options.ContainsKey("contextmenu"))
                this.setContextMenu((ContextMenuStrip) options["contextmenu"]);

            if (options.ContainsKey("autoLoad") && (bool)options["autoLoad"])
            {
                this.Load();
            }
        }

        #endregion

        #region event

        /**
         * Stock le node selectionné (onmousedown et afterselect
        */
        private void left_treeview_MouseDown(object sender, MouseEventArgs e)
        {
            this.Tree.SelectedNode = this.Tree.GetNodeAt(e.X, e.Y);
        }
        private void left_treeview_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.Tree.SelectedNode = e.Node;
        }
        private void left_treeview_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            this.ValidateEdit(e);
        }

        #endregion


        #region Drag & drop
        // DD sur treenode => treenode
        private void treeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            this.Tree.DoDragDrop(e.Item, DragDropEffects.Move);
        }
        
        // DD avec Grid => Treenode
        private void dragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        /*
         * Scroll Tree where drag limit
         * Open node where drag Over
         * */
        TreeNode lastDragDestination = null;
        DateTime lastDragDestinationTime;
        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            this.Tree.Scroll();

            // Retrieve the client coordinates of the drop location.
            Point targetPoint = this.Tree.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode currentNodeOver = this.Tree.GetNodeAt(targetPoint);
            if (currentNodeOver == null) return;

            this.Tree.SelectedNode = currentNodeOver;

            // Open Node ON dragOver
            if (currentNodeOver != this.lastDragDestination)
            {
                this.lastDragDestination = currentNodeOver;
                this.lastDragDestinationTime = DateTime.Now;
            }
            else
            {
                TimeSpan hoverTime = DateTime.Now.Subtract(this.lastDragDestinationTime);
                if (hoverTime.TotalSeconds > 1)
                {
                    currentNodeOver.Expand();
                }
            }

        }
        private void dragDrop(object sender, DragEventArgs e)
        {

            // Retrieve the client coordinates of the drop location.
            Point targetPoint = this.Tree.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = this.Tree.GetNodeAt(targetPoint);
            if (targetNode == null) return;

            this.Tree.SelectedNode = targetNode;

            // Reccup les id des items drag
            List<int> aFileId = new List<int>();

                // plusieurs fichier
            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))
            {
                ListView.SelectedListViewItemCollection items = (ListView.SelectedListViewItemCollection)e.Data.GetData(typeof(ListView.SelectedListViewItemCollection));
                foreach (ListViewItem item in items)
                {
                    aFileId.Add(item.Tag.ToString().ToInt());
                }
            }
                // fichier unique
            else if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                ListViewItem item = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
                aFileId.Add(item.Tag.ToString().ToInt());
            }
                // tag node
            else if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                //TreeNode
                TreeNode destinationNode = ((TreeView)sender).GetNodeAt(targetPoint);
                TreeNode dragedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if (destinationNode.Name != dragedNode.Name)
                {
                    if (this.Db.moveGroup(destinationNode.Name.ToString().ToInt(), dragedNode.Name.ToString().ToInt()))
                    {
                        dragedNode.Remove();
                        this.Load();
                    }
                }
            }

            // DD grid vers tree => ajout des tags file
            if (aFileId.Count > 0)
            {
                this.Db.addGroupFiles(targetNode.Name.ToString().ToInt(), aFileId);
            }
            return;
        }

        #endregion

        /**
         * Add context menu
        */
        public void setContextMenu(ContextMenuStrip ctxMenu)
        {
            this.ContextMenu = ctxMenu;
        }

        /**
         * Load Group data
        */
        public void Load(bool clearAll=false)
        {
            if(! this.Db.IsDefined) return;

            // reccup des icon
            this.ImageList.Images.Clear();
            List<string> aIcon = this.Db.GetAllTags();
            string file;
            int index = 1;
            // Add default icon
            this.ImageList.Images.Add(Image.FromFile(@"icon\default.png"));
            foreach (string tag in aIcon)
            {
                file = @"icon\" + tag + ".png";
                if (File.Exists(file))
                {
                    this.ImageList.Images.Add(Image.FromFile(file));
                    this.ImageListIndex[tag] = index;
                    index++;
                }
            }
            this.Tree.ImageList = this.ImageList;

            if (clearAll)
            {
                // remove all nodes
                this.Tree.Nodes.Clear();
            }
            TreeNode currNode;

            #region Reccup le noeud selectionné ou le root sinon
            if (this.SelectedNode != null)
            {
                currNode = this.SelectedNode;
            }
            else
            {
                currNode = null;
            }
            #endregion

            this.LoadChildNode(currNode);

            if (currNode != null)
            {
                //currNode.ExpandAll();
            }
        }

        protected void LoadChildNode(TreeNode currNode)
        {
            TreeNodeCollection nodes;
            int parentId;
            if (currNode == null)
            {
                // Root node
                nodes = this.Tree.Nodes;
                parentId = 0;
            }
            else
            {
                nodes = currNode.Nodes;
                parentId = int.Parse(currNode.Name);
            }

            // remove all nodes before load
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                nodes.RemoveAt(i);
            }

            List<Hashtable> aGroup = this.Db.GetChildGroup(parentId);

            TreeNode node;
            string nodeText;
            foreach (Hashtable row in aGroup)
            {
                node = new TreeNode();
                node.Name = row["id"].ToString();
                nodeText = Globalisation.GetString(row["name"].ToString());

                // nombre de fichier du tag
                if(row["numfile"].ToString().ToInt() > 0) {
                    nodeText += " (" + row["numfile"].ToString() + ")";
                }

                node.Text = nodeText;
                node.Tag = "root/" + row["id"].ToString();
                if (row["description"].ToString() != "")
                {
                    node.ToolTipText = row["description"].ToString();
                }

                node.ContextMenuStrip = this.ContextMenu;
                //icon
                string tag = row["tag"].ToString();
                if (tag != "" && this.ImageListIndex.ContainsKey(tag))
                {
                    node.ImageIndex = this.ImageListIndex[tag].ToString().ToInt();
                    node.SelectedImageIndex = this.ImageListIndex[tag].ToString().ToInt();
                }
                this.LoadChildNode(node);
                //node.Expand();

                nodes.Add(node);
            }
        }

        /**
         * Ajoute en group
        */
        public bool addGroup(string group)
        {
            int prtId;
            if (this.SelectedNode != null)
            {
                prtId = int.Parse(this.SelectedNode.Name);
            }
            else
            {
                prtId = 0;
            }
            return this.Db.addGroup(group, prtId);
        }

        /**
         * Edit Group Controller
         * 
        */
        public void StartEdit() 
        {
            if (this.SelectedNode != null)
            {
                this.Tree.LabelEdit = true;
                if (!this.SelectedNode.IsEditing)
                {
                    this.SelectedNode.BeginEdit();
                }
            }
        }
        public void ValidateEdit(NodeLabelEditEventArgs eNode)
        {
            if (eNode.Label != null)
            {
                if (eNode.Label.Length > 0)
                {
                    if (this.Db.renameGroup(int.Parse(eNode.Node.Name), eNode.Label))
                    {
                        // Stop editing without canceling the label change.
                        eNode.Node.EndEdit(false);
                    }
                    else
                    {
                        StatusEvent.FireStatusError(this, statusEvent, Globalisation.GetString("Group_exist"));

                        /* Cancel the label edit action, inform the user, and 
                           place the node in edit mode again. */
                        eNode.CancelEdit = true;
                        eNode.Node.BeginEdit();
                    }
                }
                else
                {
                    StatusEvent.FireStatusError(this, statusEvent, Globalisation.GetString("Empty_group"));

                    /* Cancel the label edit action,
                       place the node in edit mode again.
                     */
                    eNode.CancelEdit = true;
                    eNode.Node.BeginEdit();
                }
            }
        }

        public bool deleteGroup()
        {
            if (this.SelectedNode != null)
            {
                if (MessageBox.Show(Globalisation.GetString("Prompt_delete_group"), Globalisation.GetString("Delete_group"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (this.Db.deleteGroup(int.Parse(this.SelectedNode.Name)))
                    {
                        // select parent node
                        if (this.SelectedNode.Parent != null)
                        {
                            this.SelectedNode = this.SelectedNode.Parent;
                        }
                        this.Load();
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            StatusEvent.FireStatusError(this, statusEvent, Globalisation.GetString("Delete_error"));
            return false;
        }

        public void getCheckedNodes()
        {
            /*
            if (this.Tree.CheckedNodes.Count > 0)
            {
                foreach (TreeNode node in this.Tree.CheckedNodes)
                {
                    //The main parent node does not have a parent
                    if (node.Parent != null)
                    {
                        string checkedValue = node.Text.ToString();
                        activityData = new ActivityData { ActivityName = checkedValue };
                        listActivity.Add(activityData);
                    }
                }
            }
            */
        }

        public List<String> getCheckedName()
        {
            List<String> aTags = this.Tree.CheckedNames(this.Tree.Nodes);
            return aTags;
        }

    }
}