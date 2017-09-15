using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class TrashChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.trash.registercommand")]
    public static void AfterItemTypesServer ()
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
          playerStockpile.Remove (itemType, actualAmount);
          Chat.Send (causedBy, string.Format ("Trashed {0} x {1}", actualAmount, ItemTypes.IndexLookup.GetName (itemType)));
        } else {
          Chat.Send (causedBy, "Could not get stockpile");
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
