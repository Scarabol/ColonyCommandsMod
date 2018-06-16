using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using ChatCommands;
using Permissions;
using Server.TerrainGeneration;

namespace ScarabolMods
{

  public class CustomProtectionArea
  {
    readonly int StartX;
    readonly int EndX;
    readonly int StartZ;
    readonly int EndZ;

    public CustomProtectionArea (int startX, int endX, int startZ, int endZ)
    {
      StartX = startX;
      EndX = endX;
      StartZ = startZ;
      EndZ = endZ;
    }

    public CustomProtectionArea (Vector3Int center, int rangeX, int rangeZ)
      : this (center, rangeX, rangeX, rangeZ, rangeZ)
    {
    }

    public CustomProtectionArea (Vector3Int center, int rangeXN, int rangeXP, int rangeZN, int rangeZP)
      : this (center.x - rangeXN, center.x + rangeXP, center.z - rangeZN, center.z + rangeZP)
    {
    }

    public CustomProtectionArea (JSONNode jsonNode)
      : this (jsonNode.GetAs<int> ("startX"), jsonNode.GetAs<int> ("endX"), jsonNode.GetAs<int> ("startZ"), jsonNode.GetAs<int> ("endZ"))
    {
    }

    public JSONNode ToJSON ()
    {
      return new JSONNode ()
        .SetAs ("startX", StartX)
        .SetAs ("endX", EndX)
        .SetAs ("startZ", StartZ)
        .SetAs ("endZ", EndZ);
    }

    public bool Contains (Vector3Int point)
    {
      return StartX <= point.x && EndX >= point.x && StartZ <= point.z && EndZ >= point.z;
    }
  }

}

