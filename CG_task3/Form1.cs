using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CG_task3
{
    public partial class Painter : Form
    {
        private enum Tools { None, BorderMaker, MagicWand, Fill, Marker }
        private Tools CurrentTool = Tools.None;
        private Color CurrentColor = Color.White;
        private Graphics g;
        private Graphics borderboxGraphics;
        private List<Point> border;
        private bool BorderIsDrawn = false;
        private Pen BorderPen;
        private List<Point> full_border;
        private Dictionary<int, List<Tuple<int, int>>> colored_lines;
        public Bitmap DrawArea;

        static  private void Swap<T>(ref T v1, ref T v2) { T v3 = v1; v1 = v2; v2 = v3; }

        public Painter()
        {
            
            InitializeComponent();
            colored_lines =new Dictionary<int, List<Tuple<int, int>>>();
            full_border = new List<Point>();
            ColorBox.BackColor = CurrentColor;
            border = new List<Point>();
            BorderPen = new Pen(Color.Black, 2);
            float[] dashValues = {1,1};
            BorderPen.DashPattern = dashValues;
            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            borderboxGraphics = BorderPictureBox.CreateGraphics();
            pictureBox1.Controls.Add(BorderPictureBox);
            BorderPictureBox.Location = new Point(0, 0);
            g = Graphics.FromImage(DrawArea);
        }

        private void toolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            foreach(ToolStripButton b in toolStrip1.Items.OfType<ToolStripButton>()) {
                if (b != button)
                    b.Checked = false;
            }
            switch (button.Text)
            {
                case "Border":
                    CurrentTool = Tools.BorderMaker;
                    break;
                case "Fill":
                    CurrentTool = Tools.Fill;
                    break;
                case "Magic Wand":
                    CurrentTool = Tools.MagicWand;
                    break;
                case "Marker":
                    CurrentTool = Tools.Marker;
                    break;
                default:
                    CurrentTool = Tools.None;
                    break;
            }

            if(!button.Checked)
                CurrentTool = Tools.None;



        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            CurrentColor = colorDialog1.Color;
            ColorBox.BackColor = CurrentColor;
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            DrawArea =  Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
            pictureBox1.Size = new Size(DrawArea.Width, DrawArea.Height);
            g = Graphics.FromImage(DrawArea);
            BorderPictureBox.Size = pictureBox1.Size;
            BorderPictureBox.Invalidate();
           
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Image.Save(saveFileDialog1.FileName);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (CurrentTool == Tools.BorderMaker)
                {
                    border.Clear();
                    BorderIsDrawn = true;
                    border.Add(e.Location);
                    BorderPictureBox.Invalidate();
                }
                else if (CurrentTool == Tools.Fill) {
                    fill_border(e.Location);
                }
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                if (CurrentTool == Tools.BorderMaker) {
                    if (BorderIsDrawn && !border.Contains(e.Location))
                        border.Add(e.Location);
                    BorderPictureBox.Invalidate();
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (CurrentTool == Tools.BorderMaker)
            {
                BorderIsDrawn = false;
                BorderPictureBox.Invalidate();
            }
        }

        private void BorderPictureBox_Paint(object sender, PaintEventArgs e)
        {
            borderboxGraphics = e.Graphics;
            if (border.Count > 3)
            {
                borderboxGraphics.DrawLines(BorderPen, border.ToArray());
                if (!BorderIsDrawn)
                    borderboxGraphics.DrawLine(BorderPen, border.Last(), border.First());
                
                
            }
        }

        private void bresenham(int x, int y, int x2, int y2, ref List<Point> ls)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                ls.Add(new Point(x, y));
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        private bool is_colored(Point p) {
            if (colored_lines.ContainsKey(p.Y))
            {
                foreach (Tuple<int, int> t in colored_lines[p.Y]) {
                    if (p.X >= t.Item1 && p.X <= t.Item2)
                        return true;
                }
                
            }
            return false;

        }

        private void rec_fill(Point p) {
            if (!full_border.Contains(p) && !is_colored(p) && p.X >= 0 && p.X <= pictureBox1.Width && p.Y >= 0 && p.Y <= pictureBox1.Height) {
                Point Start = new Point(p.X - 1, p.Y);
                Point Finish = new Point(p.X + 1, p.Y);
                while (!full_border.Contains(Start) && Start.X >= 0)
                    Start.X -= 1;
                Start.X += 1;
                while (!full_border.Contains(Finish)  && Finish.X<=pictureBox1.Width)
                    Finish.X += 1;
                Finish.X -= 1;
                if(colored_lines.ContainsKey(p.Y))
                    colored_lines[p.Y].Add(new Tuple<int,int>(Start.X,Finish.X));
                else
                    colored_lines.Add(p.Y,new List<Tuple<int, int>> { new Tuple<int, int>(Start.X, Finish.X )});
                g.DrawLine(new Pen(CurrentColor), Start, Finish);
                pictureBox1.Refresh();
                for (int i = Start.X; i <= Finish.X; ++i) {
                    rec_fill(new Point(i,p.Y+1));
                    rec_fill(new Point(i,p.Y-1));
                }

            }
        } 

        private void fill_border(Point start) {
            if (border.Count == 0)
                return;
            full_border.Clear();
            colored_lines.Clear();
            full_border = new List<Point>(); // all pixels of border
            border.Add(border.First());  // make border circullar
            for (int i = 0; i < border.Count() - 1; i ++) { 
                bresenham(border[i].X, border[i].Y, border[i + 1].X ,border[i + 1].Y, ref full_border);//calculate all points of border 1->2, 2 ->3 .. n->1
                full_border.RemoveAt(full_border.Count() - 1); // remove duplicate
                
            }

            rec_fill(start);
           
            
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Image = DrawArea;
        }
    }
}
