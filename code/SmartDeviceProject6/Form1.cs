using doru;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Editor;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BallGame
{
    
    public partial class Form1 : Form
    {
        public static Form1 _This ;
        public static Assembly _Assembly = Assembly.GetCallingAssembly();
        public Form1()
        {
            this.WindowState = FormWindowState.Maximized;
            _This = this;
            InitializeComponent();
            Load += new EventHandler(Form1_Load);
            MouseDown += new MouseEventHandler(Form1_MouseDown);
            MouseMove += new MouseEventHandler(Form1_MouseMove);
            MouseUp += new MouseEventHandler(Form1_MouseUp);
        }

        void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!loaded) return;
            
        }
        Vector2 oldv;
        void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!loaded) return;
            oldv = new Vector2(e.X, e.Y);
        }

        void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!loaded) return;
            Vector2 v = new Vector2(e.X, e.Y) ;
            if (e.Button == MouseButtons.Left)
                _cur._Pos += v - oldv;
            oldv = v;
            
        }
        Camera _cur { get { return Camera._Current; } }
        static Game _Game;
        bool loaded { get { return Game.loaded; } }
        void Form1_Load(object sender, EventArgs e)
        {
            
            _Game = new Game();
            _Game.Load();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            _Game.Update();
            _Game.Draw();
            Invalidate();
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            
        }
    }
    
    
 
    
    public class Program
    {
        public static void Main()
        {
            Application.Run(new Form1());
        }
    }
}