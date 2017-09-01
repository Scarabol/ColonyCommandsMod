using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;

namespace ScarabolMods
{
  public class TradeChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/trade") || chat.StartsWith ("/trade ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        var m = Regex.Match (chattext, @"/trade (?<playername>.+) (?<material>.+) (?<amount>\d+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /trade [playername] [material] [amount]");
          return true;
        }
        string targetPlayerName = m.Groups ["playername"].Value;
        if (targetPlayerName.StartsWith ("\"")) {
          if (targetPlayerName.EndsWith ("\"")) {
            targetPlayerName = targetPlayerName.Substring (1, targetPlayerName.Length - 2);
          } else {
            Chat.Send (causedBy, "Command didn't match, missing \" after playername");
            return true;
          }
        }
        if (targetPlayerName.Length < 1) {
          Chat.Send (causedBy, "Command didn't match, no playername given");
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
        Players.Player targetPlayer = null;
        for (int c = 0; c < Players.CountConnected; c++) {
          Players.Player player = Players.GetConnectedByIndex (c);
          if (player.Name.Equals (targetPlayerName)) {
            if (targetPlayer == null) {
              targetPlayer = player;
            } else {
              Chat.Send (causedBy, "Duplicate target player name, pls use SteamID");
              return true;
            }
          }
        }
        if (targetPlayer == null) {
          Chat.Send (causedBy, string.Format ("Could not find player '{0}'", targetPlayerName));
          return true;
        }
        Stockpile sourceStockpile;
        Stockpile targetStockpile;
        if (Stockpile.TryGetStockpile (causedBy, out sourceStockpile) && Stockpile.TryGetStockpile (targetPlayer, out targetStockpile)) {
          InventoryItem tradeItem = new InventoryItem (itemType, amount);
          if (sourceStockpile.TryRemove (tradeItem)) {
            targetStockpile.Add (tradeItem);
            Chat.Send (causedBy, string.Format ("Send {0} x {1} to '{2}'", amount, ItemTypes.IndexLookup.GetName (itemType), targetPlayer.Name));
          } else {
            Chat.Send (causedBy, "You don't have enough items");
            return true;
          }
        } else {
          Chat.Send (causedBy, "Could not get stockpile for both players");
          return true;
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
