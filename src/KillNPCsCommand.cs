using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class KillNPCsChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.killnpcs.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new KillNPCsChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/killnpcs") || chat.StartsWith ("/killnpcs ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "killnpcs")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/killnpcs (?<targetplayername>['].+?[']|[^ ]+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /killnpcs [targetplayername]");
          return true;
        }
        string targetPlayerName = m.Groups ["targetplayername"].Value;
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error, true)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        List<NPC.NPCBase> cachedFollowers = new List<NPC.NPCBase> (Colony.Get (targetPlayer).Followers);
        foreach (NPC.NPCBase npc in cachedFollowers) {
          npc.OnDeath ();
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
