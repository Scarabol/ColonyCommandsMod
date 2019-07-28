using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{


  public class TrashPlayerChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/trashplayer") || chat.StartsWith ("/trashplayer ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, AntiGrief.MOD_PREFIX + "trashplayer")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/trashplayer (?<targetplayername>['].+[']|[^ ]+) (?<itemname>['].+?[']|[^ ]+)( (?<amount>.+))?");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /trashplayer [targetplayername] [itemname] [amount]");
        return true;
      }
      var todoPlayers = Players.PlayerDatabase.ValuesAsList;
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      if (!targetPlayerName.Equals ("all")) {
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
          Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
          return true;
        }
        todoPlayers = new List<Players.Player> ();
        todoPlayers.Add (targetPlayer);
      }
      var itemname = m.Groups ["itemname"].Value;
      ushort trashItemType = 0;
      if (!itemname.Equals ("all") && !ItemTypes.IndexLookup.TryGetIndex (itemname, out trashItemType) && trashItemType == 0) {
        Chat.Send (causedBy, $"Could not find item '{itemname}'");
        return true;
      }
      if (targetPlayerName.Equals ("all") && itemname.Equals ("all")) {
        Chat.Send (causedBy, $"You don't want to trash all items for all players");
        return true;
      }
      var amountNum = int.MaxValue;
      var amount = m.Groups ["amount"].Value;
      if (!string.IsNullOrEmpty (amount) && !amount.Equals ("all") && !int.TryParse (amount, out amountNum)) {
        Chat.Send (causedBy, $"Could not parse amount '{amount}'");
        return true;
      }
      int totalnum = 0;
      foreach (var player in todoPlayers) {
        int removedPerPlayer = 0;
        int numRemove = amountNum;

        // inventory first
        Inventory playerInventory;
        if (Inventory.TryGetInventory (player, out playerInventory)) {
          if (trashItemType == 0) {
            playerInventory.Clear ();
            Log.Write ($"Cleared the inventory of {player.ID.ToStringReadable()}");
          } else {
            int todoRemove = 0;
            foreach (var item in playerInventory.Items) {
              if (item.Type == trashItemType) {
                todoRemove = System.Math.Min (numRemove, item.Amount);
                if (playerInventory.TryRemove (trashItemType, todoRemove)) {
                  numRemove -= todoRemove;
                  totalnum += todoRemove;
                  removedPerPlayer += todoRemove;
                }
              }
            }
            if (removedPerPlayer > 0) {
              Log.Write ($"Removed {removedPerPlayer} items from inventory of {player.ID.ToStringReadable()}");
            }
          }
        }

        // then stockpile
        Stockpile playerStockpile = Stockpile.GetStockPile(player);
        if (playerStockpile != null) {
          if (trashItemType == 0) {
            var item = playerStockpile.GetByIndex (0);
            while (item != InventoryItem.Empty) {
              playerStockpile.TryRemove (item);
              item = playerStockpile.GetByIndex (0);
            }
            Log.Write ($"Cleared the stockpile of {player.ID.ToStringReadable()}");
          } else {
            var todoRemove = System.Math.Min (numRemove, playerStockpile.AmountContained (trashItemType));
            if (playerStockpile.TryRemove (trashItemType, todoRemove)) {
              numRemove -= todoRemove;
              totalnum += todoRemove;
              removedPerPlayer += todoRemove;
              Log.Write ($"Removed {todoRemove} items from stockpile of {player.ID.ToStringReadable()}");
            }
          }
        }

        if (removedPerPlayer > 0) {
          player.ShouldSave = true;
        }
      }
      var strnum = totalnum.ToString ();
      if (amount.Equals ("all")) {
        strnum = "all";
      }
      Chat.Send (causedBy, $"Removed {strnum} items of type {itemname} from {todoPlayers.Count} players");
      return true;
    }
  }
}
