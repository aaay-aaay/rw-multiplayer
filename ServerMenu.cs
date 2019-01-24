using Menu;
using System;
using UnityEngine;

// This really doesn't need its own menu.

public class ServerMenu : Menu.Menu
{
    public ServerMenu(ProcessManager manager, MultiplayerMod mod) : base(manager, (ProcessManager.ProcessID)(-1))
    {
        Page page;
        pages.Add(page = new Page(this, null, "main", 0));
        page.subObjects.Add(new SimpleButton(this, page, "Start", "START", new Vector2(100f, 200f), new Vector2(100f, 50f)));
        this.mod = mod;
    }
    
    public override void Singal(MenuObject sender, string message)
    {
        if (message == "START")
        {
            mod.MakeServerThread();
        }
    }
    
    public MultiplayerMod mod;
}