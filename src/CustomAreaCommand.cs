using System.Collections.Generic;
using Chatting;
using Chatting.Commands;
using Pipliz;

namespace ColonyCommands
{
	public class CustomAreaCommand : IChatCommand
	{
		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/customarea")) {
				return false;
			}
			Pipliz.Vector3Int playerPos = new Vector3Int(causedBy.Position);
			CustomProtectionArea closestArea = null;
			int shortestDistance = int.MaxValue;
			foreach (CustomProtectionArea area in AntiGrief.CustomAreas) {
				if (area.Contains(playerPos)) {
					Chat.Send(causedBy, $"You are inside a custom area: from {area.StartX}, {area.StartZ} to {area.EndX}, {area.EndZ}");
					return true;
				}
				int distance = area.DistanceToCenter(playerPos);
				if (distance < shortestDistance) {
					shortestDistance = distance;
					closestArea = area;
				}
			}

			if (closestArea != null) {
				Chat.Send(causedBy, $"The closest area is at: {closestArea.StartX}, {closestArea.StartZ} to {closestArea.EndX}, {closestArea.EndZ}. {shortestDistance} blocks away");
			} else {
				Chat.Send(causedBy, "No areas found - try /bannername?");
			}

			return true;
		}
	}
}

