using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static RuleEngine;

public class Orchestrator : MonoBehaviour
{

    private SceneManagerReferences scene;

    private Queue<RuleEngine.ResultingActionE> actionQueue = new Queue<RuleEngine.ResultingActionE>();

    //informacje wykonawicze o instancjach, zbêdne w silniku, potrzebne przy akcjach - zwi¹zane z jednym, aktualnie przetwarzanym inputem gracza i czyszczone po zakoñczeniu przetwarzania tego inputu
    private UnitManager.Unit activeUnit = null;
    //private UnitManager.Unit unitAtHex = null;
    private UnitManager.Unit attackedUnit = null;


    public void ServiceOverlayClick(string tag, HexCoords? hex)
    {

        //Debug.Log($"O: clicked: {tag}, hex: {(hex.HasValue ? $"({hex.Value.q},{hex.Value.r})" : "none")}"); //value wynika z tego, ¿e hex ma typ Nullable<HexCoords> 

        RuleEngine.InputTargetE inputTarget; 
        RuleEngine.InputHexContentE inputHexContent = RuleEngine.InputHexContentE.NA; //nieistotne przy input target = overlay
        RuleEngine.TargetHexE targetHex; 
        RuleEngine.HasActiveUnitE hasActiveUnit = RuleEngine.HasActiveUnitE.NA; //niech bêdzie NA, bo nieistotne - tak zaimplementowane w regulach; oczywicie ma, inaczej nie bylo by klikniecia w overlay
        RuleEngine.CanMoveFurtherE canMoveFurther; //do ustalenia: pozycja overlay, jednostka tamze
        RuleEngine.MovedThisTurnE movedThisTurn = RuleEngine.MovedThisTurnE.NA; //nieistotne - jesli strzalka, no to wlasnie sie rusza i traci atak; jesli atak, to wiadomo ze sie nie ruszal

        switch (tag)
        {
            case "UiOverlayLeftArrow":
                inputTarget = RuleEngine.InputTargetE.ARROW_LEFT;
                break;
            case "UiOverlayRightArrow":
                inputTarget = RuleEngine.InputTargetE.ARROW_RIGHT;
                break;
            case "UiOverlayForwardArrow":
                inputTarget = RuleEngine.InputTargetE.ARROW_FORWARD;
                break;
            case "UiOverlayAttackButton":
                inputTarget = RuleEngine.InputTargetE.ATTACK;
                break;
            default:
                Debug.Log("Pusty lub nieprawidlowy tag");
                throw new ArgumentOutOfRangeException(nameof(tag), tag, "Nieobs³ugiwany tag");
        }

        if (inputTarget == RuleEngine.InputTargetE.ATTACK)
        {
            attackedUnit = scene.Unit.UnitList.FirstOrDefault(a => a.HexCoords.Equals(hex));
        }

        HexCoords targetHexCoords;

        if (inputTarget is RuleEngine.InputTargetE.ARROW_LEFT or RuleEngine.InputTargetE.ARROW_FORWARD or RuleEngine.InputTargetE.ARROW_RIGHT) //odpowiednik IN z SQL
        {
            targetHexCoords = HexTools.Neighbor(scene.Overlay.ActiveHex, scene.Overlay.ActiveDirection);
        }
        else if (hex != null)
        {
            targetHexCoords = (HexCoords)hex;
        }
        else
        {
            throw new Exception("Brak informacji na jakim hexie kliknieto atak");
        }

        if (!scene.Terrain.TerrainHexCoordsList.Contains(targetHexCoords))
        {
            targetHex = RuleEngine.TargetHexE.OFF_MAP;
        }
        else if (scene.Unit.UnitList.Any(u => u.HexCoords.Equals(targetHexCoords)))
        {
            targetHex = RuleEngine.TargetHexE.OCCUPIED;
        }
        else
        {
            targetHex = RuleEngine.TargetHexE.EMPTY;
        }

        activeUnit = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(scene.Overlay.ActiveHex));
        int maxMoves = UnitStatsData.GetStats(activeUnit.UnitType).MaxMoves;
        
        if (activeUnit.MovesThisTurn + 1 == maxMoves)
        {
            canMoveFurther = RuleEngine.CanMoveFurtherE.FALSE;
        }
        else
        {
            canMoveFurther = RuleEngine.CanMoveFurtherE.TRUE;
        }


        //ewaluacja
        Condition condition = new Condition(inputTarget, inputHexContent, targetHex, hasActiveUnit, canMoveFurther, movedThisTurn);

        Debug.Log(condition.ToString());

        List<RuleEngine.ResultingActionE> actions = RuleEngine.PerformEvaluation(condition);

        string s = "Akcje: ";
        foreach (var a in actions)
        {
            s += a + ", ";
        }
        Debug.Log(s);


        //obs³uga akcji
        for (int i = 0; i < actions.Count; i++)
        {
            actionQueue.Enqueue(actions[i]);
        }

    }





    public void ServiceHexPlaneClick(HexCoords hexClicked)
    {
        //Debug.Log("O: input plane clicked");

        RuleEngine.InputTargetE inputTarget;
        RuleEngine.InputHexContentE inputHexContent = RuleEngine.InputHexContentE.NA; //smieciowa wartosc do nadpisania, bo kompilator sie martwi
        RuleEngine.TargetHexE targetHex;
        RuleEngine.HasActiveUnitE hasActiveUnit;
        RuleEngine.CanMoveFurtherE canMoveFurther;
        RuleEngine.MovedThisTurnE movedThisTurn = RuleEngine.MovedThisTurnE.NA; //smieciowa wartosc do nadpisania, bo kompilator sie martwi


        //klik pochodzi z input plane tak naprawde, sprawdzic czy tam jest hex
        if (scene.Terrain.TerrainHexCoordsList.Contains(hexClicked))
        {
            inputTarget = RuleEngine.InputTargetE.HEX;
        }
        else
        {
            inputTarget = RuleEngine.InputTargetE.OFF_MAP;
        }

        //zawartosc kliknietego hexa
        //bez znaczenie przy off map
        if (inputTarget == RuleEngine.InputTargetE.OFF_MAP)
        {
            inputHexContent = RuleEngine.InputHexContentE.NA;
        }
        //je¿eli hex
        else if (inputTarget == RuleEngine.InputTargetE.HEX)
        {
            UnitManager.Unit unitAtHex = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(hexClicked));

            //pusty hex
            if (unitAtHex == null)
            {
                inputHexContent = RuleEngine.InputHexContentE.EMPTY;
            }
            //wlasny samolot
            else if (unitAtHex.Player == scene.SceneState.ActivePlayer)
            {
                Debug.Log("O: klikniety wlasny samolot");
                
                if (!unitAtHex.AttackedThisTurn && unitAtHex.MovesThisTurn < UnitStatsData.GetStats(unitAtHex.UnitType).MaxMoves)
                {
                    inputHexContent = RuleEngine.InputHexContentE.OWN_READY;
                    activeUnit = unitAtHex;

                    //czy juz sie ruszal w tej turze
                    //istotne tylko przy wlasnym samolocie zdolnym do ruchu/ataku
                    if (unitAtHex.MovesThisTurn > 0)
                    {
                        movedThisTurn = RuleEngine.MovedThisTurnE.TRUE;
                    }
                    else
                    {
                        movedThisTurn = RuleEngine.MovedThisTurnE.FALSE;
                    }
                }
                else
                {
                    inputHexContent = RuleEngine.InputHexContentE.OWN_USED;
                }
            }
            //obcy samolot
            else
            {
                Debug.Log("O: klikniety wrogi samolot");
                inputHexContent = RuleEngine.InputHexContentE.ENEMY;
            }

        }

        //docelowy hex tu nie ma znaczenia, ma znaczenie przy strzalkach
        targetHex = RuleEngine.TargetHexE.NA;

        //informcja czy byl aktywny samolot / czy widac bylo strzalki ruchu lub ruchu i ataku / czy poprzednio zaznaczony hex zawieral wlasny samolot zdolny do ruchu/ataku
        if (scene.Overlay.IsOverlayVisible)
        {
            hasActiveUnit = RuleEngine.HasActiveUnitE.TRUE;
        }
        else
        {
            hasActiveUnit = RuleEngine.HasActiveUnitE.FALSE;
        }

        //nieistotne po kliknieciu hexa
        canMoveFurther = RuleEngine.CanMoveFurtherE.NA;



        //ewaluacja
        Condition condition = new Condition(inputTarget, inputHexContent, targetHex, hasActiveUnit, canMoveFurther, movedThisTurn);

        Debug.Log(condition.ToString());

        List<RuleEngine.ResultingActionE> actions = RuleEngine.PerformEvaluation(condition);

        string s = "Akcje: ";
        foreach (var a in actions)
        {
            s += a + ", ";
        }
        Debug.Log(s);   


        //obs³uga akcji - moze do wyniesienia gdzies kiedys
        for (int i = 0; i < actions.Count; i++)
        {
            actionQueue.Enqueue(actions[i]);
        }
    }

    private void ServiceActionShowOverlay(UnitManager.Unit unitAtHex)
    {
        List<UnitManager.Unit> enemies = scene.Unit.UnitList.Where(u => u.Player != scene.SceneState.ActivePlayer).ToList();

        List<HexCoords> hexesInAttackRange = UnitStatsData.InteractionArea(unitAtHex.HexCoords, unitAtHex.Direction, unitAtHex.UnitType, UnitStatsData.InteractionAreaTypeE.ATTACK);

        List<HexCoords> targets = new List<HexCoords>();

        foreach (UnitManager.Unit e in enemies)
        {
            if (hexesInAttackRange.Contains(e.HexCoords))
            {
                targets.Add(e.HexCoords);
            }
        }

        scene.Overlay.DisplayOverlay(unitAtHex.HexCoords, unitAtHex.Direction, targets);
    }

    private void ServiceActionShowMoveOverlay(UnitManager.Unit unitAtHex)
    {
        scene.Overlay.DisplayOverlay(unitAtHex.HexCoords, unitAtHex.Direction, new List<HexCoords> { });
    }

    private void ServiceActionHideOverlay()
    {
        scene.Overlay.HideOverlay();
    }

    private void ServiceActionMoveUnitForward(UnitManager.Unit unit)
    {
        scene.Unit.MoveForward(unit);
    }

    private void ServiceActionMoveUnitLeft(UnitManager.Unit unit)
    {
        scene.Unit.MoveLeft(unit);
    }

    private void ServiceActionMoveUnitRight(UnitManager.Unit unit)
    {
        scene.Unit.MoveRight(unit);
    }

    private void ServiceActionShowCombatMenu(UnitManager.Unit attackedUnit)
    {
        scene.CombatOverlay.ShowCombatOverlay();
    }

    private void ServiceActionSwapUnitsLeft(UnitManager.Unit unit)
    {
        HexCoords unitOrigin = unit.HexCoords;
        HexCoords unitDest = HexTools.Neighbor(unit.HexCoords, unit.Direction);
        scene.Unit.MoveLeft(unit);
        UnitManager.Unit unitToMove = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(unitDest));
        scene.Unit.BeMoved(unitToMove, unitOrigin);
    }

    private void ServiceActionSwapUnitsForward(UnitManager.Unit unit)
    {
        HexCoords unitOrigin = unit.HexCoords;
        HexCoords unitDest = HexTools.Neighbor(unit.HexCoords, unit.Direction);
        scene.Unit.MoveForward(unit);
        UnitManager.Unit unitToMove = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(unitDest));
        scene.Unit.BeMoved(unitToMove, unitOrigin);
    }

    private void ServiceActionSwapUnitsRight(UnitManager.Unit unit)
    {
        HexCoords unitOrigin = unit.HexCoords;
        HexCoords unitDest = HexTools.Neighbor(unit.HexCoords, unit.Direction);
        scene.Unit.MoveRight(unit);
        UnitManager.Unit unitToMove = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(unitDest));
        scene.Unit.BeMoved(unitToMove, unitOrigin);
    }

    private void ServiceActionDestroyUnit(UnitManager.Unit unit)
    {
        scene.Unit.DestroyUnit(unit);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scene = SceneManagerReferences.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!scene.SceneState.UnitMovementInProgress && actionQueue.Count > 0)
        {
            RuleEngine.ResultingActionE currentAction = actionQueue.Dequeue();

            //wykonujê to w update, aby unikn¹æ zagniezdzonych korutyn
            switch (currentAction)
            {
                case RuleEngine.ResultingActionE.HIDE_OVERLAY:
                    ServiceActionHideOverlay();
                    break;
                case RuleEngine.ResultingActionE.MOVE_UNIT_LEFT:
                    ServiceActionMoveUnitLeft(activeUnit);
                    break;
                case RuleEngine.ResultingActionE.MOVE_UNIT_FORWARD:
                    ServiceActionMoveUnitForward(activeUnit);
                    break;
                case RuleEngine.ResultingActionE.MOVE_UNIT_RIGHT:
                    ServiceActionMoveUnitRight(activeUnit);
                    break;
                case RuleEngine.ResultingActionE.SHOW_OVERLAY:
                    ServiceActionShowOverlay(activeUnit);
                    break;
                case RuleEngine.ResultingActionE.SHOW_MOVE_OVERLAY:
                    ServiceActionShowMoveOverlay(activeUnit);
                    break;
                case RuleEngine.ResultingActionE.SHOW_COMBAT_MENU:
                    ServiceActionShowCombatMenu(attackedUnit);
                    break;
                case ResultingActionE.SWAP_UNITS_LEFT:
                    ServiceActionSwapUnitsLeft(activeUnit);
                    break;
                case ResultingActionE.SWAP_UNITS_FORWARD:
                    ServiceActionSwapUnitsForward(activeUnit);
                    break;
                case ResultingActionE.SWAP_UNITS_RIGHT:
                    ServiceActionSwapUnitsRight(activeUnit);
                    break;
                case ResultingActionE.DESTROY_UNIT:
                    ServiceActionDestroyUnit(activeUnit);
                    break;

                default:
                    break;
            }

            //te dane sa do wyczyszczenia po zakonczeniu ostatniej akcji, bo byly zwiazane wylacznie z obsluga tego jednego inputu gracza
            //z rozwazan o przypadkach: ukrycie overlay nie dezaktywuje jednostki
            if (actionQueue.Count == 0)
            {
                activeUnit = null;
                attackedUnit = null;
            }
        }
    }
}
