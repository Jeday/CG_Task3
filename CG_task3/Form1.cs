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

        public Painter()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            ColorBox.BackColor = CurrentColor;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            foreach(ToolStripButton b in toolStrip1.Items.OfType<ToolStripButton>()) {
                if (b != button)
                    b.Checked = false;
            }
            switch (button.Name)
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
    }
}
