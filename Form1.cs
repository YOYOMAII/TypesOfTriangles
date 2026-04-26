// Form1 — main window for "Types of Triangles".
//
// Responsibilities:
//   • Read three side lengths from txtA, txtB, txtC.
//   • On "Show Triangle Type", validate them and show the classification on lblResult
//     (Equilateral / Isosceles / Scalene, or an error message).
//   • Restrict side text boxes to numeric input (digits + one decimal point).
//
// The live drawing in pictureBox1 is not handled here. It is owned by LiveTriangleVisualizer,
// which subscribes to the same text boxes and the picture box so the sketch updates as the
// user types. That keeps this file focused on the triangle-type logic and messages.

using System;
using System.Windows.Forms;

namespace TypesOfTriangles
{
    public partial class Form1 : Form
    {
        // Keeps the visualizer alive for the lifetime of the form and makes the dependency obvious.
        // The visualizer registers Paint and TextChanged handlers on the controls passed in.
        private readonly LiveTriangleVisualizer liveTriangleVisualizer;

        public Form1()
        {
            InitializeComponent();

            // Wire the preview: whenever A/B/C text changes, the visualizer invalidates the picture box
            // and repaints from the current text (see LiveTriangleVisualizer).
            liveTriangleVisualizer = new LiveTriangleVisualizer(pictureBox1, txtA, txtB, txtC);

            // Only allow reasonable characters while typing sides (no letters, symbols, etc.).
            txtA.KeyPress += NumericTextBox_KeyPress;
            txtB.KeyPress += NumericTextBox_KeyPress;
            txtC.KeyPress += NumericTextBox_KeyPress;
        }

        // Validates input, determines triangle type, and updates lblResult only.
        //
        // Special thanks to Lin Htut Khine for writing the original triangle-type logic
        // (valid triangle check + Equilateral/Isosceles/Scalene classification) used here.
        //
        // Flow:
        //   1) Parse all three sides as doubles. If any fails, ask for valid numbers.
        //   2) Reject zero or negative lengths (lengths must be positive).
        //   3) Triangle inequality: each side must be shorter than the sum of the other two.
        //      If not, the three lengths cannot form a closed triangle in flat geometry.
        //   4) If valid: classify by how many sides are equal — all three (equilateral),
        //      exactly two (isosceles), or all different (scalene).
        //
        // Note: This handler does not paint pictureBox1. The preview is driven separately by
        // LiveTriangleVisualizer whenever the text changes.
        private void btnShow_Click(object sender, EventArgs e)
        {
            // Step 1 — every box must contain a number the runtime can parse.
            if (!double.TryParse(txtA.Text, out double a) || !double.TryParse(txtB.Text, out double b) || !double.TryParse(txtC.Text, out double c))
            {
                lblResult.Text = "Please enter valid numbers for Side A, Side B, and Side C.";
                return;
            }

            // Step 2 — side lengths of a non-degenerate triangle are strictly greater than zero.
            if (a <= 0 || b <= 0 || c <= 0)
            {
                lblResult.Text = "All sides must be greater than 0.";
                return;
            }

            // Step 3 — triangle inequality (strict): e.g. a + b must be greater than c, and cyclically for the other pairs.
            if (a + b <= c || b + c <= a || c + a <= b)
            {
                lblResult.Text = "Not a valid triangle (sum of any 2 sides must be greater than the 3rd).";
            }
            else
            {
                // Step 4 — classification by equal sides (floating-point equality: values come from the same user input).
                if (a == b && b == c)
                    lblResult.Text = "Type: Equilateral triangle";
                else if (a == b || b == c || a == c)
                    lblResult.Text = "Type: Isosceles triangle";
                else
                    lblResult.Text = "Type: Scalene triangle";
            }
        }

        // Shared handler for txtA, txtB, txtC. Runs before the character is committed to the text box.
        //
        // Rules:
        //   • Allow control characters (Backspace, Enter, arrows, etc.) so editing still works.
        //   • Allow digits 0–9 and a single '.' for decimals.
        //   • Block everything else by setting e.Handled = true so the key is not inserted.
        //   • If the user already typed a dot, reject a second dot in the same box.
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
