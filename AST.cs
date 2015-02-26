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
            buildBlockTree(this.root);

            print("~~~Ending AST Building." + Environment.NewLine + Environment.NewLine);
            this.root.PrintPretty("", true, taOutput);
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
                // Console.Write(currentToken.value);
            }
            while (!currentToken.match("right_brace"))
            {
                buildStatementTree(root);
                currentToken = (Token)this.tokens[this.tokenIndex];
                // Console.Write(currentToken.value);
            }
        }

        private void buildStatementTree(Node root)
        {
            Token currentToken = (Token)this.tokens[this.tokenIndex];
            // Console.Write(((Token)this.tokens[this.tokenIndex]).value);
            if (currentToken.match("print"))
            {
                print("--Building Print Node.");
                Node printNode = new Node("Print");
                buildPrintStatementTree(printNode);
                // Console.Write(((Token)this.tokens[this.tokenIndex]).value);
            }
        }

        private void buildPrintStatementTree(Node root)
        {
            // Move past print.
            this.tokenIndex++;
            // Skip left paren.
            this.tokenIndex++;
            // Console.Write(((Token)this.tokens[this.tokenIndex]).value);
            buildExprTree(root);
            // Skip right paren.
            this.tokenIndex++;
        }

        private void buildExprTree(Node root)
        {
            Token currentToken = (Token)this.tokens[this.tokenIndex];
            if (currentToken.match("digit"))
            {
                buildIntExprTree(root);
            }
        }

        private void buildIntExprTree(Node root)
        {
            Token currentToken = (Token)this.tokens[this.tokenIndex++];
            Node digitNode = new Node(currentToken.value);
            Token nextToken = (Token)this.tokens[this.tokenIndex];
            if (nextToken.match("plus_op"))
            {
                Node plusOpNode = new Node("+");
                plusOpNode.addChild(digitNode);

                this.tokenIndex++;
                buildIntExprTree(plusOpNode);
            }
            else
            {
                root.addChild(digitNode);
            }
        }

        private void buildEndNode(Node root)
        {
            Token currentToken = (Token)this.tokens[this.tokenIndex++];
            Node digitValueNode = new Node(currentToken.value);
            root.addChild(digitValueNode);
        }

        private void print(string message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }
    }
}
