using System.Collections.Generic;
using Pipliz;
using Pipliz.Threading;
using Pipliz.Chatting;

namespace ColonyCommands {

	public static class StatisticManager
	{
		private static uint iteration = 0;
		private static double WAIT_DELAY = 10.0;
		private static bool didReport = true;

		private static Dictionary<Players.Player, List<TrackedItem>> trackedItems =
			new Dictionary<Players.Player, List<TrackedItem>>();

		// Add item to track
		public static bool AddTrackingItem(Players.Player owner, ushort type, ushort interval = 1)
		{
			List<TrackedItem> itemList;
			if (trackedItems.ContainsKey(owner)) {
				itemList = trackedItems[owner];
			} else {
				itemList = new List<TrackedItem>();
				trackedItems[owner] = itemList;
			}

			// avoid duplicate tracking
			foreach (TrackedItem item in itemList) {
				if (type == item.item.ItemIndex) {
					return false;
				}
			}
			// avoid non existing items
			ItemTypes.ItemType itemType;
			if (!ItemTypes.TryGetType(type, out itemType)) {
				return false;
			}

			TrackedItem theItem = new TrackedItem(itemType, interval);
			itemList.Add(theItem);
			return true;
		}

		// Remove item from tracking
		public static bool RemoveTrackingItem(Players.Player owner, ushort type)
		{
			if (!trackedItems.ContainsKey(owner)) {
				return false;
			}
			List<TrackedItem> itemList = trackedItems[owner];
			TrackedItem foundItem = null;
			foreach (TrackedItem item in itemList) {
				if (type == item.item.ItemIndex) {
					foundItem = item;
				}
			}

			if (foundItem != null) {
				itemList.Remove(foundItem);
				return true;
			}
			return false;
		}

		// Tracking background thread
		public static void TrackItems()
		{
			if (TimeCycle.IsDay && !didReport) {
				// update all tracked items with the new day value
				foreach (KeyValuePair<Players.Player, List<TrackedItem>> kvp in trackedItems) {
					Players.Player owner = kvp.Key;
					List<TrackedItem> itemList = kvp.Value;
					Stockpile playerStockpile = Stockpile.GetStockPile(owner);

					foreach (TrackedItem item in itemList) {

						if (iteration % item.interval != 0) {
							continue;
						}

						item.Update(playerStockpile.AmountContained(item.item.ItemIndex));
						string delta = item.GetDeltaFormatted();
						string name = char.ToUpper(item.Name[0]) + item.Name.Substring(1);
						if (owner.IsConnected) {
							if (item.DidIncrease) {
								Chat.Send(owner, $"<color=lightgreen>{name} per {item.interval}/day: {delta}</color>");
							}
							if (item.DidDecrease) {
								Chat.Send(owner, $"<color=red>{name} per {item.interval}/day: {delta}</color>");
							}
						}
					}
				}

				++iteration;
				didReport = true;
			}
			if (!TimeCycle.IsDay) {
				didReport = false;
			}

			ThreadManager.InvokeOnMainThread(delegate() {
				StatisticManager.TrackItems();
			}, WAIT_DELAY);
		}
	}
}

