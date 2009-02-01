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
using CounterStrikeLive.Service;


namespace CounterStrikeLive.ServiceClient
{
    public class ServerOperationEventArgs : EventArgs
    {
        private int senderID;
        private PacketType packetType;
        private byte[] data;

        public ServerOperationEventArgs(int SenderID, PacketType PacketType, byte[] Data)
            : base()
        {
            this.senderID = SenderID;
            this.packetType = PacketType;
            this.data = Data;
        }

        public int SenderID
        {
            get
            {
                return senderID;
            }
        }
        public PacketType PacketType
        {
            get
            {
                return packetType;
            }
        }
        public byte[] Data
        {
            get
            {
                return data;
            }
        }
    }
}
