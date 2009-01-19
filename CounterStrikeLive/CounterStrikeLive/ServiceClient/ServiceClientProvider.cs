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

namespace CounterStrikeLive.ServiceClient
{
    public abstract partial class ServiceClientProvider
    {

        protected SynchronizationContext uiThread;

        public ServiceClientProvider()
        {
            uiThread = SynchronizationContext.Current;
        }

        protected abstract void SendMessage(object data);
        public abstract void Disconnect();

        # region Event callers

        //protected void CallTimeSyncResponseEvent(object data)
        //{
        //    if (this.TimeSyncCompleted != null)
        //    {
        //        this.TimeSyncCompleted(this, new TimeSyncEventArgs(data));
        //    }
        //}

        #endregion

        protected virtual void ProcessMessage(object data)
        {
            //if (data is TimeSyncResponseInfo)
            //{
            //    uiThread.Post(CallTimeSyncResponseEvent, data);
            //}
            //if (data is PingRequest)
            //{
            //    SendMessage(new PingResponse());
            //}
        }

    }
}
