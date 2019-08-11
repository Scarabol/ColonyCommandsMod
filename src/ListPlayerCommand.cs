using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

	public class ListPlayerChatCommand : IChatCommand
	{

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/list")) {
				return false;
			}
			Match m = Regex.Match(chattext, @"/list (?<playername>['].+[']|[^ ]+)");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /list {playername}");
				return true;
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "listplayer")) {
				return true;
			}

			string targetName = m.Groups["playername"].Value;
			string error;
			Players.Player targetPlayer;
			if (!PlayerHelper.TryGetPlayer(targetName, out targetPlayer, out error, true)) {
				Chat.Send(causedBy, $"Could not find '{targetName}'; {error}");
				return true;
			}

			string lastSeen = ActivityTracker.GetLastSeen(targetPlayer.ID.ToStringReadable());
			string colonies = "";
			int count = 0;
			for (int i = 0; i < targetPlayer.Colonies.Length; i++) {
				if (!colonies.Equals("")) {
					colonies += ", ";
				}
				colonies += targetPlayer.Colonies[i].Name;
				count++;
			}

			Chat.Send(causedBy, $"Player {targetPlayer.Name} last seen {lastSeen}");
			Chat.Send(causedBy, $"{targetPlayer.Name} owns {count} colonies: {colonies}");
			return true;
		}
	}
}

