using System;
using System.Text.RegularExpressions;
using Pipliz.Chatting;
using Pipliz;
using ChatCommands;

namespace ScarabolMods
{

  public class TradeChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/trade") || chat.StartsWith ("/trade ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
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
      Stockpile sourceStockpile = Stockpile.GetStockPile(causedBy);
      Stockpile targetStockpile = Stockpile.GetStockPile(targetPlayer);
      if (sourceStockpile == null || targetStockpile == null) {
        Chat.Send (causedBy, "Could not get stockpile for both players");
        return false;
      }
      InventoryItem tradeItem = new InventoryItem (itemType, amount);
      if (sourceStockpile.TryRemove (tradeItem)) {
        targetStockpile.Add (tradeItem);
        Chat.Send (causedBy, $"Send {amount} x {itemTypeName} to '{targetPlayer.Name}'");
        Chat.Send (targetPlayer, $"Received {amount} x {itemTypeName} from '{causedBy.Name}'");
      } else {
        Chat.Send (causedBy, "You don't have enough items");
      }
      return true;
    }
  }
}
