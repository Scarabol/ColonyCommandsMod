using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Pipliz;
using Pipliz.JSON;
using Chatting;
using Chatting.Commands;

/*
 * Inspired by Crone's BetterChat
 */
namespace ColonyCommands
{
	[ModLoader.ModManager]
	public class BetterChatCommand : IChatCommand
	{
		static List<ChatColorSpecification> Colors = new List<ChatColorSpecification> ();

		static string ConfigFilepath {
			get {
				return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "chatcolors.json"));
			}
		}

		public bool TryDoCommand(Players.Player causedBy, string chat, List<string> splits)
		{
			MuteList.Update();
			if (MuteList.MutedMinutes.ContainsKey(causedBy)) {
				Chat.Send(causedBy, "[muted]");
				return true;
			}

			if (chat.StartsWith("/")) {
				return false;
			}
			if (PermissionsManager.HasPermission (causedBy, "")) {
				String name = causedBy != null ? causedBy.Name : "Server";
				Chat.SendToConnected ($"[<color=red>{name}</color>]: {chat}");
			} else {
				string nameColor = (from s in Colors
						where PermissionsManager.HasPermission (causedBy, AntiGrief.MOD_PREFIX + "betterchat.name." + s.Name)
						select s.Color).FirstOrDefault ();
				string textColor = (from s in Colors
						where PermissionsManager.HasPermission (causedBy, AntiGrief.MOD_PREFIX + "betterchat.text." + s.Name)
						select s.Color).FirstOrDefault ();
				Chat.SendToConnected ($"[<color={nameColor}>{causedBy.Name}</color>]: <color={textColor}>{chat}</color>");
			}
			return true;
		}

		[ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.betterchat.loadcolors")]
		public static void AfterWorldLoad ()
		{
			Load ();
		}

		public static void Load ()
		{
			try {
				JSONNode json;
				if (JSON.Deserialize (ConfigFilepath, out json, false)) {
					JSONNode jsonColors;
					if (json.TryGetAs ("colors", out jsonColors) && jsonColors.NodeType == NodeType.Array) {
						foreach (var jsonColorNode in jsonColors.LoopArray ()) {
							string colorName;
							if (jsonColorNode.TryGetAs ("name", out colorName)) {
								string hexcode;
								if (!jsonColorNode.TryGetAs ("hexcode", out hexcode)) {
									hexcode = colorName;
								}
								Colors.Add (new ChatColorSpecification (colorName, hexcode));
							} else {
								Log.WriteError ("Color entry has no name");
							}
						}
					} else {
						Log.WriteError ($"No 'colors' array found in {ConfigFilepath}");
					}
				}
			} catch (Exception exception) {
				Log.WriteError ($"Exception while loading chatcolors; {exception.Message}");
			}
		}
	}

	class ChatColorSpecification
	{
		public string Name { get; set; }

		public string Color { get; set; }

		public ChatColorSpecification (string name, string color)
		{
			Name = name;
			Color = color;
		}
	}
}
