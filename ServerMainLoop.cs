using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Net.Sockets;

public class ServerMainLoop : RainWorldGame
{
    public ServerMainLoop(ProcessManager manager, MultiplayerMod mod) : base(manager)
    {
        this.mod = mod;
        this.ns = MultiplayerMod.ns;
        
        // send save so client can load it and sync
        if (File.Exists(manager.rainWorld.progression.saveFilePath))
        {
            byte[] data = File.ReadAllBytes(manager.rainWorld.progression.saveFilePath);
            int length = data.Length;
            ns.Write(BitConverter.GetBytes(length), 0, 4);
            ns.Write(data, 0, data.Length);
        }
        else
        {
            // no save to send.
            ns.Write(BitConverter.GetBytes(0), 0, 4);
        }
        // follow the server's player
        cameras[0].followAbstractCreature = Players[0];
        
        On.RWInput.PlayerInput += InputHook;
    }
    
    public override void Update()
    {
        base.Update();
        
        // Player.InputPackage imp = RWInput.PlayerInput(0, manager.rainWorld.options, manager.rainWorld.setup);
        Player.InputPackage imp = (Player.InputPackage)PlayerInput.Invoke(null, new object[] { 0, manager.rainWorld.options, manager.rainWorld.setup });
        ns.Write(new byte[]{MultiplayerMod.ConvertInputPackage(imp)}, 0, 1);
        
        byte[] bimp = new byte[1];
        ns.Read(bimp, 0, 1);
        multiplayerInput = MultiplayerMod.GetInputPackage(bimp[0]);
    }
    
    public Player.InputPackage InputHook(On.RWInput.orig_PlayerInput orig, int playerNumber, Options options, RainWorldGame.SetupValues setup)
    {
        if (playerNumber == 0)
        {
            return orig(0, options, setup);
        }
        else
        {
            return multiplayerInput;
        }
    }
    
    public MultiplayerMod mod;
    public NetworkStream ns;
    public Player.InputPackage multiplayerInput;
    
    public static MethodInfo PlayerInput = typeof(Player).Assembly.GetType("RWInput", true).GetMethod("PlayerInput", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
}