using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

	public class ColorTestCommand : IChatCommand
	{

		public bool IsCommand(string chat)
		{
			return (chat.Equals("/colortest") || chat.StartsWith("/colortest "));
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext)
		{

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

