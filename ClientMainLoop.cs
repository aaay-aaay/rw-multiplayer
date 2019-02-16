using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Net.Sockets;

public class ClientMainLoop : RainWorldGame
{
    public ClientMainLoop(ProcessManager manager, MultiplayerMod mod) : base(manager)
    {
        this.mod = mod;
        this.ns = MultiplayerMod.ns;
        // avoid deleting existing save
        manager.rainWorld.options.saveSlot = 42;
        
        // get save
        byte[] lenBytes = new byte[4];
        ns.Read(lenBytes, 0, 4);
        int len = BitConverter.ToInt32(lenBytes, 0);
        if (len != 0) // if len = 0 there isn't a save
        {
            byte[] data = new byte[len];
            ns.Read(data, 0, len);
            File.WriteAllBytes(manager.rainWorld.progression.saveFilePath, data);
        }
        // follow the client's player
        cameras[0].followAbstractCreature = Players[1];
        
        On.RWInput.PlayerInput += InputHook;
    }
    
    public override void Update()
    {
        base.Update();
        
        byte[] bimp = new byte[1];
        ns.Read(bimp, 0, 1);
        multiplayerInput = MultiplayerMod.GetInputPackage(bimp[0]);
        
        // Player.InputPackage imp = RWInput.PlayerInput(0, manager.rainWorld.options, manager.rainWorld.setup);
        Player.InputPackage imp = (Player.InputPackage)PlayerInput.Invoke(null, new object[] { 1, manager.rainWorld.options, manager.rainWorld.setup });
        ns.Write(new byte[]{MultiplayerMod.ConvertInputPackage(imp)}, 0, 1);
    }
    
    public Player.InputPackage InputHook(On.RWInput.orig_PlayerInput orig, int playerNumber, Options options, RainWorldGame.SetupValues setup)
    {
        if (playerNumber == 1)
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