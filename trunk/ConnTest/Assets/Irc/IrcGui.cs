using System;
using UnityEngine;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using Random = UnityEngine.Random;
public class Base2:MonoBehaviour
{
    
    static IrcGui __IrcGui;
    public static IrcGui _IrcGui { get { if (__IrcGui == null) __IrcGui = (IrcGui)MonoBehaviour.FindObjectOfType(typeof(IrcGui)); return __IrcGui; } }

}
public interface iIrcGui { void onIrcSend(string Input);
void onIrcPrivateSend(string nick, string Input);
}
public class IrcGui : IrcGuiBase
{
    iIrcGui controller;

    public void Show(iIrcGui ircSample)
    {
        enabled = true;
        controller = ircSample;
    }
    
    internal string[] Users = new string[] { };
    public int selectedUser;
    public Rect wndRect;
    public GameObject PmWnd;
    public Rect usersRect;
    public Rect u;
    public Vector2 scrolpos;
 
    public void Start()
    {
        id = Random.Range(0, 9999);    
    }
    public bool showWindow;

    public void OnGUI()
    {
        
        if (GUI.Button(new Rect(u.x + Screen.width, u.y, u.width, u.height), "Users Online:" + Users.Length))
            showWindow = !showWindow; 
        if(showWindow) wndRect = GUI.Window(1,wndRect, Draw, "Irc");
    }
    public void Draw(int id)
    {
        if (GUI.Button(closeButton, "Close"))
            showWindow = false;
        Rect r = new Rect(0, 0, usersRect.width * 3 / 4, 25f * Users.Length);
        GUI.Box(usersRect, "");
        scrolpos = GUI.BeginScrollView(usersRect, scrolpos, r);
        int old = selectedUser;
        selectedUser = GUI.SelectionGrid(r, selectedUser, Users, 1);
        if (old != selectedUser)
            OnMessageReceived(Users[selectedUser],"");
        GUI.EndScrollView();

        if (GUI.Button(buttonRect, "Send"))
            controller.onIrcSend(Input);
        BaseDraw();


        

    }

    public void OnMessageReceived(string nick, string msg)
    {
        GameObject g;
        g = GameObject.Find(nick+ " pm Window");
        if (g == null) g = ((GameObject)Instantiate(PmWnd));
        PmWnd p = g.GetComponent<PmWnd>();
        p.controller = this.controller;
        p.nick = nick;
        p.Msgs += msg + "\r\n";
    }
}