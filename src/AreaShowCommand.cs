using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;

namespace ColonyCommands
{
	public class AreaShowCommand : IChatCommand
	{
		public bool IsCommand(string chat)
		{
			return chat.Equals("/areashow") || chat.StartsWith("/areashow ");
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext)
		{
			// if parameter action given toggle all areas shown
			Match m = Regex.Match(chattext, @"/areashow ?(?<action>.+)?");
			string action = m.Groups["action"].Value;
			if ((action.Equals("add") || action.Equals("remove")) &&
				!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "areashow")) {
				return true;
			}
			if (action.Equals("add")) {
				if (AreaShowManager.Add(causedBy)) {
					Chat.Send(causedBy, "You now see area job highlights of all players");
				}
				return true;
			} else if (action.Equals("remove")) {
				if (AreaShowManager.Remove(causedBy)) {
					Chat.Send(causedBy, "You no longer see area job highlights of all players");
				}
				return true;
			}

			// without action parameter just trigger a data send
			AreaJobTracker.SendData(causedBy);
			return true;
		}
	}
}
