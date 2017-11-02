using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using System.Linq;

/*
 * Inspired by Crone's BetterChat
 */
namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class BetterChatCommand : ChatCommands.IChatCommand
  {
    private static List<ChatColorSpecification> colors = new List<ChatColorSpecification> ();
    private bool selfLookup = false;

    private static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "chatcolors.json"));
      }
    }

    public bool IsCommand (string chat)
    {
      if (selfLookup) {
        return false;
      } else {
        selfLookup = true;
        bool result = ChatCommands.CommandManager.GetCommand (chat) == null;
        selfLookup = false;
        return result;
      }
    }

    public bool TryDoCommand (Players.Player causedBy, string chat)
    {
      if (Permissions.PermissionsManager.HasPermission (causedBy, "")) {
        String name = causedBy != null ? causedBy.Name : "Server";
        Chat.SendToAll ($"[<color=red>{name}</color>]: {chat}");
      } else {
        string nameColor = (from s in colors
                                where Permissions.PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "betterchat.name." + s.Name)
                                select s.Color).FirstOrDefault ();
        string textColor = (from s in colors
                                where Permissions.PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "betterchat.text." + s.Name)
                                select s.Color).FirstOrDefault ();
        Chat.SendToAll ($"[<color={nameColor}>{causedBy.Name}</color>]: <color={textColor}>{chat}</color>");
      }
      return true;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "mods.scarabol.commands.betterchat.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new BetterChatCommand ());
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
            foreach (JSONNode jsonColorNode in jsonColors.LoopArray()) {
              string colorName;
              if (jsonColorNode.TryGetAs ("name", out colorName)) {
                string hexcode;
                if (!jsonColorNode.TryGetAs ("hexcode", out hexcode)) {
                  hexcode = colorName;
                }
                colors.Add (new ChatColorSpecification (colorName, hexcode));
              } else {
                Pipliz.Log.WriteError ("Color entry has no name");
              }
            }
          } else {
            Pipliz.Log.WriteError ($"No 'colors' array found in {ConfigFilepath}");
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while loading chatcolors; {0}", exception.Message));
      }
    }
  }

  internal class ChatColorSpecification
  {
    public string Name { get; set; }

    public string Color { get; set; }

    public ChatColorSpecification (string name, string color)
    {
      this.Name = name;
      this.Color = color;
    }
  }
}
