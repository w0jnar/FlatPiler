using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class SymbolTable
    {
        private Node astRoot;
        private TextBox taOutput;
        private List<ScopeNode> scopes = new List<ScopeNode>();
        private int currentScope = -1;
        public int errorCount = 0;

        public SymbolTable(Node astRoot, TextBox taOutput)
        {
            this.astRoot = astRoot;
            this.taOutput = taOutput;
        }

        public void generateSymbolTable()
        {
            print("");
            print("~~~Starting To Generate Symbol Table.");
            // print(this.astRoot.name);
            // print(this.astRoot.children[0].name);
            generateBlock(this.astRoot);
            print("");
            if (this.errorCount == 0)
            {
                print("~~~Generation of Symbol Table finished successfully. Nice!");
            }
            else
            {
                print("~~~Generation of Symbol Table fail. Check errors.");
            }
        }

        private void generateBlock(Node root)
        {
            if (this.currentScope != -1)
            {
                this.currentScope = this.scopes[this.currentScope].scopeId;
            }
            this.scopes.Add(new ScopeNode(this.scopes.Count, this.currentScope));
            this.currentScope = this.scopes[this.scopes.Count - 1].scopeId;
            print("~~~New Scope " + this.currentScope + " opened.");
            for (int i = 0; i < root.children.Count; i++)
            {
                if (root.children[i].name == "Statement List")
                {
                    generateBlock(root.children[i]);
                }
            }

            if (this.errorCount == 0)
            {
                print("~~~Closing scope " + this.currentScope + ".");
                print("~~~Scope contained:");
                for (int i = 0; i < this.scopes[this.currentScope].scopeMembers.Count; i++)
                {
                    print(this.scopes[this.currentScope].scopeMembers[i]);
                }
                this.currentScope = this.scopes[this.currentScope].parentId;
            }
            else
            {
                print("~~~Error: There was a Symbol Table generation error.");
            }
        }

        private void print(Object message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }
    }

    class ScopeNode
    {
        public int scopeId;
        public int parentId;
        public List<ScopeElement> scopeMembers = new List<ScopeElement>();

        public ScopeNode(int scopeId, int parentId)
        {
            this.scopeId = scopeId;
            this.parentId = parentId;
        }

        public void addChildScope(ScopeElement childScope)
        {
            this.scopeMembers.Add(childScope);
        }

    }
    class ScopeElement
    {
        public string type;
        public char id;
        public Boolean isInitialized = false;
        public Boolean isUsed = false;
        public Object value;

        public ScopeElement(string type, char id)
        {
            this.type = type;
            this.id = id;
        }

        public override string ToString()
        {
            return "id: " + this.id + ", type: " + this.type + ", value: " + this.value;
        }
    }
}
