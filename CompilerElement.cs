using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    class CompilerElement
    {
        protected StringBuilder outputString = new StringBuilder("");
        protected TextBox taOutput;

        public CompilerElement(TextBox taOutput)
        {
            this.taOutput = taOutput;
        }

        protected void buildPrintMessage(Object message)
        {
            this.outputString.Append(Environment.NewLine).Append(message);
        }

        protected void print()
        {
            this.taOutput.Text += this.outputString;
        }
    }
}
