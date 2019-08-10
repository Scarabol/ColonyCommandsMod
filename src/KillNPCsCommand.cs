using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;
using NPC;

namespace ColonyCommands
{

	public class KillNPCsChatCommand : IChatCommand
	{

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/killnpcs")) {
				return false;
			}

			Players.Player targetPlayer = causedBy;
			var m = Regex.Match(chattext, @"/killnpcs ?(?<targetplayername>['].+[']|[^ ]+)");
			if (m.Success) {
				string targetPlayerName = m.Groups["targetplayername"].Value;
				string error;
				if (!PlayerHelper.TryGetPlayer(targetPlayerName, out targetPlayer, out error, true)) {
					Chat.Send(causedBy, $"Could not find '{targetPlayerName}'; {error}");
					return true;
				}
			}

			string permission = AntiGrief.MOD_PREFIX + "killnpcs";
			if (targetPlayer == causedBy) {
				permission += ".self";
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, permission)) {
				return true;
			}

			if (targetPlayer.ActiveColony == null) {
				Chat.Send(targetPlayer, $"You have to be at an active colony to allow NPC killing");
				if (targetPlayer != causedBy) {
					Chat.Send(causedBy, $"{targetPlayer.Name} is not at an active colony");
				}
				return true;
			}

			List<NPCBase> cachedFollowers = new List<NPCBase>(targetPlayer.ActiveColony.Followers);
			foreach (NPCBase npc in cachedFollowers) {
				npc.OnDeath();
			}
			return true;
		}

	}
}

