using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class CodeGenerator : CompilerElement
    {
        private Node astRoot;
        private int errorCount = 0;
        private int index = 0;
        private int currentScope = 0;
        private int currentScopeId = 0;

        public CodeGenerator(Node astRoot, TextBox taOutput)
            : base(taOutput)
        {
            this.astRoot = astRoot;
        }

        public void generateCode()
        {
            for (int i = 0; i < astRoot.children.Count; i++)
            {
                generateFromNode(astRoot.children[i]);
            }
        }

        private void generateFromNode(Node currentNode)
        {
            string currentNodeName = currentNode.name;
            if (currentNodeName == "Print")
            {
            }
            else if (currentNodeName == "Assignment")
            {
            }
            else if (currentNodeName == "Variable Declaration")
            {
            }
            else if (currentNodeName == "While")
            {
            }
            else if (currentNodeName == "If")
            {
            }
            else if (currentNodeName == "Statement List")
            {
            }
        }
    }
}
