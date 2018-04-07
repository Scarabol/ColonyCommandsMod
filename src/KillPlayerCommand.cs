using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class KillPlayerChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.killplayer.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new KillPlayerChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/killplayer") || chat.StartsWith ("/killplayer ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "killplayer")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/killplayer (?<targetplayername>['].+?[']|[^ ]+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /killplayer [targetplayername]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      Players.OnDeath (targetPlayer);
      targetPlayer.SendHealthPacket ();
      if (targetPlayer == causedBy) {
        Chat.SendToAll ($"Player {causedBy.Name} killed himself");
      } else {
        Chat.SendToAll ($"Player {targetPlayer.Name} was killed by {causedBy.Name}");
      }
      return true;
    }
  }
}
