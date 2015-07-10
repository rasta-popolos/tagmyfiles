using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TagMyFiles
{
    public partial class Utils
    {

        public static List<String> CheckedNames(this TreeView tree, TreeNodeCollection theNodes)
        {
            List<String> aResult = new List<String>();

            if (theNodes != null)
            {
                foreach (System.Windows.Forms.TreeNode aNode in theNodes)
                {
                    if (aNode.Checked)
                    {
                        aResult.Add(aNode.Name);
                    }

                    aResult.AddRange(CheckedNames(tree, aNode.Nodes));
                }
            }
            return aResult;
        }
    }
}
