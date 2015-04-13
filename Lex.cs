using System;
using System.Collections.Generic;
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
        private static Regex isBadString = new Regex("\"[a-zA-Z ]*\"");
        private static Regex isWhiteSpace = new Regex("[\\s]");

        private static List<string> stringsToCheck = new List<string>() { "int", "string", "boolean", "print", "if", "while", "false", "true" };

        public string inputString;
        private TextBox taOutput;
        public List<Token> tokens = new List<Token>();
        public int errorCount = 0;

        public Lex(string inputString, TextBox taOutput)
        {
            // this.inputString = Regex.Replace(inputString, @"\s+", " ");
            this.inputString = inputString;
            this.taOutput = taOutput;
        }

        public void analysis()
        {
            // this.taOutput.Text += (Environment.NewLine + this.inputString);
            if (this.inputString.Length == 0)
            {
                this.errorCount++;
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
                        this.taOutput.Text += (Environment.NewLine + "symbol: \"" + currentChar + "\"");
                        if (isWhiteSpace.Match(currentChar.ToString()).Success)
                        {
                            // print("meow");
                        }
                        else if (isDigit.IsMatch(currentChar.ToString()))
                        {
                            print("-num: " + currentChar.ToString());
                            createToken(currentChar, "digit");
                        }
                        else if (isChar.IsMatch(currentChar.ToString()) && !isChar.IsMatch(getNextChar(i)))
                        {
                            print("-id: " + currentChar.ToString());
                            createToken(currentChar, "var_id");
                        }
                        else if (isBrace.IsMatch(currentChar.ToString()))
                        {
                            matchBrace(currentChar);
                        }
                        else if (isChar.IsMatch(currentChar.ToString()) && isChar.IsMatch(getNextChar(i)))
                        {
                            int offset = this.tokens.Count;
                            matchKeyWord(i);
                            if (this.tokens.Count > 0 && this.errorCount == 0)
                            {
                                Token lastToken = this.tokens[(this.tokens.Count - 1)];
                                // If this next part is true, we just had a key word. Vars are printed as they are tokenized.
                                if (!lastToken.type.Equals("var_id"))
                                {
                                    i += lastToken.value.Length - 1;
                                    print("-word: " + lastToken.value);
                                }
                                else {
                                    i += this.tokens.Count - offset - 1;
                                }
                            }
                        }
                        else if (currentChar.Equals('\"'))
                        {
                            int offset = matchString(i);
                            i += offset - 1;
                        }
                        else if (currentChar.Equals('='))
                        {
                            int offset = matchEquals(i);
                            i += offset;
                        }
                        else if (currentChar.Equals('!'))
                        {
                            int offset = matchNotEquals(i);
                            i += offset;
                        }
                        else if (currentChar.Equals('+'))
                        {
                            print("-plus_op: +");
                            createToken("+", "plus_op");
                        }
                        else if (currentChar.Equals('$'))
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
                            print("~~~Error: " + currentChar + " is not valid");
                        }
                    }
                    else
                    {
                        print("There was an error, execution stopped.");
                        i = this.inputString.Length;
                    }
                }
                if (!(this.tokens[(this.tokens.Count - 1)]).type.Equals("end_of_file") && this.errorCount == 0)
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
            StringBuilder suspectKeyword = new StringBuilder(this.inputString[index].ToString());
            // stringsToCheck
            Boolean canEscape = false;
            String nextChar;
            int offset = 0;

            while (!canEscape)
            {
                print(suspectKeyword);


                if (!stringsToCheck[0].StartsWith(suspectKeyword.ToString())
                    && !stringsToCheck[1].StartsWith(suspectKeyword.ToString())
                    && !stringsToCheck[2].StartsWith(suspectKeyword.ToString())
                    && !stringsToCheck[3].StartsWith(suspectKeyword.ToString())
                    && !stringsToCheck[4].StartsWith(suspectKeyword.ToString())
                    && !stringsToCheck[5].StartsWith(suspectKeyword.ToString())
                    && !stringsToCheck[6].StartsWith(suspectKeyword.ToString())
                    && !stringsToCheck[7].StartsWith(suspectKeyword.ToString()))
                {
                    canEscape = true;
                }
                else if (stringsToCheck.Contains(suspectKeyword.ToString()))
                {
                    canEscape = true;
                }
                else
                {
                    nextChar = getNextChar(index + offset++);
                    if (isChar.IsMatch(nextChar))
                    {
                        suspectKeyword.Append(nextChar);
                    }
                    else
                    {
                        canEscape = true;
                    }
                }
            }



            if (suspectKeyword.ToString() == "int")
            {
                createToken(suspectKeyword, "int");
            }
            else if (suspectKeyword.ToString() == "string")
            {
                createToken(suspectKeyword, "string");
            }
            else if (suspectKeyword.ToString() == "boolean")
            {
                createToken(suspectKeyword, "boolean");
            }
            else if (suspectKeyword.ToString() == "print")
            {
                createToken(suspectKeyword, "print");
            }
            else if (suspectKeyword.ToString() == "if")
            {
                createToken(suspectKeyword, "if");
            }
            else if (suspectKeyword.ToString() == "while")
            {
                createToken(suspectKeyword, "while");
            }
            else if (suspectKeyword.ToString() == "false")
            {
                createToken(suspectKeyword, "false");
            }
            else if (suspectKeyword.ToString() == "true")
            {
                createToken(suspectKeyword, "true");
            }
            else
            {
                print("-id: " + suspectKeyword[0]);
                createToken(suspectKeyword[0], "var_id");
            }
        }

        private void matchBrace(char braceChar)
        {
            if (braceChar.Equals('('))
            {
                createToken(braceChar, "left_paren");
            }
            else if (braceChar.Equals(')'))
            {
                createToken(braceChar, "right_paren");
            }
            else if (braceChar.Equals('{'))
            {
                createToken(braceChar, "left_brace");
            }
            else if (braceChar.Equals('}'))
            {
                createToken(braceChar, "right_brace");
            }
            print("-brace: " + braceChar);
        }

        private int matchString(int index)
        {
            Match match = isString.Match(this.inputString.Substring(index));
            Match badMatch = isBadString.Match(this.inputString.Substring(index));
            int offset = 0;
            if (match.Success)
            {
                string currentString = match.Value;
                createToken(currentString, "string");
                print("-string: " + currentString);
                offset = currentString.Length;
            }
            else if (badMatch.Success)
            {
                this.errorCount++;
                print("~~~Error: Invalid character Found in string.");
            }
            else
            {
                this.errorCount++;
                print("~~~Error: End of execution reached before string end or invalid non capital character found.");
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

        private void print(Object message)
        {
            this.taOutput.Text += (Environment.NewLine + message);
        }

        private void createToken(Object symbol, string name)
        {
            this.tokens.Add(new Token(symbol.ToString(), name));
        }

        private void printTokens()
        {
            for (int i = 0; i < this.tokens.Count; i++)
            {
                print(this.tokens[i]);
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

        public Boolean match(string type)
        {
            return this.type.Equals(type);
        }

        public override string ToString()
        {
            return "Type: " + this.type + " Value: " + this.value;
        }
    }
}
