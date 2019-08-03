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

			Players.Player targetPlayer = causedBy;
			var m = Regex.Match(chattext, @"/warpbanner (?<targetplayer>['].+[']|[^ ]+)");
			if (m.Success) {
				if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "warp.banner")) {
					return true;
				}
				string error;
				if (!PlayerHelper.TryGetPlayer(m.Groups["targetplayer"].Value, out targetPlayer, out error)) {
					Chat.Send(causedBy, $"Could not find target: {error}");
					return true;
				}
			}

			BannerTracker.Banner targetBanner = null;
			int num = int.MaxValue;
			Colony[] colonies = targetPlayer.Colonies;
			for (int i = 0; i < colonies.Length; i++) {
				BannerTracker.Banner found;
				int closestDistance = colonies[i].Banners.GetClosestDistance(targetPlayer.VoxelPosition, out found);
				if (closestDistance < num) {
					targetBanner = found;
					num = closestDistance;
				}
			}
			if (targetBanner == null) {
				Chat.Send(causedBy, $"Could not find any banner for '{targetPlayer.Name}'");
				return true;
			}

			Helper.TeleportPlayer(targetPlayer, targetBanner.Position.Vector);
			return true;
		}
	}
}

