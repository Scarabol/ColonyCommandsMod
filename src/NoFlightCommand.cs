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
          if (values.Remove ("pipliz.setflight")) {
            player.SetTempValues (values);
            if (!player.ID.IsInvalid) {
              Chat.Send (player, "Please don't fly");
              Players.Disconnect (player);
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
