﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class SymbolTable : CompilerElement
    {
        private static Regex isDigit = new Regex("^[0-9]$");
        private static Regex isString = new Regex("\"[a-z ]*\"");
        private static Regex isChar = new Regex("^[a-z]$");

        private Node astRoot;
        public List<ScopeNode> scopes { get; private set; }
        private int currentScope = -1;
        public int errorCount { get; private set; }

        public SymbolTable(Node astRoot, TextBox taOutput)
            : base(taOutput)
        {
            this.astRoot = astRoot;
            this.errorCount = 0;
            this.scopes = new List<ScopeNode>();
        }

        public void generateSymbolTable()
        {
            buildPrintMessage("");
            buildPrintMessage("~~~Starting To Generate Symbol Table.");
            generateBlock(this.astRoot);
            buildPrintMessage("");
            if (this.errorCount == 0)
            {
                buildPrintMessage("~~~Generation of Symbol Table finished successfully. Nice!");
            }
            else
            {
                buildPrintMessage("~~~Generation of Symbol Table failed. Check errors.");
            }
            print();
        }

        private void generateBlock(Node root)
        {
            if (this.currentScope != -1)
            {
                this.currentScope = this.scopes[this.currentScope].scopeId;
            }
            this.scopes.Add(new ScopeNode(this.scopes.Count, this.currentScope));
            this.currentScope = this.scopes[this.scopes.Count - 1].scopeId;
            buildPrintMessage("~~~New Scope " + this.currentScope + " opened.");
            for (int i = 0; i < root.children.Count; i++)
            {
                if (root.children[i].name == "Statement List")
                {
                    generateBlock(root.children[i]);
                }
                else if (root.children[i].name == "Variable Declaration")
                {
                    generateVarDecl(root.children[i]);
                }
                else if (root.children[i].name == "Assignment")
                {
                    generateAssignment(root.children[i]);
                }
                else if (root.children[i].name == "Print")
                {
                    generatebuildPrintMessage(root.children[i]);
                }
                else if (root.children[i].name == "While" || root.children[i].name == "If")
                {
                    generateWhileIf(root.children[i]);
                }

                if (this.errorCount > 0)
                {
                    break;
                }
            }

            if (this.errorCount == 0)
            {
                buildPrintMessage("~~~Closing scope " + this.currentScope + ".");
                if (this.scopes[this.currentScope].scopeMembers.Count > 0)
                {
                    buildPrintMessage("~~~Scope contained:");
                    ScopeElement currentScopeElement;
                    for (int i = 0; i < this.scopes[this.currentScope].scopeMembers.Count; i++)
                    {
                        currentScopeElement = this.scopes[this.currentScope].scopeMembers[i];
                        buildPrintMessage(currentScopeElement);
                        if (!currentScopeElement.isUsed && !currentScopeElement.isInitialized)
                        {
                            buildPrintMessage("~~~Warning, id " + currentScopeElement.id + " is declared but not initialized or used.");
                        }
                        else if (!currentScopeElement.isUsed && currentScopeElement.isInitialized)
                        {
                            buildPrintMessage("~~~Warning, id " + currentScopeElement.id + " is declared and initialized, but not used.");
                        }
                    }
                }
                else
                {
                    buildPrintMessage("~~~Scope did not contain any ids.");
                }
                this.currentScope = this.scopes[this.currentScope].parentId;
            }
        }

        private void generateVarDecl(Node root)
        {
            String type = root.children[0].name;
            String id = root.children[1].name;
            Boolean isNew = inCurrentScope(id);

            if (isNew)
            {
                ScopeNode currentScope = this.scopes[this.currentScope];
                currentScope.scopeMembers.Add(new ScopeElement(type, id));
                buildPrintMessage("id: " + id + " created in scope " + this.currentScope + ".");
            }
            else
            {
                buildPrintMessage("~~~Error: id " + id + " already exists in scope " + this.currentScope + ".");
                this.errorCount++;
            }
        }

        private void generateAssignment(Node root)
        {
            Node assignOpNode = root.children[0];
            Node idNode = assignOpNode.children[0];
            Node valueNode = assignOpNode.children[1];
            Tuple<int, int> scopeLocations = findScope(idNode.name);
            int scopeIndex = scopeLocations.Item1;
            int locationInScope = scopeLocations.Item2;
            if (scopeIndex != -1 && locationInScope != -1)
            {
                ScopeElement idToModify = this.scopes[scopeIndex].scopeMembers[locationInScope];
                Tuple<string, Object> exprValues = generateExpr(valueNode);
                String exprType = exprValues.Item1.ToString();
                if (exprType == idToModify.type)
                {
                    idToModify.value = exprValues.Item2;
                    idToModify.isInitialized = true;
                    buildPrintMessage("id: " + idNode.name + " assigned " + exprValues.Item2.ToString() + ".");
                }
                else if (exprType != "error")
                {
                    buildPrintMessage("~~~Error: id " + idNode.name + " of type " + idToModify.type + " is does not match the type of the expression: " + exprType + ".");
                    this.errorCount++;
                }
            }
            else
            {
                buildPrintMessage("~~~Error: id " + idNode.name + " is not declared in its scope or any parent scope.");
                this.errorCount++;
            }
        }

        private void generatebuildPrintMessage(Node root)
        {
            generateExpr(root.children[0]);
        }

        private void generateWhileIf(Node root)
        {
            Tuple<string, Object> booleanExprList = generateBooleanExpr(root.children[0]);
            if (booleanExprList.Item1.ToString() == "boolean")
            {
                generateBlock(root.children[1]);
            }
            // No else case, as any error for booleanExpr's will already be printed.
        }

        private Tuple<string, Object> generateExpr(Node root)
        {
            Tuple<string, Object> returnValues;
            if (isDigit.IsMatch(root.name))
            {
                returnValues = new Tuple<string, Object>("int", Int32.Parse(root.name));
            }
            else if (root.name == "==" || root.name == "!=" || root.name == "false" || root.name == "true")
            {
                returnValues = generateBooleanExpr(root);
            }
            else if (isString.IsMatch(root.name))
            {
                returnValues = new Tuple<string, Object>("string", root.name);
            }
            else if (root.name == "+")
            {
                returnValues = generateIntExpr(root);
            }
            else if (isChar.IsMatch(root.name))
            {
                Tuple<int, int> scopeLocations = findScope(root.name);
                int scopeIndex = scopeLocations.Item1;
                int locationInScope = scopeLocations.Item2;
                if (scopeIndex != -1 && locationInScope != -1)
                {
                    ScopeElement idToUse = this.scopes[scopeIndex].scopeMembers[locationInScope];
                    idToUse.isUsed = true;
                    if (!idToUse.isInitialized)
                    {
                        buildPrintMessage("~~~Warning, id " + root.name + " is used without being initialized.");
                    }
                    returnValues = new Tuple<string, Object>(idToUse.type, idToUse.value);
                }
                else
                {
                    buildPrintMessage("~~~Error: id " + root.name + " is not declared in its scope or any parent scope.");
                    this.errorCount++;
                    returnValues = new Tuple<string, Object>("error", "You've met with a terrible fate, haven't you?");
                }
            }
            else
            {
                // Should be impossible to reach when this function is finished, but I felt having detailed
                // if/else blocks was more desirable than letting whatever the end branch was going to else.
                this.errorCount++;
                returnValues = new Tuple<string, Object>("error", "You've met with a terrible fate, haven't you?");
            }
            return returnValues;
        }

        private Tuple<string, Object> generateBooleanExpr(Node root)
        {
            string returnType = "boolean";
            string returnBoolean;
            if (root.name == "true" || root.name == "false")
            {
                returnBoolean = root.name;
            }
            else
            {
                Tuple<string, Object> leftExpr = generateExpr(root.children[0]);
                Tuple<string, Object> rightExpr = generateExpr(root.children[1]);
                string leftType = leftExpr.Item1.ToString();
                string rightType = rightExpr.Item1.ToString();
                if (leftType != rightType)
                {
                    buildPrintMessage("~~~ERROR: Invalid Boolean Expression, types do not match.");
                    this.errorCount++;
                    returnType = "error";
                    returnBoolean = "You've met with a terrible fate, haven't you?";
                }
                else if (root.name == "==")
                {
                    returnBoolean = (leftExpr.Item2.ToString() == rightExpr.Item2.ToString()).ToString();
                }
                else if (root.name == "!=")
                {
                    returnBoolean = (leftExpr.Item2.ToString() != rightExpr.Item2.ToString()).ToString();
                }
                else
                {
                    this.errorCount++;
                    returnType = "error";
                    returnBoolean = "You've met with a terrible fate, haven't you?";
                }
            }
            return new Tuple<string, Object>(returnType, returnBoolean.ToLower());
        }

        private Tuple<string, Object> generateIntExpr(Node root)
        {
            string returnType = "int";
            int returnValue;
            // Digit.
            Tuple<string, Object> leftExpr = generateExpr(root.children[0]);
            // Expr.
            Tuple<string, Object> rightExpr = generateExpr(root.children[1]);
            if (leftExpr.Item1.ToString() == "int" && rightExpr.Item1.ToString() == "int")
            {
                returnValue = (int)leftExpr.Item2 + (int)rightExpr.Item2;
            }
            else if (rightExpr.Item1.ToString() == "error")
            {
                // This case is so the same error does not get printed multiple times in a cascade.
                returnType = "error";
                returnValue = -1;
            }
            else
            {
                buildPrintMessage("~~~ERROR: Invalid Int Expression, types do not match.");
                this.errorCount++;
                returnType = "error";
                returnValue = -1;
            }
            return new Tuple<string, Object>(returnType, returnValue);
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
    }

    class ScopeNode
    {
        public int scopeId { get; private set; }
        public int parentId { get; private set; }
        public List<ScopeElement> scopeMembers { get; private set; }

        public ScopeNode(int scopeId, int parentId)
        {
            this.scopeId = scopeId;
            this.parentId = parentId;
            this.scopeMembers = new List<ScopeElement>();
        }

        public void addChildScope(ScopeElement childScope)
        {
            this.scopeMembers.Add(childScope);
        }

    }
    class ScopeElement
    {
        public string type { get; private set; }
        public string id { get; private set; }
        public Boolean isInitialized = false;
        public Boolean isUsed = false;
        public Object value = "undefined";

        public ScopeElement(string type, string id)
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
