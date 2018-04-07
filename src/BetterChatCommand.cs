using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using ChatCommands;
using Permissions;

/*
 * Inspired by Crone's BetterChat
 */
namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class BetterChatCommand : IChatCommand
  {
    static List<ChatColorSpecification> Colors = new List<ChatColorSpecification> ();
    bool SelfLookup;

    static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "chatcolors.json"));
      }
    }

    public bool IsCommand (string chat)
    {
      if (SelfLookup) {
        return false;
      }
      SelfLookup = true;
      bool result = CommandManager.GetCommand (chat) == null;
      SelfLookup = false;
      return result;
    }

    public bool TryDoCommand (Players.Player causedBy, string chat)
    {
      if (PermissionsManager.HasPermission (causedBy, "")) {
        String name = causedBy != null ? causedBy.Name : "Server";
        Chat.SendToAll ($"[<color=red>{name}</color>]: {chat}");
      } else {
        string nameColor = (from s in Colors
                            where PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "betterchat.name." + s.Name)
                            select s.Color).FirstOrDefault ();
        string textColor = (from s in Colors
                            where PermissionsManager.HasPermission (causedBy, CommandsModEntries.MOD_PREFIX + "betterchat.text." + s.Name)
                            select s.Color).FirstOrDefault ();
        Chat.SendToAll ($"[<color={nameColor}>{causedBy.Name}</color>]: <color={textColor}>{chat}</color>");
      }
      return true;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "mods.scarabol.commands.betterchat.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new BetterChatCommand ());
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
