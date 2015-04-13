using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class AST
    {
        public List<Token> tokens;
        private TextBox taOutput;
        public Node root;
        private int tokenIndex = 0;

        public AST(List<Token> tokens, TextBox taOutput)
        {
            this.tokens = tokens;
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
            Token currentToken = this.tokens[this.tokenIndex];
            if (!currentToken.match("right_brace"))
            {
                buildStatementTree(root);
                currentToken = this.tokens[this.tokenIndex];
            }
            while (!currentToken.match("right_brace"))
            {
                buildStatementTree(root);
                currentToken = this.tokens[this.tokenIndex];
            }
        }

        private void buildStatementTree(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex];
            if (currentToken.match("print"))
            {
                print("--Building Print Node.");
                Node printNode = new Node("Print");
                buildPrintStatementTree(printNode);
                root.addChild(printNode);
            }
            else if (currentToken.match("var_id"))
            {
                print("--Building Assignment Node.");
                Node assignmentNode = new Node("Assignment");
                buildAssignmentStatementTree(assignmentNode);
                root.addChild(assignmentNode);
            }
            else if (currentToken.match("int") || currentToken.match("string") || currentToken.match("boolean")) 
            {
                print("--Building Variable Declaration Node.");
                Node varDeclNode = new Node("Variable Declaration");
                buildVarDeclTree(varDeclNode);
                root.addChild(varDeclNode);
            }
            else if (currentToken.match("while"))
            {
                print("--Building While Node.");
                Node whileNode = new Node("While");
                // Skip while token.
                this.tokenIndex++;
                buildBoolExprTree(whileNode);

                Node blockNode = new Node("Statement List");
                buildBlockTree(blockNode);
                whileNode.addChild(blockNode);

                root.addChild(whileNode);
            }
            else if (currentToken.match("if"))
            {
                print("--Building If Node.");
                Node ifNode = new Node("If");
                // Skip if token.
                this.tokenIndex++;
                buildBoolExprTree(ifNode);

                Node blockNode = new Node("Statement List");
                buildBlockTree(blockNode);
                ifNode.addChild(blockNode);

                root.addChild(ifNode);
            }
            else if (currentToken.match("left_brace"))
            {
                print("--Building Block Node.");
                Node blockNode = new Node("Statement List");
                buildBlockTree(blockNode);
                root.addChild(blockNode);
            }
        }

        private void buildPrintStatementTree(Node root)
        {
            // Move past print.
            this.tokenIndex++;
            // Skip left paren.
            this.tokenIndex++;
            buildExprTree(root);
            // Skip right paren.
            this.tokenIndex++;
        }

        private void buildAssignmentStatementTree(Node root)
        {
            Node assignmentOpNode = new Node("=");
            buildEndNode(assignmentOpNode);

            // Skip assignment Op.
            this.tokenIndex++;

            buildExprTree(assignmentOpNode);
            root.addChild(assignmentOpNode);
        }

        private void buildVarDeclTree(Node root)
        {
            // Type.
            buildEndNode(root);
            // Variable.
            buildEndNode(root);
        }

        private void buildExprTree(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex];
            if (currentToken.match("digit"))
            {
                buildIntExprTree(root);
            }
            else if (currentToken.match("var_id"))
            {
                buildEndNode(root);
            }
            // This does not need to be its own case, it was more for a logic perspective. 
            else if (currentToken.match("string"))
            {
                buildEndNode(root);
            }
            else if (currentToken.match("left_paren") || currentToken.match("true") || currentToken.match("false"))
            {
                buildBoolExprTree(root);
            }
        }

        private void buildIntExprTree(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex++];
            Node digitNode = new Node(currentToken.value);
            Token nextToken = this.tokens[this.tokenIndex];
            if (nextToken.match("plus_op"))
            {
                Node plusOpNode = new Node("+");
                plusOpNode.addChild(digitNode);

                this.tokenIndex++;
                buildIntExprTree(plusOpNode);
                root.addChild(plusOpNode);
            }
            else
            {
                root.addChild(digitNode);
            }
        }

        private void buildBoolExprTree(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex];
            if(currentToken.match("left_paren")) 
            {
                // Skip left paren.
                this.tokenIndex++;
                // Temporary Node to hold the value of the left Expr.
                Node tempNode = new Node("");

                buildExprTree(tempNode);
                // Console.Write(tempNode.children[0].name);

                Token boolOpToken = this.tokens[this.tokenIndex++];
                if (boolOpToken.match("boolop_equal"))
                {
                    print("--Building Boolean Operator Equal Node");
                }
                else
                {
                    print("--Building Boolean Operator Not Equal Node");
                }
                Node boolOpNode = new Node(boolOpToken.value);

                boolOpNode.addChild(tempNode.children[0]);

                buildExprTree(boolOpNode);

                root.addChild(boolOpNode);

                // Skip right paren.
                this.tokenIndex++;
            }
            else 
            {
                buildEndNode(root);
            }
        }

        private void buildEndNode(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex++];
            Node valueNode = new Node(currentToken.value);
            root.addChild(valueNode);
        }

        private void print(Object message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }
    }
}
