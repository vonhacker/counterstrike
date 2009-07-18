#define SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;
using doru.Mathematics;
using System.Windows.Media;
using System.Net.Sockets;
using System.Windows.Browser;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Collections;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using System.Threading;

using ICSharpCode.SharpZipLib.Zip;
using doru;
using doru.Tcp;
using CounterStrikeLive.Service;
using LevelEditor4;
using CounterStrikeLive.Controls;

namespace CounterStrikeLive
{
    
    public class Game
    {
        public Game()
        {
            _This = this;
        }
        public bool _EasyBots
        {
            get
            {
                if (null==EscMenu._This) return true;
                return EscMenu._This.Easy_Bots.IsChecked ?? false;
            }
        }
        public static Game _This;
        public bool _IsEverthingVisible;
        public List<AnimDamage> _AnimDamages = new List<AnimDamage>();
        internal void ShowDamage(Vector2 pos, float change)
        {
            AnimDamage _AnimDamage = new AnimDamage { _Game = this, _Position = pos };
            _AnimDamage._Game = this;
            _AnimDamage.Load(change.ToString());
        }
        public Cursor _Cursor;
        public Menu.GameState _GameState { get { return _Menu._GameState; } set { _Menu._GameState = value; } }
        public void OnKeyDown(Key _Key)
        {
            if (Key.Add == _Key)
            {
                ZoomIn();
            }
            if (Key.Subtract == _Key)
            {
                ZoomOut();
            }

            if (_Key == Key.M || _Key == Key.End)
            {
                new TeamSelect();
            }

            if ((_Key == Key.PageUp || _Key == Key.R) && _TotalPatrons != 0 && _LocalPlayer != null)
            {
                _Sender.Send(PacketType.Reloading);
                _LocalPlayer.ReloadSound();
                _LocalPlayer._isReloading = true;
                _TotalPatrons -= 30;
                WriteCenterText("Reloading");

                Menu._TimerA.AddMethod(1500, delegate { _Patrons = 30; if (_LocalClient != null) _LocalClient._IsReloading = false; });
            }

            if ((_Key == Key.PageDown || _Key == Key.Space) && _GameState != Menu.GameState.alive)
            {
                _SpeciateId++;
                if (_SpeciateId < _Players.Count) WriteCenterText("Player: " + _Players[_SpeciateId]._Client._Nick);
            }
            if (Menu.GameState.alive == _Menu._GameState)
            {
                _Sender.Send(new byte[] { (byte)PacketType.keyDown, (byte)_Key });
                _Key.Trace("sending Key");
                //_This.Provider.SendMessage(new byte[] { (byte)_PacketType.keyDown, (byte)_Key });
                _LocalPlayer.OnKeyDown(_Key);
            }
        }

        public void ZoomOut()
        {
            _ScaleTransform.ScaleX = _ScaleTransform.ScaleY -= .2;
            if (_Scale < .2) _Scale = .2;
        }

        public void ZoomIn()
        {
            _ScaleTransform.ScaleX = _ScaleTransform.ScaleY += .2;
        }
        public void OnKeyUp(Key _Key)
        {
            if (Menu.GameState.alive == _Menu._GameState)
            {
                using (MemoryStream _MemoryStream = new MemoryStream())
                {
                    BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                    _BinaryWriter.Write((byte)PacketType.keyUp);
                    _BinaryWriter.Write((byte)_Key);
                    _BinaryWriter.Write((Int16)_LocalPlayer._x);
                    _BinaryWriter.Write((Int16)_LocalPlayer._y);
                    _Sender.Send(_MemoryStream.ToArray());
                }
                _LocalPlayer.OnKeyUp(_Key);
            }
        }
        public List<Player> _Players = new List<Player>();
        public Map _Map;
        public Menu _Menu;
        public Canvas _Canvas = new Canvas();
        TranslateTransform _TranslateTransform = new TranslateTransform();

        public Point _FreeViewPos;
        Config _Config = Config._This;
        
        public static bool _SinglePlayer { get { return Menu._SinglePlayer; } }
        public static bool _Multiplayer { get { return Menu._Multiplayer; } }
        public void Load(MapDatabase _MapDatabase)
        {
            

            _Scale = .5;
            _FreeViewPos = _MapDatabase._CStartPos;

            if (_Config._AutoSelect)
                TerroristsButtonClick();
            else
                new TeamSelect();


            _Menu._GameState = Menu.GameState.teamSelect;

            _Cursor = new Cursor();
            _Cursor._Menu = _Menu;
            _Cursor.Load();

            _Map = new Map();
            _Canvas.Children.Add(_Map._Canvas);
            TransformGroup _TransformGroup = new TransformGroup();
            _TransformGroup.Children.Add(_ScaleTransform);
            _TransformGroup.Children.Add(_TranslateTransform);
            _Canvas.RenderTransform = _TransformGroup;
            _Map.LoadMap(_MapDatabase);

            
        }
        public ScaleTransform _ScaleTransform = new ScaleTransform();
        public void SpectatorButtonClick()
        {
            _LocalClient._Team = Player.Team.spectator;
            OnTeamSelect();
        }

        private void OnTeamSelect()
        {
            _LocalClient._PlayerState = SharedClient.PlayerState.dead;
            _GameState = Menu.GameState.spectator;

            SendCheckWins();
        }

        public void TerroristsButtonClick()
        {
            OnTeamSelect();
            _LocalClient._PlayerType = Database.PlayerType.TPlayer;
            _LocalClient._Team = Player.Team.terr;
            Debug.WriteLine("Terrorsits team selected");
        }
        public void AutoSelectClick()
        {
            int cterr = (from pl in _Clients where pl != null && pl._Team == Player.Team.cterr select pl).Count();
            int terr = (from pl in _Clients where pl != null && pl._Team == Player.Team.terr select pl).Count();
            if (cterr > terr)
            {
                TerroristsButtonClick();
            } else
            {
                CTerroritsButtonClick();
            }
        }
        public void CTerroritsButtonClick()
        {
            OnTeamSelect();
            _LocalClient._PlayerType = Database.PlayerType.CPlayer;
            _LocalClient._Team = Player.Team.cterr;
            Debug.WriteLine("Counter terrorsits team selected");
        }

        private void SendCheckWins()
        {
            Debug.WriteLine("check Wins Sended");
            _Sender.Send(new byte[] { (byte)PacketType.checkWins });
            //_This.Provider.SendMessage(new byte[] { (byte)_PacketType.checkWins });
            CheckWins();
        }
        public LocalPlayer _LocalPlayer { get { return (LocalPlayer)_LocalClient._Player; } }
        public SharedClient _LocalClient { get { return _Menu._LocalClient; } }
        
        public List<SharedClient> _Bots { get { return _Menu._Bots; } }
        public MyObs<SharedClient> _Clients { get { return _Menu._Clients; } }

        public static void PlaySound(string s)
        {
            MediaElement m = new MediaElement();
            Menu._This._GameCanvas.Children.Add(m);
            m.SetSource(s);
            m.Play();
            m.MediaEnded += delegate { Menu._This._GameCanvas.Children.Remove(m); };
        }
        void CreateBotReset()
        {
            foreach (SharedClient _BotClient in _Bots)
            {

                _BotClient._PlayerState = SharedClient.PlayerState.removed;
                _BotClient._PlayerState = SharedClient.PlayerState.alive;
                _BotClient._Life = 100;
            }     

        }
        protected void CreateLocalPlayerReset()
        {
            if(_SinglePlayer)
                CreateBotReset();
            PlaySound("jingle.mp3");

            _ScoreBoard.Hide();
            _Patrons = 30;
            _TotalPatrons = 90;
            Trace.WriteLine("CreateLocalPlayerRestart");
            for (int i = _Explosions.Count - 1; i >= 0; i--)
            {
                GameObjA _Explosion = _Explosions[i];
                _Explosion.Remove();
                Trace.WriteLine(_Explosion + " " + i + " removed");
            }
            _Map._Canvas1.Children.Clear();

            if (_LocalClient._Team != Player.Team.spectator)
            {
                _LocalClient._PlayerState = SharedClient.PlayerState.removed;
                _Menu._GameState = Menu.GameState.alive;
                _LocalClient._PlayerState = SharedClient.PlayerState.alive;

                Point _Point = _Map.GetPos(_LocalClient._Team);
                int _dist = 100;
                //do
                //{
                _LocalClient._StartPosX = (int)_Point.X + (Random.Next(-_dist, _dist));
                _LocalClient._StartPosY = (int)_Point.Y + (Random.Next(-_dist, _dist));
                //} while (_LocalPlayer.PlayerCollide() != null);
                _LocalPlayer._Life = 100;
            }
        }

        public int _Points { get { return _Menu._LocalClient._Points; } set { _Menu._LocalClient._Points = value; } }

        public void LocalPlayerDead(Player _Killer)
        {

            if (_Killer == _LocalPlayer)
            {
                _Points--;
            } else
            {
                _Sender.Send(new byte[] { (byte)PacketType.addPoint, (byte)_Killer._ID });
                //_This.Provider.SendMessage(new byte[] { (byte)_PacketType.addPoint, (byte)_Killer._ID });
            }
            //if (_This._GameState == Menu.GameState.spectator) throw new Exception("Break");
            _LocalClient._Deaths++;
            _ScoreBoard.Show();
            _Menu._GameState = Menu.GameState.spectator;
            _SpeciateId = _Killer._ID;
            _Menu.ShowKilledMessage(_Killer._Client, _LocalPlayer._Client);
            _LocalClient._PlayerState = SharedClient.PlayerState.dead;
            Trace.WriteLine("Local Plyater Dead");
            SendCheckWins();
        }
        public ScoreBoard _ScoreBoard { get { return _Menu._ScoreBoard; } }
        public void CheckWins()
        {
            Trace.WriteLine("CheckWins");
            const int interval = 3000;
            if ((from pl in _Players where pl._Client._Team == Player.Team.cterr select pl).Count() == 0)
            {
                Trace.WriteLine("Terrorists Win");
                _CenterText.Text += "Terrorists Win\n";
                Menu._TimerA.AddMethod(interval, CreateLocalPlayerReset);

            } else if ((from pl in _Players where pl._Client._Team == Player.Team.terr select pl).Count() == 0)
            {
                Trace.WriteLine("Counter Terrorists Win");
                _CenterText.Text += "Counter Terrorists Win\n";
                Menu._TimerA.AddMethod(interval, CreateLocalPlayerReset);
            }
        }


        TextBlock _CenterText { get { return _Menu._CenterText; } set { _Menu._CenterText = value; } }


        public void Die(Player _Player)
        {
            AnimationB _Explosion = new AnimationB();
            _Player.PlaySound("death1.mp3");
            _Explosion.name = _Player._PlaeyerModel + "_dead";
            _Explosion._Position = _Player._Position;
            _Explosion._Angle = _Player._Angle;
            _Explosion._Game = this;
            _Explosion._Remove = false;
            _Explosion.Load();
        }
        public readonly double _ShootInterval = 100;
        public List<GameObjA> _Explosions = new List<GameObjA>();
        public Sender _Sender;

        int patrons;
        public int _Patrons
        {
            get { return patrons; }
            set
            {
                patrons = value;
                _Menu._patrons.Text = patrons.ToString();
            }
        }
        int totalPatrons;
        public int _TotalPatrons
        {
            get { return totalPatrons; }
            set
            {
                totalPatrons = value;
                _Menu._totalpatrons.Text = totalPatrons.ToString();
            }
        }

        public void Update()
        {

            for (int i = _Explosions.Count - 1; i >= 0; i--)
            {
                _Explosions[i].CheckVisibility(_LocalPlayer);
                _Explosions[i].Update();
            }
            for (int i = _Players.Count - 1; i >= 0; i--)
            {
                Player _Player = _Players[i];
                _Player.CheckVisibility(_LocalPlayer);
                _Player.Update();
            }

            for (int i = _AnimDamages.Count - 1; i >= 0; i--)
            {
                AnimDamage _AnimDamage = _AnimDamages[i];
                _AnimDamage.CheckVisibility(_LocalPlayer);
                _AnimDamage.Update();
            }


            UpdatePlayer();
            _Cursor.Update();
        }
        double _ShootTimeElapsed;
        protected void UpdatePlayer()
        {
            if (_Menu._GameState == Menu.GameState.alive)
            {
                int _distance = 2000;
                UpdateView(_distance, _LocalPlayer._Position);
                Point _Mouse = new Point();
                if (Menu._MouseEventArgs != null) _Mouse = Menu._MouseEventArgs.GetPosition(_Canvas);

                float _x = (float)(_Mouse.X - _LocalPlayer._x);
                float _y = (float)(_Mouse.Y - _LocalPlayer._y);
                Vector2 _Vector2 = new Vector2(_x, _y);
                _LocalPlayer._Angle = (DCalculator.VectorToRadians(_Vector2) / DCalculator.RadiansToDegreesRatio);

                if (Menu._MouseLeftButtonDown && _Patrons != 0 && !_LocalPlayer._isReloading)
                {
                    _ShootTimeElapsed += Menu._TimerA._MilisecondsElapsed;
                    if (_ShootTimeElapsed > _ShootInterval)
                    {

                        _ShootTimeElapsed = 0;
                        _Patrons--;
                        float _d = 2;
                        _d = _d * (_Cursor.Scale * _Cursor.Scale);
                        float _Angle = _LocalPlayer._Angle + (float)Random.Next(-_d, _d);

                        _Angle = Player.Cangl(_Angle);
                        using (MemoryStream _MemoryStream = new MemoryStream())
                        {
                            BinaryWriter _BinaryWriter = new BinaryWriter(_MemoryStream);
                            if (_d == 0)
                            {
                                Shoot(_LocalPlayer._x, _LocalPlayer._y, _Angle, _LocalPlayer, true);
                                _BinaryWriter.Write((byte)PacketType.firstshoot);
                            } else
                            {
                                _BinaryWriter.Write((byte)PacketType.shoot);
                                Shoot(_LocalPlayer._x, _LocalPlayer._y, _Angle, _LocalPlayer, false);
                            }
                            _BinaryWriter.Write(Sender.EncodeInt(_Angle, 0, 360));
                            _Sender.Send(_MemoryStream.ToArray());
                            //_This.Provider.SendMessage(_MemoryStream.ToArray());
                        }
                        _Cursor._Scale += .5f;
                        if (_Cursor._Scale > 4) _Cursor._Scale = 4;
                    }
                }
            }
            if (_GameState != Menu.GameState.alive)
            {
                if (_SpeciateId == -1)
                {
                    if (Menu._Keyboard.Contains(Key.Left))
                    {
                        _FreeViewPos.X -= 30;
                    }
                    if (Menu._Keyboard.Contains(Key.Right))
                    {
                        _FreeViewPos.X += 30;
                    }
                    if (Menu._Keyboard.Contains(Key.Down))
                    {
                        _FreeViewPos.Y += 30;
                    }
                    if (Menu._Keyboard.Contains(Key.Up))
                    {
                        _FreeViewPos.Y -= 30;
                    }
                    _TranslateTransform.X = -_FreeViewPos.X * _Scale + _Menu._Width / 2;
                    _TranslateTransform.Y = -_FreeViewPos.Y * _Scale + _Menu._Height / 2;
                } else if (_Players.Count - 1 < _SpeciateId)
                {
                    _SpeciateId = -1;
                    WriteCenterText("Free View Mode");
                } else
                {
                    Player _Player = _Players[_SpeciateId];
                    UpdateView(2000, _Player._Position);
                }
            }
        }
        public void WriteCenterText(string text)
        {
            _CenterText.Text += text + "\n";
        }
        public double _Scale { get { return _ScaleTransform.ScaleX; } set { _ScaleTransform.ScaleX = _ScaleTransform.ScaleY = value; } }
        public double _Width { get { return _Menu._Width * _Scale; } }
        public double _Height { get { return _Menu._Height * _Scale; } }
        private void UpdateView(int _distance, Vector2 _Pos)
        {
            _FreeViewPos.X = _Pos.X;
            _FreeViewPos.Y = _Pos.Y;
            double _mousex = (Menu._Mouse.X / _Menu._Width) - .5;
            double _mousey = (Menu._Mouse.Y / _Menu._Height) - .5;

            _TranslateTransform.X = ((-_Pos.X * _Scale) + (_Menu._Width / 2) - _mousex * _distance * _Scale);
            _TranslateTransform.Y = ((-_Pos.Y * _Scale) + (_Menu._Height / 2) - _mousey * _distance * _Scale);
        }
        int _SpeciateId = 0;
        bool _friendlyfire = false;
        public void Shoot(float _x, float _y, float _Angle, Player _ShootingPlayer, bool firstshoot)
        {
            _ShootingPlayer.ShootAnimation();

            Vector2 _MaxShootPoint = new Vector2(0, -5000);
            Vector2 _PlayerPos = new Vector2(_x, _y);
            DCalculator.RotateVector(ref _MaxShootPoint, DCalculator.DegreesToRadians(_Angle));
            _MaxShootPoint += _PlayerPos;
            Line2 _Line2;
            List<Map.LV> _Collisions = _Map.Collision(_MaxShootPoint, _PlayerPos, out _Line2);

            if (_Collisions.Count > 0)
            {
                Vector2 _VectorHoleRotation = new Vector2(0, -10);
                DCalculator.RotateVector(ref _VectorHoleRotation, DCalculator.DegreesToRadians(_Angle));

                Vector2 _CollisionPos;
                _CollisionPos = _Collisions.First()._Vector2;

                Animation _SparkExplosion = new Animation();
                _SparkExplosion._VisibleToAll = true;
                _SparkExplosion._AnimatedBitmap = Menu._Database._Sparks.Random();
                _SparkExplosion._Position = _CollisionPos;
                _SparkExplosion._Game = this;
                _SparkExplosion.Load();
                _SparkExplosion.PlaySound(Helper.Random("ric_conc-1.mp3", "ric_conc-2.mp3"), 1000);

                for (int i = 0, Power = 0; i < _Collisions.Count && Power < 3; i++, Power++)
                {
                    _CollisionPos = _Collisions[i]._Vector2;
                    Ellipse _Ellipse = new Ellipse();
                    _Ellipse.Width = 5;
                    _Ellipse.Height = 5;
                    _Ellipse.Fill = new SolidColorBrush(Colors.Black);
                    Canvas.SetLeft(_Ellipse, _CollisionPos.X - (_Ellipse.Width / 2) + _VectorHoleRotation.X);
                    Canvas.SetTop(_Ellipse, _CollisionPos.Y - (_Ellipse.Height / 2) + _VectorHoleRotation.Y);
                    _Map._Canvas1.Children.Add(_Ellipse);
                    _VectorHoleRotation = Vector2.Multiply(_VectorHoleRotation, -1);
                }

                for (int i = _Players.Count - 1; i >= 0; i--)
                {
                    Player _EnemyPlayer = _Players[i];
                    if (_ShootingPlayer._Team != _EnemyPlayer._Team && !_friendlyfire)
                        if (_ShootingPlayer != _EnemyPlayer)
                        {
                            Vector2 _CollisionPoint;
                            float dist = DCalculator.DistanceBetweenPointAndLineSegment(_EnemyPlayer._Position, _CollisionPos, _PlayerPos, out _CollisionPoint);
                            if (dist < 40)
                            {
                                Animation _BloodExplosion = new Animation();
                                _BloodExplosion._Position = _CollisionPoint;
                                _BloodExplosion._AnimatedBitmap = Menu._Database._Blood.Random();
                                _BloodExplosion._Game = this;
                                _BloodExplosion._Angle = _ShootingPlayer._Angle + 180;
                                _BloodExplosion.PlaySound(Helper.Random("damage1.mp3", "damage2.mp3", "damage3.mp3"));
                                _BloodExplosion.Load();
                                if (_EnemyPlayer is LocalPlayer || _EnemyPlayer is BotPlayer) /////////////shootLocalplayer
                                {
                                    
                                    int damage;
                                    if (dist < 8 && firstshoot)
                                    {
                                        damage = 110;
                                        _EnemyPlayer.PlaySound("headshot1.mp3");
                                    } else damage = 10;
                                    float life = (_EnemyPlayer)._Life -= damage;


                                    if (life < 0)
                                        if (_EnemyPlayer is LocalPlayer)
                                            LocalPlayerDead(_ShootingPlayer);
                                        else if (_EnemyPlayer is BotPlayer)
                                        {
                                            SharedClient _SharedClient = _EnemyPlayer._Client;
                                            _SharedClient._PlayerState = SharedClient.PlayerState.dead;
                                            _SharedClient._Deaths++;
                                            _ShootingPlayer._Client._Points++;
                                            SendCheckWins();
                                        }

                                    if (_EnemyPlayer is LocalPlayer)
                                    {
                                        _Menu._DamageRotation.Angle = DCalculator.VectorToRadians(_ShootingPlayer._Position - _EnemyPlayer._Position) * DCalculator.DegreesToRadiansRatio;
                                        ((Storyboard)_Menu.Resources["DamageStoryboard"]).Begin();
                                    }
                                    _EnemyPlayer._IsSlowDown = true;
                                }
                            }
                        }
                }
            }
        }


        internal void Dispose()
        {
            this._Canvas.Children.Clear();
        }

        public void MouseMove()
        {
            if (_Cursor != null)
            {
                _Cursor._x = Menu._Mouse.X;
                _Cursor._y = Menu._Mouse.Y;
            }
        }
    }    
}
