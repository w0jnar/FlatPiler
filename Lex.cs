using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class Lex
    {
        private static Regex isDigit = new Regex("^[0-9]$");
        private static Regex isChar = new Regex("^[a-z]$");
        private static Regex isBrace = new Regex("^[\\{\\}\\(\\)]$");
        private static Regex isString = new Regex("\"[a-z ]*\"");

        public string inputString;
        public TextBox taOutput;
        public ArrayList tokens = new ArrayList();
        public int errorCount = 0;

        public Lex(string inputString, TextBox taOutput)
        {
            this.inputString = Regex.Replace(inputString, @"\s+", " ");
            this.taOutput = taOutput;
        }

        public void analysis()
        {
            this.taOutput.Text += (Environment.NewLine + this.inputString);
            if (this.inputString.Length == 0)
            {
                print("~~~Error, this program is empty.");
            }
            else
            {
                char currentChar;
                // string currentString = "";
                for (int i = 0; i < this.inputString.Length; i++)
                {
                    if (this.errorCount == 0)
                    {
                        currentChar = this.inputString[i];
                        // currentString += currentChar;
                        this.taOutput.Text += (Environment.NewLine + "symbol: \"" + currentChar.ToString() + "\"");
                        if (currentChar.ToString().Equals(" "))
                        {
                        }
                        else if (isDigit.IsMatch(currentChar.ToString()))
                        {
                            print("-num: " + currentChar.ToString());
                            createToken(currentChar.ToString(), "digit");
                        }
                        else if (isChar.IsMatch(currentChar.ToString()) && !isChar.IsMatch(getNextChar(i)))
                        {
                            print("-id: " + currentChar.ToString());
                            createToken(currentChar.ToString(), "id");
                        }
                        else if (isBrace.IsMatch(currentChar.ToString()))
                        {
                            matchBrace(currentChar);
                        }
                        else if (isChar.IsMatch(currentChar.ToString()) && isChar.IsMatch(getNextChar(i)))
                        {
                            matchKeyWord(i);
                            if (this.tokens.Count > 0 && this.errorCount == 0)
                            {
                                string lastTokenValue = ((Token)this.tokens[(this.tokens.Count - 1)]).value;
                                i += lastTokenValue.Length - 1;
                                print("-word: " + lastTokenValue);
                            }
                        }
                        else if (currentChar.ToString().Equals("\""))
                        {
                            int offset = matchString(i);
                            i += offset - 1;
                        }
                        else if (currentChar.ToString().Equals("="))
                        {
                            int offset = matchEquals(i);
                            i += offset;
                        }
                        else if (currentChar.ToString().Equals("!"))
                        {
                            int offset = matchNotEquals(i);
                            i += offset;
                        }
                        else if (currentChar.ToString().Equals("+"))
                        {
                            print("-plus_op: +");
                            createToken("+", "plus_op");
                        }
                        else if (currentChar.ToString().Equals("$"))
                        {
                            print("-eof: $");
                            createToken("$", "end_of_file");
                            if (i != this.inputString.Length - 1)
                            {
                                print("~~~Warning, there is code after the EOF character is reached.");
                            }
                        }
                        else
                        {
                            this.errorCount++;
                            print("~~~Error: " + currentChar.ToString() + "is not valid");
                        }
                    }
                    else
                    {
                        print("There was an error, execution stopped.");
                        i = this.inputString.Length;
                    }
                }
                if (!((Token)this.tokens[(this.tokens.Count - 1)]).type.Equals("end_of_file"))
                {
                    print("~~~Warning, end of file is reached before an EOF character, appending one now.");
                    createToken("$", "end_of_file");
                }

                if (this.errorCount == 0)
                {
                    print("---Lex Finished Successfully. Nice!");

                    // printTokens();
                }
            }
        }

        private string getNextChar(int index)
        {
            char returnChar = ' ';
            if (index + 1 != this.inputString.Length)
            {
                returnChar = this.inputString[index + 1];
            }
            return returnChar.ToString();
        }

        private int nextWordIndex(int index)
        {
            int offset = index;
            while (offset < this.inputString.Length && isChar.IsMatch(this.inputString[offset].ToString()))
            {
                offset++;
            }
            return offset - index;
        }

        private void matchKeyWord(int index)
        {
            int offset = nextWordIndex(index);
            string suspectKeyword = this.inputString.Substring(index, offset);
            // this.taOutput.Text += (Environment.NewLine + "symbol: \"" + suspectKeyword + "\"");
            // this.tokens.Add(new Token(suspectKeyword, "meow"));
            if (suspectKeyword == "int")
            {
                createToken(suspectKeyword, "int");
            }
            else if (suspectKeyword == "string")
            {
                createToken(suspectKeyword, "string");
            }
            else if (suspectKeyword == "boolean")
            {
                createToken(suspectKeyword, "boolean");
            }
            else if (suspectKeyword == "print")
            {
                createToken(suspectKeyword, "print");
            }
            else if (suspectKeyword == "if")
            {
                createToken(suspectKeyword, "if");
            }
            else if (suspectKeyword == "while")
            {
                createToken(suspectKeyword, "while");
            }
            else if (suspectKeyword == "false")
            {
                createToken(suspectKeyword, "false");
            }
            else if (suspectKeyword == "true")
            {
                createToken(suspectKeyword, "true");
            }
            else
            {
                print("~~~Error: " + suspectKeyword + " is not valid");
                this.errorCount++;
            }
        }

        private void matchBrace(char braceChar)
        {
            if (braceChar.Equals("("))
            {
                createToken(braceChar.ToString(), "left_paran");
            }
            else if (braceChar.Equals(")"))
            {
                createToken(braceChar.ToString(), "right_paran");
            }
            else if (braceChar.Equals("{"))
            {
                createToken(braceChar.ToString(), "left_brace");
            }
            else if (braceChar.Equals("}"))
            {
                createToken(braceChar.ToString(), "right_brace");
            }
            print("-brace: " + braceChar.ToString());
        }

        private int matchString(int index)
        {
            Match match = isString.Match(this.inputString.Substring(index));
            // print("Match: " + match.Success);
            // print(this.inputString.Substring(index));
            int offset = 0;
            if (match.Success)
            {
                string currentString = match.Value;
                createToken(currentString, "string");
                print("-string: " + currentString);
                offset = currentString.Length;
            }
            else
            {
                this.errorCount++;
                print("~~~Error: End of Execution reached before string end.");
            }
            return offset;
        }

        private int matchEquals(int index)
        {
            string nextChar = getNextChar(index);
            int offset = 0;
            if (nextChar.Equals("="))
            {
                createToken("==", "boolop_equal");
                print("-boolop_equal: ==");
                offset++;
            }
            else
            {
                createToken("=", "assignment_op");
                print("-assignment_op: =");
            }
            return offset;
        }

        private int matchNotEquals(int index)
        {
            string nextChar = getNextChar(index);
            int offset = 0;
            if (nextChar.Equals("="))
            {
                createToken("!=", "boolop_not_equal");
                print("-boolop_not_equal: !=");
                offset++;
            }
            else
            {
                this.errorCount++;
                print("~~~Error: Invalid character after a !.");
            }
            return offset;
        }

        private void print(string message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }

        private void createToken(string symbol, string name)
        {
            this.tokens.Add(new Token(symbol, name));
        }

        private void printTokens()
        {
            for (int i = 0; i < this.tokens.Count; i++)
            {
                print(((Token)this.tokens[i]).ToString());
            }
        }
    }
    class Token
    {
        public string type;
        public string value;

        public Token(string token, string type)
        {
            this.type = type;
            this.value = token;
        }

        public override string ToString()
        {
            return "Type: " + this.type + " Value: " + this.value;
        }
    }
}
