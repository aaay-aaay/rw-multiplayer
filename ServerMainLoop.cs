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
        On.RWInput.PlayerInput += PlayerInputHook;
        
        if (File.Exists(manager.rainWorld.progression.saveFilePath))
        {
            byte[] data = File.ReadAllBytes(manager.rainWorld.progression.saveFilePath);
            int length = data.Length;
            ns.Write(BitConverter.GetBytes(length), 0, 4);
            ns.Write(data, 0, data.Length);
        }
        else
        {
            ns.Write(BitConverter.GetBytes(0), 0, 4);
        }
        cameras[0].followAbstractCreature = Players[0];
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
        Player.InputPackage inp = (Player.InputPackage)typeof(RainWorld).Assembly.GetType("RWInput", true).GetMethod("PlayerInput", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[]{0, manager.rainWorld.options, manager.rainWorld.setup});
        data[0] = MultiplayerMod.ConvertInputPackage(inp);
        ns.Write(data, 0, 1);
        
        int seed = UnityEngine.Random.seed;
        ns.Write(BitConverter.GetBytes(seed), 0, 4);
        UnityEngine.Random.seed = seed;
        
        ns.Read(data, 0, 1);
        multiplayerInput = MultiplayerMod.GetInputPackage(data[0]);
        
        base.Update();
        savedSeed = UnityEngine.Random.seed;
    }
    
    public Player.InputPackage PlayerInputHook(On.RWInput.orig_PlayerInput orig, int playerNumber, Options options, RainWorldGame.SetupValues setup)
    {
        if (playerNumber == 1) return multiplayerInput;
        return orig(playerNumber, options, setup);
    }
    
    public Player.InputPackage multiplayerInput;
    public MultiplayerMod mod;
    public NetworkStream ns;
    public bool hasSetSeed;
    public int savedSeed;
}