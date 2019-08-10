using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{
	public class AreaShowCommand : IChatCommand
	{
		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/areashow")) {
				return false;
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "areashow")) {
				return true;
			}

			// if parameter action given toggle all areas shown
			Match m = Regex.Match(chattext, @"/areashow ?(?<action>.+)?");
			string action = m.Groups["action"].Value;
			if (action.Equals("add")) {
				if (AreaShowManager.Add(causedBy)) {
					Chat.Send(causedBy, "You now see area jobs of all players");
				}
				return true;
			} else if (action.Equals("remove")) {
				if (AreaShowManager.Remove(causedBy)) {
					Chat.Send(causedBy, "You no longer see area jobs of all players");
				}
				return true;
			}

			// without action parameter just trigger a data send
			AreaJobTracker.SendData(causedBy);
			return true;
		}
	}
}

