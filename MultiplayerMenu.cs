using Menu;
using System;
using UnityEngine;

public class MultiplayerMenu : Menu.Menu
{
    public MultiplayerMenu(ProcessManager manager, MultiplayerMod mod) : base(manager, (ProcessManager.ProcessID)(-1))
    {
        Page page;
        pages.Add(page = new Page(this, null, "main", 0));
        float x = 100f;
        page.subObjects.Add(new SimpleButton(this, page, "player 2 (connect)", "CLIENT", new Vector2(100f, x += 100f), new Vector2(100f, 50f)));
        page.subObjects.Add(new SimpleButton(this, page, "player 1 (host)", "SERVER", new Vector2(100f, x += 100f), new Vector2(100f, 50f)));
        this.mod = mod;
    }
    
    public override void Singal(MenuObject sender, string message)
    {
        if (message == "SERVER")
        {
            ShutDownProcess();
            manager.currentMainLoop = new ServerMenu(manager, mod);
        }
        else if (message == "CLIENT")
        {
            ShutDownProcess();
            manager.currentMainLoop = new ClientMenu(manager, mod);
        }
    }
    
    public MultiplayerMod mod;
}