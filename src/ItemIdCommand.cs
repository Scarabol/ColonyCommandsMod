using System;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class ItemIdChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.itemid.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new ItemIdChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/itemid");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        var inventory = Inventory.GetInventory (causedBy);
        string reply = "";
        for (int c = 0; c < inventory.Items.Length; c++) {
          var item = inventory.Items [c];
          string typename;
          if (ItemTypes.TryGetType (item.Type, out ItemTypes.ItemType itemType)) {
            typename = itemType.Name;
          } else {
            typename = "unknown";
          }
          reply += $"slot {c + 1} contains: '{typename}'\n";
        }
        Chat.Send (causedBy, reply);
      } catch (Exception exception) {
        Log.WriteError (string.Format ("Exception while parsing command; {0} - {1}", exception.Message, exception.StackTrace));
      }
      return true;
    }
  }
}
