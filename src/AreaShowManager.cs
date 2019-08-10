using System.Collections.Generic;
using System.Reflection;
using Pipliz;
using Jobs;
using BlockEntities.Implementations;

namespace ColonyCommands
{

	[ModLoader.ModManager]
	public static class AreaShowManager
	{
		// list of players that want to see all area highlights
		private static List<Players.Player> playerList = new List<Players.Player>();

		private static int MaxDistance = ServerManager.HostingSettings.MaxDrawDistance;

		private static Dictionary<Colony, List<IAreaJob>> allAreaJobs = typeof(AreaJobTracker).GetField("colonyTrackedJobs", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<Colony, List<IAreaJob>>;

		// check if player is on list
		public static bool IsShowAllActive(Players.Player player)
		{
			if (playerList.Contains(player)) {
				return true;
			}
			return false;
		}

		// add player to list
		public static bool Add(Players.Player player)
		{
			if (playerList.Contains(player)) {
				return false;
			}
			playerList.Add(player);
			AreaJobTracker.SendData(player);
			return true;
		}

		// remove player from list
		public static bool Remove(Players.Player player)
		{
			if (playerList.Contains(player)) {
				playerList.Remove(player);
				AreaJobTracker.SendData(player);
				return true;
			}
			return false;
		}

		[ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, AntiGrief.NAMESPACE + ".OnSendAreaHighlights")]
		public static void OnSendAreaHighlights(Players.Player causedBy, List<AreaJobTracker.AreaHighlight> jobs, List<ushort> activeTypes)
		{
			if (!IsShowAllActive(causedBy)) {
				return;
			}

			Pipliz.Vector3Int playerPos = causedBy.VoxelPosition;
			foreach (KeyValuePair<Colony, List<IAreaJob>> kvp in allAreaJobs) {
				if (kvp.Key == causedBy.ActiveColony) {
					continue;
				}
				BannerTracker.Banner closestBanner = kvp.Key.GetClosestBanner(playerPos);
				if (Math.ManhattanDistance(closestBanner.Position, playerPos) > MaxDistance) {
					continue;
				}
				foreach (IAreaJob job in kvp.Value) {
					jobs.Add(new AreaJobTracker.AreaHighlight(job.Minimum, job.Maximum, job.AreaTypeMesh, job.AreaType));
				}
			}
			return;
		}

	} // class

} // namespace

