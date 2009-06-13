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

namespace CounterStrikeLive
{
    public class GameObj
    {
        public bool _VisibleToAll;

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
