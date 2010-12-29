using UnityEngine;
using System.Collections;

public class IrcSample : Base2 ,iIrc,iIrcGui
{
    Irc ircProtocol;
	void Start () {
        ircProtocol = GetComponent<Irc>();
        ircProtocol.controller = this;
        ircProtocol.enabled = true;        
        _IrcGui.Show(this);
	}
	
	void Update () {
        
        _IrcGui.Users = ircProtocol.users;        
	}

    public void OnPm(string u, string s)
    {
        _IrcGui.OnMessageReceived(u, s);
    }
    public void OnMessage(string msg)
    {
        _IrcGui.Msgs += msg + "\r\n";
    }
    public void onIrcPrivateSend(string nick ,string msg)
    {
        ircProtocol.SendIrcPrivateMessage(nick, msg);
    }
    public void onIrcSend(string msg)
    {
        ircProtocol.SendIrcMessage(msg);
        _IrcGui.Input = "";
    }


}
