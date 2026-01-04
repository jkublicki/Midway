using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Test1 : MonoBehaviour
{
    public SceneManagerReferences Scene;

    public Button DebugTurnButton;



    private void DebugSwitchPlayer()
    {
        Scene.SceneState.EndTurn();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DebugTurnButton.onClick.AddListener(DebugSwitchPlayer);



        //Debug.Log(HexTools.RotateVector(new HexCoords(-2, 2), HexTools.HexDirectionPT.WEST, HexTools.HexDirectionPT.NORTH_WEST).ToString());



        //Debug.Log(HexTools.HexDistance(new HexCoords(-1, -2), new HexCoords(2, 1)).ToString());


        //Scene.Overlay.DisplayOverlay(new HexCoords(1, -1), HexTools.HexDirectionPT.EAST, new List<HexCoords>() { new HexCoords(0,0), new HexCoords(-3, 3) });
    }

    bool didIt = false;


    // Update is called once per frame
    void Update()
    {
        /*
        if (!didIt)
        {
            didIt = true;
            Scene.Highlight.AddOverlay(UnitStatsData.InteractionArea(new HexCoords(3, -1), HexTools.HexDirectionPT.SOUTH_WEST, UnitManager.UnitType.US_FIGHTER,
                //UnitStatsData.InteractionAreaTypeE.US_F_OFFENSIVE_FIRE_ZONE_3_CARDS));
                UnitStatsData.InteractionAreaTypeE.ATTACK));
        }
        */


    }
}
