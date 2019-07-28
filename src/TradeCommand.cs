using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class TradeChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/trade")) {
		return false;
		}
      var m = Regex.Match (chattext, @"/trade (?<playername>['].+[']|[^ ]+) (?<material>.+) (?<amount>\d+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /trade [playername] [material] [amount]");
        return true;
      }
      var targetPlayerName = m.Groups ["playername"].Value;
      if (targetPlayerName.StartsWith ("'")) {
        if (targetPlayerName.EndsWith ("'")) {
          targetPlayerName = targetPlayerName.Substring (1, targetPlayerName.Length - 2);
        } else {
          Chat.Send (causedBy, "Command didn't match, missing ' after playername");
          return true;
        }
      }
      if (targetPlayerName.Length < 1) {
        Chat.Send (causedBy, "Command didn't match, no playername given");
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
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
	  Colony sourceColony = causedBy.ActiveColony;
	  Colony targetColony = targetPlayer.ActiveColony;
      if (sourceColony == null || targetColony == null) {
        Chat.Send (causedBy, "Coud not get active colony for both players");
        return false;
      }
      InventoryItem tradeItem = new InventoryItem (itemType, amount);
      if (sourceColony.Stockpile.TryRemove (tradeItem)) {
        targetColony.Stockpile.Add (tradeItem);
        Chat.Send (causedBy, $"Send {amount} x {itemTypeName} to '{targetPlayer.Name}'");
        Chat.Send (targetPlayer, $"Received {amount} x {itemTypeName} from '{causedBy.Name}'");
      } else {
        Chat.Send (causedBy, "You don't have enough items");
      }
      return true;
    }
  }
}
