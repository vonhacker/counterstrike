using System;
using UnityEngine;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class IrcGuiBase : Base2
{
    public Rect closeButton;
    public GUISkin skin;
    string msgs;
    public string Msgs
    {
        get { if (msgs == null) msgs = PlayerPrefs.GetString("msgs"); return msgs; }
        set { msgs = value; msgs = msgs.Substring(Math.Max(0, msgs.Length - 5000)); PlayerPrefs.SetString("msgs", msgs); }
    }
    
    public Rect msgRect;
    public Rect inputRect;
    public string Input = "";
    public Rect buttonRect;
    protected int id;
    public void BaseDraw()
    {
        Rect gr = new Rect(0, 0, msgRect.width, msgRect.height);
        GUI.BeginGroup(msgRect);
        GUI.Box(gr, "");
        GUI.TextArea(gr, Msgs, skin.FindStyle("msgs"));
        GUI.EndGroup();
        Input = GUI.TextField(inputRect, Input);
        GUI.DragWindow();
    }
}