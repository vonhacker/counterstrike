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
using CounterStrikeLive.ServiceClient;

namespace CounterStrikeLive
{
    public partial class Application
    {
        private static ServiceClientProvider service = null;

        public static ServiceClientProvider GameService
        {
            get
            {
                if (service == null)
                {
                    string host = "localhost";
                    int port = 4520; // For socket provider
                    service = new SocketsProvider(host, port);
                }
                return service;
            }
        }

    }
}
