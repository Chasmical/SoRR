using System;
using UnityEngine;

namespace SoRR
{
    public static class Entrypoint
    {
        public static void Run()
        {
            Item item = Game.CreateItem<LampOfCubey>(1);
            DroppedItem dropped = Game.DropItem(item, Vector2.zero);
        }

    }
}
