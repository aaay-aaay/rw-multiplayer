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
        manager.rainWorld.options.saveSlot = 42;
        
        byte[] lenBytes = new byte[4];
        ns.Read(lenBytes, 0, 4);
        int len = BitConverter.ToInt32(lenBytes, 0);
        if (len != 0)
        {
            byte[] data = new byte[len];
            ns.Read(data, 0, len);
            File.WriteAllBytes(manager.rainWorld.progression.saveFilePath, data);
        }
        cameras[0].followAbstractCreature = Players[1];
        
        On.RWInput.PlayerInput += PlayerInputHook;
        On.AbstractRoom.RealizeRoom += RealizeRoomHook;
    }
    
    public unsafe override void Update()
    {
        if (!hasSetSeed)
        {
            UnityEngine.Random.seed = 4;
            hasSetSeed = true;
        }
        else
        {
            UnityEngine.Random.seed = savedSeed;
        }
        
        byte[] data = new byte[1];
        ns.Read(data, 0, 1);
        multiplayerInput = MultiplayerMod.GetInputPackage(data[0]);
        
        byte[] seedData = new byte[4];
        ns.Read(seedData, 0, 4);
        int newSeed = BitConverter.ToInt32(seedData, 0);
        if (newSeed != UnityEngine.Random.seed)
        {
        }
        UnityEngine.Random.seed = newSeed;
        
        Player.InputPackage inp = (Player.InputPackage)typeof(RainWorld).Assembly.GetType("RWInput", true).GetMethod("PlayerInput", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[]{-1, manager.rainWorld.options, manager.rainWorld.setup});
        data[0] = MultiplayerMod.ConvertInputPackage(inp);
        ns.Write(data, 0, 1);
        
        
        base.Update();
        
        data = new byte[]{0,0};
        ns.Write(data, 0, 2);
        
        savedSeed = UnityEngine.Random.seed;
    }
    
    public Player.InputPackage PlayerInputHook(On.RWInput.orig_PlayerInput orig, int playerNumber, Options options, RainWorldGame.SetupValues setup)
    {
        if (playerNumber == 0) return multiplayerInput;
        return orig(0, options, setup);
    }
    
    /*
    public void RealizeRoomHook(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom room, World world, RainWorldGame game)
    {
        if (room.realizedRoom != null || room.offScreenDen) return;
        orig(room, world, game);
    }
    */
    
    public Player.InputPackage multiplayerInput;
    public MultiplayerMod mod;
    public NetworkStream ns;
    public bool hasSetSeed;
    public int savedSeed;
    
    public static bool requestedRoomRealize;
}