using System;
using UnityEngine;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using Random = UnityEngine.Random;
public class PmWnd : IrcGuiBase
{
    public string nick;    
    public Rect r;
    public iIrcGui controller;
    public void Start()
    {
        id = Random.Range(0, 9999);
        name = nick + " pm Window";
        r = new Rect(Random.Range(0, Screen.width / 2), Random.Range(0, Screen.height / 2), r.width, r.height);
    }
    bool onfront;
    public void OnGUI()
    {
        r = GUI.Window(id, r, Draw, nick);
        if (!onfront)
        {
            GUI.BringWindowToFront(id);
            GUI.FocusWindow(id);
            onfront = true;
        }
    }
    public void Draw(int id)
    {
        if (GUI.Button(closeButton, "Close"))
            Destroy(this.gameObject);

        if (GUI.Button(buttonRect, "Send"))
        {            
            controller.onIrcPrivateSend(nick, Input);
            Input = "";
        }
        GUI.DragWindow();
    }
}