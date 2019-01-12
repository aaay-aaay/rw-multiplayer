using Menu;
using System;
using System.Net;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Partiality.Modloader;

public class MultiplayerMod : PartialityMod
{
    public override void Init()
    {
        ModID = "Multiplayer v0.0.0";
    }
    
    public override void OnLoad()
    {
        On.Menu.MainMenu.ctor += MainMenuCtorHook;
        On.Menu.MainMenu.Singal += MainMenuSingalHook;
        On.ProcessManager.Update += UpdateHook;
        On.RainWorld.Start += StartHook;
    }
    
    public void StartHook(On.RainWorld.orig_Start orig, RainWorld rw)
    {
        UnityEngine.Random.seed = 4; // i'm not really sure why this is here
        orig(rw);
    }
    
    public void MainMenuCtorHook(On.Menu.MainMenu.orig_ctor orig, MainMenu menu, ProcessManager manager, bool showRegionSpecificBkg)
    {
        orig(menu, manager, showRegionSpecificBkg);
        Page firstPage = menu.pages[0];
        float num3 = (menu.CurrLang != InGameTranslator.LanguageID.Italian) ? 110f : 150f;
        firstPage.subObjects.Add(new SimpleButton(menu, firstPage, "MULTIPLAYER", "MULTIPLAYER", new Vector2(683f - num3 / 2, 410f), new Vector2(num3, 30f))); // add a button that says "MULTIPLAYER"
    }
    
    public void MainMenuSingalHook(On.Menu.MainMenu.orig_Singal orig, MainMenu menu, MenuObject sender, string message)
    {
        orig(menu, sender, message);
        if (message == "MULTIPLAYER") // the MULTIPLAYER button has been pressed
        {
            menu.ShutDownProcess(); // shut down the menu, remove the graphics for all the buttons
            menu.manager.currentMainLoop = new MultiplayerMenu(menu.manager, this); // create a new multiplayer menu
        }
    }
    
    public unsafe void UpdateHook(On.ProcessManager.orig_Update orig, ProcessManager manager, float deltaTime)
    {
        if (startGame) // start the game as a "server"
        {
            ns.Write(BitConverter.GetBytes(UnityEngine.Random.seed), 0, 4);
            manager.currentMainLoop.ShutDownProcess();
            manager.currentMainLoop = new ServerMainLoop(manager, this);
            startGame = false;
        }
        else if (startClient) // start the game as a "client"
        {
            byte[] randBytes = new byte[4];
            ns.Read(randBytes, 0, 4);
            UnityEngine.Random.seed = BitConverter.ToInt32(randBytes, 0);
            manager.currentMainLoop.ShutDownProcess();
            manager.currentMainLoop = new ClientMainLoop(manager, this);
            startClient = false;
        }
        orig(manager, deltaTime);
    }
    
    public void MakeServerThread() // listen on a different thread to prevent the game from crashing
    {
        new Thread(new ThreadStart(this.Server)).Start();
    }
    
    public void Server()
    {
        if (this == null) return;
        TcpListener listener = new TcpListener(49157);
        listener.Start();
        client = listener.AcceptTcpClient();
        ns = client.GetStream();
        
        byte[] init = new byte[13];
        ns.Read(init, 0, 13);
        string inits = Encoding.UTF8.GetString(init);
        if (inits != "RWMULTIPLAYER")
        {
            return;
        }
        byte[] resp = Encoding.UTF8.GetBytes("RWMULTIPLAYER");
        ns.Write(resp, 0, 13);
        startGame = true;
    }
    
    public void MakeClientThread(string cip) // connect on a different thread because... uh...
    {
        ip = cip;
        new Thread(new ThreadStart(this.Client)).Start();
    }
    
    public void Client()
    {
        if (this == null) return;
        client = new TcpClient(ip, 49157);
        ns = client.GetStream();
        
        byte[] init = Encoding.UTF8.GetBytes("RWMULTIPLAYER");
        ns.Write(init, 0, 13);
        byte[] resp = new byte[13];
        ns.Read(resp, 0, 13);
        if (Encoding.UTF8.GetString(resp) != "RWMULTIPLAYER")
        {
            return;
        }
        startClient = true;
    }
    
    // suggestion by fyre: [left right down up pickup throw jump spare] significantly reduces network usage
    public static byte ConvertInputPackage(Player.InputPackage pkg)
    {
        return (byte)((pkg.x == -1 ? 1 : 0) | (pkg.x == 1 ? 2 : 0) | (pkg.y == -1 ? 4 : 0) | (pkg.y == 1 ? 8 : 0) | (pkg.pckp ? 16 : 0) | (pkg.thrw ? 32 : 0) | (pkg.jmp ? 64 : 0));
    }
    
    public static Player.InputPackage GetInputPackage(byte b)
    {
        return new Player.InputPackage(false, ((b & 1) == 1) ? -1 : ((b & 2) == 2) ? 1 : 0, ((b & 4) == 4) ? -1 : ((b & 8) == 8) ? 1 : 0, ((b & 64) == 64), (b & 32) == 32, (b & 16) == 16, false, false);
    }
    
    public static TcpClient client;
    public static NetworkStream ns;
    public static string ip;
    // volatile prevents certain optimizations that could cause a problem with multiple threads doing stuff with a variable
    public static volatile bool startGame = false;
    public static volatile bool startClient = false;
    
    public int RWGctr = 0; // i have no idea what this is for???
}