using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VectorWorld
{
    public partial class FormWorld2D : Form
    {
        private Vector2D[] walls = new Vector2D[] {
            new Vector2D(new PointF(100.0f, 300.0f), new PointF(300.0f, 200.0f)),
            new Vector2D(new PointF(200.0f, 300.0f), new PointF(300.0f, 200.0f))
        };
        private float minDistance = 15.0f;
        private PointF lastDot = new PointF();
        private PointF Dot = new PointF();
        public FormWorld2D()
        {
            InitializeComponent();
        }

        private void FormWorld2D_KeyDown(object sender, KeyEventArgs e)
        {
            
            Dot = lastDot;
            if (e.KeyCode == Keys.A) Dot.X -= 5.0f;
            if (e.KeyCode == Keys.S) Dot.Y += 5.0f;
            if (e.KeyCode == Keys.W) Dot.Y -= 5.0f;
            if (e.KeyCode == Keys.D) Dot.X += 5.0f;
            Vector2D way = new Vector2D(lastDot, Dot); // sDot - предыдущее положение, eDot - текущее положение
            /////////////////////////////////////////////////////////
            Bitmap bmp = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);
            for (int i = 0; i < walls.Length; i ++)
                g.DrawLine(new Pen(Color.Blue), walls[i].dot1, walls[i].dot2);
            g.DrawLine(new Pen(Color.Black), way.dot1, way.dot2);
            PointF newDot;
            PointF newDotResult = new PointF(0.0f, 0.0f);
            int countCross = 0;
            // Находим стены пересекающиеся с новым положением и предположительные точки
            for (int i = 0; i < walls.Length; i++)
                if (checkCrossWalls(walls[i], lastDot, Dot, out newDot))
                {
                    countCross++;
                    newDotResult.X += newDot.X;
                    newDotResult.Y += newDot.Y;                    
                }
            if (countCross == 0) lastDot = Dot;
            else if (countCross == 1) lastDot = newDotResult;
            else
            {
                newDotResult.X /= (float)countCross;
                newDotResult.Y /= (float)countCross;
                lastDot = newDotResult;
            }
            g.DrawEllipse(new Pen(Color.Indigo), lastDot.X - minDistance, lastDot.Y - minDistance, minDistance * 2.0f, minDistance * 2.0f);
            CreateGraphics().DrawImage(bmp, 0, 0);
        }

        private void FormWorld2D_MouseClick(object sender, MouseEventArgs e)
        {
            Dot = new PointF(e.X, e.Y);
        }

        private bool checkCrossWalls(Vector2D wall, PointF lastDot, PointF Dot, out PointF newDot)
        {
            Vector2D way = new Vector2D(lastDot, Dot);
            PointF cross;
            bool isCross = wall.cross(way, out cross);
            float distance = wall.distance(way.dot2);
            if (isCross || Math.Abs(distance) < minDistance)
            {
                Vector2D n;
                if (isCross)
                    n = new Vector2D(Dot, distance < 0.0f ? -wall.normal() : wall.normal());
                else
                    n = new Vector2D(Dot, distance < 0.0f ? wall.normal() : -wall.normal());
                if (wall.cross(n, out cross, false))
                {
                    n = new Vector2D(cross, n);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                }
                else if (Math.Sqrt(Math.Pow(wall.dot1.X - Dot.X, 2.0f) + Math.Pow(wall.dot1.Y - Dot.Y, 2.0f)) < minDistance)
                {
                    n = new Vector2D(wall.dot1, Dot);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                }
                else if (Math.Sqrt(Math.Pow(wall.dot2.X - Dot.X, 2.0f) + Math.Pow(wall.dot2.Y - Dot.Y, 2.0f)) < minDistance)
                {
                    n = new Vector2D(wall.dot2, Dot);
                    n.length = minDistance + 1.5f;
                    newDot = n.dot2;
                    return true;
                }
            }
            newDot = Dot;
            return false;
        }
    }
}
