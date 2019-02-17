using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Net.Sockets;
using System.Collections.Generic;

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
        On.AbstractRoom.Abstractize += AbstractizeHook;
    }
    
    public override void Update()
    {
        base.Update();
        RealizeRequestedRooms();
        
        // Player.InputPackage imp = RWInput.PlayerInput(0, manager.rainWorld.options, manager.rainWorld.setup);
        Player.InputPackage imp = (Player.InputPackage)PlayerInput.Invoke(null, new object[] { 0, manager.rainWorld.options, manager.rainWorld.setup });
        ns.Write(new byte[]{MultiplayerMod.ConvertInputPackage(imp)}, 0, 1);
        
        byte[] bimp = new byte[1];
        ns.Read(bimp, 0, 1);
        multiplayerInput = MultiplayerMod.GetInputPackage(bimp[0]);
    }
    
    public void RealizeRequestedRooms()
    {
        byte[] room = new byte[2];
        ns.Read(room, 0, 2);
        short result = BitConverter.ToInt16(room, 0);
        while (result != -1) // -1 means stop
        {
            if (result >= 0) // positive = request room
            {
                AbstractRoom aroom = world.GetAbstractRoom((int)result);
                if (aroom.realizedRoom == null)
                {
                    world.GetAbstractRoom((int)result).RealizeRoom(world, this);
                    Debug.Log("Server - realizing room " + result + " from request");
                }
                else
                {
                    Debug.Log("Server - requested already realized room " + result);
                }
                clientRooms.Add((int)result);
            }
            else // negative = room not needed anymore
            {
                int abstractizedRoom = -result + 2;
                if (clientRooms.Contains(abstractizedRoom))
                {
                    clientRooms.Remove(abstractizedRoom);
                    Debug.Log("Server - client no longer needs " + abstractizedRoom);
                }
                else
                {
                    Debug.LogError("Server - possible desync? Client double-abstractized " + abstractizedRoom);
                }
            }
            ns.Read(room, 0, 2);
            result = BitConverter.ToInt16(room, 0);
        }
    }
    
    public void AbstractizeHook(On.AbstractRoom.orig_Abstractize orig, AbstractRoom room)
    {
        if (clientRooms.Contains(room.index))
        {
            Debug.Log("Server - Could not abstractize " + room.index + " because client still needs it!");
        }
        else
        {
            Debug.Log("Server - Client does not need " + room.index + ", abstractizing...");
            room.Abstractize();
        }
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
    public List<int> clientRooms = new List<int>();
    
    public static MethodInfo PlayerInput = typeof(Player).Assembly.GetType("RWInput", true).GetMethod("PlayerInput", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
}