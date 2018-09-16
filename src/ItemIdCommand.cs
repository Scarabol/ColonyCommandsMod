﻿using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class ItemIdChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/itemid");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var inventory = Inventory.GetInventory (causedBy);
      var reply = "";
      for (var slot = 0; slot < inventory.Items.Length; slot++) {
        var item = inventory.Items [slot];
        string typename;
        if (ItemTypes.TryGetType (item.Type, out ItemTypes.ItemType itemType)) {
          typename = itemType.Name;
        } else {
          typename = "unknown";
        }
        reply += $"slot {slot + 1} contains: '{typename}'\n";
      }
      Chat.Send (causedBy, reply);
      return true;
    }
  }
}
