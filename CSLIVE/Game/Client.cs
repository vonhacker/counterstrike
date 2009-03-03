#if(!SILVERLIGHT)
using System.Windows.Navigation;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using doru;
using System.IO;
using System.ComponentModel;
using doru.Tcp;
using System.Reflection;

namespace CSLIVE.Game
{
    public interface ClientListItem
    {
        Player.Team _Team { get; }
        float _ping { get; }
        int _Deaths { get; }
        int _Points { get; }
        int? _id { get; }
        string _Nick { get; }
    }
    public class Client : INotifyPropertyChanged, ClientListItem ,ISh
    {
        public void Load()
        {
            _Game._Clients.Add(this);            
        }

        public int? _id { get; set; }
        public object _SharedObj { get; set; }
        public bool _Local { get { return _SharedObj is LocalSharedObj<Client>; } }         
        private void CreatePlayer()
        {
            if (_Local)
                _Player = new LocalPlayer();
            else
                _Player = new RemotePlayer();

            _Player._Client = this;
            _Player.Load();
        }
        
        bool isSLowDown;        
        [SharedObject(4)]
        public bool _IsSlowDown
        {
            get { return isSLowDown; }
            set
            {
                if (isSLowDown != value)
                {
                    isSLowDown = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("_IsSlowDown"));
                }

            }
        }

        float life;
        [SharedObject(3)]
        public float _Life
        {
            get { return life; }
            set
            {
                float lifechange = value - life;
                life = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_Life"));
                if (_Player != null)
                    _Game.ShowDamage(_Player._Position, lifechange);
            }
        }
        int deaths;
        [SharedObject(3)]
        public int _Deaths
        {
            get { return deaths; }
            set
            {
                deaths = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_Deaths"));
            }
        }

        public Sender _Sender { get { return _Game._Sender; } }
        
        
        [SharedObjectAttribute(1)]
        public double? _StartPosX
        {
            get
            {
                if (_Player == null) return null;
                return _Player._x;
            }
            set
            {
                _Player._x = value.Value;
                PropertyChanged(this, new PropertyChangedEventArgs("_StartPosX"));
            }
        }
        [SharedObjectAttribute(1)]
        public double? _StartPosY
        {
            get
            {
                if (_Player == null) return null;
                return _Player._y;
            }
            set
            {
                _Player._y = value.Value;
                PropertyChanged(this, new PropertyChangedEventArgs("_StartPosY"));
            }
        }
        
        Player.Team team = Player.Team.spectator;
        [SharedObjectAttribute(-1)]
        public Player.Team _Team
        {
            get { return team; }
            set
            {
                if (team == value) return;
                team = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_Team"));
            }
        }
        public enum PlayerState { dead, alive, removed };

        private Player.PlayerModel? playerModel;
        [SharedObjectAttribute(-3)]
        public Player.PlayerModel? _PlayerModel
        {
            get { return playerModel; }
            set
            {
                if (playerModel == value) return;
                playerModel = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_PlayerModel"));
            }
        }
        PlayerState playerState = PlayerState.removed;
        [SharedObjectAttribute(0)]
        public PlayerState _PlayerState
        {
            get
            {
                return playerState;
            }
            set
            {
                if (value == playerState) return;
                playerState = value;
                PropertyChanged(this, new PropertyChangedEventArgs("_PlayerState"));
                if (playerState == PlayerState.alive)
                {
                    CreatePlayer();
                }
                else if (playerState == PlayerState.dead)
                {
                    _Player.Death();
                    //_Game.Die(_Player);
                    //_Player.Remove();
                }
                else if (playerState == PlayerState.removed)
                {
                    if (_Player != null)
                        _Player.Remove();
                }
            }
        }

        

        protected float ping;
        //[SharedObjectAttribute(1)]
        public float _ping
        {
            get { return ping; }
            set
            {
                ping = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_ping"));
            }
        }


        public List<Client> _RemoteClients { get { return _Game._Clients; } }
        //private int? id;
        //public int? _id
        //{
        //    get { return id; }
        //    set
        //    {
        //        id = value;
        //        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_id"));
        //    }
        //}
        public LocalDatabase _LocalDatabase = new LocalDatabase();

        [SharedObjectAttribute(-2)]
        public string _Nick
        {
            get { return _LocalDatabase._Nick; }
            set
            {
                _LocalDatabase._Nick = value;
                _Game.WriteKillText(value + " Joined");
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_Nick"));
            }
        }
        public int points;
        [SharedObjectAttribute(1)]
        public int _Points
        {
            get { return points; }
            set
            {
                points = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("_Points"));
            }
        }


        
        public Player _Player { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Game _Game { get { return Game._Game; } }


        public void Update()
        {
            if(_PlayerState == PlayerState.alive)
                _Player.Update();
        }
    }
}
