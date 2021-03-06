﻿using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class WhisperChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.whisper.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new WhisperChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/w") || chat.StartsWith ("/w ") || chat.Equals ("/whisper") || chat.StartsWith ("/whisper ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var m = Regex.Match (chattext, @"/((w)|(whisper)) (?<targetplayername>['].+?[']|[^ ]+) (?<message>.+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /w [targetplayername] [message] or /whisper [targetplayername] [message]");
        return true;
      }
      var targetPlayerName = m.Groups ["targetplayername"].Value;
      Players.Player targetPlayer;
      string error;
      if (!PlayerHelper.TryGetPlayer (targetPlayerName, out targetPlayer, out error)) {
        Chat.Send (causedBy, $"Could not find target player '{targetPlayerName}'; {error}");
        return true;
      }
      var message = m.Groups ["message"].Value;
      Chat.Send (targetPlayer, $"<color=cyan>From [{causedBy.Name}]: {message}</color>");
      Chat.Send (causedBy, $"<color=cyan>To [{targetPlayer}]: {message}</color>");
      return true;
    }
  }
}
