using System.Collections.Generic;
using Pipliz;
using Chatting;
using Chatting.Commands;
using BlockEntities.Implementations;

namespace ColonyCommands
{
	public class BannerNameChatCommand : IChatCommand
	{
		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/bannername")) {
				return false;
			}
			BannerTracker.Banner closestBanner = null;
			int shortestDistance = int.MaxValue;
			foreach (Colony checkColony in ServerManager.ColonyTracker.ColoniesByID.Values) {
				bool isOwner = false;
				foreach (Players.Player owner in checkColony.Owners) {
					if (owner == causedBy) {
						isOwner = true;
						break;
					}
				}
				if (isOwner) {
					continue;
				}

				foreach (BannerTracker.Banner checkBanner in checkColony.Banners) {
					int distX = (int)(causedBy.Position.x - checkBanner.Position.x);
					int distZ = (int)(causedBy.Position.z - checkBanner.Position.z);
					int distance = (int)System.Math.Sqrt(System.Math.Pow(distX, 2) + System.Math.Pow(distZ, 2));
					if (distance < shortestDistance) {
						shortestDistance = distance;
						closestBanner = checkBanner;
					}
				}
			}

			if (closestBanner != null) {
				string owners = "";
				foreach (Players.Player owner in closestBanner.Colony.Owners) {
					if (!owners.Equals("")) {
						owners += ", ";
					}
					owners += owner.Name;
				}
				Chat.Send(causedBy, $"Closest banner is at {closestBanner.Position.x},{closestBanner.Position.z}. {shortestDistance} blocks away. It belongs to colony {closestBanner.Colony.Name} owned by {owners}");
			}
			return true;
		}
	}
}

