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

        // Slight adaptation from http://stackoverflow.com/questions/1649027/how-do-i-print-out-a-tree-structure
        public void PrintPretty(string indent, bool last, TextBox ta)
        {
            ta.Text += indent;
            if (last)
            {
                ta.Text += "\\-";
                indent += "  ";
            }
            else
            {
                ta.Text += "|-";
                indent += "| ";
            }
            ta.Text += (this.name + Environment.NewLine);

            for (int i = 0; i < this.children.Count; i++)
                this.children[i].PrintPretty(indent, i == this.children.Count - 1, ta);
        }

    }
}
