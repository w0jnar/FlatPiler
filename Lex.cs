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
            this.inputString = Regex.Replace(inputString, @"\s+", "");
            this.taOutput = taOutput;
        }

        public void analysis()
        {
            // taOutput.Text += (Environment.NewLine + this.inputString);
            if (this.inputString.Length == 0)
            {
                taOutput.Text += (Environment.NewLine + "~~~Error, this program is empty.");
            }
            else
            {
                char currentChar;
                string currentString = "";
                Regex isDigit = new Regex("^[0-9]*$");

                for (int i = 0; i < this.inputString.Length; i++) 
                {
                    currentChar = this.inputString[i];
                    // currentString += currentChar;
                    if (isDigit.IsMatch(currentChar.ToString())) 
                    {
                        taOutput.Text += (Environment.NewLine + "num");
                    }

                }
            }
        }
    }
    class Token
    {
        public string type;
        public string value;

        public Token(string token, string type, int lineNumber, int position)
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
