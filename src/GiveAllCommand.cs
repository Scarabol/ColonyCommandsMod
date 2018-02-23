using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class GiveAllChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands..registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new GiveAllChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/giveall") || chat.StartsWith ("/giveall ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "giveall")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/giveall (?<material>.+) (?<amount>\d+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /giveall [material] [amount]");
          return true;
        }
        string itemTypeName = m.Groups ["material"].Value;
        ushort itemType;
        if (!ItemTypes.IndexLookup.TryGetIndex (itemTypeName, out itemType)) {
          Chat.Send (causedBy, "Command didn't match, item type not found");
          return true;
        }
        int amount = Int32.Parse (m.Groups ["amount"].Value);
        if (amount < 1) {
          Chat.Send (causedBy, "Command didn't match, amount too low");
          return true;
        }
        foreach (Players.Player player in Players.PlayerDatabase.ValuesAsList) {
          Stockpile playerStockpile;
          if (Stockpile.TryGetStockpile (causedBy, out playerStockpile)) {
            playerStockpile.Add (itemType, amount);
            player.ShouldSave = true;
            Chat.Send (player, string.Format ("You received some items from {0}, check your stockpile", causedBy.Name));
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
