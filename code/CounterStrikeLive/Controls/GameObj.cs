using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Mathematics;
using doru;
using CounterStrikeLive.Service;

namespace CounterStrikeLive
{
    public class GameObj
    {        

        public bool _VisibleToAll;
        Menu _Menu = Menu._This;
        public void PlaySound(string s)
        {
            PlaySound(s, 5000);
        }
        Config _Config = Config._This;
        public void PlaySound(string s, double distance)
        {
            //if (_Game._LocalPlayer == this) return;

            double volume = GetVolume(distance);

            if (volume != 0)
            {
                MediaElement _MediaElement = new MediaElement();
                _Menu._GameCanvas.Children.Add(_MediaElement);
                _MediaElement.SetSource(s);                
                _MediaElement.MediaEnded += delegate { _Menu._GameCanvas.Children.Remove(_MediaElement); };
                _MediaElement.Volume = volume;
            }
        }

        public double GetVolume(double distance)
        {
            double x, y, x2, y2;

            if (_Game._LocalPlayer == null)
            {
                x = _Game._FreeViewPos.X;
                y = _Game._FreeViewPos.Y;
            } else
            {
                x = _Game._LocalPlayer._Position.X;
                y = _Game._LocalPlayer._Position.Y;
            }
            x2 = position.X;
            y2 = position.Y;

            double len = Math.Sqrt((x - x2).Pow() + (y - y2).Pow());
            double volume = Math.Max(0, 1 - len / distance) * _LocalDatabase.Volume;
            return volume;
        }
        LocalDatabase _LocalDatabase = LocalDatabase._This;
        
        public Vector2 position;
        public Vector2 _Position { get { return position; } set { if (value.X == float.NaN) throw new Exception("Break"); position = value; } }
        public Map _Map { get { return _Game._Map; } }
        protected Canvas _Canvas = new Canvas();
        public Game _Game;
        public virtual void CheckVisibility(LocalPlayer _LocalPlayer)
        {
            if (_LocalPlayer != null && this != _LocalPlayer && !_VisibleToAll && !_Game._IsEverthingVisible)
            {
                float a1 = Player.CorrectAngle(Calculator.VectorToRadians(this._Position - _LocalPlayer._Position) * Calculator.DegreesToRadiansRatio);
                float a2 = Player.CorrectAngle(a1 - _LocalPlayer._Angle + 45);
                if (Math.Abs(a2) < 90)
                {
                    Line2 wall;
                    if (_Map.Collision(this._Position, _LocalPlayer._Position, out wall).Count != 0)
                    {
                        this._Visibility = Visibility.Collapsed;
                    }
                    else this._Visibility = Visibility.Visible;
                }
                else this._Visibility = Visibility.Collapsed;
            }
            else this._Visibility = Visibility.Visible;
        }
        public Visibility _Visibility
        { get { return _Canvas.Visibility; } set { _Canvas.Visibility = value; } }

    }
    public class AnimDamage : GameObj
    {
        TextBlock _TextBlock = new TextBlock();
        public void Load(string text)
        {
            _Game._AnimDamages.Add(this);

            _TextBlock.Text = text;
            _TextBlock.Foreground = new SolidColorBrush(Colors.Yellow);
            _TextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            _TextBlock.VerticalAlignment = VerticalAlignment.Center;
            _Canvas.Children.Add(_TextBlock);
            _Game._Canvas.Children.Add(_Canvas);

            Canvas.SetTop(_Canvas, _Position.Y);
            Canvas.SetLeft(_Canvas, _Position.X);
        }

        public void Update()
        {
            position.Y -= 3;
            Canvas.SetTop(_Canvas, _Position.Y);
            Canvas.SetLeft(_Canvas, _Position.X);
            _TextBlock.Opacity -= .03;
            if (_TextBlock.Opacity < 0)
            {
                _Game._Canvas.Children.Remove(_TextBlock);
                _Game._AnimDamages.Remove(this);
            }
        }
    }
}
