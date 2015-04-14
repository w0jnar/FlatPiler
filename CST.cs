using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class CST : CompilerElement
    {
        public List<Token> tokens;
        public Node root;
        private int tokenIndex = 0;

        public CST(List<Token> tokens, TextBox taOutput)
            : base(taOutput)
        {
            this.tokens = tokens;
        }

        public void buildCST()
        {
            buildPrintMessage("");
            buildPrintMessage("~~~Starting To Build CST.");
            buildPrintMessage("--Building Program Node.");
            this.root = new Node("Program");
            buildProgramTree(this.root);
            buildPrintMessage("~~~Ending CST Building." + Environment.NewLine + Environment.NewLine);
            buildPrintMessage(this.root.PrintPretty("", true, ""));
            print();
        }

        private void buildProgramTree(Node root)
        {
            buildPrintMessage("--Building Block Node.");
            Node blockNode = new Node("Block");
            buildBlockTree(blockNode);
            root.addChild(blockNode);

            buildPrintMessage("--Building End of File Node.");
            Node eofNode = new Node("$");
            root.addChild(eofNode);
            this.tokenIndex++;
        }

        private void buildBlockTree(Node root)
        {
            buildPrintMessage("--Building Left Bracket Node.");
            Node leftBraceNode = new Node("{");
            root.addChild(leftBraceNode);
            this.tokenIndex++;

            Node statementListNode = new Node("Statement List");
            buildStatementListTree(statementListNode);
            root.addChild(statementListNode);

            buildPrintMessage("--Building Right Bracket Node.");
            Node rightBraceNode = new Node("}");
            root.addChild(rightBraceNode);
            this.tokenIndex++;
        }

        private void buildStatementListTree(Node root)
        {
            buildPrintMessage("--Building StatementList Node.");
            Token currentToken = this.tokens[this.tokenIndex];
            if (!currentToken.match("right_brace"))
            {
                Node statementNode = new Node("Statement");
                buildStatementTree(statementNode);
                root.addChild(statementNode);

                Node statementListNode = new Node("Statement List");
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
            buildPrintMessage("--Building Statement Node.");
            Token currentToken = this.tokens[this.tokenIndex];
            if (currentToken.match("print"))
            {
                buildPrintMessage("--Building Print Statement Node.");
                Node printStatementNode = new Node("Print Statement");
                buildPrintStatementTree(printStatementNode);
                root.addChild(printStatementNode);
            }
            else if (currentToken.match("var_id"))
            {
                buildPrintMessage("--Building Assignment Statement Node.");
                Node assignmentStatementNode = new Node("Assignment Statement");
                buildAssignmentStatementTree(assignmentStatementNode);
                root.addChild(assignmentStatementNode);
            }
            else if (currentToken.match("int") || currentToken.match("string") || currentToken.match("boolean"))
            {
                buildPrintMessage("--Building Variable Declaration Statement Node.");
                Node varDeclNode = new Node("Variable Declaration Statement");
                buildVarDeclStatementTree(varDeclNode);
                root.addChild(varDeclNode);

            }
            else if (currentToken.match("while"))
            {
                buildPrintMessage("--Building While Node");
                Node whileStatementNode = new Node("While Statement");
                buildWhileIfStatementTree(whileStatementNode);
                root.addChild(whileStatementNode);
            }
            else if (currentToken.match("if"))
            {
                buildPrintMessage("--Building If Node");
                Node ifStatementNode = new Node("If Statement");
                buildWhileIfStatementTree(ifStatementNode);
                root.addChild(ifStatementNode);
            }
            else if (currentToken.match("left_brace"))
            {
                buildPrintMessage("--Building Block Node.");
                Node blockNode = new Node("Block");
                buildBlockTree(blockNode);
                root.addChild(blockNode);
            }
        }

        private void buildPrintStatementTree(Node root)
        {
            buildPrintMessage("--Building Print Node.");
            Node printNode = new Node("Print");
            root.addChild(printNode);
            this.tokenIndex++;

            buildPrintMessage("--Building Left Parenthesis Node.");
            Node leftParenNode = new Node("(");
            root.addChild(leftParenNode);
            this.tokenIndex++;

            buildPrintMessage("--Building Expr Node.");
            Node exprNode = new Node("Expr");
            buildExprTree(exprNode);
            root.addChild(exprNode);

            buildPrintMessage("--Building Right Parenthesis Node.");
            Node rightParenNode = new Node(")");
            root.addChild(rightParenNode);
            this.tokenIndex++;
        }

        private void buildAssignmentStatementTree(Node root)
        {
            buildPrintMessage("--Building Id Node.");
            Node idExprNode = new Node("Id");
            buildIdTree(idExprNode);
            root.addChild(idExprNode);

            buildPrintMessage("--Building Assignment Op Node.");
            Node assignmentOpNode = new Node("Assignment Op");
            buildEndNode(assignmentOpNode);
            root.addChild(assignmentOpNode);

            buildPrintMessage("--Building Expr Node.");
            Node exprNode = new Node("Expr");
            buildExprTree(exprNode);
            root.addChild(exprNode);
        }

        private void buildVarDeclStatementTree(Node root)
        {
            buildPrintMessage("--Building Type Node.");
            buildEndNode(root);

            buildPrintMessage("--Building Id Node.");
            Node idExprNode = new Node("Id");
            buildIdTree(idExprNode);
            root.addChild(idExprNode);
        }

        private void buildWhileIfStatementTree(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex++];
            Node whileIfNode = new Node(currentToken.value);
            root.addChild(whileIfNode);

            buildPrintMessage("--Building BooleanExpr Node.");
            Node booleanExprNode = new Node("Boolean Expr");
            buildBooleanExprTree(booleanExprNode);
            root.addChild(booleanExprNode);

            buildPrintMessage("--Building Block Node.");
            Node blockNode = new Node("Block");
            buildBlockTree(blockNode);
            root.addChild(blockNode);
        }

        private void buildExprTree(Node root)
        {
            buildPrintMessage("--Building Expr Node.");
            Token currentToken = this.tokens[this.tokenIndex];

            if (currentToken.match("digit"))
            {
                buildPrintMessage("--Building IntExpr Node.");
                Node intExprNode = new Node("Int Expr");
                buildIntExprTree(intExprNode);
                root.addChild(intExprNode);
            }
            else if (currentToken.match("string"))
            {
                buildPrintMessage("--Building String Node.");
                Node stringExprNode = new Node("String Expr");
                buildStringExprTree(stringExprNode);
                root.addChild(stringExprNode);
            }
            else if (currentToken.match("var_id"))
            {
                buildPrintMessage("--Building Id Node.");
                Node idExprNode = new Node("Id");
                buildIdTree(idExprNode);
                root.addChild(idExprNode);
            }
            else if (currentToken.match("left_paren") || currentToken.match("true") || currentToken.match("false"))
            {
                buildPrintMessage("--Building BooleanExpr Node.");
                Node booleanExprNode = new Node("Boolean Expr");
                buildBooleanExprTree(booleanExprNode);
                root.addChild(booleanExprNode);
            }
        }

        private void buildIntExprTree(Node root)
        {
            buildPrintMessage("--Building Digit Node.");
            Node digitNode = new Node("Digit");
            buildEndNode(digitNode);

            Token currentToken = this.tokens[this.tokenIndex];
            if (currentToken.match("plus_op"))
            {
                buildPrintMessage("--Building Int Op Node.");
                buildPrintMessage("--Building Plus Node.");

                Node intOpNode = new Node("Int Op");
                buildEndNode(intOpNode);
                intOpNode.addChild(digitNode);

                buildPrintMessage("--Building Expr Node.");
                Node exprNode = new Node("Expr");
                buildExprTree(exprNode);
                intOpNode.addChild(exprNode);

                root.addChild(intOpNode);
            }
            else
            {
                root.addChild(digitNode);
            }
        }

        private void buildStringExprTree(Node root)
        {
            buildPrintMessage("--Building CharList Node.");
            Node charListNode = new Node("Char List");
            buildEndNode(charListNode);
            root.addChild(charListNode);
        }

        private void buildIdTree(Node root)
        {
            buildPrintMessage("--Building Char Node.");
            Node charNode = new Node("Char");
            buildEndNode(charNode);
            root.addChild(charNode);
        }

        private void buildBooleanExprTree(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex];
            if (currentToken.match("left_paren"))
            {
                buildPrintMessage("--Building Left Parenthesis Node.");
                Node leftParenNode = new Node("(");
                root.addChild(leftParenNode);
                this.tokenIndex++;

                buildPrintMessage("--Building Expr Node.");
                Node exprNode = new Node("Expr");
                buildExprTree(exprNode);
                root.addChild(exprNode);

                currentToken = this.tokens[this.tokenIndex];
                if (currentToken.match("boolop_equal"))
                {
                    buildPrintMessage("--Building Boolean Operator Equal Node");
                }
                else
                {
                    buildPrintMessage("--Building Boolean Operator Not Equal Node");
                }
                Node boolOpNode = new Node("Bool Op");
                buildEndNode(boolOpNode);
                root.addChild(boolOpNode);

                buildPrintMessage("--Building Expr Node.");
                Node secondExprNode = new Node("Expr");
                buildExprTree(secondExprNode);
                root.addChild(secondExprNode);

                buildPrintMessage("--Building Right Parenthesis Node.");
                Node rightParenNode = new Node(")");
                root.addChild(rightParenNode);
                this.tokenIndex++;
            }
            else
            {
                buildPrintMessage("--Building BoolVal Node.");
                Node boolValNode = new Node("Bool Val");
                buildEndNode(boolValNode);
                root.addChild(boolValNode);
            }
        }

        private void buildEndNode(Node root)
        {
            Token currentToken = this.tokens[this.tokenIndex++];
            Node digitValueNode = new Node(currentToken.value);
            root.addChild(digitValueNode);
        }
    }
}
