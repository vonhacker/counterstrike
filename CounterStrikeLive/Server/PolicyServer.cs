using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Server;
using Server.Properties;
namespace PolicyServer
{
    class PolicyConnection
    {
        private Socket m_connection;
        private byte[] m_buffer;
        private int m_received;
        private byte[] m_policy;
        private static string s_policyRequestString = "<policy-file-request/>";
        public PolicyConnection(Socket client, byte[] policy)
        {
            m_connection = client;
            m_policy = policy;
            m_buffer = new byte[s_policyRequestString.Length];
            m_received = 0;
            try
            {
                m_connection.BeginReceive(m_buffer, 0, s_policyRequestString.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (SocketException)
            {
                m_connection.Close();
            }
        }
        private void OnReceive(IAsyncResult res)
        {
            try
            {
                m_received += m_connection.EndReceive(res);
                if (m_received < s_policyRequestString.Length)
                {
                    m_connection.BeginReceive(m_buffer, m_received, s_policyRequestString.Length - m_received, SocketFlags.None, new AsyncCallback(OnReceive), null);
                    return;
                }
                string request = System.Text.Encoding.UTF8.GetString(m_buffer, 0, m_received);
                if (StringComparer.InvariantCultureIgnoreCase.Compare(request, s_policyRequestString) != 0)
                {
                    m_connection.Close();
                    return;
                }
                m_connection.BeginSend(m_policy, 0, m_policy.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (SocketException)
            {
                m_connection.Close();
            }
        }
        public void OnSend(IAsyncResult res)
        {
            try
            {
                m_connection.EndSend(res);
            }
            finally
            {
                m_connection.Close();
            }
        }
    }
    public class PolicyServer
    {
		
        private Socket m_listener;
        private byte[] m_policy;
        public string policyFile;
		public int _PolicyPort { get { return Settings.Default._PolicyPort; } }
        public void StartAsync()
        {
            Console.WriteLine("PolicyServer Started");
            FileStream policyStream = new FileStream(policyFile, FileMode.Open);
            m_policy = new byte[policyStream.Length];
            policyStream.Read(m_policy, 0, m_policy.Length);
            policyStream.Close();
            m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listener.Bind(new IPEndPoint(IPAddress.Any, _PolicyPort));
            m_listener.Listen(10);
            m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }        

        public void OnConnection(IAsyncResult res)
        {
            Socket client = null;
            try
            {
                client = m_listener.EndAccept(res);
            }
            catch (SocketException)
            {
                return;
            }
            Trace.WriteLine("Client Connected " + ((IPEndPoint)client.RemoteEndPoint).Address);
            PolicyConnection pc = new PolicyConnection(client, m_policy);
            m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }
        public void Close()
        {
            m_listener.Close();
        }
    }
}