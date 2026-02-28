using PM = PugMod;
using UE = UnityEngine;

public class QuickUnlock : PM.IMod
{
    // Update is called once per frame
    public void EarlyInit()
    {
    }

    public void Init()
    {
    }

    public void Shutdown()
    {
    }

    public void ModObjectLoaded(UE.Object obj)
    {
    }

    public void Update()
    {
        var player = Manager.main.player;

        if (player == null)
            return;

        // we only care if the player is right-clicking
        if (!player.inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.SECOND_INTERACT))
            return;

        var held = player.GetHeldObject();
        if (held.objectID == ObjectID.None)
            return;

        var wantedType = GetWantedType(held.objectID);
        if (wantedType == null)
            return;

        var target = player.GetCurrentInteractableObject();
        if (target == null)
            return;

        foreach (var o in target.GetComponentsInParent<object>())
        {
            if (o is LockedChest chest && chest.objectInfo.objectID == wantedType)
            {
                HandleInteraction(player, chest);
                return;
            }
        }
    }

    private void HandleInteraction(PlayerController player, LockedChest chest)
    {
        var iHandler = chest.inventoryHandler;
        var chestSlot = GetOpenSlot(iHandler);

        if (chestSlot == -1)
            return;

        player.playerInventoryHandler.TryMoveTo(player, player.equippedSlotIndex, iHandler, -1, 1);
    }

    private static ObjectID? GetWantedType(ObjectID held) => held switch
    {
        ObjectID.CopperKey => ObjectID.LockedCopperChest,
        ObjectID.IronKey => ObjectID.LockedIronChest,
        ObjectID.ScarletKey => ObjectID.LockedScarletChest,
        ObjectID.OctarineKey => ObjectID.LockedOctarineChest,
        ObjectID.GalaxiteKey => ObjectID.LockedGalaxiteChest,
        ObjectID.SolariteKey => ObjectID.LockedSolariteChest,
        _ => null
    };

    private static int GetOpenSlot(InventoryHandler chest)
    {
        for (int i = 0; i < chest.size; i++)
        {
            if (chest.GetObjectData(i).objectID == ObjectID.None)
                return i;
        }

        return -1;
    }
}