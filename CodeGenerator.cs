using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class CodeGenerator : CompilerElement
    {
        private static Regex isDigit = new Regex("^[0-9]$");
        private static Regex isChar = new Regex("^[a-z]$");
        private static Regex isPlus = new Regex("^[\\+]$");
        private static Regex isString = new Regex("\"[a-z ]*\"");
        private static Regex isBadString = new Regex("\"[a-zA-Z ]*\"");
        private static Regex isWhiteSpace = new Regex("[\\s]");

        private Node astRoot;
        private int errorCount = 0;
        private int index = 0;
        private int currentScope = 0;
        private int currentScopeId = 0;

        private List<string> generatedCode = new List<string>();
        private int tempCount = 0;
        private int offset = 0;
        private string currentTemp;

        private List<Tuple<string, string, int, int>> staticData = new List<Tuple<string, string, int, int>>();

        public CodeGenerator(Node astRoot, TextBox taOutput)
            : base(taOutput)
        {
            this.astRoot = astRoot;
        }

        public void generateCode()
        {
            buildPrintMessage("");
            buildPrintMessage("~~~Now Starting Code Generation from AST");
            for (int i = 0; i < astRoot.children.Count; i++)
            {
                generateFromNode(astRoot.children[i]);
            }

            this.generatedCode[this.index++] = "00";

            string currentJump = "";
        }

        private void generateFromNode(Node currentNode)
        {
            string currentNodeName = currentNode.name;
            if (currentNodeName == "Print")
            {
                generatePrintCode(currentNode);
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

        private void generatePrintCode(Node printNode)
        {
            Node exprNode = printNode.children[0];
            buildPrintMessage("--Generating Print Code.");
            Tuple<string, string> exprTuple = generateExprTuple(exprNode);
        }

        private Tuple<string, string> generateExprTuple(Node exprNode)
        {
            string exprNodeName = exprNode.name;
            Tuple<string, string> exprTuple;
            if (isDigit.IsMatch(exprNodeName))
            {
                exprTuple = new Tuple<string, string>("digit", exprNodeName);
            }
            else if (isPlus.IsMatch(exprNodeName))
            {
                exprTuple = new Tuple<string, string>("digit", generateIntExpr(exprNode));
            }
            else
            {
                exprTuple = new Tuple<string, string>("digit", exprNodeName);
            }
            return exprTuple;
        }

        private string generateIntExpr(Node intExprNode)
        {
            Tuple<string, string> leftExpr = generateExprTuple(intExprNode.children[0]);
            Tuple<string, string> rightExpr = generateExprTuple(intExprNode.children[1]);
            string exprString;
            if (rightExpr.Item2[0] == 'T')
            {
                string tempName = "S" + this.tempCount.ToString();
                tempCount++;
                this.generatedCode[this.index++] = "A9";

                if (int.Parse(leftExpr.Item2).ToString("x").Length == 1)
                {
                    this.generatedCode[this.index++] = "0" + int.Parse(leftExpr.Item2).ToString("x").ToUpper();
                }
                else
                {
                    this.generatedCode[this.index++] = int.Parse(leftExpr.Item2).ToString("x").ToUpper();
                }

                this.generatedCode[this.index++] = "6D";
                this.generatedCode[this.index++] = rightExpr.Item2;
                this.generatedCode[this.index++] = "XX";

                this.generatedCode[this.index++] = "8D";
                this.generatedCode[this.index++] = this.currentTemp;
                this.generatedCode[this.index++] = "XX";

                exprString = this.currentTemp;
            }
            else
            {
                exprString = (int.Parse(leftExpr.Item2) + int.Parse(rightExpr.Item2)).ToString("x");
            }

            return exprString;
        }

        private void createTemp(string tempName)
        {
            buildPrintMessage("--Creating Temporary Variable " + tempName + ".");
            if (this.currentTemp == null)
            {
                this.currentTemp = "T0";
            }
            else
            {
                this.currentTemp = "T" + (int.Parse(this.currentTemp[1].ToString()) + 1).ToString();
            }
            this.staticData.Add(new Tuple<string, string, int, int>(this.currentTemp, tempName, this.currentScopeId, this.offset++));
        }
    }
}
