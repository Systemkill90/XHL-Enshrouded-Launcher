using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace XHLLauncher
{
    public static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(20, 21, 23);
        public static readonly Color Sidebar = Color.FromArgb(24, 25, 27);
        public static readonly Color Panel = Color.FromArgb(29, 30, 32);
        public static readonly Color Panel2 = Color.FromArgb(24, 25, 27);
        public static readonly Color Panel3 = Color.FromArgb(38, 38, 38);
        public static readonly Color Border = Color.FromArgb(63, 61, 56);
        public static readonly Color Text = Color.FromArgb(231, 226, 214);
        public static readonly Color Muted = Color.FromArgb(170, 167, 157);
        public static readonly Color Muted2 = Color.FromArgb(140, 137, 129);
        public static readonly Color Cyan = Color.FromArgb(190, 164, 112);
        public static readonly Color Teal = Color.FromArgb(159, 173, 153);
        public static readonly Color Green = Color.FromArgb(157, 183, 148);
        public static readonly Color Amber = Color.FromArgb(211, 178, 119);
        public static readonly Color Orange = Color.FromArgb(199, 147, 106);
        public static readonly Color Red = Color.FromArgb(205, 133, 125);
        public static readonly Color Purple = Color.FromArgb(171, 159, 181);


        public static Image LoadAsset(string appBase, string fileName)
        {
            try
            {
                string path = Path.Combine(appBase, "assets", fileName);
                if (!File.Exists(path)) return null;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image src = Image.FromStream(fs))
                    return new Bitmap(src);
            }
            catch { return null; }
        }

        public static void DrawImageCover(Graphics g, Image image, Rectangle bounds)
        {
            if (image == null || bounds.Width <= 0 || bounds.Height <= 0) return;
            float scale = Math.Max((float)bounds.Width / image.Width, (float)bounds.Height / image.Height);
            int w = Math.Max(1, (int)Math.Ceiling(image.Width * scale));
            int h = Math.Max(1, (int)Math.Ceiling(image.Height * scale));
            int x = bounds.Left + (bounds.Width - w) / 2;
            int y = bounds.Top + (bounds.Height - h) / 2;
            g.DrawImage(image, new Rectangle(x, y, w, h));
        }

        public static Font Font(float size, FontStyle style)
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Point);
        }

        public static Button Button(string text, Color accent)
        {
            Button b = new Button();
            b.Text = text;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(Math.Min(255, accent.R + 5), Math.Min(255, accent.G + 5), Math.Min(255, accent.B + 5));
            b.BackColor = Color.FromArgb(34, 35, 36);
            b.ForeColor = Text;
            b.Font = Font(9.5f, FontStyle.Bold);
            b.Height = 36;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            b.MouseEnter += delegate { b.BackColor = Color.FromArgb(46, 46, 44); };
            b.MouseLeave += delegate { b.BackColor = Color.FromArgb(34, 35, 36); };
            return b;
        }

        public static Label Label(string text, float size, Color color, FontStyle style)
        {
            Label l = new Label();
            l.Text = text;
            l.ForeColor = color;
            l.Font = Font(size, style);
            l.AutoSize = true;
            l.BackColor = Color.Transparent;
            return l;
        }

        public static Panel Card()
        {
            RoundedPanel p = new RoundedPanel();
            p.BackColor = Panel;
            p.BorderColor = Border;
            p.Radius = 3;
            p.Padding = new Padding(14);
            return p;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Panel2;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(38, 38, 38);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = Font(9f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(38, 38, 38);
            grid.DefaultCellStyle.BackColor = Panel2;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(66, 61, 49);
            grid.DefaultCellStyle.SelectionForeColor = Text;
            grid.DefaultCellStyle.Font = Font(9f, FontStyle.Regular);
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        public static void StyleTextBox(TextBox tb)
        {
            tb.BackColor = Color.FromArgb(24, 25, 27);
            tb.ForeColor = Text;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Font = Font(9f, FontStyle.Regular);
        }

        public static Color HealthColor(int score)
        {
            if (score >= 90) return Green;
            if (score >= 75) return Teal;
            if (score >= 55) return Amber;
            if (score >= 35) return Orange;
            return Red;
        }

        public static string HealthLabel(int score)
        {
            if (score >= 90) return "EXCELLENT";
            if (score >= 75) return "GOOD";
            if (score >= 55) return "FAIR";
            if (score >= 35) return "POOR";
            return "CRITICAL";
        }
    }

    public class RoundedPanel : Panel
    {
        public int Radius = 10;
        public Color BorderColor = Theme.Border;
        public int BorderWidth = 1;
        public Image BackgroundArt = null;
        public int BackgroundOverlayAlpha = 178;
        public RoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
            if (r.Width <= 0 || r.Height <= 0) return;
            using (GraphicsPath gp = RoundedRect(r, Radius))
            {
                GraphicsState state = e.Graphics.Save();
                e.Graphics.SetClip(gp);
                if (BackgroundArt != null)
                {
                    Theme.DrawImageCover(e.Graphics, BackgroundArt, r);
                    using (SolidBrush shade = new SolidBrush(Color.FromArgb(Math.Max(0, Math.Min(255, BackgroundOverlayAlpha)), 6, 13, 20)))
                        e.Graphics.FillRectangle(shade, r);
                }
                else
                {
                    using (SolidBrush br = new SolidBrush(BackColor)) e.Graphics.FillRectangle(br, r);
                }
                e.Graphics.Restore(state);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
            if (r.Width <= 0 || r.Height <= 0) return;
            using (GraphicsPath gp = RoundedRect(r, Radius))
            using (Pen pen = new Pen(BorderColor, BorderWidth))
                e.Graphics.DrawPath(pen, gp);
            base.OnPaint(e);
        }
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class HeaderPanel : Panel
    {
        public Image BackgroundArt = null;
        public HeaderPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle r = ClientRectangle;
            if (BackgroundArt != null)
            {
                Theme.DrawImageCover(e.Graphics, BackgroundArt, r);
                using (LinearGradientBrush shade = new LinearGradientBrush(r, Color.FromArgb(225, 7, 14, 21), Color.FromArgb(105, 7, 14, 21), 0f))
                    e.Graphics.FillRectangle(shade, r);
                using (SolidBrush lower = new SolidBrush(Color.FromArgb(65, 5, 10, 16)))
                    e.Graphics.FillRectangle(lower, new Rectangle(0, r.Height / 2, r.Width, r.Height / 2));
            }
            else
            {
                using (LinearGradientBrush b = new LinearGradientBrush(r, Color.FromArgb(18, 42, 58), Color.FromArgb(24, 25, 27), 0f))
                    e.Graphics.FillRectangle(b, r);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            using (Pen p = new Pen(Color.FromArgb(52, 91, 116), 1)) e.Graphics.DrawLine(p, 0, Height - 1, Width, Height - 1);
            using (Pen accent = new Pen(Color.FromArgb(70, Theme.Cyan), 2)) e.Graphics.DrawLine(accent, 0, 0, Math.Min(420, Width), 0);
            base.OnPaint(e);
        }
    }

    public class MetricCard : RoundedPanel
    {
        private Label title;
        private Label value;
        private Label sub;
        private MiniChart chart;
        public MetricCard(string titleText, Color accent)
        {
            Height = 86;
            BackColor = Theme.Panel;
            BorderColor = Theme.Border;
            Radius = 3;
            Padding = new Padding(12, 9, 12, 8);
            title = Theme.Label(titleText.ToUpperInvariant(), 8.2f, Theme.Muted, FontStyle.Bold);
            title.Location = new Point(12, 9);
            value = Theme.Label("--", 15.5f, Theme.Text, FontStyle.Bold);
            value.Location = new Point(12, 28);
            sub = Theme.Label("", 7.8f, Theme.Muted2, FontStyle.Regular);
            sub.Location = new Point(12, 57);
            chart = new MiniChart();
            chart.LineColor = accent;
            chart.Location = new Point(10, 68);
            chart.Height = 12;
            chart.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(title); Controls.Add(value); Controls.Add(sub); Controls.Add(chart);
            Resize += delegate { chart.Width = Math.Max(20, Width - 20); };
        }
        public void Set(string v, string s)
        {
            value.Text = v; sub.Text = s == null ? "" : s;
        }
        public MiniChart Chart { get { return chart; } }
    }

    public class MiniChart : Control
    {
        public Color LineColor = Theme.Cyan;
        private readonly System.Collections.Generic.List<double> values = new System.Collections.Generic.List<double>();
        public MiniChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }
        public void Add(double v)
        {
            values.Add(v);
            while (values.Count > 120) values.RemoveAt(0);
            Invalidate();
        }
        public void SetValues(System.Collections.Generic.IEnumerable<double> vals)
        {
            values.Clear(); values.AddRange(vals); while (values.Count > 120) values.RemoveAt(0); Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (values.Count < 2 || Width < 4 || Height < 4) return;
            double max = 1.0;
            foreach (double v in values) if (v > max) max = v;
            PointF[] pts = new PointF[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                float x = (float)i * (Width - 2) / (values.Count - 1);
                float y = (float)(Height - 2 - ((values[i] / max) * (Height - 4)));
                pts[i] = new PointF(x + 1, y + 1);
            }
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(LineColor, 1.5f)) e.Graphics.DrawLines(p, pts);
        }
    }

    public class GaugeControl : Control
    {
        private int score = 100;
        private string caption = "EXCELLENT";
        public int Score { get { return score; } set { score = Math.Max(0, Math.Min(100, value)); caption = Theme.HealthLabel(score); Invalidate(); } }
        public string Caption { get { return caption; } set { caption = value; Invalidate(); } }
        public GaugeControl()
        {
            DoubleBuffered = true;
            Size = new Size(150, 100);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF arc = new RectangleF(10, 10, Width - 20, Width - 20);
            float start = 180f;
            float sweep = 180f;
            using (Pen bg = new Pen(Color.FromArgb(38, 55, 69), 10))
            {
                bg.StartCap = LineCap.Round; bg.EndCap = LineCap.Round;
                e.Graphics.DrawArc(bg, arc, start, sweep);
            }
            using (Pen fg = new Pen(Theme.HealthColor(score), 10))
            {
                fg.StartCap = LineCap.Round; fg.EndCap = LineCap.Round;
                e.Graphics.DrawArc(fg, arc, start, sweep * score / 100f);
            }
            using (Font f = Theme.Font(21f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Theme.Text))
            {
                string s = score.ToString();
                SizeF z = e.Graphics.MeasureString(s, f);
                e.Graphics.DrawString(s, f, b, (Width - z.Width) / 2, 42);
            }
            using (Font f = Theme.Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Theme.HealthColor(score)))
            {
                SizeF z = e.Graphics.MeasureString(caption, f);
                e.Graphics.DrawString(caption, f, b, (Width - z.Width) / 2, 72);
            }
        }
    }

    public class LineChart : Control
    {
        public Color LineColor = Theme.Cyan;
        public string Title = "";
        public string Unit = "";
        public double Ceiling = 0;
        private readonly System.Collections.Generic.List<double> values = new System.Collections.Generic.List<double>();
        public LineChart() { DoubleBuffered = true; BackColor = Theme.Panel2; MinimumSize = new Size(100, 70); }
        public void Add(double v) { values.Add(v); while (values.Count > 240) values.RemoveAt(0); Invalidate(); }
        public void SetValues(System.Collections.Generic.IEnumerable<double> vals) { values.Clear(); values.AddRange(vals); while (values.Count > 240) values.RemoveAt(0); Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Font f = Theme.Font(8f, FontStyle.Bold)) using (SolidBrush b = new SolidBrush(Theme.Muted)) e.Graphics.DrawString(Title.ToUpperInvariant(), f, b, 10, 7);
            double last = values.Count > 0 ? values[values.Count - 1] : 0;
            using (Font f = Theme.Font(12f, FontStyle.Bold)) using (SolidBrush b = new SolidBrush(Theme.Text))
            {
                string s = last.ToString(last >= 100 ? "0" : "0.0") + Unit;
                SizeF z = e.Graphics.MeasureString(s, f); e.Graphics.DrawString(s, f, b, Width - z.Width - 10, 4);
            }
            Rectangle plot = new Rectangle(10, 31, Math.Max(10, Width - 20), Math.Max(10, Height - 41));
            using (Pen gp = new Pen(Color.FromArgb(28, 49, 64)))
            {
                for (int i = 0; i < 4; i++) { int y = plot.Top + i * plot.Height / 3; e.Graphics.DrawLine(gp, plot.Left, y, plot.Right, y); }
            }
            if (values.Count < 2) return;
            double max = Ceiling > 0 ? Ceiling : 1.0;
            foreach (double v in values) if (Ceiling <= 0 && v > max) max = v;
            PointF[] pts = new PointF[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                double vv = values[i]; if (vv > max) vv = max;
                float x = plot.Left + (float)i * plot.Width / (values.Count - 1);
                float y = plot.Bottom - (float)(vv / max * plot.Height);
                pts[i] = new PointF(x, y);
            }
            using (Pen p = new Pen(LineColor, 1.6f)) e.Graphics.DrawLines(p, pts);
        }
    }

    public class BarChart : Control
    {
        public Color BarColor = Theme.Cyan;
        public string Title = "";
        public string[] Labels = new string[0];
        public double[] Values = new double[0];
        public BarChart() { DoubleBuffered = true; BackColor = Theme.Panel2; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Font f = Theme.Font(8f, FontStyle.Bold)) using (SolidBrush b = new SolidBrush(Theme.Muted)) e.Graphics.DrawString(Title.ToUpperInvariant(), f, b, 10, 7);
            Rectangle plot = new Rectangle(10, 28, Math.Max(10, Width - 20), Math.Max(10, Height - 49));
            double max = 1;
            for (int i = 0; i < Values.Length; i++) if (Values[i] > max) max = Values[i];
            int count = Math.Max(1, Values.Length);
            float cell = (float)plot.Width / count;
            for (int i = 0; i < Values.Length; i++)
            {
                float h = (float)(Values[i] / max * plot.Height);
                RectangleF br = new RectangleF(plot.Left + i * cell + cell * .16f, plot.Bottom - h, cell * .68f, h);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(180, BarColor))) e.Graphics.FillRectangle(b, br);
                if (Labels != null && i < Labels.Length && count <= 16)
                {
                    using (Font f = Theme.Font(7f, FontStyle.Regular)) using (SolidBrush tb = new SolidBrush(Theme.Muted2))
                    {
                        string t = Labels[i]; SizeF z = e.Graphics.MeasureString(t, f); e.Graphics.DrawString(t, f, tb, br.Left + (br.Width - z.Width) / 2, plot.Bottom + 3);
                    }
                }
            }
        }
    }
}

