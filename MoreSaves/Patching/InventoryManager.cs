namespace MoreSaves.Patching
{
    using HarmonyLib;
    using JumpKing.MiscEntities.WorldItems.Inventory;

    public class InventoryManager
    {
        private static readonly Traverse Inventory;

        static InventoryManager()
        {
            var inventoryManager = AccessTools.TypeByName("JumpKing.MiscEntities.WorldItems.Inventory.InventoryManager");

            Inventory = Traverse.Create(inventoryManager).Property("inventory");
        }

        public static void SetInventory(Inventory inv) => Inventory.SetValue(inv);

        public static Inventory GetInventory() => Inventory.GetValue<Inventory>();
    }
}
