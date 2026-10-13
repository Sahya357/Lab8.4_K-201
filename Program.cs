using System;
using System.Windows.Forms;

namespace Lab8_TrianglesWinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TriangleCollection col = new TriangleCollection();
            col.Generate(10);       // початковий набір із n = 10 трикутників

            Application.Run(new MainForm(col));
        }
    }
}
