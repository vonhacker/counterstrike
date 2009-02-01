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
    public class LocalPlayer : Player
    {
        public bool _isReloading;
        public override void Load()
        {
            base.Load();
            this._Visibility = Visibility.Visible;
        }
        protected override void UpdateKeys()
        {
            if (_MoveVector != default(Vector2))
            {
                if (_Game._Cursor._Scale < 2) _Game._Cursor._Scale = 2;
            }
            base.UpdateKeys();
        }
        public float _Life { get { return _Client._Life; } set { _Client._Life = value; } }
        public double _slowdowntimeelapsed;
        public override void Update()
        {
            if (_IsSlowDown)
            {
                _slowdowntimeelapsed += STimer._TimeElapsed;
                if (_slowdowntimeelapsed > 1000)
                {
                    _IsSlowDown = false;
                    _slowdowntimeelapsed = 0;
                }
            }
            base.Update();
        }
    }
}
