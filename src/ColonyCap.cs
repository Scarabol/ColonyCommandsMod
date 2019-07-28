using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Pipliz;
using Pipliz.JSON;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{
	public class ColonyCap : IChatCommand
	{

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/colonycap")) {
				return false;
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "colonycap")) {
				return true;
			}

			var m = Regex.Match(chattext, @"/colonycap (?<colonistslimit>-?\d+)( (?<checkintervalseconds>\d+))?");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /colonycap {colonistslimit} [checkintervalseconds]");
				return true;
			}

			int limit;
			if (!int.TryParse(m.Groups["colonistslimit"].Value, out limit)) {
				Chat.Send (causedBy, "Could not parse limit");
				return true;
			}

			AntiGrief.ColonistLimit = limit;
			if (AntiGrief.ColonistLimit > 0) {
				Chat.SendToConnected($"Colony population limit set to {AntiGrief.ColonistLimit}");
			} else {
				Chat.SendToConnected("Colony population limit disabled");
			}

			string strInterval = m.Groups["checkintervalseconds"].Value;
			if (strInterval.Length > 0) {
				int interval;
				if (!int.TryParse(strInterval, out interval)) {
					Chat.Send(causedBy, "Could not parse interval");
					return true;
				}
				AntiGrief.ColonistLimitCheckSeconds = System.Math.Max(1, interval);
				Chat.Send(causedBy, $"Check interval seconds set to {AntiGrief.ColonistLimitCheckSeconds}");
			}
			return true;
		}

		public static void CheckColonistNumbers()
		{
			new Thread(() => {
				Thread.CurrentThread.IsBackground = true;
				while (true) {
					if (AntiGrief.ColonistLimit > 0) {
						foreach (Colony colony in ServerManager.ColonyTracker.ColoniesByID.Values) {
							bool killed = false;
							while (colony.FollowerCount > AntiGrief.ColonistLimit) {
								killed = true;
								if (colony.LaborerCount > 0) {
									colony.FindLaborer().OnDeath();
								} else {
									colony.Followers[colony.Followers.Count - 1].OnDeath();
								}
							}
							if (killed) {
								Chat.Send(colony.Owners, $"<color=red>Colonists are dying, limit per colony is {AntiGrief.ColonistLimit}");
							}
						}
					}
					Thread.Sleep(AntiGrief.ColonistLimitCheckSeconds * 1000);
				}
			}).Start();
		}

	}
}

