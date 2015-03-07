using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class SymbolTable
    {
        private static Regex isDigit = new Regex("^[0-9]$");

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
                    generatePrint(root.children[i]);
                }

                if (this.errorCount > 0)
                {
                    break;
                }
            }

            if (this.errorCount == 0)
            {
                print("~~~Closing scope " + this.currentScope + ".");
                if (this.scopes[this.currentScope].scopeMembers.Count > 0)
                {
                    print("~~~Scope contained:");
                    for (int i = 0; i < this.scopes[this.currentScope].scopeMembers.Count; i++)
                    {
                        print(this.scopes[this.currentScope].scopeMembers[i]);
                    }
                }
                else
                {
                    print("~~~Scope did not contain any ids.");
                }
                this.currentScope = this.scopes[this.currentScope].parentId;
            }
            else
            {
                // print("~~~Error: There was a Symbol Table generation error.");
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
                print("id: " + id + " created in scope " + this.currentScope + ".");
            }
            else
            {
                print("~~~Error: id " + id + " already exists in scope " + this.currentScope + ".");
                this.errorCount++;
            }
        }

        private void generateAssignment(Node root)
        {
            Node assignOpNode = root.children[0];
            Node idNode = assignOpNode.children[0];
            Node valueNode = assignOpNode.children[1];
            List<int> scopeLocations = findScope(idNode.name);
            int scopeIndex = scopeLocations[0];
            int locationInScope = scopeLocations[1];
            if (scopeIndex != -1 && locationInScope != -1)
            {
                ScopeElement idToModify = this.scopes[scopeIndex].scopeMembers[locationInScope];
                List<Object> exprValues = generateExpr(valueNode);
                String exprType = exprValues[0].ToString();
                if (exprType == idToModify.type)
                {
                    idToModify.value = exprValues[1];
                    idToModify.isInitialized = true;
                    print("id: " + idNode.name + " assigned " + exprValues[1].ToString() + ".");
                }
                else
                {
                    print("~~~Error: id " + idNode.name + " is does not match the type of the expression being assigned.");
                    this.errorCount++;
                }
            }
            else
            {
                print("~~~Error: id " + idNode.name + " is not declared in its scope or any parent scope.");
                this.errorCount++;
            }
        }

        private void generatePrint(Node root)
        {
            generateExpr(root.children[0]);
        }

        private List<Object> generateExpr(Node root)
        {
            List<Object> returnValues;
            if (isDigit.IsMatch(root.name))
            {
                returnValues = new List<object>() { "int", Int32.Parse(root.name) };
            }
            else if (root.name == "==" || root.name == "!=" || root.name == "false" || root.name == "true")
            {
                returnValues = generateBooleanExpr(root);
            }
            else
            {
                // Should be impossible to reach when this function is finished, but I felt having detailed
                // if/else blocks was more desirable than letting whatever the end branch was going to else.
                this.errorCount++;
                returnValues = new List<Object>() { "error", "You've met with a terrible fate, haven't you?" };
            }
            // return new List<Object>() {"int", 2 };
            return returnValues;
        }

        private List<Object> generateBooleanExpr(Node root)
        {
            string returnType = "boolean";
            string returnBoolean;
            if (root.name == "true" || root.name == "false")
            {
                returnBoolean = root.name;
            }
            else
            {
                List<Object> leftExpr = generateExpr(root.children[0]);
                List<Object> rightExpr = generateExpr(root.children[1]);
                string leftType = leftExpr[0].ToString();
                string rightType = rightExpr[0].ToString();
                if (leftType != rightType)
                {
                    print("~~~ERROR: Invalid Boolean Expression, types do not match");
                    this.errorCount++;
                    returnType = "error";
                    returnBoolean = "You've met with a terrible fate, haven't you?";
                }
                else if (root.name == "==")
                {
                    returnBoolean = (leftExpr[1].ToString() == rightExpr[1].ToString()).ToString();
                }
                else if (root.name == "!=")
                {
                    returnBoolean = (leftExpr[1].ToString() != rightExpr[1].ToString()).ToString();
                }
                else
                {
                    this.errorCount++;
                    returnType = "error";
                    returnBoolean = "You've met with a terrible fate, haven't you?";
                }
            }
            return new List<Object>() { returnType, returnBoolean.ToLower() };
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

        private List<int> findScope(String id)
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
            return new List<int>() { scopeIndex, locationInScope };
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
        public string id;
        public Boolean isInitialized = false;
        public Boolean isUsed = false;
        public Object value;

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
