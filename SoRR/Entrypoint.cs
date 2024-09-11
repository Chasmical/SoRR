using System;
using Chasm.Collections;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

namespace SoRR
{
    public static class Entrypoint
    {
        public static void Run()
        {
            string[] tags = CurrentPlayer.ReadOnlyTags();
            NetworkManager network = NetworkManager.Singleton;

            if (tags.Contains("Client"))
            {
                network.StartClient();
                return;
            }
            _ = tags.Contains("Server") ? network.StartServer() : network.StartHost();

            Item item = Game.CreateItem<LampOfCubey>(1);
            DroppedItem dropped = Game.DropItem(item, Vector2.zero);

            Game.SpawnAgent<ResistanceLeader>(new Vector2(4, 0));
        }

    }
}
