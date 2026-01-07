using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitManager;

//wazne zalozenie: modele obszarow interakcji, czyli zbiory hexow, sa dla samolotu na 0, 0 z kierunkiem NW

public static class UnitStatsData
{
    public struct UnitStats
    {
        public int MaxMoves;
        public int MinMoves;
        public Dictionary<InteractionAreaType, List<HexCoords>> RelativeInteractionAreas;

        public UnitStats(int maxMoves, int minMoves, Dictionary<InteractionAreaType, List<HexCoords>> relativeInteractionAreas = null)
        {
            MaxMoves = maxMoves;
            MinMoves = minMoves;
            RelativeInteractionAreas = relativeInteractionAreas;
        }

        
    }

    private static readonly Dictionary<UnitType, UnitStats> stats = new Dictionary<UnitType, UnitStats>
    {
        //oczywiœcie mo¿na 4, 3 zamiast maxMoves: 4, ale tak czytleniej
        { UnitType.UsFighter, new UnitStats(maxMoves: 4, minMoves: 3, new Dictionary<InteractionAreaType, List<HexCoords>> 
            { { InteractionAreaType.AttackRange, HelperVectorSetAttack5() },
              { InteractionAreaType.FireThree, HelperVectorSet_US_F_OFFENSIVE_FIRE_ZONE_3_CARDS() },
              { InteractionAreaType.FireTwo, HelperVectorSet_US_F_OFFENSIVE_FIRE_ZONE_2_CARDS() },
              { InteractionAreaType.DodgeTwo, HelperVectorSet_US_F_DEFENSIVE_ZONE_2_CARDS() },
              { InteractionAreaType.DodgeOne, HelperVectorSet_US_F_DEFENSIVE_ZONE_1_CARDS() }
            }) },
        { UnitType.JpFighter, new UnitStats(maxMoves: 4, minMoves: 3, new Dictionary<InteractionAreaType, List<HexCoords>>
            { { InteractionAreaType.AttackRange, HelperVectorSetAttack5() },
              { InteractionAreaType.FireThree, HelperVectorSet_JP_F_OFFENSIVE_FIRE_ZONE_3_CARDS() },
              { InteractionAreaType.FireTwo, HelperVectorSet_JP_F_OFFENSIVE_FIRE_ZONE_2_CARDS() },
              { InteractionAreaType.DodgeTwo, HelperVectorSet_JP_F_DEFENSIVE_ZONE_2_CARDS() },
              { InteractionAreaType.DodgeOne, HelperVectorSet_JP_F_DEFENSIVE_ZONE_1_CARDS() }
            }) },        
        { UnitType.UsBomber, new UnitStats(maxMoves: 3, minMoves: 2) },
        { UnitType.JpBomber, new UnitStats(maxMoves: 3, minMoves: 2) }
    };

    public static UnitStats GetStats(UnitType unitType)
    {
        return stats[unitType];
    }

    

    private static List<HexCoords> HelperVectorSetAttack5()
    {
        return new List<HexCoords> { new HexCoords(-4, -1), new HexCoords(-4, 0), new HexCoords(-4, 1), new HexCoords(-4, 2), new HexCoords(-3, -2), new HexCoords(-3, -1),
            new HexCoords(-3, 0), new HexCoords(-3, 1), new HexCoords(-3, 2), new HexCoords(-2, -3), new HexCoords(-2, -2), new HexCoords(-2, -1), new HexCoords(-2, 0),
            new HexCoords(-2, 1), new HexCoords(-2, 2), new HexCoords(-1, -4), new HexCoords(-1, -3), new HexCoords(-1, -2), new HexCoords(-1, -1), new HexCoords(-1, 1),
            new HexCoords(0, -5), new HexCoords(0, -4), new HexCoords(0, -3), new HexCoords(0, -2), new HexCoords(0, -1), new HexCoords(1, -5), new HexCoords(1, -4),
            new HexCoords(1, -3), new HexCoords(1, -2), new HexCoords(1, 0), new HexCoords(2, -5), new HexCoords(2, -4), new HexCoords(2, -3), new HexCoords(2, -2),
            new HexCoords(2, -1), new HexCoords(2, 0), new HexCoords(3, -5), new HexCoords(3, -4), new HexCoords(3, -3), new HexCoords(3, -2), new HexCoords(3, -1),
            new HexCoords(4, -5), new HexCoords(4, -4), new HexCoords(4, -3), new HexCoords(4, -2)  };
    }

    private static List<HexCoords> HelperVectorSet_US_F_OFFENSIVE_FIRE_ZONE_3_CARDS()
    {
        return new List<HexCoords> { new HexCoords(0, -1), new HexCoords(0, -2), new HexCoords(0, -3)  };
    }

    private static List<HexCoords> HelperVectorSet_US_F_OFFENSIVE_FIRE_ZONE_2_CARDS()
    {
        return new List<HexCoords> { new HexCoords(-3, -0), new HexCoords(-2, -1), new HexCoords(-2, 0), new HexCoords(-1, -2), new HexCoords(-1, -1), new HexCoords(-1, 0),
            new HexCoords(1, -3), new HexCoords(2, -3), new HexCoords(3, -3), new HexCoords(1, -2), new HexCoords(2, -2), new HexCoords(1, -1) };
    }

    private static List<HexCoords> HelperVectorSet_US_F_DEFENSIVE_ZONE_2_CARDS()
    {
        return new List<HexCoords> { new HexCoords(-3, 1), new HexCoords(-3, 2), new HexCoords(-3, 3), new HexCoords(-2, 1), new HexCoords(-2, 2), new HexCoords(-1, 1),
            new HexCoords(1, 0), new HexCoords(2, -1), new HexCoords(2, 0), new HexCoords(3, -2), new HexCoords(3, -1), new HexCoords(3, 0) };
    }

    private static List<HexCoords> HelperVectorSet_US_F_DEFENSIVE_ZONE_1_CARDS()
    {
        return new List<HexCoords> { new HexCoords(0, 1), new HexCoords(1, 1), new HexCoords(2, 1), new HexCoords(-1, 2), new HexCoords(0, 2), new HexCoords(1, 2),
            new HexCoords(-2, 3), new HexCoords(-1, 3), new HexCoords(0, 3) };
    }







    private static List<HexCoords> HelperVectorSet_JP_F_OFFENSIVE_FIRE_ZONE_3_CARDS()
    {
        return new List<HexCoords> { new HexCoords(0, -1), new HexCoords(0, -2), new HexCoords(0, -3), new HexCoords(-2, -1), new HexCoords(-1, -2), new HexCoords(-1, -1),
            new HexCoords(1, -3), new HexCoords(2, -3), new HexCoords(1, -2) };
    }

    private static List<HexCoords> HelperVectorSet_JP_F_OFFENSIVE_FIRE_ZONE_2_CARDS()
    {
        return new List<HexCoords> { new HexCoords(-3, 0), new HexCoords(-2, 0), new HexCoords(-1, 0),
            new HexCoords(3, -3), new HexCoords(2, -2), new HexCoords(1, -1) };
    }

    private static List<HexCoords> HelperVectorSet_JP_F_DEFENSIVE_ZONE_2_CARDS()
    {
        return new List<HexCoords> { new HexCoords(-3, 1), new HexCoords(-3, 2), new HexCoords(-3, 3), new HexCoords(-2, 1), new HexCoords(-2, 2), new HexCoords(-1, 1),
            new HexCoords(1, 0), new HexCoords(2, -1), new HexCoords(2, 0), new HexCoords(3, -2), new HexCoords(3, -1), new HexCoords(3, 0) };
    }

    private static List<HexCoords> HelperVectorSet_JP_F_DEFENSIVE_ZONE_1_CARDS()
    {
        return new List<HexCoords> { new HexCoords(0, 1), new HexCoords(1, 1), new HexCoords(2, 1), new HexCoords(-1, 2), new HexCoords(0, 2), new HexCoords(1, 2),
            new HexCoords(-2, 3), new HexCoords(-1, 3), new HexCoords(0, 3) };
    }








    public static List<HexCoords> InteractionArea(HexCoords unitHex, HexDirection unitDirection, UnitType unitType, InteractionAreaType interactionAreaType)
    {
        if (!stats[unitType].RelativeInteractionAreas.ContainsKey(interactionAreaType))
        {
            return new List<HexCoords>();
        }

        //zbior hexow okreslajacy obszar pobrany ze stats dla 0,0 NW
        List<HexCoords> areaModel = stats[unitType].RelativeInteractionAreas[interactionAreaType];
        List<HexCoords> adjustedAreaModel = new List<HexCoords>();

        //obracanie i przesuwanie
        foreach (HexCoords h in areaModel)
        {
            HexCoords h1 = HexTools.RotateVector(h, HexDirection.NorthWest, unitDirection);
            HexCoords h2 = new HexCoords(h1.q + unitHex.q, h1.r + unitHex.r);

            adjustedAreaModel.Add(h2);
        }

        return adjustedAreaModel;

    }


}
