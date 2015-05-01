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
            Lex lexer = new Lex(inputText, taOutput);
            lexer.analysis();

            // Creating this as it will be used in CST generation.
            List<Token> tokens = lexer.tokens;

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

                    if (symbolTable.errorCount == 0)
                    {
                        CodeGenerator codeGenerator = new CodeGenerator(ast.root, symbolTable.scopes, taOutput);
                        codeGenerator.generateCode();
                    }
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
            taInput.Focus();
        }
    }
}
