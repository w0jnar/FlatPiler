using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class Node
    {
        public string name;
        public List<Node> children = new List<Node>();

        public Node(string name)
        {
            this.name = name;
        }

        public void addChild(Node child)
        {
            this.children.Add(child);
        }

        // Adaptation of http://stackoverflow.com/questions/1649027/how-do-i-print-out-a-tree-structure
        public string PrintPretty(string indent, bool last, string input)
        {
            input += indent;
            if (last)
            {
                input += "\\:";
                indent += "  ";
            }
            else
            {
                input += "|:";
                indent += "| ";
            }
            input += (this.name + Environment.NewLine);

            for (int i = 0; i < this.children.Count; i++)
            {
                input += this.children[i].PrintPretty(indent, i == this.children.Count - 1, "");
            }
            return input;
        }

    }
}
