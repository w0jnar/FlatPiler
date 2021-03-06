﻿using System;
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
        private List<int> lastScopeIdStack = new List<int>();

        private static int programSize = 255;
        private List<string> generatedCode = new List<string>();
        private List<string> heap = new List<string>();
        private int heapPointer = programSize;
        private int tempCount = 0;
        private int offset = 0;
        private string currentTemp;
        private int currentJumpCount = 0;

        private string truePointer = "";
        private string falsePointer = "";

        private List<Tuple<string, string, int, int>> staticData = new List<Tuple<string, string, int, int>>();
        private Dictionary<string, int> jumpTable = new Dictionary<string, int>();

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

            if (this.jumpTable.Count > 0)
            {
                string currentJump = "";
                buildPrintMessage("--Backpatching Jumps.");
                for (int i = 0; i < this.generatedCode.Count; i++)
                {
                    if (this.generatedCode[i][0] == 'J')
                    {
                        int jumpTableInt = this.jumpTable[this.generatedCode[i]];
                        if (jumpTableInt.ToString("x").Length == 1)
                        {
                            currentJump = "0" + jumpTableInt.ToString("x").ToUpper();
                        }
                        else
                        {
                            currentJump = jumpTableInt.ToString("x").ToUpper();
                        }
                        this.generatedCode[i] = currentJump;
                    }
                }
            }

            if (this.staticData.Count > 0)
            {
                Tuple<string, string, int, int> currentTemp;
                Tuple<string, string> currentReplace;
                string currentTempString;
                buildPrintMessage("--Backpatching Temps.");
                for (int i = 0; i < this.staticData.Count; i++)
                {
                    appendCode("00");
                    currentTemp = this.staticData[i];
                    currentTempString = currentTemp.Item1;
                    currentReplace = convertToHexPair((this.index - 1).ToString("x"));

                    for (int j = 0; j < this.generatedCode.Count; j++)
                    {
                        if (this.generatedCode[j] == currentTempString)
                        {
                            this.generatedCode[j] = currentReplace.Item1;
                            this.generatedCode[j + 1] = currentReplace.Item2;
                        }
                    }
                }
            }

            buildPrintMessage("--Checking if Code is of valid length.");
            if ((this.generatedCode.Count + this.heap.Count) <= (programSize + 1))
            {
                buildPrintMessage("--Code is valid!");
                buildPrintMessage("--Filling space with \"00\" as needed.");
                int codeSize = this.generatedCode.Count;
                for (int i = 0; i <= (programSize - codeSize - this.heap.Count); i++)
                {
                    appendCode("00");
                }
                buildPrintMessage("--Joining Heap to Code.");
                this.generatedCode.AddRange(this.heap);
                buildPrintMessage("~~~Now Ending Code Generation.");
                buildPrintMessage("--Output Code:");

                for (int i = 0; i < this.generatedCode.Count; i += 16)
                {
                    buildPrintMessage(this.generatedCode[i] + " "
                        + this.generatedCode[i + 1] + " "
                        + this.generatedCode[i + 2] + " "
                        + this.generatedCode[i + 3] + " "
                        + this.generatedCode[i + 4] + " "
                        + this.generatedCode[i + 5] + " "
                        + this.generatedCode[i + 6] + " "
                        + this.generatedCode[i + 7] + "   "
                        + this.generatedCode[i + 8] + " "
                        + this.generatedCode[i + 9] + " "
                        + this.generatedCode[i + 10] + " "
                        + this.generatedCode[i + 11] + " "
                        + this.generatedCode[i + 12] + " "
                        + this.generatedCode[i + 13] + " "
                        + this.generatedCode[i + 14] + " "
                        + this.generatedCode[i + 15]);
                }
            }
            else
            {
                buildPrintMessage("~~~Error: Too much code generated for the supported operation.");
            }
            print();
        }


        private void generateFromNode(Node currentNode)
        {
            string currentNodeName = currentNode.name;
            if (currentNodeName == "Print")
            {
                generatePrint(currentNode);
            }
            else if (currentNodeName == "Assignment")
            {
                generateAssignment(currentNode);
            }
            else if (currentNodeName == "Variable Declaration")
            {
                generateVarDecl(currentNode);
            }
            else if (currentNodeName == "While")
            {
                generateWhile(currentNode);
            }
            else if (currentNodeName == "If")
            {
                generateIf(currentNode);
            }
            else if (currentNodeName == "Statement List")
            {
                generateBlock(currentNode);
            }
        }

        private void generatePrint(Node printNode)
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
            else
            {
                if (exprTuple.Item1 == "boolean" && exprTuple.Item2[0] == 'T')
                {
                    string tempName = "S" + this.tempCount.ToString();
                    this.tempCount++;
                    createTemp(tempName);
                    appendCode("A9");
                    appendCode("01");
                    appendCode("8D");
                    appendCode(this.currentTemp);
                    appendCode("XX");
                    appendCode("A2");
                    appendCode("01");
                    appendCode("EC");
                    appendCode(exprTuple.Item2);
                    appendCode("XX");
                    appendCode("D0");
                    appendCode("0C");
                    appendCode("A0");
                    appendCode(this.falsePointer);
                    appendCode("A2");
                    appendCode("02");
                    appendCode("FF");
                    appendCode("A2");
                    appendCode("00");
                    appendCode("EC");
                    appendCode(this.currentTemp);
                    appendCode("XX");
                    appendCode("D0");
                    appendCode("05");
                    appendCode("A0");
                    appendCode(this.truePointer);
                    appendCode("A2");
                    appendCode("02");
                    appendCode("FF");
                }
                else
                {
                    appendCode("A0");
                    appendCode(exprTuple.Item2);
                    appendCode("A2");
                    appendCode("02");
                    appendCode("FF");
                }
            }
        }

        private void generateAssignment(Node assignmentNode)
        {
            buildPrintMessage("--Generating Assignment Code.");
            Node assignmentOpNode = assignmentNode.children[0];
            Node varIdNode = assignmentOpNode.children[0];
            Node exprNode = assignmentOpNode.children[1];
            Tuple<string, string, int, int> currentTemp = getTemp(varIdNode.name, this.currentScopeId);
            Tuple<string, string> exprTuple = generateExprTuple(exprNode);
            string exprValue = exprTuple.Item2;
            string exprType = exprTuple.Item1;

            if (isString.IsMatch(exprValue))
            {
                exprValue = allocateHeap(exprValue);
            }

            if (exprValue.Length == 1)
            {
                exprValue = "0" + exprValue;
            }

            if (exprValue[0] != 'T')
            {
                appendCode("A9");
                appendCode(exprValue);
                appendCode("8D");
                appendCode(currentTemp.Item1);
                appendCode("XX");
            }
            else if ((exprValue[0] == 'T' && exprType != "boolean"))
            {
                appendCode("AD");
                appendCode(exprValue);
                appendCode("XX");
                appendCode("8D");
                appendCode(currentTemp.Item1);
                appendCode("XX");
            }
            else
            {
                string tempName = "S" + this.tempCount.ToString();
                this.tempCount++;
                createTemp(tempName);

                appendCode("A9");
                appendCode("01");
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("A2");
                appendCode("01");
                appendCode("EC");
                appendCode(exprValue);
                appendCode("XX");
                appendCode("D0");
                appendCode("0C");
                appendCode("A9");
                appendCode(this.falsePointer);
                appendCode("8D");
                appendCode(currentTemp.Item1);
                appendCode("XX");
                appendCode("A2");
                appendCode("00");
                appendCode("EC");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("D0");
                appendCode("05");
                appendCode("A9");
                appendCode(this.truePointer);
                appendCode("8D");
                appendCode(currentTemp.Item1);
                appendCode("XX");

            }
        }

        private void generateVarDecl(Node varDeclNode)
        {
            buildPrintMessage("--Generating Variable Declaration Code.");
            createTemp(varDeclNode.children[1].name);
            appendCode("A9");
            appendCode("00");
            appendCode("8D");
            appendCode(this.currentTemp);
            appendCode("XX");
        }

        private void generateWhile(Node whileNode)
        {
            buildPrintMessage("--Generating While Statement Code.");
            int whileIndex = this.index;
            string booleanString = generateBooleanExpr(whileNode.children[0]);
            string tempName = "S" + this.tempCount.ToString();
            this.tempCount++;
            createTemp(tempName);

            if (booleanString[0] == 'T')
            {
                appendCode("A9");
                appendCode("00");
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("A2");
                appendCode("01");
                appendCode("EC");
                appendCode(booleanString);
                appendCode("XX");
                appendCode("D0");
                appendCode("0C");
                appendCode("A9");
                appendCode(this.truePointer);
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("A2");
                appendCode("00");
                appendCode("EC");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("D0");
                appendCode("05");
                appendCode("A9");
                appendCode(this.falsePointer);
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");

                appendCode("A2");
                appendCode(this.falsePointer);

            }
            else
            {
                appendCode("A9");
                appendCode(this.truePointer);
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");

                appendCode("A2");
                appendCode(booleanString);
            }
            appendCode("EC");
            appendCode(this.currentTemp);
            appendCode("XX");
            appendCode("D0");
            string jumpName = "J" + this.currentJumpCount.ToString();
            this.currentJumpCount++;
            appendCode(jumpName);

            int currentIndex = this.index;

            generateBlock(whileNode.children[1]);


            string whileTempName = "S" + this.tempCount.ToString();
            this.tempCount++;
            createTemp(whileTempName);
            appendCode("A9");
            appendCode("01");
            appendCode("8D");
            appendCode(this.currentTemp);
            appendCode("XX");

            appendCode("A2");
            appendCode("00");
            appendCode("EC");
            appendCode(this.currentTemp);
            appendCode("XX");
            appendCode("D0");
            string whileJumpName = "J" + this.currentJumpCount.ToString();
            this.currentJumpCount++;
            appendCode(whileJumpName);
            whileIndex = (programSize - (this.index - whileIndex)) + 1;
            this.jumpTable.Add(whileJumpName, whileIndex);

            currentIndex = this.index - currentIndex;
            this.jumpTable.Add(jumpName, currentIndex);
        }

        private void generateIf(Node ifNode)
        {
            buildPrintMessage("--Generating If Statement Code.");
            string booleanExpression = generateBooleanExpr(ifNode.children[0]);

            string tempName = "S" + this.tempCount.ToString();
            this.tempCount++;
            createTemp(tempName);
            if (booleanExpression[0] == 'T')
            {
                appendCode("A9");
                appendCode("01");
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("A2");
                appendCode("01");
                appendCode("EC");
                appendCode(booleanExpression);
                appendCode("XX");
                appendCode("D0");
                appendCode("0C");
                appendCode("A9");
                appendCode(this.falsePointer);
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("A2");
                appendCode("00");
                appendCode("EC");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("D0");
                appendCode("05");
                appendCode("A9");
                appendCode(this.truePointer);
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");

                appendCode("A2");
                appendCode(this.falsePointer);
            }
            else
            {
                appendCode("A9");
                appendCode(this.truePointer);
                appendCode("8D");
                appendCode(this.currentTemp);
                appendCode("XX");
                appendCode("A2");
                appendCode(booleanExpression);
            }
            appendCode("EC");
            appendCode(this.currentTemp);
            appendCode("XX");
            appendCode("D0");
            string jumpName = "J" + this.currentJumpCount.ToString();
            this.currentJumpCount++;
            appendCode(jumpName);

            int currentIndex = this.index;

            generateBlock(ifNode.children[1]);

            currentIndex = this.index - currentIndex;
            this.jumpTable.Add(jumpName, currentIndex);
        }

        private void generateBlock(Node statementListNode)
        {
            this.currentScope++;
            this.lastScopeIdStack.Add(this.currentScopeId);
            this.currentScopeId = this.currentScope;
            for (int i = 0; i < statementListNode.children.Count; i++)
            {
                generateFromNode(statementListNode.children[i]);
            }
            this.currentScopeId = this.lastScopeIdStack[this.lastScopeIdStack.Count - 1];
            this.lastScopeIdStack.RemoveAt(this.lastScopeIdStack.Count - 1);
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
            else if (isString.IsMatch(exprNodeName))
            {
                exprTuple = new Tuple<string, string>("string", allocateHeap(exprNodeName));
            }
            else if (exprNodeName == "==" || exprNodeName == "!=" || exprNodeName == "false" || exprNodeName == "true")
            {
                exprTuple = new Tuple<string, string>("boolean", generateBooleanExpr(exprNode));
            }
            else
            {
                Tuple<string, string, int, int> currentTemp = getTemp(exprNodeName, this.currentScopeId);
                exprTuple = new Tuple<string, string>(getIdInfo(exprNodeName).Item1, currentTemp.Item1);
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
                this.tempCount++;
                createTemp(tempName);
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

        private string generateBooleanExpr(Node boolExprNode)
        {
            string currentNodeName = boolExprNode.name;
            string booleanPointer;
            if (this.truePointer == "")
            {
                this.truePointer = allocateHeap("\"true\"");
                this.falsePointer = allocateHeap("\"false\"");
            }

            if (currentNodeName == "true" || currentNodeName == "false")
            {
                string boolString = "\"" + currentNodeName + "\"";
                booleanPointer = allocateHeap(boolString);
            }
            else
            {
                Tuple<string, string> leftExpr = generateExprTuple(boolExprNode.children[0]);
                Tuple<string, string> rightExpr = generateExprTuple(boolExprNode.children[1]);
                string leftType = leftExpr.Item1;
                string leftValue = leftExpr.Item2;
                string rightType = rightExpr.Item1;
                string rightValue = rightExpr.Item2;
                if (isString.IsMatch(leftValue))
                {
                    leftValue = allocateHeap(leftValue);
                }
                if (isString.IsMatch(rightValue))
                {
                    rightValue = allocateHeap(rightValue);
                }

                if (leftValue[0] == 'T' || rightValue[0] == 'T')
                {
                    if (leftValue.Length == 1)
                    {
                        leftValue = "0" + leftValue;
                    }
                    if (rightValue.Length == 1)
                    {
                        rightValue = "0" + rightValue;
                    }
                    // Left side to X reg.
                    if (leftValue[0] == 'T' && leftType == "boolean")
                    {
                        appendCode("A2");
                        appendCode("01");
                        appendCode("EC");
                        appendCode(leftValue);
                        appendCode("XX");
                        appendCode("D0");
                        appendCode("09");
                        appendCode("A2");
                        appendCode("01");
                        appendCode("EC");
                        appendCode(leftValue);
                        appendCode("XX");
                        appendCode("D0");
                        appendCode("02");
                        appendCode("A2");
                        appendCode(this.falsePointer);
                    }
                    else if (leftValue[0] == 'T')
                    {
                        appendCode("AE");
                        appendCode(leftValue);
                        appendCode("XX");
                    }
                    else
                    {
                        appendCode("A2");
                        appendCode(leftValue);
                    }

                    // Create a temp for right side.
                    string tempName = "S" + this.tempCount.ToString();
                    this.tempCount++;
                    createTemp(tempName);
                    // Right side needs to end up in memory.
                    if (rightValue[0] == 'T' && rightType == "boolean")
                    {
                        appendCode("AD");
                        appendCode(rightValue);
                        appendCode("XX");
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");


                        appendCode("A2");
                        appendCode("01");
                        appendCode("EC");
                        appendCode(rightValue);
                        appendCode("XX");
                        appendCode("D0");
                        appendCode("0C");
                        appendCode("A9");
                        appendCode(this.truePointer);
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                        appendCode("A2");
                        appendCode("01");
                        appendCode("EC");
                        appendCode(rightValue);
                        appendCode("XX");
                        appendCode("D0");
                        appendCode("05");
                        appendCode("A9");
                        appendCode(this.falsePointer);
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");

                    }
                    else if (rightValue[0] == 'T')
                    {
                        appendCode("AD");
                        appendCode(rightValue);
                        appendCode("XX");
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                    }
                    else
                    {
                        appendCode("A9");
                        appendCode(rightValue);
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                    }

                    appendCode("EC");
                    appendCode(this.currentTemp);
                    appendCode("XX");

                    string tempName2 = "S" + this.tempCount.ToString();
                    this.tempCount++;
                    createTemp(tempName2);
                    if (currentNodeName == "!=")
                    {
                        appendCode("D0");
                        appendCode("0C");
                        appendCode("A2");
                        appendCode("00");
                        appendCode("A9");
                        appendCode("01");
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                        appendCode("EC");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                        appendCode("D0");
                        appendCode("07");
                        appendCode("A2");
                        appendCode("00");
                        appendCode("A9");
                        appendCode("00");
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                    }
                    else
                    {
                        appendCode("D0");
                        appendCode("0C");
                        appendCode("A2");
                        appendCode("01");
                        appendCode("A9");
                        appendCode("00");
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                        appendCode("EC");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                        appendCode("D0");
                        appendCode("07");
                        appendCode("A2");
                        appendCode("00");
                        appendCode("A9");
                        appendCode("01");
                        appendCode("8D");
                        appendCode(this.currentTemp);
                        appendCode("XX");
                    }
                    booleanPointer = this.currentTemp;
                }
                else // Static, as there are no variables involved.
                {
                    if (leftValue == rightValue && currentNodeName == "==")
                    {
                        booleanPointer = this.truePointer;
                    }
                    else if (leftValue != rightValue && currentNodeName == "==")
                    {
                        booleanPointer = this.falsePointer;
                    }
                    else if (leftValue == rightValue && currentNodeName == "!=")
                    {
                        booleanPointer = this.falsePointer;
                    }
                    else if (leftValue != rightValue && currentNodeName == "!=")
                    {
                        booleanPointer = this.truePointer;
                    }
                    else
                    {
                        // This case will never be reached, but the verboseness of the cases above felt necessary.
                        booleanPointer = "";
                    }
                }
            }
            return booleanPointer;
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

        private string allocateHeap(string stringToAllocate)
        {
            string stringWithoutQuotes = stringToAllocate.Substring(1, stringToAllocate.Length - 2);
            Tuple<bool, int> heapCheck = existsInHeap(stringWithoutQuotes);
            string heapLocation;
            if (!heapCheck.Item1)
            {
                buildPrintMessage("--Allocating Heap Space for String " + stringWithoutQuotes + ".");
                this.heap.Insert(0, "00");
                List<string> stringArray = new List<string>();
                for (int i = 0; i < stringWithoutQuotes.Length; i++)
                {
                    stringArray.Add(((int)stringWithoutQuotes[i]).ToString("x").ToUpper());
                }
                stringArray.AddRange(this.heap);
                this.heap = stringArray;
                this.heapPointer = this.heapPointer - stringWithoutQuotes.Length;
                heapLocation = this.heapPointer.ToString("x").ToUpper();
                this.heapPointer--;
            }
            else
            {
                heapLocation = heapCheck.Item2.ToString("x").ToUpper();
            }
            if (heapLocation.Length == 1)
            {
                heapLocation = "0" + heapLocation;
            }
            return heapLocation;
        }

        private Tuple<bool, int> existsInHeap(string stringToFind)
        {
            int pointer;
            int offset = 0;
            bool booleanCheck;
            string stringToCheck;
            string stringCharAsHex = ((int)stringToFind[0]).ToString("x").ToUpper();
            Tuple<bool, int> returnTuple = new Tuple<bool, int>(false, 0);
            for (int i = 0; i < this.heap.Count; i++)
            {

                if (stringCharAsHex == this.heap[i])
                {
                    pointer = this.heap.Count - i;
                    booleanCheck = false;
                    for (int j = 0; j < stringToFind.Length; j++)
                    {
                        stringCharAsHex = ((int)stringToFind[j]).ToString("x").ToUpper();
                        if (stringCharAsHex == this.heap[i + j])
                        {
                            booleanCheck = true;
                        }
                        else
                        {
                            booleanCheck = false;
                            break;
                        }
                    }
                    if (booleanCheck)
                    {
                        stringToCheck = this.heap[i + stringToFind.Length];
                        if (stringToCheck == "00")
                        {
                            returnTuple = new Tuple<bool, int>(true, (programSize - pointer + 1));
                        }
                    }
                    else
                    {
                        // String was not found, now we must reach the next null to check the next string.
                        offset = i;
                        while (offset < this.heap.Count && this.heap[offset] != "00")
                        {
                            offset++;
                        }
                    }
                    i += offset;
                }
            }
            return returnTuple;
        }

        private Tuple<string, string, int, int> getTemp(string idName, int scope)
        {
            int tempIndex = -1;
            Tuple<string, string, int, int> currentTemp;
            for (int i = 0; i < this.staticData.Count; i++)
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

                for (int i = 0; i < this.staticData.Count; i++)
                {
                    currentTemp = this.staticData[i];
                    if (currentTemp.Item2 == idName && currentTemp.Item3 == idLocation.Item1)
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

        private Tuple<string, string> convertToHexPair(string hexNum)
        {
            var offset = 4 - hexNum.Length;
            for (int i = 0; i < offset; i++)
            {
                hexNum = "0" + hexNum;
            }
            hexNum = hexNum.ToUpper();
            return new Tuple<string, string>(hexNum.Substring(2, 2), hexNum.Substring(0, 2));
        }

        private void appendCode(string code)
        {
            this.generatedCode.Add(code);
            this.index++;
        }
    }
}
