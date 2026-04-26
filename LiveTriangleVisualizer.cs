using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace TypesOfTriangles
{
    /// <summary>
    /// Draws a live preview of the triangle defined by three side lengths typed into text boxes.
    /// </summary>
    /// <remarks>
    /// <para><b>How it plugs in</b></para>
    /// <para>
    /// The form constructs this type once and passes the same <see cref="PictureBox"/> and three
    /// <see cref="TextBox"/> controls the user edits. This class subscribes to:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>TextChanged</c> on each side box — calls <see cref="Control.Invalidate"/> on the
    /// picture box so Windows schedules a repaint as soon as the text changes.</description></item>
    /// <item><description><c>Paint</c> on the picture box — reads the current text, and either draws the
    /// triangle or a short placeholder message.</description></item>
    /// </list>
    /// <para>
    /// Validation rules for drawing match the idea of a real triangle: positive sides and the
    /// triangle inequality. Until the user presses "Show Triangle Type", the preview shows a short
    /// prompt; after that, the form and this control both follow the text boxes live.
    /// </para>
    /// <para><b>How the shape is computed</b></para>
    /// <para>
    /// Sides are named A, B, C like the labels on the form. For drawing we place vertex A at the
    /// origin, vertex B at (c, 0) so the edge between A and B has length c. Vertex C is found with
    /// the law of cosines: the angle at A between sides of length b and c gives
    /// <c>xC = (b² + c² − a²) / (2c)</c> and <c>yC = √(b² − xC²)</c> (height above the base).
    /// </para>
    /// <para><b>How it appears on screen</b></para>
    /// <para>
    /// Model coordinates are scaled uniformly to fit inside the picture box with a margin. The Y axis
    /// is flipped when mapping to pixels because GDI+ Y grows downward, while school-style diagrams
    /// usually have Y up. Each edge is labeled <c>A</c>, <c>B</c>, or <c>C</c> to match Side A / B / C
    /// on the form (the segment with length <c>a</c> is labeled A, etc.).
    /// </para>
    /// </remarks>
    internal sealed class LiveTriangleVisualizer
    {
        private readonly PictureBox pictureBox;
        private readonly TextBox txtA;
        private readonly TextBox txtB;
        private readonly TextBox txtC;
        private readonly Func<bool> isLiveUnlocked;

        /// <summary>
        /// Attaches live preview behavior to the given controls (does not remove handlers).
        /// </summary>
        /// <param name="isLiveUnlocked">When false, the preview stays on a short prompt until the user has pressed Show once (Form1 sets this).</param>
        public LiveTriangleVisualizer(PictureBox pictureBox, TextBox txtA, TextBox txtB, TextBox txtC, Func<bool> isLiveUnlocked)
        {
            this.pictureBox = pictureBox;
            this.txtA = txtA;
            this.txtB = txtB;
            this.txtC = txtC;
            this.isLiveUnlocked = isLiveUnlocked ?? (() => true);

            pictureBox.Paint += PictureBoxOnPaint;
            txtA.TextChanged += OnSideTextChanged;
            txtB.TextChanged += OnSideTextChanged;
            txtC.TextChanged += OnSideTextChanged;
        }

        /// <summary>
        /// Marks the picture box as "dirty" so the next paint pass redraws from the latest text.
        /// </summary>
        private void OnSideTextChanged(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        /// <summary>
        /// Parses three strings as doubles and returns true only if all are strictly positive.
        /// Empty or partial input fails until all three boxes contain valid numbers.
        /// </summary>
        private static bool TryParsePositiveSides(string sa, string sb, string sc, out double a, out double b, out double c)
        {
            a = b = c = 0;
            if (!double.TryParse(sa, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out a) ||
                !double.TryParse(sb, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out b) ||
                !double.TryParse(sc, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out c))
            {
                return false;
            }

            return a > 0 && b > 0 && c > 0;
        }

        /// <summary>
        /// True when the three lengths can form a non-degenerate triangle (each side less than the sum of the other two).
        /// </summary>
        private static bool IsValidTriangle(double a, double b, double c)
        {
            return a + b > c && b + c > a && c + a > b;
        }

        /// <summary>
        /// Paints the picture box: placeholder text if input is not a valid triangle; otherwise fills and outlines the triangle.
        /// </summary>
        private void PictureBoxOnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (!isLiveUnlocked())
            {
                DrawCenteredHint(g, "Press Show Triangle Type.");
                return;
            }

            // Not enough valid geometry to draw — show a simple centered hint instead of a blank box.
            if (!TryParsePositiveSides(txtA.Text, txtB.Text, txtC.Text, out double a, out double b, out double c)
                || !IsValidTriangle(a, b, c))
            {
                DrawCenteredHint(g, "Triangle shows here.");
                return;
            }

            // Third vertex C: base AB on x-axis from (0,0) to (c,0); law of cosines gives C's x, then Pythagoras on the height.
            double xC = (b * b + c * c - a * a) / (2.0 * c);
            double yC2 = (b * b) - (xC * xC);

            if (yC2 <= 0)
            {
                // Should not happen for a valid triangle with this construction; guard avoids NaN from Sqrt.
                return;
            }

            double yC = Math.Sqrt(yC2);

            var pA = new PointF(0f, 0f);
            var pB = new PointF((float)c, 0f);
            var pC = new PointF((float)xC, (float)yC);

            // Bounding box of the three vertices in model space — used to center and scale the shape.
            float minX = Math.Min(pA.X, Math.Min(pB.X, pC.X));
            float maxX = Math.Max(pA.X, Math.Max(pB.X, pC.X));
            float minY = Math.Min(pA.Y, Math.Min(pB.Y, pC.Y));
            float maxY = Math.Max(pA.Y, Math.Max(pB.Y, pC.Y));

            float modelWidth = maxX - minX;
            float modelHeight = maxY - minY;
            float margin = 20f;

            float availableWidth = pictureBox.ClientSize.Width - (margin * 2f);
            float availableHeight = pictureBox.ClientSize.Height - (margin * 2f);

            if (modelWidth <= 0 || modelHeight <= 0 || availableWidth <= 0 || availableHeight <= 0)
            {
                return;
            }

            // Uniform scale so the whole triangle fits; same factor on X and Y preserves angles.
            float scale = Math.Min(availableWidth / modelWidth, availableHeight / modelHeight);

            // Map model (y up) to control client pixels (y down).
            PointF ToScreen(PointF p)
            {
                float x = (p.X - minX) * scale + margin;
                float y = pictureBox.ClientSize.Height - (((p.Y - minY) * scale) + margin);
                return new PointF(x, y);
            }

            PointF sA = ToScreen(pA);
            PointF sB = ToScreen(pB);
            PointF sC = ToScreen(pC);

            using (var fillBrush = new SolidBrush(Color.FromArgb(60, 100, 149, 237)))
            using (var pen = new Pen(Color.RoyalBlue, 2f))
            {
                g.FillPolygon(fillBrush, new[] { sA, sB, sC });
                g.DrawPolygon(pen, new[] { sA, sB, sC });
            }

            // Label each edge to match Side A / B / C on the form (lengths a, b, c in the model).
            // In this layout: |AB| = c → Side C, |AC| = b → Side B, |BC| = a → Side A.
            var centroid = new PointF((sA.X + sB.X + sC.X) / 3f, (sA.Y + sB.Y + sC.Y) / 3f);
            const float labelOffset = 16f;
            using (var labelFont = new Font("Segoe UI", 10.5f, FontStyle.Bold, GraphicsUnit.Point))
            using (var labelBrush = new SolidBrush(Color.FromArgb(245, 30, 64, 175)))
            {
                DrawEdgeSideLabel(g, labelFont, labelBrush, "A", sB, sC, centroid, labelOffset);
                DrawEdgeSideLabel(g, labelFont, labelBrush, "B", sA, sC, centroid, labelOffset);
                DrawEdgeSideLabel(g, labelFont, labelBrush, "C", sA, sB, centroid, labelOffset);
            }
        }

        private void DrawCenteredHint(Graphics g, string hint)
        {
            using (var hintFont = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Point))
            using (var brush = new SolidBrush(Color.FromArgb(180, 71, 85, 105)))
            {
                var rect = new RectangleF(16f, 16f, pictureBox.ClientSize.Width - 32f, pictureBox.ClientSize.Height - 32f);
                var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(hint, hintFont, brush, rect, format);
            }
        }

        /// <summary>
        /// Draws a side letter (A/B/C) near the middle of an edge, nudged outward away from the triangle center.
        /// </summary>
        private static void DrawEdgeSideLabel(Graphics g, Font font, Brush brush, string text, PointF v0, PointF v1, PointF insideRef, float offset)
        {
            float mx = (v0.X + v1.X) * 0.5f;
            float my = (v0.Y + v1.Y) * 0.5f;
            float dx = v1.X - v0.X;
            float dy = v1.Y - v0.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-3f)
            {
                return;
            }

            // Unit perpendicular to the edge (one of the two outward normals).
            float nx = -dy / len;
            float ny = dx / len;

            // Pick the normal that points away from the centroid so labels sit outside the triangle.
            float vx = insideRef.X - mx;
            float vy = insideRef.Y - my;
            if (nx * vx + ny * vy > 0f)
            {
                nx = -nx;
                ny = -ny;
            }

            float px = mx + nx * offset;
            float py = my + ny * offset;

            SizeF sz = g.MeasureString(text, font);
            g.DrawString(text, font, brush, px - sz.Width * 0.5f, py - sz.Height * 0.5f);
        }
    }
}
