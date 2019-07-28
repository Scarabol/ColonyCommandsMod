using System.Text.RegularExpressions;
using System.Collections.Generic;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

	public class ColorTestCommand : IChatCommand
	{
		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/colortest")) {
				return false;
			}

			var m = Regex.Match(chattext, @"/colortest (?<color>[^ ]+)");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /colortest {colorvalue}");
				return true;
			}
			string color = m.Groups["color"].Value;
			Chat.Send(causedBy, $"<color={color}>This is text in color {color}</color>");

			return true;
		}

	}

}

