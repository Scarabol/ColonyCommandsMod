using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz.Chatting;
using Pipliz.Collections.Threadsafe;
using ChatCommands;
using Permissions;
using BlockTypes.Builtin;
using NPC;

namespace ColonyCommands
{

	public class PurgeAllChatCommand : IChatCommand
	{
		public static int MIN_DAYS_TO_PURGE = 7;

		public bool IsCommand(string chat)
		{
			return chat.Equals("/purgeall") || chat.StartsWith("/purgeall ");
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext)
		{
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "purgeall")) {
				return true;
			}

			Match m = Regex.Match(chattext, @"/purgeall (?<days>\d+) ?(?<max>\d+)?");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /purgeall {days} [max]");
				return true;
			}
			int days;
			if (!int.TryParse(m.Groups["days"].Value, out days) || days < MIN_DAYS_TO_PURGE) {
				Chat.Send(causedBy, $"Min days is {MIN_DAYS_TO_PURGE}");
				return true;
			}
			int max_days = int.MaxValue;
			int.TryParse(m.Groups["max_days"].Value, out max_days);
			if (days > max_days) {
				Chat.Send(causedBy, $"Max should be more than min days.");
				return true;
			}

			var resultMsg = "";
			foreach (KeyValuePair<Players.Player, int> entry in ActivityTracker.GetInactivePlayers(days, max_days)) {
				Players.Player player = entry.Key;
				var inactiveDays = entry.Value;
				var banner = BannerTracker.Get(player);
				if (banner != null) {
					var cachedFollowers = new List<NPCBase>(Colony.Get(player).Followers);
					foreach (var npc in cachedFollowers) {
						npc.OnDeath();
					}
					ServerManager.TryChangeBlock(banner.KeyLocation, BuiltinBlocks.Air);
					BannerTracker.Remove(banner.KeyLocation, BuiltinBlocks.Banner, banner.Owner);
					DeleteJobsManager.DeleteAreaJobs(causedBy, player);
				}

				if (resultMsg.Length > 0) {
					resultMsg += ", ";
				}
				resultMsg += $"{player.IDString}({inactiveDays})";
			}
			if (resultMsg.Length < 1) {
				resultMsg = "No inactive players found";
			} else {
				resultMsg = "Purged: " + resultMsg;
			}
			Chat.Send(causedBy, resultMsg);
			return true;
		}

	} // class

} // namespace

