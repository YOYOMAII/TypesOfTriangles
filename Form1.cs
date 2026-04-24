using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TypesOfTriangles
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtA.Text, out int a) || !int.TryParse(txtB.Text, out int b) || !int.TryParse(txtC.Text, out int c))
            {
                lblResult.Text = "Enter whole numbers for a, b, and c";
                return;
            }

            if (a + b <= c || b + c <= a || c + a <= b)
                lblResult.Text = "Not a valid triangle";
            else if (a == b && b == c)
                lblResult.Text = "Equilateral";
            else if (a == b || b == c || a == c)
                lblResult.Text = "Isosceles";
            else
                lblResult.Text = "Scalene";
        }
    }
}
