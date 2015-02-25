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
                            this.tokens.Add(new Token(currentChar.ToString(), "digit"));
                        }
                        else if (isChar.IsMatch(currentChar.ToString()) && !isChar.IsMatch(nextChar(i)))
                        {
                            print("-id: " + currentChar.ToString());
                            this.tokens.Add(new Token(currentChar.ToString(), "id"));
                        }
                        else if (isBrace.IsMatch(currentChar.ToString()))
                        {
                            matchBrace(currentChar);
                        }
                        else if (isChar.IsMatch(currentChar.ToString()) && isChar.IsMatch(nextChar(i)))
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
                    }
                    else
                    {
                        print("There was an error, execution stopped.");
                        i = this.inputString.Length;
                    }
                }
            }
        }

        private string nextChar(int index)
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
                this.tokens.Add(new Token(suspectKeyword, "int"));
            }
            else if (suspectKeyword == "string")
            {
                this.tokens.Add(new Token(suspectKeyword, "string"));
            }
            else if (suspectKeyword == "boolean")
            {
                this.tokens.Add(new Token(suspectKeyword, "boolean"));
            }
            else if (suspectKeyword == "print")
            {
                this.tokens.Add(new Token(suspectKeyword, "print"));
            }
            else if (suspectKeyword == "if")
            {
                this.tokens.Add(new Token(suspectKeyword, "if"));
            }
            else if (suspectKeyword == "while")
            {
                this.tokens.Add(new Token(suspectKeyword, "while"));
            }
            else if (suspectKeyword == "false")
            {
                this.tokens.Add(new Token(suspectKeyword, "false"));
            }
            else if (suspectKeyword == "true")
            {
                this.tokens.Add(new Token(suspectKeyword, "true"));
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
                this.tokens.Add(new Token(braceChar.ToString(), "left_paran"));
            }
            else if (braceChar.Equals(")"))
            {
                this.tokens.Add(new Token(braceChar.ToString(), "right_paran"));
            }
            else if (braceChar.Equals("{"))
            {
                this.tokens.Add(new Token(braceChar.ToString(), "left_brace"));
            }
            else if (braceChar.Equals("}"))
            {
                this.tokens.Add(new Token(braceChar.ToString(), "right_brace"));
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
                this.tokens.Add(new Token(currentString, "string"));
                print("-string: " + currentString);
                offset = currentString.Length;
            }
            else {
                this.errorCount++;
                print("~~~Error: End of Execution reached before string end.");
            }
            return offset;
        }

        private void print(string message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
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
