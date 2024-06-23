using HarmonyLib;
using JumpKing.MiscEntities.WorldItems.Inventory;
using System;

namespace MoreSaves.Patching
{
    public class InventoryManager
    {
        private static readonly Type inventoryManager;

        private static readonly Traverse inventory;

        static InventoryManager()
        {
            inventoryManager = AccessTools.TypeByName("JumpKing.MiscEntities.WorldItems.Inventory.InventoryManager");

            inventory = Traverse.Create(inventoryManager).Property("inventory");
        }

        public static Inventory GetInventory()
        {
            return inventory.GetValue<Inventory>();
        }
    }
}
