using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class WhisperChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.whisper.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new WhisperChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/w") || chat.StartsWith ("/w ") || chat.Equals ("/whisper") || chat.StartsWith ("/whisper ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        var m = Regex.Match (chattext, @"/[w|whisper] (?<targetplayername>['].+?[']|[^ ]+) (?<message>.+)");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /w [targetplayername] [message]");
          return true;
        }
        string targetPlayerName = m.Groups ["targetplayername"].Value;
        Players.Player targetPlayer;
        string error;
        if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", targetPlayerName, error));
          return true;
        }
        string message = m.Groups ["message"].Value;
        Chat.Send (targetPlayer, string.Format ("<color=cyan>From [{0}]: {1}</color>", causedBy.Name, message));
        Chat.Send (causedBy, string.Format ("<color=cyan>To [{0}]: {1}</color>", targetPlayer, message));
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
