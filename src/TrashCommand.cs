using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class TrashChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/trash")) {
		return false;
		}
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
      var removeAmount = Int32.Parse (m.Groups ["amount"].Value);
      if (removeAmount <= 0) {
        Chat.Send (causedBy, "Command didn't match, amount too low");
        return true;
      }

      Colony colony = causedBy.ActiveColony;
	  if (colony == null) {
		Chat.Send(causedBy, "You have to be near an active colony to use this command");
		return true;
	  }

      // delete from the player's inventory first
      int totalRemoved = 0;
      Inventory playerInventory = causedBy.Inventory;
        foreach (var item in playerInventory.Items) {
          if (item.Type == itemType) {
            int todoRemove = System.Math.Min(removeAmount, item.Amount);
            if (playerInventory.TryRemove(item.Type, todoRemove)) {
              removeAmount -= todoRemove;
              totalRemoved += todoRemove;
            }
          }
        }

      // then delete from the stockpile
      Stockpile playerStockpile = colony.Stockpile;
      if (playerStockpile == null) {
        Chat.Send(causedBy, "Could not get stockpile");
      } else {
        var actualAmount = System.Math.Min (playerStockpile.AmountContained (itemType), removeAmount);
        if (playerStockpile.TryRemove (itemType, actualAmount)) {
          totalRemoved += actualAmount;
          Chat.Send (causedBy, $"Trashed {totalRemoved} x {ItemTypes.IndexLookup.GetName (itemType)}");
        } else {
          Chat.Send (causedBy, $"Not enough items in stockpile");
        }
      }

      if (totalRemoved > 0) {
        // causedBy.ShouldSave = true;
      }

      return true;
    }

  }
}

