using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

	public class InactiveChatCommand : IChatCommand
	{

		public bool IsCommand(string chat)
		{
			return chat.Equals("/inactive") || chat.StartsWith("/inactive ");
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext)
		{
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "inactive")) {
				return true;
			}
			var m = Regex.Match(chattext, @"/inactive (?<days>\d+) ?(?<max>\d+)?");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /inactive {days} {max}");
				return true;
			}
			int days;
			if (!int.TryParse(m.Groups["days"].Value, out days) || days < 1) {
				Chat.Send(causedBy, $"Min days should be larger than 1");
				return true;
			}
			int max_days = int.MaxValue;
			int.TryParse(m.Groups["max_days"].Value, out max_days);
			if (days > max_days) {
				Chat.Send(causedBy, $"Max should be more than min days.");
				return true;
			}

			String resultMsg = "";
			foreach (KeyValuePair<Players.Player, int> entry in ActivityTracker.GetInactivePlayers(days, max_days)) {
				var player = entry.Key;
				var inactiveDays = entry.Value;
				if (BannerTracker.Get(player) != null) {
					if (resultMsg.Length > 0) {
						resultMsg += ", ";
					}
					resultMsg += $"{player.ID.ToStringReadable()} ({inactiveDays})";
				}
			}
			if (resultMsg.Length < 1) {
				resultMsg = "No inactive players found";
			}
			Chat.Send (causedBy, resultMsg);
			return true;
		}
	}
}

