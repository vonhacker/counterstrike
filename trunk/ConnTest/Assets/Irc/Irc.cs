using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;
public interface iIrc { void OnPm(string u, string s);
void OnMessage(string m);
}
public class Irc : MonoBehaviour
{
    [UnityEngine.HideInInspector]
    public string[] users = new string[]{};
    public string host = "irc.quakenet.org";
    public int port = 6667;
    [UnityEngine.HideInInspector]
    public bool success;
    public iIrc controller;
    Socket _Socket;
    NetworkStream _NetworkStream;
    StreamWriter sw;
    StreamReader sr;
    Queue<Action> actions = new Queue<Action>();
    public Thread _thread;
    public string ircNick;//{ get { return nick; } }
    public string user = "physxwarsuser1";
    public string user2 = "localhost";
    public string about = "about";
    public string channel = "PhysxWars";
    public long startDate
    {
        get
        {
            string s = PlayerPrefs.GetString("unity_v");
            if (s == "")
                startDate = DateTime.Now.Ticks;

            return long.Parse(PlayerPrefs.GetString("unity_v"));            
        }
        set
        {
            PlayerPrefs.SetString("unity_v", value.ToString());
        }
    }
    void Start()
    {
       
        {
            _thread = new Thread(StartThread);
            _thread.IsBackground = true;
            _thread.Start();
        }
    }


    void OnApplicationQuit()
    {
        if (_Socket != null)
            _Socket.Close();
        if (_thread != null)
            _thread.Abort();

    }
    public void SendIrcMessage(string s)
    {
        if (sw != null)
        {
            OnMessage(ircNick + ": " + s);
            sw.WriteLine("PRIVMSG #" + channel + " :" + s);
        }
        else
            OnMessage("Not Connected");
    }
    public void SendIrcPrivateMessage(string u, string s)
    {
        if (sw != null)
        {
            OnPmMessage(u, ircNick +": " + s);
            sw.WriteLine("PRIVMSG " + u + " :" + s);
        }
        else
            OnPmMessage(u, "Not Connected");
    }
    void StartThread()
    {
        try
        {
            Connect();
            Write(string.Format("NICK {0}", ircNick));
            Write("USER " + user + " " + user2 + " server :" + about);
            while (true)
            {
                string s = sr.ReadLine();
                if(enableLog) print(s);
                if (s == null || s == "") throw new Exception("string is null");
                if (Regex.Match(s, @":.+? 005").Success && !success)
                {
                    Write("JOIN #" + channel);
                    OnMessage("Connected");
                    success = true;
                }
                Match m;
                if ((m = Regex.Match(s, @"PING \:(\w+)", RegexOptions.IgnoreCase)).Success)
                    Write(("PONG :" + m.Groups[1]));
                if ((m = Regex.Match(s, @"\:.+? 353 .+? = #(.+?) \:(.+)")).Success)
                {
                    users = users.Union(m.Groups[2].Value.Trim().Split(' ')).ToArray();
                    success = true;
                }
                if ((m = Regex.Match(s, @"\:(.+?)!.*? PRIVMSG " + ircNick.Trim(new[]{ '@'}) + @" \:(.*)")).Success)
                    OnPmMessage(m.Groups[1].Value, m.Groups[1].Value + ": " + m.Groups[2].Value+ "\r\n");
                else
                if ((m = Regex.Match(s, @"\:(.+?)!.*? PRIVMSG .*? \:(.*)")).Success)
                    OnMessage(m.Groups[1].Value + ": " + m.Groups[2].Value);


                if ((m = Regex.Match(s, @"\:(.+?)!.*? PART")).Success)
                {
                    users = users.Where(a => !a.Contains(m.Groups[1].Value)).ToArray();
                    OnMessage(m.Groups[1].Value + " leaved");
                }
                if ((m = Regex.Match(s, @"\:(.+?)!.*? JOIN")).Success)
                {
                    users = users.Union(new[] { m.Groups[1].Value }).ToArray();
                    OnMessage(m.Groups[1].Value + " joined");
                }
            }
        }
        catch (Exception e)
        {
            print(e);
            OnMessage("Disconnected:" + e);
        }
    }
    void Update()
    {
        if(actions.Count>0)
            actions.Dequeue()();
    }
    void OnMessage(string m)
    {
        actions.Enqueue(delegate
        {
            controller.OnMessage(m);
        });
    }
    void OnPmMessage(string u, string s)
    {
        actions.Enqueue(delegate
        {
            controller.OnPm(u, s);
        });
    }
    void Write(string s)
    {
        if(enableLog) print(s);
        sw.WriteLine(s);
    }
    private void Connect()
    {
        OnMessage("Connecting");
        _Socket = new TcpClient(host, port).Client;
        _NetworkStream = new NetworkStream(_Socket);
        sw = new StreamWriter(_NetworkStream, Encoding.Default);
        sr = new StreamReader(_NetworkStream, Encoding.Default);
        sw.AutoFlush = true;
    }
    public bool enableLog;




}
