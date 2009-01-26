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
using System.Threading;
using System.IO;

namespace CounterStrikeLive.ServiceClient
{
    public abstract partial class ServiceClientProvider
    {
        public event EventHandler<ServerOperationEventArgs> ServerOperationCompleted;
        public event EventHandler<EventArgs> ServerConnected;
        public event EventHandler<EventArgs> ServerFailed;
        protected SynchronizationContext uiThread;

        public ServiceClientProvider()
        {
            uiThread = SynchronizationContext.Current;
        }

        protected abstract void SendMessage(object data);
        public abstract void Disconnect();

        # region Event callers

        protected void CallServerOperationEvent(object data)
        {
            if (this.ServerOperationCompleted != null)
            {
                using (MemoryStream memoryStream = new MemoryStream(data as byte[]))
                {
                    var binaryReader = new BinaryReader(memoryStream);
                    int senderId = binaryReader.ReadByte();
                    PacketType packetType = (PacketType)binaryReader.ReadByte();

                    byte[] otherData = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length - 2);
                    this.ServerOperationCompleted(this, new ServerOperationEventArgs(senderId, packetType, otherData));
                }
            }
        }

        protected void CallServerConnectedEvent(object data)
        {
            if (this.ServerConnected != null)
            {
                this.ServerConnected(this, new EventArgs());
            }
        }

        protected void CallServerFailedEvent(object data)
        {
            if (this.ServerFailed != null)
            {
                this.ServerFailed(this, new EventArgs());
            }
        }

        #endregion

        protected virtual void ProcessMessage(byte[] data)
        {
            // пока что тип сообщения один :)
            uiThread.Post(CallServerOperationEvent, data);
        }

    }
}
