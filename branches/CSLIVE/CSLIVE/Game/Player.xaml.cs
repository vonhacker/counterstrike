using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using doru;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using doru.Vectors;
using System.Windows.Shapes;
using CSLIVE.Game.Controls;

namespace CSLIVE.Game
{
    public class LocalPlayer : Player
    {
        public static double _leftright = 5;
        public static double _updown = 5;

        private void UpdateKeyboard()
        {
            SetAnimation(Anims._stay);
            _power = new Vector();
            if (_Keys.Contains(Key.Q))
            {
                _angle -= 3;
            } else if (_Keys.Contains(Key.E))
            {
                _angle += 3;
            }

            if (_Keys.Contains(Key.A))
            {
                SetAnimation(Anims._run_left);
                _power -= new Vector(_leftright, 0);
            }
            if (_Keys.Contains(Key.D))
            {
                SetAnimation(Anims._run_right);
                _power += new Vector(_leftright, 0);
            }

            if (_Keys.Contains(Key.Space))
            {
                SetAnimation(Anims._dead);
            }
            if (_Keys.Contains(Key.R))
            {
                SetAnimation(Anims._reload);
            }
            if (_Keys.Contains(Key.W))
            {
                SetAnimation(Anims._run);
                _power += new Vector(0, -_updown);
            } else if (_Keys.Contains(Key.S))
            {
                SetAnimation(Anims._run_back);
                _power += new Vector(0, _updown);
            }
        }
        public override void Update()
        {
            base.Update();

            if (Page._MouseEventArgs != null)
            {
                Point _Mouse = Page._MouseEventArgs.GetPosition(_Game.RootLayout);
                double _x = (_Mouse.X - this._x);
                double _y = (_Mouse.Y - this._y);
                _rad = Calculator.VectorToRadians(new Vector(_x, _y));
            }
            UpdateKeyboard();
        }
        public override void Load()
        {
            base.Load();
            _Game._CurrentCamera = _PerspCamera;
        }
    }
    public class RemotePlayer : Player { }
    public partial class Player : GameObj
    {
        #region vars

        public Client _Client;
        public enum Team { spectator = 2, terr = 3, cterr = 4, auto = 5 };
        public enum PlayerModel { phoenix };
        public enum Anims { _run, _run_left, _run_right, _stay, _dead, _run_back, _reload }
        public enum PlayerGunModel { ak47 };
        public PlayerModel _PlModel { get { return _Client._PlayerModel.Value; } }
        public PlayerGunModel _GunModel = PlayerGunModel.ak47;

        public PerspCamera _PerspCamera = new PerspCamera();
        public Ellipse _Ellipse { get { return this.Get<Ellipse>("_Ellipse"); } set { this.Set("_Ellipse", value); } }

        public AnimatedImage _PlayerCurrentAnimation { get { return (AnimatedImage)_GunCurrentAnimationContentControl.Content; } set { _GunCurrentAnimationContentControl.Content = value; } }
        public AnimatedImage _GunCurrentAnimation { get { return (AnimatedImage)_PlayerCurrentAnimationContentControl.Content; } set { _PlayerCurrentAnimationContentControl.Content = value; } }
        public static GameContentDataBase _GameContentDataBase { get { return Game._GameContentDataBase; } }

        #endregion
        
                        
        public Player()
        {
            InitializeComponent();
            //_Ellipse.Width = 30; _Ellipse.Height = 30; _Ellipse.Fill = new SolidColorBrush(Colors.Brown);
            _PerspCamera._GameObj = this;            
        }
        public Game _Game { get { return Game._Game; } }
        public void Death() //create death sprite
        {
            AnimatedImage _AnimatedImage = new AnimatedImage();
            _GameContentDataBase.Get(_PlModel + "" + Anims._dead);
            _Game.AddSprite(_AnimatedImage);
            Remove();
            throw new NotImplementedException();
        }
        public void Remove()
        {
            _Client._Player = null;
            _Game._Players.Remove(this);
        }
        public override void Load()
        {
            _Game._Players.Add(this);
            SetAnimation(Anims._stay);
            base.Load();
        }
        public Dictionary<string, AnimatedImage> _Bitmaps = new Dictionary<string, AnimatedImage>();
        public AnimatedImage GetBitmap(string s)
        {
            if (_Bitmaps.ContainsKey(s))
                return _Bitmaps[s];
            else
            {

                AnimatedImage anim = new AnimatedImage();
                anim._AnimatedBitmap = _GameContentDataBase.Get(s);
                if (anim._AnimatedBitmap == null)
                    Trace.Fail(s + " animation not found");

                anim.Load();
                _Bitmaps[s] = anim;
                return anim;
            }

        }

        public override void Update()
        {
            _PlayerCurrentAnimation.Update();
            _GunCurrentAnimation.Update();            
            //UpdateKeyboard();
            _x += _S.X;
            _y += _S.Y;
            base.Update();
        }

        public static List<Key> _Keys { get { return Page._Keys; } }
        public void SetAnimation(Anims _Anims)
        {
            _PlayerCurrentAnimation = GetBitmap(_PlModel + "" + _Anims);
            _GunCurrentAnimation = GetBitmap(_GunModel + "" + _Anims);
        }

        

    }


}
