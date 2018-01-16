using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class TrashChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.trash.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new TrashChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/trash") || chat.StartsWith ("/trash ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        var m = Regex.Match (chattext, @"/trash (?<material>.+) (?<amount>\d+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /trash [material] [amount]");
          return true;
        }
        string itemTypeName = m.Groups ["material"].Value;
        ushort itemType;
        if (!ItemTypes.IndexLookup.TryGetIndex (itemTypeName, out itemType)) {
          Chat.Send (causedBy, "Command didn't match, item type not found");
          return true;
        }
        int amount = Int32.Parse (m.Groups ["amount"].Value);
        if (amount <= 0) {
          Chat.Send (causedBy, "Command didn't match, amount too low");
          return true;
        }
        Stockpile playerStockpile;
        if (Stockpile.TryGetStockpile (causedBy, out playerStockpile)) {
          int actualAmount = System.Math.Min (playerStockpile.AmountContained (itemType), amount);
          if (playerStockpile.TryRemove (itemType, actualAmount)) {
            Chat.Send (causedBy, string.Format ("Trashed {0} x {1}", actualAmount, ItemTypes.IndexLookup.GetName (itemType)));
          } else {
            Chat.Send (causedBy, string.Format ("Not enough items in stockpile"));
          }
        } else {
          Chat.Send (causedBy, "Could not get stockpile");
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class TrashAllChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.trashall.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new TrashAllChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/trashall") || chat.StartsWith ("/trashall ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "trashall")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/trashall (?<targetplayername>['].+?[']|[^ ]+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /trashall [targetplayername]");
          return true;
        }
        string targetPlayerName = m.Groups ["targetplayername"].Value;
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        Stockpile playerStockpile;
        if (Stockpile.TryGetStockpile (targetPlayer, out playerStockpile)) {
          InventoryItem item = playerStockpile.GetByIndex (0);
          while (item != InventoryItem.Empty) {
            playerStockpile.TryRemove (item);
            item = playerStockpile.GetByIndex (0);
          }
          targetPlayer.ShouldSave = true;
          Chat.Send (causedBy, $"You cleared the stockpile of {targetPlayer.IDString}");
        } else {
          Chat.Send (causedBy, $"Could not get the stockpile for {targetPlayer.IDString}");
        }
        Inventory playerInventory;
        if (Inventory.TryGetInventory (targetPlayer, out playerInventory)) {
          playerInventory.Clear ();
          Chat.Send (causedBy, $"You cleared the inventory of {targetPlayer.IDString}");
        } else {
          Chat.Send (causedBy, $"Could not get the inventory for {targetPlayer.IDString}");
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
