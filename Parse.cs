using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class Parse
    {
        public ArrayList tokens;
        private TextBox taOutput;
        public int errorCount = 0;
        private int tokenIndex = 0;
        private Token currentToken;
        private Boolean wasSuccessful;

        public Parse(ArrayList tokens, TextBox taOutput)
        {
            this.tokens = new ArrayList(tokens);
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
                this.wasSuccessful = match("end_of_file");
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

            this.wasSuccessful = match("left_brace");
            if (this.wasSuccessful)
            {
                print("-Parsing Block");
                this.wasSuccessful = false;
                this.wasSuccessful = parseStatementList();
                if (this.wasSuccessful)
                {
                    this.wasSuccessful = match("right_brace");
                    if (this.wasSuccessful)
                    {
                        print("-Finished Parsing Block");
                        returnValue = true;

                    }
                    else
                    {
                        this.errorCount++;
                        print("~~~Error: program block improperly ended.");
                        returnValue = false;
                    }
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: program block improperly ended.");
                    returnValue = false;
                }
            }
            else
            {
                this.errorCount++;
                print("~~~Error: program improperly ended.");
                returnValue = false;
            }

            return returnValue;
        }

        private Boolean parseStatementList()
        {
            newTokenSetup();
            this.wasSuccessful = true;
            while (!match("right_brace") || this.errorCount != 0)
            {
                this.wasSuccessful = parseStatement();
                if (this.wasSuccessful)
                {
                    newTokenSetup();
                    this.wasSuccessful = true; //work around, as otherwise, if everything went well, it would always return false.
                }
                else
                {
                    this.errorCount++;
                    print("~~~Error: statement improperly parsed");
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
                print("~~~Parse ending due to Error(s)");
                returnValue = false;
            }
            return returnValue;
        }

        private Boolean parseStatement()
        {
            return true;
        }

        private Boolean parseId()
        {
            return match("id");
        }

        private Boolean parseStringExpr()
        {
            return match("string");
        }

        // Helpers
        private Boolean match(string type)
        {
            return this.currentToken.type.Equals(type);
        }

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
                this.currentToken = (Token)this.tokens[this.tokenIndex++];
                print("-parsing token: " + this.currentToken.ToString());
                this.wasSuccessful = false;
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Ran out of tokens when they were expected.");
            }
        }

        private void print(string message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }
    }
}
