using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace TagMyFiles.Component
{
    partial class TreeGroup
    {
        
        private readonly Size CHECK_BOX_SIZE = new Size(13, 13);
        private readonly Size IMAGE_SIZE = new Size(16, 16);
        private void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            //Get the backcolor and forecolor
            Color backColor, foreColor;

            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                backColor = SystemColors.Highlight;
                foreColor = SystemColors.HighlightText;
            }

            else if ((e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot)
            {
                backColor = SystemColors.HotTrack;
                foreColor = SystemColors.HighlightText;
            }
            else
            {
                backColor = e.Node.BackColor;
                foreColor = e.Node.ForeColor;
            }

            //Calculate the text rectangle.
            Rectangle textRect = e.Node.Bounds;
            textRect.Offset(new Point(-CHECK_BOX_SIZE.Width - 3));
            textRect.Offset(1, 1);
            textRect.Width -= 2;
            textRect.Height -= 2;

            //The first level nodes has images but no checkboxes.
            //The second level nodes has checkboxes but no images.
            //The other level nodes is drawn by default.
            if (e.Node.Level == 0)
            {
                try
                {
                    //Draw the background.
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, textRect);
                    }

                    //Draw the image.
                    if (e.Node.TreeView.ImageList != null && e.Node.ImageIndex >= 0
                        && e.Node.ImageIndex < e.Node.TreeView.ImageList.Images.Count)
                    {
                        Image img = this.Tree.ImageList.Images[e.Node.ImageIndex];
                        if (img != null)
                            e.Graphics.DrawImage(img, new Point(textRect.X - img.Width - 1, textRect.Y + 1));
                    }

                    //Draw the text.
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, this.Tree.Font, textRect, foreColor, backColor);
                    //Draw the focused rectangle.
                    if ((e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused)
                    {
                        ControlPaint.DrawFocusRectangle(e.Graphics, textRect, foreColor, backColor);
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.ToString());
                }

                e.DrawDefault = false;
            }
            else if (e.Node.Level == 1)
            {

                try
                {

                    //get the checked state and draw the checkbox.
                    bool isChecked = e.Node.Checked;
                    CheckBoxState state;
                    if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
                    {
                        state = isChecked ? CheckBoxState.CheckedPressed : CheckBoxState.UncheckedPressed;
                    }
                    else if ((e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot)
                    {
                        state = isChecked ? CheckBoxState.CheckedHot : CheckBoxState.UncheckedHot;
                    }
                    else
                    {
                        state = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    }
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(textRect.X - CHECK_BOX_SIZE.Width - 3, textRect.Y + 1),
                        state);

                    //Draw background
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, textRect);
                    }

                    //draw text
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, this.Tree.Font, textRect, foreColor, backColor);

                    //draw focused rectangle.
                    if ((e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused)
                    {
                        ControlPaint.DrawFocusRectangle(e.Graphics, textRect, foreColor, backColor);
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.ToString());
                }

                e.DrawDefault = false;
            }
            else
            {
                e.DrawDefault = true;
            }
        }
    }
}
