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
        // Controls whether the triangle should be drawn in the preview box.
        private bool drawTriangle = false;
        // Stores the last valid side lengths entered by the user.
        private double triangleA = 0;
        private double triangleB = 0;
        private double triangleC = 0;

        // Initializes the form and wires up UI events.
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Paint += new PaintEventHandler(pictureBox1_Paint);
            txtA.KeyPress += NumericTextBox_KeyPress;
            txtB.KeyPress += NumericTextBox_KeyPress;
            txtC.KeyPress += NumericTextBox_KeyPress;
        }

        // Validates input, determines triangle type, and triggers redraw.
        // Special thanks to Lin Htut Khine for writing the original triangle-type logic
        // (valid triangle check + Equilateral/Isosceles/Scalene classification) used here.
        private void btnShow_Click(object sender, EventArgs e)
        {
            drawTriangle = false;
            pictureBox1.Invalidate();

            if (!double.TryParse(txtA.Text, out double a) || !double.TryParse(txtB.Text, out double b) || !double.TryParse(txtC.Text, out double c))
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
                    lblResult.Text = "Type: Equilateral triangle";
                else if (a == b || b == c || a == c)
                    lblResult.Text = "Type: Isosceles triangle";
                else
                    lblResult.Text = "Type: Scalene triangle";

                drawTriangle = true;
                triangleA = a;
                triangleB = b;
                triangleC = c;
                pictureBox1.Invalidate();
            }
        }

        // Allows only numeric input (digits and one decimal point) in side text boxes.
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

        // Draws a scaled triangle in the picture box using the last valid side lengths.
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (drawTriangle)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                double a = triangleA;
                double b = triangleB;
                double c = triangleC;

                double xC = (b * b + c * c - a * a) / (2.0 * c);
                double yC2 = (b * b) - (xC * xC);

                if (yC2 <= 0)
                {
                    return;
                }

                double yC = Math.Sqrt(yC2);

                var pA = new PointF(0f, 0f);
                var pB = new PointF((float)c, 0f);
                var pC = new PointF((float)xC, (float)yC);

                float minX = Math.Min(pA.X, Math.Min(pB.X, pC.X));
                float maxX = Math.Max(pA.X, Math.Max(pB.X, pC.X));
                float minY = Math.Min(pA.Y, Math.Min(pB.Y, pC.Y));
                float maxY = Math.Max(pA.Y, Math.Max(pB.Y, pC.Y));

                float modelWidth = maxX - minX;
                float modelHeight = maxY - minY;
                float margin = 20f;

                float availableWidth = pictureBox1.ClientSize.Width - (margin * 2f);
                float availableHeight = pictureBox1.ClientSize.Height - (margin * 2f);

                if (modelWidth <= 0 || modelHeight <= 0 || availableWidth <= 0 || availableHeight <= 0)
                {
                    return;
                }

                float scale = Math.Min(availableWidth / modelWidth, availableHeight / modelHeight);

                Func<PointF, PointF> toScreen = p =>
                {
                    float x = (p.X - minX) * scale + margin;
                    float y = pictureBox1.ClientSize.Height - (((p.Y - minY) * scale) + margin);
                    return new PointF(x, y);
                };

                PointF sA = toScreen(pA);
                PointF sB = toScreen(pB);
                PointF sC = toScreen(pC);

                using (var fillBrush = new SolidBrush(Color.FromArgb(60, 100, 149, 237)))
                using (var pen = new Pen(Color.RoyalBlue, 2f))
                {
                    g.FillPolygon(fillBrush, new[] { sA, sB, sC });
                    g.DrawPolygon(pen, new[] { sA, sB, sC });
                }
            }
        }
    }
}
