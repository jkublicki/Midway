using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Test1 : MonoBehaviour
{


    public Button DebugTurnButton;


    public TextMeshProUGUI LogText;


    private void DebugSwitchPlayer()
    {
        BattleSceneState.Instance.EndTurn();
    }


    
    void Start()
    {
        DebugTurnButton.onClick.AddListener(DebugSwitchPlayer);
        Application.logMessageReceived += (log, trace, type) =>
        {
            // Get current timestamp
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

            // Format log with timestamp
            string logWithTimestamp = $"[{timestamp}] {log}";

            var oldLines = LogText.text.Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            var keepLines = oldLines.Skip(System.Math.Max(0, oldLines.Length - 6)); // keep last 3 lines
            LogText.text = string.Join("\r\n", keepLines) + "\r\n" + logWithTimestamp;
        };


        Debug.Log("JOINED GAME " + LobbyManager.Instance.CurrentLobby.Name + ", ID: " + LobbyManager.Instance.CurrentLobby.Id);


        //Debug.Log(HexTools.RotateVector(new HexCoords(-2, 2), HexDirection.West, HexDirection.NorthWest).ToString());

        /*
        for (int i = 0; i < 100; i++)
        {
            Debug.Log(TitleGenerator.GetTitle());
        }
        */

        //Debug.Log(HexTools.HexDistance(new HexCoords(-1, -2), new HexCoords(2, 1)).ToString());


        //Scene.Overlay.DisplayOverlay(new HexCoords(1, -1), HexDirection.East, new List<HexCoords>() { new HexCoords(0,0), new HexCoords(-3, 3) });
    }

    bool didIt = false;


    
    void Update()
    {
        /*
        if (!didIt)
        {
            didIt = true;
            Scene.Highlight.AddOverlay(InteractionArea(new HexCoords(0, 0), HexDirection.NorthWest, UnitManager.UnitType.JpFighter,
                //InteractionAreaType.US_F_OFFENSIVE_FIRE_ZONE_3_CARDS));
                InteractionAreaType.FireThree));
        }
        */



    }
}
