// Form1 — main window for "Types of Triangles".
//
// Until the user clicks "Show Triangle Type" once, lblResult and the diagram stay on the default
// prompts. After that, both update live as sides change. Reset clears sides and locks again.

using System;
using System.Globalization;
using System.Windows.Forms;

namespace TypesOfTriangles
{
    public partial class Form1 : Form
    {
        private readonly LiveTriangleVisualizer liveTriangleVisualizer;
        private bool liveUnlocked;

        public Form1()
        {
            InitializeComponent();
            liveTriangleVisualizer = new LiveTriangleVisualizer(pictureBox1, txtA, txtB, txtC, () => liveUnlocked);

            txtA.KeyPress += NumericTextBox_KeyPress;
            txtB.KeyPress += NumericTextBox_KeyPress;
            txtC.KeyPress += NumericTextBox_KeyPress;

            txtA.TextChanged += SideText_TextChanged;
            txtB.TextChanged += SideText_TextChanged;
            txtC.TextChanged += SideText_TextChanged;
        }

        private void SideText_TextChanged(object sender, EventArgs e)
        {
            if (liveUnlocked)
            {
                RefreshTriangleTypeResult();
            }
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            liveUnlocked = true;
            RefreshTriangleTypeResult();
            pictureBox1.Invalidate();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            liveUnlocked = false;
            txtA.Clear();
            txtB.Clear();
            txtC.Clear();
            lblResult.Text = IdleResultText();
            txtA.Focus();
            pictureBox1.Invalidate();
        }

        private static string IdleResultText()
        {
            return "Result will appear here";
        }

        // Special thanks to Lin Htut Khine for the original triangle-type logic used here.
        private void RefreshTriangleTypeResult()
        {
            string sa = txtA.Text.Trim();
            string sb = txtB.Text.Trim();
            string sc = txtC.Text.Trim();

            if (sa.Length == 0 && sb.Length == 0 && sc.Length == 0)
            {
                lblResult.Text = IdleResultText();
                return;
            }

            if (!double.TryParse(sa, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double a) ||
                !double.TryParse(sb, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double b) ||
                !double.TryParse(sc, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double c))
            {
                lblResult.Text = "Please enter valid numbers for Side A, Side B, and Side C.";
                return;
            }

            if (a <= 0 || b <= 0 || c <= 0)
            {
                lblResult.Text = "All sides must be greater than 0.";
                return;
            }

            if (a + b <= c || b + c <= a || c + a <= b)
            {
                lblResult.Text = "Not a valid triangle (sum of any 2 sides must be greater than the 3rd).";
            }
            else
            {
                if (a == b && b == c)
                {
                    lblResult.Text = "Type: Equilateral triangle";
                }
                else if (a == b || b == c || a == c)
                {
                    lblResult.Text = "Type: Isosceles triangle";
                }
                else
                {
                    lblResult.Text = "Type: Scalene triangle";
                }
            }
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '.' && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }
    }
}
