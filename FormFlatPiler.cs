using System;
using System.Collections;
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

            // taOutput.Text += (Environment.NewLine + Environment.NewLine + ((Token)lexer.tokens[1]).ToString());

            // Creating this as it will be used in CST generation.
            ArrayList tokens = new ArrayList(lexer.tokens);

            if (lexer.errorCount == 0)
            {
                Parse parser = new Parse(tokens, taOutput);
                parser.parseProgram();
                if (parser.errorCount == 0)
                {
                    CST cst = new CST(tokens, taOutput);
                    cst.buildCST();

                    AST ast = new AST(tokens, taOutput);
                    ast.buildAST();

                    SymbolTable symbolTable = new SymbolTable(ast.root, taOutput);
                    symbolTable.generateSymbolTable();
                }
            }
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
