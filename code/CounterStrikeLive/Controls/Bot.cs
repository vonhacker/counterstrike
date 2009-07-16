using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Windows.Media.Imaging;
using doru;
using CounterStrikeLive.Service;
using System.ComponentModel;
using doru.Tcp;
using System.Reflection;
using System.IO;
using CounterStrikeLive.Controls;
using LevelEditor4;

namespace CounterStrikeLive.Controls
{
    public static class Ext
    {
        public static double Normalize(this double a)
        {
            return a / Math.Abs(a);
        }
        public static float Normalize(this float a)
        {
            return (float)(a / Math.Abs(a));
        }
    }
    public class BotPlayer : Player
    {
        
        public static BotPlayer _This;

        public BotPlayer()
        {
            _This = this;
        }

        Botbase _Botbase = Botbase._This;
        TreePoint _oldtp;
        TreePoint _nwtp;
        public override void Load()
        {

            _oldtp = _Team == Team.terr ? _Botbase._TStartPos.Random() : _Botbase._CStartPos.Random();
            _Position = _oldtp.ToVector() + new Vector2(Random.Next(-100, 100), Random.Next(-100, 100));            
            _nwtp = _oldtp._Way.Random();
            base.Load();
        }
        public float CRA(float a)
        {
            return a > 180 ? a - 360 : a;
        }
        public const int obs = 50;
        public float? _nextAngle;
        public bool _EasyBots { get { return _Game._EasyBots; } }
        public Vector2 _Offset = new Vector2(Random.Next(-1, 1), Random.Next(-1, 1));
        public new Vector2 _Position { get { return base._Position + _Offset; } set { base._Position = value - _Offset; } }
        bool _isShooting;
        public override void Update()
        {
            if (_IsSlowDown)
            {
                _slowdowntimeelapsed += Menu._TimerA._TimeElapsed;
                if (_slowdowntimeelapsed > 1000)
                {
                    _IsSlowDown = false;
                    _slowdowntimeelapsed = 0;
                }
            }

            Vector2 v = _nwtp.ToVector() - _Position;
            double len = v.Length();
            float _dir = (float)(Calculator.VectorToRadians(v) / Math.PI * 180);

            if (_nextAngle == null) _nextAngle = Cangl(_dir + Random.Next(-obs, obs));
            _isShooting = ShootCheck();
            var a = _Angle;
            var b = _nextAngle.Value;
            _Angle += CRA(Cangl(b - a)) / (_isShooting ? 5 : 30);

            if (Math.Abs(b - a) < 10) _nextAngle = null;

            if (Menu._TimerA._SecodsElapsed < 1)
            {
                Vector2 V = Vector2.Multiply(v.Normalize2(), _speed * (float)Menu._TimerA._SecodsElapsed);
                if (_IsSlowDown) V = Vector2.Multiply(V, _slowdownspeed);
                _Position += V;
            }
            

            if (len < 20)
            {
                var value = _oldtp;
                _oldtp = _nwtp;
                _nwtp = _nwtp._Way.Random(value);
                //List<TreePoint> nbp= new List<TreePoint>();//_notbackpoints 
                //foreach (TreePoint p in _nwtp._Way)
                //    if (Math.Abs((Calculator.VectorToRadians(p.ToVector() - _nwtp.ToVector()) / Math.PI * 180) - _dir) < 120)
                //        nbp.Add(p);
                //if (nbp.Count == 0)
                //    _nwtp = _nwtp._Way.Random(value);
                //else
                //    _nwtp = nbp.Random();

                _nextAngle = null;
            }

            base.Update();
        }
        public double _shottimeelapsed;
        private bool ShootCheck()
        {
            _shottimeelapsed += _TimerA._TimeElapsed;
            foreach (Player p in _Game._Players)
            {


                if (p._Team != _Team && CheckVisibility2(p))
                {
                    _nextAngle = (float)(Calculator.VectorToRadians(p._Position - _Position) / Math.PI * 180);


                    if (_shottimeelapsed > _Game._ShootInterval * 1.5)
                    {
                        _shottimeelapsed = 0;
                        _TimerA.AddMethod(_EasyBots ? 1000 : 500, Shoot);
                    }

                    float a = Calculator.DistanceBetweenPointAndLineSegment(p._Position, _Position, _nwtp.ToVector());
                    float b = Calculator.DistanceBetweenPointAndLineSegment(p._Position, _Position, _oldtp.ToVector());
                    if (b < a)
                    {
                        Helper.Switch(ref _nwtp, ref _oldtp);
                    }
                    return true;
                }

            }
            return false;
        }

        private void Shoot()
        {
            float ra = _EasyBots ? 8 : 5;
            if(_Client._PlayerState == SharedClient.PlayerState.alive)
                _Game.Shoot(_x, _y, _Angle + Random.Next(-ra, ra), this, false);
        }
        TimerA _TimerA = Menu._TimerA;
        public bool CheckVisibility2(Player p)
        {
            
            if (_EasyBots && (_Position - p._Position).Length() > 3000) return false;
            //float a1 = Animation.Cangl(Calculator.VectorToRadians(this._Position - p._Position) * Calculator.DegreesToRadiansRatio);
            //float a2 = Animation.Cangl(a1 - p._Angle + 45);
            //if (Math.Abs(a2) < 90)
            {
                Line2 wall;
                if (_Map.Collision(this._Position, p._Position, out wall).Count != 0)
                {
                    return false;
                } else return true;
            }// else return false;
        }
    }
}
