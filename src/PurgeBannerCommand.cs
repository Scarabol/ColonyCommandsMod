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

			// command: /purgebanner all <range> (Purge ALL colonies within range
			if (splits.Count == 3 && splits[1].Equals("all")) {
				if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "purgebanner")) {
					return true;
				}
				int range = 0;
				if (!int.TryParse(splits[2], out range)) {
					Chat.Send(causedBy, "Syntax: /purgebanner all <range>");
					return true;
				}
				int counter = PurgeAllColonies(causedBy, range);
				Chat.Send(causedBy, $"Deleted {counter} colonies within range");
				return true;
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
		public bool PurgePlayerFromColonies(Players.Player causedBy, string targetName)
		{
			Players.Player target;
			string error;
			if (!PlayerHelper.TryGetPlayer(targetName, out target, out error)) {
				Chat.Send(causedBy, $"Could not find target: {error}");
				return true;
			}
			foreach (Colony colony in target.Colonies) {
				if (colony.Owners.Length == 1) {
					ServerManager.ClientCommands.DeleteColonyAndBanner(causedBy, colony, colony.Banners[0].Position);
				} else {
					colony.RemoveOwner(target);
				}
			}

			Chat.Send(causedBy, $"Deleted all colonies of {target.Name} and revoked ownership from shared colonies");
			return true;
		}

		// purge all colonies within a given range
		public int PurgeAllColonies(Players.Player causedBy, int range)
		{
			List<Colony> colonies = new List<Colony>();
			foreach (Colony checkColony in ServerManager.ColonyTracker.ColoniesByID.Values) {
				foreach (BannerTracker.Banner checkBanner in checkColony.Banners) {
					int distX = (int)(causedBy.Position.x - checkBanner.Position.x);
					int distZ = (int)(causedBy.Position.z - checkBanner.Position.z);
					int distance = (int)System.Math.Sqrt(System.Math.Pow(distX, 2) + System.Math.Pow(distZ, 2));
					if (distance < range) {
						colonies.Add(checkColony);
					}
				}
			}

			// second delete loop since delete within a foreach in unsafe
			int counter = 0;
			foreach (Colony colony in colonies) {
				while (colony.Banners.Length > 1) {
					ServerManager.ClientCommands.DeleteBannerTo(causedBy, colony, colony.Banners[0].Position);
				}
				ServerManager.ClientCommands.DeleteColonyAndBanner(causedBy, colony, colony.Banners[0].Position);
			}

			return counter;
		}

	}
}

