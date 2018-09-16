using System;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;
using TerrainGeneration;
using UnityEngine;

namespace ColonyCommands
{

  public class JailLeaveCommand : IChatCommand
  {

    public bool IsCommand(string chat)
    {
      return chat.Equals("/jailleave");
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {

      Vector3 oldPos;
      if (JailManager.visitorPreviousPos.TryGetValue(causedBy, out oldPos)) {
        Teleport.TeleportTo(causedBy, oldPos);
      } else {
        Chat.Send(causedBy, "Found no old position record, looks like you have to walk");
      }

      return true;
    }
  }
}

