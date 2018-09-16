using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.JSON;
using Chatting;
using Chatting.Commands;
using TerrainGeneration;

namespace ColonyCommands
{

  public class CustomProtectionArea
  {
    public int StartX { get; }
    public int EndX { get; }
    public int StartZ { get; }
    public int EndZ { get; }

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

    public bool Equals(Vector3Int center, uint range)
    {
      // allow +/-2 blocks difference for converting from float to int
      if (System.Math.Abs(StartX - center.x) <= range + 2 &&
        System.Math.Abs(EndX - center.x)  <= range + 2 &&
        System.Math.Abs(StartZ - center.z) <= range + 2 &&
        System.Math.Abs(EndZ - center.z) <= range + 2) {
        return true;
      }
      return false;
    }

    public int DistanceToCenter(Vector3Int position)
    {
      int centerX = (int)(EndX + StartX) / 2;
      int centerZ = (int)(EndZ + StartZ) / 2;
      return (int)System.Math.Sqrt(System.Math.Pow(centerX - position.x, 2) + System.Math.Pow(centerZ - position.z, 2));
    }
  }

}

