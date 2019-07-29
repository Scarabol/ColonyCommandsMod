using System.Collections.Generic;
using Pipliz;
using Chatting;
using Chatting.Commands;
using BlockEntities.Implementations;

namespace ColonyCommands
{
	public class PurgeBannerCommand : IChatCommand
	{
		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/purgebanner")) {
				return false;
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.PERMISSION_SUPER)) {
				return true;
			}

			Colony colony = null;
			BannerTracker.Banner banner = null;
			int shortestDistance = int.MaxValue;
			foreach (Colony checkColony in ServerManager.ColonyTracker.ColoniesByID.Values) {
				foreach (BannerTracker.Banner checkBanner in checkColony.Banners) {
					int distX = (int)(causedBy.Position.x - checkBanner.Position.x);
					int distZ = (int)(causedBy.Position.z - checkBanner.Position.z);
					int distance = (int)System.Math.Sqrt(System.Math.Pow(distX, 2) + System.Math.Pow(distZ, 2));
					if (distance < shortestDistance) {
						shortestDistance = distance;
						banner = checkBanner;
						colony = checkColony;
					}
				}
			}

			if (banner == null) {
				Chat.Send(causedBy, "No banners found at all");
				return true;
			}
			if (shortestDistance > 100) {
				Chat.Send(causedBy, "Closest banner is more than 100 blocks away. Not doing anything, too risky");
				return true;
			}

			// command: /purgebanner { colony | <playername> }
			if (splits.Count == 2) {
				if (splits[1].Equals("colony") && colony != null) {
					return PurgeColony(causedBy, colony);
				} else {
					return PurgePlayerFromColonies(causedBy, splits[1]);
				}
			}

			if (colony.Banners.Length > 1) {
				ServerManager.ClientCommands.DeleteBannerTo(causedBy, colony, banner.Position);
				Chat.Send(causedBy, $"Deleted banner at {banner.Position.x},{banner.Position.z}. Colony still has more banners");
			} else {
				ServerManager.ClientCommands.DeleteColonyAndBanner(causedBy, colony, banner.Position);
				Chat.Send(causedBy, $"Deleted banner at {banner.Position.x},{banner.Position.z} and also the colony.");
			}
			return true;
		}

		// purge a full colony at once
		public bool PurgeColony(Players.Player causedBy, Colony colony)
		{
			while (colony.Banners.Length > 1) {
				ServerManager.ClientCommands.DeleteBannerTo(causedBy, colony, colony.Banners[0].Position);
			}
			ServerManager.ClientCommands.DeleteColonyAndBanner(causedBy, colony, colony.Banners[0].Position);
			Chat.Send(causedBy, "Deleted the full colony");
			return true;
		}

		// purge all colonies of a given player (or remove him/her in case of multiple owners)
		public bool PurgePlayerFromColonies(Players.Player causedBy, string targetPlayerName)
		{
			return true;
		}

	}
}

