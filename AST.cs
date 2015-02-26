using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class AST
    {
        public ArrayList tokens;
        private TextBox taOutput;
        public Node root;
        private int tokenIndex = 0;

        public AST(ArrayList tokens, TextBox taOutput)
        {
            this.tokens = new ArrayList(tokens);
            this.taOutput = taOutput;
        }

        public void buildAST()
        {
            print("");
            print("~~~Starting To Build AST.");

            this.root = new Node("Statement List");
            // buildBlockTree(this.root);

            print("~~~Ending AST Building." + Environment.NewLine + Environment.NewLine);
            // this.root.PrintPretty("", true, taOutput);
        }

        private void buildBlockTree(Node root)
        {
            // Left brace.
            this.tokenIndex++;

            buildStatementListTree(root);

            // Right brace.
            this.tokenIndex++;
        }

        private void buildStatementListTree(Node root)
        {
            Token currentToken = (Token)this.tokens[this.tokenIndex];
            if (!currentToken.match("right_brace"))
            {
                buildStatementTree(root);
                currentToken = (Token)this.tokens[this.tokenIndex];
            }
            while (!currentToken.match("right_brace"))
            {
                buildStatementTree(root);
                currentToken = (Token)this.tokens[this.tokenIndex];
            }
        }

        private void buildStatementTree(Node root) 
        {
            Token currentToken = (Token)this.tokens[this.tokenIndex];
            if (currentToken.match("print")) 
            {

            }
        }

        private void print(string message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }
    }
}
