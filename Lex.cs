using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class Lex
    {
        public string inputString;
        public TextBox outputField;

        public Lex(string input, TextBox output) {
            inputString = input;
            outputField = output;
        }

        public void analysis() {
            outputField.Text += (Environment.NewLine + this.inputString);
        }
    }
}
