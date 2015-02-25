using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlatPiler
{
    public partial class FormFlatPiler : Form
    {
        public FormFlatPiler()
        {
            InitializeComponent();
        }

        private void buttonCompile_Click(object sender, EventArgs e)
        {
            string inputText = taInput.Text;
            taOutput.Text = "~~~Starting Lexical Analysis";
            // taOutput.Text += (Environment.NewLine + inputText);
            Lex lexer = new Lex(inputText, taOutput);
            lexer.analysis();
        }

        private void taInput_TextChanged(object sender, EventArgs e)
        {

        }

        private void taOutput_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            taInput.Text = "";
            taOutput.Text = "";
        }
    }
}
