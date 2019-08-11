using System.Text.RegularExpressions;
using System.Collections.Generic;
using Pipliz;
using Chatting;
using Chatting.Commands;
using BlockEntities.Implementations;

namespace ColonyCommands
{

	public class WarpBannerChatCommand : IChatCommand
	{

		public bool TryDoCommand (Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/warpbanner")) {
				return false;
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "warp.banner.self")) {
				return true;
			}

			Players.Player targetPlayer = null;
			Colony targetColony = null;
			Match m = Regex.Match(chattext, @"/warpbanner (?<target>['].+[']|[^ ]+)");
			if (m.Success) {
				string error;
				if (!PlayerHelper.TryGetColony(m.Groups["target"].Value, out targetColony, out error) &&
					!PlayerHelper.TryGetPlayer(m.Groups["target"].Value, out targetPlayer, out error)) {
						Chat.Send(causedBy, $"Could not find target: {error}");
						return true;
				}
			}

			string permission = AntiGrief.MOD_PREFIX + "warp.banner";
			if (targetColony != null) {
				if (targetColony.Owners.ContainsByReference(causedBy)) {
					permission += ".self";
				}
				if (!PermissionsManager.CheckAndWarnPermission(causedBy, permission)) {
					return true;
				}

			} else if (targetPlayer != null) {
				if (!PermissionsManager.CheckAndWarnPermission(causedBy, permission)) {
					return true;
				}
			} else {
				targetPlayer = causedBy;
			}

			if (targetColony == null) {
				int num = int.MaxValue;
				Colony[] colonies = targetPlayer.Colonies;
				for (int i = 0; i < colonies.Length; i++) {
					BannerTracker.Banner found;
					int closestDistance = colonies[i].Banners.GetClosestDistance(targetPlayer.VoxelPosition, out found);
					if (closestDistance < num) {
						targetColony = colonies[i];
						num = closestDistance;
					}
				}
				if (targetColony == null) {
					Chat.Send(causedBy, $"Could not find any banner for '{targetPlayer.Name}'");
					return true;
				}
			}

			Helper.TeleportPlayer(causedBy, targetColony.Banners[0].Position.Vector);
			return true;
		}
	}
}

