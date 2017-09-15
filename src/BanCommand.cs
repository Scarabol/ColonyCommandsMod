using System;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class BanChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.ban.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new BanChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/ban") || chat.StartsWith ("/ban ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "ban")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/ban (?<targetplayername>['].+?[']|[^ ]+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /ban [targetplayername]");
          return true;
        }
        string targetPlayerName = m.Groups ["targetplayername"].Value;
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        Chat.Send (targetPlayer, "<color=red>You were banned from the server</color>");
        Chat.SendToAll (string.Format ("{0} is banned by {1}", targetPlayer.Name, causedBy.Name));
        BlackAndWhitelisting.AddBlackList (targetPlayer.ID.steamID.m_SteamID);
        Players.Disconnect (targetPlayer);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
