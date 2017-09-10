using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;
using System.Linq;

/*
 * Inspired by Crone's BetterChat
 */
namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class BetterChatCommand : ChatCommands.IChatCommand
  {
    private static ChatColorSpecification[] settings = new ChatColorSpecification[] {
      new ChatColorSpecification ("name", "red", "redname"),
      new ChatColorSpecification ("name", "green", "greenname"),
      new ChatColorSpecification ("name", "blue", "bluename"),
      new ChatColorSpecification ("name", "#a335ee", "epicname"),
      new ChatColorSpecification ("name", "#00ccff", "heirloomname"),
      new ChatColorSpecification ("text", "red", "redtext"),
      new ChatColorSpecification ("text", "green", "greentext"),
      new ChatColorSpecification ("text", "blue", "bluetext"),
      new ChatColorSpecification ("text", "#a335ee", "epictext"),
      new ChatColorSpecification ("text", "#00ccff", "heirloomtext")
    };

    private bool selfLookup = false;

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
        string nameColor = (from s in settings
                                where s.ColorArea == "name" && Permissions.PermissionsManager.HasPermission (causedBy, s.Permission)
                                select s.Color).FirstOrDefault ();
        string textColor = (from s in settings
                                where s.ColorArea == "text" && Permissions.PermissionsManager.HasPermission (causedBy, s.Permission)
                                select s.Color).FirstOrDefault ();
        Chat.SendToAll ($"[<color={nameColor}>{causedBy.Name}</color>]: <color={textColor}>{chat}</color>");
      }
      return true;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "mods.scarabol.commands.betterchat.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new BetterChatCommand ());
    }
  }

  internal class ChatColorSpecification
  {
    public string ColorArea { get; set; }

    public string Color { get; set; }

    public string Permission { get; set; }

    public ChatColorSpecification (string area, string color, string permission)
    {
      this.ColorArea = area;
      this.Color = color;
      this.Permission = CommandsModEntries.MOD_PREFIX + "betterchat." + permission;
    }
  }
}
