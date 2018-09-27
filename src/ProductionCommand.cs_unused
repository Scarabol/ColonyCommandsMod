using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

	public class ProductionCommand : IChatCommand
	{

		public bool IsCommand(string chat)
		{
			return (chat.Equals("/production") || chat.StartsWith("/production "));
		}

		public bool SyntaxError(Players.Player causedBy)
		{
			Chat.Send(causedBy, "Syntax: /production {add|list|remove} {item} [interval]");
			return true;
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext)
		{

			var m = Regex.Match(chattext, @"/production (?<action>[^ ]+) ?(?<item>[^ ]+)? ?(?<interval>[0-9]+)?$");
			if (!m.Success) {
				return SyntaxError(causedBy);
			}
			string action = m.Groups["action"].Value;
			if ( !(action.Equals("add") || action.Equals("list") || action.Equals("remove")) ) {
				return SyntaxError(causedBy);
			}

			if (action.Equals("list")) {
				string itemList;
				if (StatisticManager.GetItemNameList(causedBy, out itemList)) {
					Chat.Send(causedBy, $"Tracked Production: {itemList}");
				} else {
					Chat.Send(causedBy, "No production is tracked");
				}
				return true;
			}

			string itemName = m.Groups["item"].Value;
			ushort itemType;
			if (!ItemTypes.IndexLookup.TryGetIndex(itemName, out itemType)) {
				Chat.Send(causedBy, $"Could not find item named {itemName}");
				return true;
			}

			ushort interval = 1;
			string val = m.Groups["interval"].Value;
			if (!val.Equals("")) {
				if (!ushort.TryParse(val, out interval)) {
					Chat.Send(causedBy, $"Could not identify interval {val}");
					return true;
				}
			}

			if (action.Equals("add")) {
				if (!StatisticManager.AddTrackingItem(causedBy, itemType, interval)) {
					Chat.Send(causedBy, $"Item {itemName} already tracked");
					return true;
				} else {
					Chat.Send(causedBy, $"Added production tracking for {itemName}");
				}

			} else if (action.Equals("remove")) {
				if (!StatisticManager.RemoveTrackingItem(causedBy, itemType)) {
					Chat.Send(causedBy, $"Item {itemName} was not tracked");
				} else {
					Chat.Send(causedBy, $"Removed {itemName} from tracking");
				}
			}

			return true;
		}

	}

}

