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
        private enum Tools { None, BorderMaker, MagicWand, Fill, Marker, ImageFill}
        private Tools CurrentTool = Tools.None;
        private Color CurrentColor = Color.White;
        private Graphics g;
        private Graphics borderboxGraphics;
        private List<List<Point>> border;
        private bool BorderIsDrawn = false;
        private Pen BorderPen;
        private List<Point> full_border;
        private Dictionary<int, List<Tuple<int, int>>> colored_lines;
        private Bitmap DrawArea;
        private Bitmap ImageFill;
        private int diff_img;
        private Point p_marker;
        private int width_marker;

        static  private void Swap<T>(ref T v1, ref T v2) { T v3 = v1; v1 = v2; v2 = v3; }

        public Painter()
        {
            
            InitializeComponent();
            colored_lines =new Dictionary<int, List<Tuple<int, int>>>(); //  Y : (X1,X2) (X3,X4) это две горизонтальные линии
            full_border = new List<Point>();
            ColorBox.BackColor = CurrentColor;
            border = new List<List<Point>>();
            BorderPen = new Pen(Color.Black, 2);
            float[] dashValues = {1,1};
            BorderPen.DashPattern = dashValues;
            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            diff_img = 50;
            width_marker = 3;
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
                case "Image Fill":
                    CurrentTool = Tools.ImageFill;
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
            Hide();
            colorDialog1.ShowDialog();
            CurrentColor = colorDialog1.Color;
            ColorBox.BackColor = CurrentColor;
            Show();
            WindowState = FormWindowState.Normal;
            TopMost = true;
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
                    border.Add(new List<Point>());
                    border.Last().Add(e.Location);
                    BorderPictureBox.Invalidate();
                }

                else if (CurrentTool == Tools.Fill)
                {
                    colored_lines.Clear();
                    //if (full_border.Count()>0)
                    rec_fill(e.Location);
                }
                else if (CurrentTool == Tools.ImageFill)
                {
                    colored_lines.Clear();
                    openFileDialog2.ShowDialog();
                    if (openFileDialog2.FileName == "")
                        return;
                    ImageFill = Image.FromFile(openFileDialog2.FileName) as Bitmap;
                    //if (full_border.Count > 0)
                    fill_image(e.Location, new Point(0, 0));
                }
                else if (CurrentTool == Tools.MagicWand) {

                    full_border.Clear();
                    border.Clear();
                    int x;
                    Point start = e.Location;
                    x = start.X;
                    Color c = DrawArea.GetPixel(x, start.Y);
                    while (x < DrawArea.Width) {
                        Color _c = DrawArea.GetPixel(x, start.Y);
                        if (!equal_color(_c,c) && magic_border(new Point(x, start.Y), start, c, out x))
                            break;
                        else
                            x++;
                    }

                    BorderPictureBox.Invalidate();

                }
                else if (CurrentTool == Tools.Marker)
                    p_marker = e.Location;
                    
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                if (CurrentTool == Tools.BorderMaker) {
                    if (BorderIsDrawn && !border.First().Contains(e.Location))
                        border.First().Add(e.Location);
                    BorderPictureBox.Invalidate();
                }
                else if(CurrentTool == Tools.Marker)
                {
                    g.DrawLine(new Pen(CurrentColor, width_marker), p_marker, e.Location);
                    p_marker = e.Location;
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (CurrentTool == Tools.BorderMaker)
            {
                BorderIsDrawn = false;
                BorderPictureBox.Invalidate();

                if (border.First().Count == 0)
                    return;
                full_border.Clear();
                full_border = new List<Point>(); // all pixels of border
                border.First().Add(border.First().First());  // make border circullar
                for (int i = 0; i < border.First().Count() - 1; i++)
                {
                    bresenham(border.First()[i].X, border.First()[i].Y, border.First()[i + 1].X, border.First()[i + 1].Y, ref full_border);//calculate all points of border 1->2, 2 ->3 .. n->1
                    full_border.RemoveAt(full_border.Count() - 1); // remove duplicate

                }
            }
            else if(CurrentTool == Tools.Marker)
                g.DrawLine(new Pen(CurrentColor, width_marker), p_marker, e.Location);
        }

        private void BorderPictureBox_Paint(object sender, PaintEventArgs e)
        {
            borderboxGraphics = e.Graphics;
            foreach (List<Point> b in border)
            if (b.Count > 3)
            {
                borderboxGraphics.DrawLines(BorderPen, b.ToArray());
                if (!BorderIsDrawn)
                    borderboxGraphics.DrawLine(BorderPen, b.Last(), b.First());
                
                
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
                while (!full_border.Contains(Finish)  && Finish.X<pictureBox1.Width)
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

        private void CopyPixels(Point to_start, Point to_finish, Point from_start) {
            to_finish.X += 1;
            for (Point i = to_start; i != to_finish; i.X += 1) {
                if (from_start.X  >= ImageFill.Width)
                    from_start.X = 0;
                DrawArea.SetPixel(i.X,i.Y,ImageFill.GetPixel(from_start.X, from_start.Y));
                from_start.X += 1;
            }
        }

        private void fill_image(Point p, Point loaded_p) {
            if (!full_border.Contains(p) && !is_colored(p) && p.X >= 0 && p.X < pictureBox1.Width && p.Y >= 0 && p.Y < pictureBox1.Height)
            {
                if (loaded_p.Y >= ImageFill.Height)
                    loaded_p.Y = 0;
                if (loaded_p.Y <0)
                    loaded_p.Y = ImageFill.Height-1;
                Point Start = new Point(p.X - 1, p.Y);
                Point Finish = new Point(p.X + 1, p.Y);
                while (!full_border.Contains(Start) && Start.X >= 0)
                    Start.X -= 1;
                Start.X += 1;
                while (!full_border.Contains(Finish) && Finish.X < pictureBox1.Width)
                    Finish.X += 1;
                Finish.X -= 1;
                if (colored_lines.ContainsKey(p.Y))
                    colored_lines[p.Y].Add(new Tuple<int, int>(Start.X, Finish.X));
                else
                    colored_lines.Add(p.Y, new List<Tuple<int, int>> { new Tuple<int, int>(Start.X, Finish.X) });
                Point from_start = new Point(Math.Abs(loaded_p.X - ((p.X - Start.X) % ImageFill.Width)), loaded_p.Y);
                CopyPixels(Start, Finish, from_start);
                pictureBox1.Refresh();
                for (int i = Start.X; i <= Finish.X; ++i)
                {
                    if (from_start.X == ImageFill.Width)
                        from_start.X = 0;
                    from_start.Y += 1;
                    fill_image(new Point(i, p.Y + 1), from_start);
                    from_start.Y -= 2;
                    fill_image(new Point(i, p.Y - 1), from_start);
                    from_start.Y += 1;
                    from_start.X += 1;
                }

            }
        }
        private Point next_point(int dir, Point p)
        {
            switch (dir)
            {
                case 0:
                    return new Point(p.X + 1, p.Y);
                case 1:
                    return new Point(p.X + 1, p.Y + 1);
                case 2:
                    return new Point(p.X, p.Y + 1);
                case 3:
                    return new Point(p.X - 1, p.Y + 1);
                case 4:
                    return new Point(p.X - 1, p.Y);
                case 5:
                    return new Point(p.X - 1, p.Y - 1);
                case 6:
                    return new Point(p.X, p.Y - 1);
                default:
                    return new Point(p.X + 1, p.Y - 1);
            }
        }

        bool equal_color(Color cl1, Color cl2)
        {
            return (System.Math.Abs(cl1.R - cl2.R) < diff_img && System.Math.Abs(cl1.G - cl2.G) < diff_img && System.Math.Abs(cl1.B - cl2.B) < diff_img);
        }

        private bool magic_border(Point start, Point beam_start, Color clr_img, out int rightmost)
        {
            

            List<Point> local_border = new List<Point>();
            int count_intersections = 0;

            Point LeftmostBeam = new Point(start.X, start.Y);
            Point RightMostBeam = new Point(start.X, start.Y);
            Point curr_p = new Point(start.X, start.Y);
            int dir = 6;
            Point view_p = new Point();
            do
            {
                int old = dir;
                int i;
                for (i = 0; i<8; ++i)
                {
                    view_p = next_point(dir, curr_p);
                    if (view_p.X < 0 || view_p.X >= DrawArea.Width || view_p.Y < 0 || view_p.Y >= DrawArea.Height)
                        break;
                    else if (!equal_color(DrawArea.GetPixel(view_p.X, view_p.Y), clr_img))
                        break;
                    dir += 1;
                    if (dir > 7)
                        dir = 0;
                }

                full_border.Add(curr_p);
                local_border.Add(curr_p);
                curr_p = view_p;

                if (curr_p.Y == beam_start.Y) {
                    if (curr_p.X > RightMostBeam.X)
                        RightMostBeam = curr_p;
                    if (curr_p.X < LeftmostBeam.X)
                        LeftmostBeam = curr_p;
                    if (curr_p.X > beam_start.X)
                        count_intersections++;
                }

                if (dir - 2 < 0)
                    dir += 8;
                dir -= 2;

                if (i == 8) // single point
                    break;
            } while (curr_p != start);

            border.Add(local_border);

            rightmost = RightMostBeam.X;
            if (RightMostBeam == LeftmostBeam)
                return false;
            if (count_intersections % 2 == 0)
                return false;
            else
                return true;
            
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Image = DrawArea;
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            ColorButton_Click(sender, e);
        }
    }
}
