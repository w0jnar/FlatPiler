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
        private static Regex isId = new Regex("^[a-z]$");
        private static Regex isPlus = new Regex("^[\\+]$");
        private static Regex isString = new Regex("\"[a-z ]*\"");

        private Node astRoot;
        private List<ScopeNode> scopes;
        private int index = 0;
        private int currentScope = 0;
        private int currentScopeId = 0;

        private List<string> generatedCode = new List<string>();
        private int tempCount = 0;
        private int offset = 0;
        private string currentTemp;

        private List<Tuple<string, string, int, int>> staticData = new List<Tuple<string, string, int, int>>();
        private List<Tuple<string, int>> jumpTable = new List<Tuple<string, int>>();

        public CodeGenerator(Node astRoot, List<ScopeNode> scopes, TextBox taOutput)
            : base(taOutput)
        {
            this.astRoot = astRoot;
            this.scopes = scopes;
        }

        public void generateCode()
        {
            buildPrintMessage("");
            buildPrintMessage("~~~Now Starting Code Generation.");
            for (int i = 0; i < astRoot.children.Count; i++)
            {
                generateFromNode(astRoot.children[i]);
            }

            appendCode("00");

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

            if (isId.IsMatch(exprNode.name) && getIdInfo(exprNode.name).Item1 == "int")
            {
                Tuple<string, string, int, int> currentTemp = getTemp(exprNode.name, this.currentScopeId);
                appendCode("AC");
                appendCode(currentTemp.Item1);
                appendCode("XX");
                appendCode("A2");
                appendCode("01");
                appendCode("FF");
            }

            else if (isId.IsMatch(exprNode.name) && (getIdInfo(exprNode.name).Item1 == "string" || getIdInfo(exprNode.name).Item1 == "boolean"))
            {
                Tuple<string, string, int, int> currentTemp = getTemp(exprNode.name, this.currentScopeId);
                appendCode("AC");
                appendCode(currentTemp.Item1);
                appendCode("XX");
                appendCode("A2");
                appendCode("02");
                appendCode("FF");
            }
            else if (exprTuple.Item1 == "digit")
            {
                if (exprTuple.Item2[0] != 'T')
                {
                    appendCode("A0");
                    if (int.Parse(exprTuple.Item2).ToString("x").Length == 1)
                    {
                        appendCode("0" + int.Parse(exprTuple.Item2).ToString("x").ToUpper());
                    }
                    else
                    {
                        appendCode(int.Parse(exprTuple.Item2).ToString("x").ToUpper());
                    }
                }
                else
                {
                    appendCode("AC");
                    appendCode(exprTuple.Item2);
                    appendCode("XX");
                }
                appendCode("A2");
                appendCode("01");
                appendCode("FF");

            }

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
                appendCode("A9");

                if (int.Parse(leftExpr.Item2).ToString("x").Length == 1)
                {
                    appendCode("0" + int.Parse(leftExpr.Item2).ToString("x").ToUpper());
                }
                else
                {
                    appendCode(int.Parse(leftExpr.Item2).ToString("x").ToUpper());
                }

                appendCode("6D");
                appendCode(rightExpr.Item2);
                appendCode("XX");

                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");

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

        private Tuple<string, string, int, int> getTemp(string idName, int scope)
        {
            int tempIndex = -1;
            Tuple<string, string, int, int> currentTemp;
            for (var i = 0; i < this.staticData.Count; i++)
            {
                currentTemp = this.staticData[i];
                if (currentTemp.Item2 == idName && currentTemp.Item3 == scope)
                {
                    tempIndex = i;
                    break;
                }
            }
            if (tempIndex == -1)
            {
                Tuple<int, int> idLocation = findScope(idName);
                Tuple<string, string, int, int> parentTemp = getTemp(idName, idLocation.Item1);

                for (var i = 0; i < this.staticData.Count; i++)
                {
                    currentTemp = this.staticData[i];
                    if (currentTemp.Item2 == idName && currentTemp.Item3 == scope)
                    {
                        tempIndex = i;
                        break;
                    }
                }
            }
            return this.staticData[tempIndex];
        }

        private Tuple<string, Object> getIdInfo(string id)
        {
            Tuple<int, int> idLocation = findScope(id);
            ScopeElement idElement = this.scopes[idLocation.Item1].scopeMembers[idLocation.Item2];
            return new Tuple<string, Object>(idElement.type, idElement.value);
        }

        private Tuple<int, int> findScope(String id)
        {
            Boolean foundId = false;
            int scopeIndex = -1;
            int locationInScope = -1;
            ScopeNode currentScopeNode = this.scopes[this.currentScope];
            int scopePlaceHolder = this.currentScope;
            while (this.currentScope != -1 && !foundId)
            {
                foundId = !inCurrentScope(id);
                if (!foundId)
                {
                    this.currentScope = currentScopeNode.parentId;
                    if (this.currentScope != -1)
                    {
                        currentScopeNode = this.scopes[this.currentScope];
                    }
                }
            }

            if (foundId)
            {
                scopeIndex = this.currentScope;
                this.currentScope = scopePlaceHolder;
                currentScopeNode = this.scopes[scopeIndex];
                for (int i = 0; i < currentScopeNode.scopeMembers.Count; i++)
                {
                    if (id == currentScopeNode.scopeMembers[i].id)
                    {
                        locationInScope = i;
                    }
                }
            }
            return new Tuple<int, int>(scopeIndex, locationInScope);
        }

        private Boolean inCurrentScope(String id)
        {
            Boolean isNewId = true;
            ScopeNode currentScopeNode = this.scopes[this.currentScope];
            for (int i = 0; i < currentScopeNode.scopeMembers.Count; i++)
            {
                if (id == currentScopeNode.scopeMembers[i].id.ToString())
                {
                    isNewId = false;
                }
            }
            return isNewId;
        }

        private void appendCode(string code)
        {
            this.generatedCode.Add(code);
            this.index++;
        }
    }
}
