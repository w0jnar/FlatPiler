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
        public string inputString;
        public TextBox taOutput;
        public ArrayList tokens = new ArrayList();

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
                this.taOutput.Text += (Environment.NewLine + "~~~Error, this program is empty.");
            }
            else
            {
                char currentChar;
                // string currentString = "";
                Regex isDigit = new Regex("^[0-9]$");
                Regex isChar = new Regex("^[a-z]$");

                for (int i = 0; i < this.inputString.Length; i++) 
                {
                    currentChar = this.inputString[i];
                    // currentString += currentChar;
                    this.taOutput.Text += (Environment.NewLine + "symbol: \"" + currentChar.ToString() + "\"");

                    if (isDigit.IsMatch(currentChar.ToString())) 
                    {
                        this.taOutput.Text += (Environment.NewLine + "-num: " + currentChar.ToString());
                        this.tokens.Add(new Token(currentChar.ToString(), "digit"));
                    }
                    else if (isChar.IsMatch(currentChar.ToString()) && !isChar.IsMatch(nextChar(i)))
                    {
                        this.taOutput.Text += (Environment.NewLine + "-id: " + currentChar.ToString());
                        this.tokens.Add(new Token(currentChar.ToString(), "id"));
                    }

                }
            }
        }

        private string nextChar(int index)
        {
            char returnChar = ' ';
            if (index + 1 != this.inputString.Length) {
                returnChar = this.inputString[index + 1];
            }
            return returnChar.ToString();
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
