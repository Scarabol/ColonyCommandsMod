using System.Collections.Generic;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class ItemIdChatCommand : IChatCommand
  {

    public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
    {
	  if (!splits[0].Equals ("/itemid")) {
		return false;
	}
      string reply = "";
      for (int slot = 0; slot < causedBy.Inventory.Items.Length; slot++) {
        var item = causedBy.Inventory.Items [slot];
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
