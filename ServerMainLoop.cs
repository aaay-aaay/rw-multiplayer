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
    
    public override void Update()
    {
        base.Update();
    }
    
    public MultiplayerMod mod;
    public NetworkStream ns;
}