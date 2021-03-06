﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class TrashChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.trash.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new TrashChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/trash") || chat.StartsWith ("/trash ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var m = Regex.Match (chattext, @"/trash (?<material>.+) (?<amount>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /trash [material] [amount]");
        return true;
      }
      var itemTypeName = m.Groups ["material"].Value;
      ushort itemType;
      if (!ItemTypes.IndexLookup.TryGetIndex (itemTypeName, out itemType)) {
        Chat.Send (causedBy, "Command didn't match, item type not found");
        return true;
      }
      var amount = Int32.Parse (m.Groups ["amount"].Value);
      if (amount <= 0) {
        Chat.Send (causedBy, "Command didn't match, amount too low");
        return true;
      }
      Stockpile playerStockpile;
      if (Stockpile.TryGetStockpile (causedBy, out playerStockpile)) {
        var actualAmount = System.Math.Min (playerStockpile.AmountContained (itemType), amount);
        if (playerStockpile.TryRemove (itemType, actualAmount)) {
          Chat.Send (causedBy, $"Trashed {actualAmount} x {ItemTypes.IndexLookup.GetName (itemType)}");
        } else {
          Chat.Send (causedBy, $"Not enough items in stockpile");
        }
      } else {
        Chat.Send (causedBy, "Could not get stockpile");
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class TrashPlayerChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.trashplayer.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new TrashPlayerChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/trashplayer") || chat.StartsWith ("/trashplayer ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "trashplayer")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/trashplayer (?<targetplayername>['].+?[']|[^ ]+) (?<itemname>['].+?[']|[^ ]+)( (?<amount>.+))?");
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
      var totalnum = 0;
      foreach (var player in todoPlayers) {
        var numRemove = amountNum;
        Stockpile playerStockpile;
        if (Stockpile.TryGetStockpile (player, out playerStockpile)) {
          if (trashItemType == 0) {
            var item = playerStockpile.GetByIndex (0);
            while (item != InventoryItem.Empty) {
              playerStockpile.TryRemove (item);
              item = playerStockpile.GetByIndex (0);
            }
            Log.Write ($"Cleared the stockpile of {player.IDString}");
          } else {
            var todoRemove = System.Math.Min (numRemove, playerStockpile.AmountContained (trashItemType));
            if (playerStockpile.TryRemove (trashItemType, todoRemove)) {
              numRemove -= todoRemove;
              totalnum += todoRemove;
              Log.Write ($"Removed {todoRemove} items from stockpile of {player.IDString}");
            }
          }
          player.ShouldSave = true;
        }
        Inventory playerInventory;
        if (Inventory.TryGetInventory (player, out playerInventory)) {
          if (trashItemType == 0) {
            playerInventory.Clear ();
            Log.Write ($"Cleared the inventory of {player.IDString}");
          } else {
            int todoRemove = 0;
            foreach (var item in playerInventory.Items) {
              if (item.Type == trashItemType) {
                todoRemove = System.Math.Min (numRemove, item.Amount);
                break;
              }
            }
            if (playerInventory.TryRemove (trashItemType, todoRemove)) {
              numRemove -= todoRemove;
              totalnum += todoRemove;
              Log.Write ($"Removed {todoRemove} items from inventory of {player.IDString}");
            }
          }
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
