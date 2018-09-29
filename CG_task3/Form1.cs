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

        public Painter()
        {
            InitializeComponent();
            
            
            ColorBox.BackColor = CurrentColor;
            border = new List<Point>();
            BorderPen = new Pen(Color.Black, 2);
            float[] dashValues = {1,1};
            BorderPen.DashPattern = dashValues;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            borderboxGraphics = BorderPictureBox.CreateGraphics();
            g = pictureBox1.CreateGraphics();
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
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = Bitmap.FromFile(openFileDialog1.FileName);
            
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
            //borderboxGraphics.Clear(Color.Transparent);
            if (border.Count > 3)
            {
                borderboxGraphics.DrawCurve(BorderPen, border.ToArray());
                if (!BorderIsDrawn)
                    borderboxGraphics.DrawLine(BorderPen, border.Last(), border.First());
                
                
            }
        }
    }
}
