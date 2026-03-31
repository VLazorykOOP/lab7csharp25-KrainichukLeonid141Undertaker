using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Timer = System.Windows.Forms.Timer;

namespace Lab_GUI_Code
{
    public abstract class Figure
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Color Color { get; set; }
        public int Size { get; set; }
        protected Figure(int x, int y, Color c, int s) { X = x; Y = y; Color = c; Size = s; }
        public abstract void Draw(Graphics g);
    }

    public class Rhombus : Figure
    {
        public Rhombus(int x, int y, Color c, int s) : base(x, y, c, s) { }
        public override void Draw(Graphics g) {
            Point[] pts = { new(X, Y - Size), new(X + Size, Y), new(X, Y + Size), new(X - Size, Y) };
            using Pen p = new(Color, 2); g.DrawPolygon(p, pts);
        }
    }

    public class Triangle : Figure
    {
        public Triangle(int x, int y, Color c, int s) : base(x, y, c, s) { }
        public override void Draw(Graphics g) {
            Point[] pts = { new(X, Y - Size), new(X + Size, Y + Size), new(X - Size, Y + Size) };
            using Pen p = new(Color, 2); g.DrawPolygon(p, pts);
        }
    }

    public class Hexagon : Figure
    {
        public Hexagon(int x, int y, Color c, int s) : base(x, y, c, s) { }
        public override void Draw(Graphics g) {
            Point[] pts = new Point[6];
            for (int i = 0; i < 6; i++) {
                double angle = Math.PI / 3 * i;
                pts[i] = new Point(X + (int)(Size * Math.Cos(angle)), Y + (int)(Size * Math.Sin(angle)));
            }
            using Pen p = new(Color, 2); g.DrawPolygon(p, pts);
        }
    }

    public class MyArc : Figure
    {
        public MyArc(int x, int y, Color c, int s) : base(x, y, c, s) { }
        public override void Draw(Graphics g) {
            using Pen p = new(Color, 2); g.DrawArc(p, X - Size, Y - Size, Size * 2, Size * 2, 0, 180);
        }
    }

    public class MainForm : Form
    {
        private Button btnCenter;
        private Panel pnlOrbit;
        private Timer timerAnim;
        private double angle = 0;
        private Random rnd = new();

        private PictureBox pbMain;
        private Label lblRGB;
        private Panel pnlColorPreview;
        private Bitmap currentBitmap;

        private PictureBox pbCanvas;

        public MainForm()
        {
            this.Text = "Лабораторна 6";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            TabControl tabs = new() { Dock = DockStyle.Fill };
            
            TabPage tab1 = new() { Text = "Анімація (Завд. 1)" };
            btnCenter = new Button { Text = "ЦЕНТР", Bounds = new Rectangle(200, 200, 100, 50) };
            pnlOrbit = new Panel { BackColor = Color.Red, Size = new Size(30, 30) };
            timerAnim = new Timer { Interval = 50, Enabled = true };
            timerAnim.Tick += (s, e) => {
                angle += 0.1;
                int r = 100;
                int cx = btnCenter.Left + btnCenter.Width / 2;
                int cy = btnCenter.Top + btnCenter.Height / 2;
                pnlOrbit.Left = cx + (int)(r * Math.Cos(angle)) - pnlOrbit.Width / 2;
                pnlOrbit.Top = cy + (int)(r * Math.Sin(angle)) - pnlOrbit.Height / 2;
                
                if (rnd.Next(10) > 7) {
                    pnlOrbit.BackColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    pnlOrbit.Width = rnd.Next(20, 50);
                }
            };
            tab1.Controls.Add(pnlOrbit);
            tab1.Controls.Add(btnCenter);

            // --- ТАБ 2: Піпетка ---
            TabPage tab2 = new() { Text = "Піпетка (Завд. 2)" };
            Button btnLoad = new() { Text = "Завантажити фото", Location = new Point(10, 10), Size = new Size(150, 30) };
            pbMain = new PictureBox { Bounds = new Rectangle(10, 50, 400, 400), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            lblRGB = new Label { Text = "RGB: ...", Location = new Point(420, 50), AutoSize = true };
            pnlColorPreview = new Panel { Bounds = new Rectangle(420, 80, 50, 50), BorderStyle = BorderStyle.FixedSingle };

            btnLoad.Click += (s, e) => {
                using OpenFileDialog ofd = new();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    currentBitmap = new Bitmap(ofd.FileName);
                    pbMain.Image = currentBitmap;
                }
            };

            pbMain.MouseDown += (s, e) => {
                if (pbMain.Image == null) return;
                Bitmap b = (Bitmap)pbMain.Image;
                Color pixel = b.GetPixel(e.X * b.Width / pbMain.Width, e.Y * b.Height / pbMain.Height);
                pnlColorPreview.BackColor = pixel;
                lblRGB.Text = $"R: {pixel.R}, G: {pixel.G}, B: {pixel.B}";
            };
            tab2.Controls.AddRange(new Control[] { btnLoad, pbMain, lblRGB, pnlColorPreview });

            TabPage tab3 = new() { Text = "Фігури (Завд. 3)" };
            pbCanvas = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White };
            Button btnDraw = new() { Text = "Намалювати випадково", Dock = DockStyle.Bottom };
            
            btnDraw.Click += (s, e) => {
                Graphics g = pbCanvas.CreateGraphics();
                g.Clear(Color.White);
                Figure[] figures = new Figure[15];
                for (int i = 0; i < figures.Length; i++) {
                    int x = rnd.Next(50, pbCanvas.Width - 50);
                    int y = rnd.Next(50, pbCanvas.Height - 50);
                    Color c = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    int size = rnd.Next(20, 50);

                    figures[i] = rnd.Next(4) switch {
                        0 => new Rhombus(x, y, c, size),
                        1 => new Triangle(x, y, c, size),
                        2 => new Hexagon(x, y, c, size),
                        _ => new MyArc(x, y, c, size)
                    };
                    figures[i].Draw(g);
                }
            };
            tab3.Controls.Add(pbCanvas);
            tab3.Controls.Add(btnDraw);

            tabs.TabPages.AddRange(new[] { tab1, tab2, tab3 });
            this.Controls.Add(tabs);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}