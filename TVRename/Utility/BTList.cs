using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TVRename
{
    public class BTList : BTItem
    {
        public readonly List<BTItem> Items;

        public BTList()
            : base(BTChunk.kList)
        {
            Items = new List<BTItem>();
        }

        public override string AsText()
        {
            return "List={" + Items.Select(x => x.AsText()).ToCsv() + "}";
        }

        public override void Tree(TreeNodeCollection tn)
        {
            TreeNode n = new TreeNode("List");
            tn.Add(n);
            foreach (BTItem t in Items)
                t.Tree(n.Nodes);
        }

        public override void Write(System.IO.Stream sw)
        {
            sw.WriteByte((byte) 'l');
            foreach (BTItem i in Items)
                i.Write(sw);

            sw.WriteByte((byte) 'e');
        }
    }
}