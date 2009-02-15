using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using doru;

namespace CSLIVE.Server
{
    public partial class Program
    {        
        public static TimerA _TimerA = new TimerA();
        
        public static Config _Config;
        
        public partial class GameServer
        {            
            public static List<Client> _Clients = new List<Client>();
            public static List<RoomDb> _Rooms { get { return _Config.Rooms; } }            
            
        }
    }
    
}
//public static IEnumerable<Client> _ClientsInRooms
//{
//    get { return _Rooms.SelectMany(room => room._Clients).OfType<Client>(); }
//}