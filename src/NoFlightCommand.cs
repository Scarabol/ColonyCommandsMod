using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using BlockTypes.Builtin;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class NoFlightChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.noflight.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new NoFlightChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/noflight");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "noflight")) {
          return true;
        }
        foreach (Players.Player player in Players.PlayerDatabase.ValuesAsList) {
          var values = player.GetTempValues ();
          if (!Permissions.PermissionsManager.HasPermission (player, "setflight") && values.Remove ("pipliz.setflight")) {
            player.SetTempValues (values);
            player.SavegameNode.RemoveChild ("pipliz.setflight");
            player.ShouldSave = true;
            if (player.IsConnected) {
              Chat.Send (player, "Please don't fly");
              Players.Disconnect (player);
            } else {
              Log.Write ($"Removed flight state from offline player {player.IDString}");
            }
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }

    public static void SendFlightState (Players.Player player, bool state)
    {
      using (ByteBuilder byteBuilder = ByteBuilder.Get ()) {
        byteBuilder.Write (30);
        byteBuilder.Write (state);
        NetworkWrapper.Send (byteBuilder.ToArray (), player, NetworkMessageReliability.ReliableWithBuffering);
      }
    }
  }
}
