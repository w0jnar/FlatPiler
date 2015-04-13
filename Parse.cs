using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class Parse
    {
        public List<Token> tokens;
        private TextBox taOutput;
        public int errorCount = 0;
        private int tokenIndex = 0;
        private Token currentToken;
        private Boolean wasSuccessful;

        public Parse(List<Token> tokens, TextBox taOutput)
        {
            this.tokens = tokens;
            this.taOutput = taOutput;
        }

        public void parseProgram()
        {
            print("");
            print("~~~Starting Parse");
            if (this.tokens.Count == 0)
            {
                this.errorCount++;
                print("~~~Error: No tokens to parse!");
            }
            else
            {
                startParse();
            }

            if (this.errorCount > 0)
            {
                print("There was an error, execution stopped.");
            }
            else
            {
                print("No Parse Errors Found! Nice!");
            }
            print("~~~Ending Parse");
        }

        private void startParse()
        {
            newTokenSetup();
            this.wasSuccessful = parseBlock();
            if (this.wasSuccessful)
            {
                newTokenSetup();
                this.wasSuccessful = this.currentToken.match("end_of_file");
                if (this.wasSuccessful)
                {
                    print("-Program parsing ended.");
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: Program improperly ended.");
                }
            }
        }

        // Parsers

        private Boolean parseBlock()
        {
            Boolean returnValue;

            this.wasSuccessful = this.currentToken.match("left_brace");
            if (this.wasSuccessful)
            {
                print("-Parsing Block");
                this.wasSuccessful = false;
                this.wasSuccessful = parseStatementList();
                if (this.wasSuccessful)
                {
                    this.wasSuccessful = this.currentToken.match("right_brace");
                    if (this.wasSuccessful)
                    {
                        print("-Finished Parsing Block.");
                        returnValue = true;

                    }
                    else
                    {
                        this.errorCount++;
                        print("~~~Error: Program block improperly ended.");
                        returnValue = false;
                    }
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: Program block improperly ended.");
                    returnValue = false;
                }
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Program improperly ended.");
                returnValue = false;
            }

            return returnValue;
        }

        private Boolean parseStatementList()
        {
            newTokenSetup();
            this.wasSuccessful = true;
            while (!this.currentToken.match("right_brace") || this.errorCount != 0)
            {
                this.wasSuccessful = parseStatement();
                if (this.wasSuccessful)
                {
                    newTokenSetup();
                    this.wasSuccessful = true;
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: Statement improperly parsed.");
                    break;
                }
            }

            Boolean returnValue;
            if (this.wasSuccessful && this.errorCount == 0)
            {
                returnValue = true;
            }
            else
            {
                print("~~~Parse ending due to Error(s).");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseStatement()
        {
            print("-Beginning Parse of a Statement.");
            Boolean returnValue;
            if (this.currentToken.match("print"))
            {
                returnValue = parsePrintStatement();
            }
            else if (this.currentToken.match("var_id"))
            {
                returnValue = parseAssignmentStatement();
            }
            else if (matchType())
            {
                returnValue = parseVarDecl();
            }
            else if (this.currentToken.match("while"))
            {
                returnValue = parseWhileStatement();
            }
            else if (this.currentToken.match("if"))
            {
                returnValue = parseIfStatement();
            }
            else if (this.currentToken.match("left_brace"))
            {
                returnValue = parseBlock();
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid statement, improper starting token.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parsePrintStatement()
        {
            Boolean returnValue;
            newTokenSetup();
            if (this.currentToken.match("left_paren"))
            {
                this.wasSuccessful = parseExpr();
                if (this.wasSuccessful)
                {
                    newTokenSetup();
                    if (this.currentToken.match("right_paren"))
                    {
                        print("Valid Print Statement Parsed.");
                        returnValue = true;
                    }
                    else
                    {
                        this.errorCount++;
                        print("~~~Error: Invalid print statement, right parenthesis expected, but not found.");
                        returnValue = false;
                    }
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: Invalid print statement, expression not parsed properly.");
                    returnValue = false;
                }
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid print statement, left parenthesis not found.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseAssignmentStatement()
        {
            Token tokenToAssignTo = this.currentToken;
            newTokenSetup();
            Boolean returnValue;
            if (this.currentToken.match("assignment_op"))
            {
                this.wasSuccessful = parseExpr();
                if (this.wasSuccessful)
                {
                    print("Valid Assignment Statement parsed.");
                    returnValue = true;
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: Invalid assignment statement, invalid expression.");
                    returnValue = false;
                }
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid assignment statement, assignment operator missing.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseVarDecl()
        {
            newTokenSetup();
            Boolean returnValue;
            if (this.currentToken.match("var_id"))
            {
                print("Valid Variable Declaration Statement parsed.");
                returnValue = true;
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid variable declaration statement, invalid variable token.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseWhileStatement()
        {
            newTokenSetup();
            this.wasSuccessful = parseBooleanExpr();
            Boolean returnValue;
            if (this.wasSuccessful)
            {
                newTokenSetup();
                this.wasSuccessful = parseBlock();
                if (this.wasSuccessful)
                {
                    print("Valid While Statement parsed.");
                    returnValue = true;
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: Invalid while statement, invalid block.");
                    returnValue = false;
                }
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid while statement, invalid boolean expression.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseIfStatement()
        {
            newTokenSetup();
            this.wasSuccessful = parseBooleanExpr();
            Boolean returnValue;
            if (this.wasSuccessful)
            {
                newTokenSetup();
                this.wasSuccessful = parseBlock();
                if (this.wasSuccessful)
                {
                    print("Valid If Statement parsed");
                    returnValue = true;
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error Invalid if statement, invalid block.");
                    returnValue = false;
                }
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid if statement, invalid boolean expression.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseExpr()
        {
            newTokenSetup();
            Boolean returnValue;
            if (this.currentToken.match("digit"))
            {
                returnValue = parseIntExpr();
            }
            else if (this.currentToken.match("string"))
            {
                returnValue = parseStringExpr();
            }
            else if (this.currentToken.match("left_paren") || this.currentToken.match("false") || this.currentToken.match("true"))
            {
                returnValue = parseBooleanExpr();
            }
            else if (this.currentToken.match("var_id"))
            {
                returnValue = parseId();
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid expression.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseIntExpr()
        {
            this.currentToken = this.tokens[this.tokenIndex];
            Boolean returnValue;
            if (this.currentToken.match("plus_op"))
            {
                print("-Parsing token: " + this.currentToken);
                this.tokenIndex++;
                returnValue = parseExpr();
            }
            else
            {
                returnValue = true;
            }
            return returnValue;
        }

        private Boolean parseBooleanExpr()
        {
            Boolean returnValue;
            if (this.currentToken.match("left_paren"))
            {
                this.wasSuccessful = parseExpr();
                if (this.wasSuccessful)
                {
                    newTokenSetup();
                    if (this.currentToken.match("boolop_equal") || this.currentToken.match("boolop_not_equal"))
                    {
                        this.wasSuccessful = parseExpr();
                        if (this.wasSuccessful)
                        {
                            newTokenSetup();
                            this.wasSuccessful = this.currentToken.match("right_paren");
                            if (this.wasSuccessful)
                            {
                                print("Valid Boolean Expression parsed.");
                                returnValue = true;
                            }
                            else
                            {
                                this.errorCount++;
                                print("~~~Error: Invalid boolean expression, right parenthesis not found.");
                                returnValue = false;
                            }
                        }
                        else
                        {
                            this.errorCount++;
                            print("~~~Error: Invalid boolean expression, invalid token.");
                            returnValue = false;
                        }
                    }
                    else
                    {
                        this.errorCount++;
                        print("~~~Error: Invalid boolean expression, invalid boolean operator.");
                        returnValue = false;
                    }
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: Invalid boolean expression, invalid token.");
                    returnValue = false;
                }
            }
            else if (this.currentToken.match("true") || this.currentToken.match("false"))
            {
                returnValue = true;
            }
            else
            {
                this.errorCount++;
                print("~~~Error Invalid boolean expression, invalid token.");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseId()
        {
            return this.currentToken.match("var_id");
        }

        private Boolean parseStringExpr()
        {
            return this.currentToken.match("string");
        }

        // Helpers
        private Boolean matchType()
        {
            return ((this.currentToken.type.Equals("int")) || (this.currentToken.type.Equals("string")) || (this.currentToken.type.Equals("boolean")));
        }

        private Boolean checkTokensRemaining()
        {
            return this.tokenIndex < this.tokens.Count;
        }

        private void newTokenSetup()
        {
            if (checkTokensRemaining())
            {
                this.currentToken = this.tokens[this.tokenIndex++];
                print("-parsing token: " + this.currentToken);
                this.wasSuccessful = false;
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Ran out of tokens when they were expected.");
            }
        }

        private void print(Object message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }
    }
}
