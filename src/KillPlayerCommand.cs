using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

	public class KillPlayerChatCommand : IChatCommand
	{

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/killplayer")) {
				return false;
			}
			var m = Regex.Match(chattext, @"/killplayer (?<targetplayername>['].+[']|[^ ]+)");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /killplayer [targetplayername]");
				return true;
			}
			string targetPlayerName = m.Groups["targetplayername"].Value;
			Players.Player targetPlayer;
			string error;
			if (!PlayerHelper.TryGetPlayer(targetPlayerName, out targetPlayer, out error, true)) {
				Chat.Send(causedBy, $"Could not find '{targetPlayerName}'; {error}");
				return true;
			}

			string permission = AntiGrief.MOD_PREFIX + "killplayer";
			if (causedBy == targetPlayer) {
				permission += ".self";
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, permission)) {
				return true;
			}

			Players.OnDeath(targetPlayer);
			targetPlayer.SendHealthPacket();
			if (targetPlayer == causedBy) {
				Chat.SendToConnected($"Player {causedBy.Name} committed suicide");
			} else {
				Chat.SendToConnected($"Player {targetPlayer.Name} was killed by {causedBy.Name}");
			}
			return true;
		}
	}
}

