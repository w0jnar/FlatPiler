using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class CST
    {
        public ArrayList tokens;
        private TextBox taOutput;
        public Node root;
        private int tokenIndex = 0;

        public CST(ArrayList tokens, TextBox taOutput)
        {
            this.tokens = new ArrayList(tokens);
            this.taOutput = taOutput;
        }

        public void buildCST()
        {
            print("");
            print("~~~Starting To Build CST.");
            print("--Building Program Node.");
            this.root = new Node("Program");
            buildProgramTree(this.root);
            // print(this.root.children[0].name);
            print("~~~Ending CST Building.");
            this.root.PrintPretty("", true, taOutput);
        }

        private void buildProgramTree(Node root)
        {
            print("--Building Block Node.");
            Node blockNode = new Node("Block");
            buildBlockTree(blockNode);
            root.addChild(blockNode);

            print("--Building End of File Node.");
            Node eofNode = new Node("$");
            root.addChild(eofNode);
            tokenIndex++;
        }

        private void buildBlockTree(Node root)
        {
            print("--Building Left Bracket Node.");
            Node leftBraceNode = new Node("{");
            root.addChild(leftBraceNode);
            tokenIndex++;

            Node statementListNode = new Node("StatementList");
            buildStatementListTree(statementListNode);
            root.addChild(statementListNode);

            print("--Building Right Bracket Node.");
            Node rightBraceNode = new Node("}");
            root.addChild(rightBraceNode);
            tokenIndex++;
        }

        private void buildStatementListTree(Node root)
        {
            print("--Building StatementList Node.");
            Token currentToken = (Token)this.tokens[this.tokenIndex];
            if (!currentToken.type.Equals("right_brace"))
            {
                Node statementNode = new Node("Statement");
                buildStatementTree(statementNode);
                root.addChild(statementNode);

                Node statementListNode = new Node("StatementList");
                buildStatementListTree(statementListNode);
                root.addChild(statementListNode);
            }
            else 
            { 
                Node epsilonNode = new Node("ε");
                root.addChild(epsilonNode);
            }
        }

        private void buildStatementTree(Node root) 
        {
            print("--Building Statement Node.");
            Token currentToken = (Token)this.tokens[this.tokenIndex];
        }

        private void print(string message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }
    }
}
