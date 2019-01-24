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
    }
    
    public override void Update()
    {
        base.Update()
    }
    
    public MultiplayerMod mod;
    public NetworkStream ns;
}