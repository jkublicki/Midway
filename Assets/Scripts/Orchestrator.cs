using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static RuleEngine;

public class Orchestrator : MonoBehaviour
{
    public static Orchestrator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }





    private Queue<ResultingAction> actionQueue = new Queue<ResultingAction>();

    //informacje wykonawicze o instancjach, zbêdne w silniku, potrzebne przy akcjach - zwi¹zane z jednym, aktualnie przetwarzanym inputem gracza i czyszczone po zakoñczeniu przetwarzania tego inputu
    private UnitManager.Unit activeUnit = null;
    //private UnitManager.Unit unitAtHex = null;
    private UnitManager.Unit attackedUnit = null;


    public void ServiceOverlayClick(string tag, HexCoords? hex)
    {

        //Debug.Log($"O: clicked: {tag}, hex: {(hex.HasValue ? $"({hex.Value.q},{hex.Value.r})" : "none")}"); //value wynika z tego, ¿e hex ma typ Nullable<HexCoords> 

        InputTarget inputTarget; 
        InputHexContent inputHexContent = InputHexContent.NA; //nieistotne przy input target = overlay
        TargetHex targetHex; 
        HasActiveUnit hasActiveUnit = HasActiveUnit.NA; //niech bêdzie NA, bo nieistotne - tak zaimplementowane w regulach; oczywicie ma, inaczej nie bylo by klikniecia w overlay
        CanMoveFurther canMoveFurther; //do ustalenia: pozycja overlay, jednostka tamze
        MovedThisTurn movedThisTurn = MovedThisTurn.NA; //nieistotne - jesli strzalka, no to wlasnie sie rusza i traci atak; jesli atak, to wiadomo ze sie nie ruszal

        switch (tag)
        {
            case "UiOverlayLeftArrow":
                inputTarget = InputTarget.ArrowLeft;
                break;
            case "UiOverlayRightArrow":
                inputTarget = InputTarget.ArrowRight;
                break;
            case "UiOverlayForwardArrow":
                inputTarget = InputTarget.ArrowForward;
                break;
            case "UiOverlayAttackButton":
                inputTarget = InputTarget.Attack;
                break;
            default:
                Debug.Log("Pusty lub nieprawidlowy tag");
                throw new ArgumentOutOfRangeException(nameof(tag), tag, "Nieobs³ugiwany tag");
        }

        if (inputTarget == InputTarget.Attack)
        {
            attackedUnit = UnitManager.Instance.UnitList.FirstOrDefault(a => a.HexCoords.Equals(hex));
        }

        HexCoords targetHexCoords;

        if (inputTarget is InputTarget.ArrowLeft or InputTarget.ArrowForward or InputTarget.ArrowRight) //odpowiednik IN z SQL
        {
            targetHexCoords = HexTools.Neighbor(OverlayManager.Instance.ActiveHex, OverlayManager.Instance.ActiveDirection);
        }
        else if (hex != null)
        {
            targetHexCoords = (HexCoords)hex;
        }
        else
        {
            throw new Exception("Brak informacji na jakim hexie kliknieto atak");
        }

        if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(targetHexCoords))
        {
            targetHex = TargetHex.OffMap;
        }
        else if (UnitManager.Instance.UnitList.Any(u => u.HexCoords.Equals(targetHexCoords)))
        {
            targetHex = TargetHex.Occupied;
        }
        else
        {
            targetHex = TargetHex.Empty;
        }

        activeUnit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(OverlayManager.Instance.ActiveHex));
        int maxMoves = UnitStatsData.GetStats(activeUnit.UnitType).MaxMoves;
        
        if (activeUnit.MovesThisTurn + 1 == maxMoves)
        {
            canMoveFurther = CanMoveFurther.FALSE;
        }
        else
        {
            canMoveFurther = CanMoveFurther.TRUE;
        }


        //ewaluacja
        Condition condition = new Condition(inputTarget, inputHexContent, targetHex, hasActiveUnit, canMoveFurther, movedThisTurn);

        Debug.Log(condition.ToString());

        List<ResultingAction> actions = RuleEngine.PerformEvaluation(condition);

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

        InputTarget inputTarget;
        InputHexContent inputHexContent = InputHexContent.NA; //smieciowa wartosc do nadpisania, bo kompilator sie martwi
        TargetHex targetHex;
        HasActiveUnit hasActiveUnit;
        CanMoveFurther canMoveFurther;
        MovedThisTurn movedThisTurn = MovedThisTurn.NA; //smieciowa wartosc do nadpisania, bo kompilator sie martwi


        //klik pochodzi z input plane tak naprawde, sprawdzic czy tam jest hex
        if (TerrainManager.Instance.TerrainHexCoordsList.Contains(hexClicked))
        {
            inputTarget = InputTarget.Hex;
        }
        else
        {
            inputTarget = InputTarget.OffMap;
        }

        //zawartosc kliknietego hexa
        //bez znaczenie przy off map
        if (inputTarget == InputTarget.OffMap)
        {
            inputHexContent = InputHexContent.NA;
        }
        //je¿eli hex
        else if (inputTarget == InputTarget.Hex)
        {
            UnitManager.Unit unitAtHex = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(hexClicked));

            //pusty hex
            if (unitAtHex == null)
            {
                inputHexContent = InputHexContent.Empty;
            }
            //wlasny samolot
            else if (unitAtHex.Player == SceneState.Instance.ActivePlayer)
            {
                //Debug.Log("O: klikniety wlasny samolot");
                
                if (!unitAtHex.AttackedThisTurn && unitAtHex.MovesThisTurn < UnitStatsData.GetStats(unitAtHex.UnitType).MaxMoves)
                {
                    inputHexContent = InputHexContent.OwnReady;
                    activeUnit = unitAtHex;

                    //czy juz sie ruszal w tej turze
                    //istotne tylko przy wlasnym samolocie zdolnym do ruchu/ataku
                    if (unitAtHex.MovesThisTurn > 0)
                    {
                        movedThisTurn = MovedThisTurn.TRUE;
                    }
                    else
                    {
                        movedThisTurn = MovedThisTurn.FALSE;
                    }
                }
                else
                {
                    inputHexContent = InputHexContent.OwnUsed;
                }
            }
            //obcy samolot
            else
            {
                //Debug.Log("O: klikniety wrogi samolot");
                inputHexContent = InputHexContent.Enemy;
            }

        }

        //docelowy hex tu nie ma znaczenia, ma znaczenie przy strzalkach
        targetHex = TargetHex.NA;

        //informcja czy byl aktywny samolot / czy widac bylo strzalki ruchu lub ruchu i ataku / czy poprzednio zaznaczony hex zawieral wlasny samolot zdolny do ruchu/ataku
        if (OverlayManager.Instance.IsOverlayVisible)
        {
            hasActiveUnit = HasActiveUnit.TRUE;
        }
        else
        {
            hasActiveUnit = HasActiveUnit.FALSE;
        }

        //nieistotne po kliknieciu hexa
        canMoveFurther = CanMoveFurther.NA;



        //ewaluacja
        Condition condition = new Condition(inputTarget, inputHexContent, targetHex, hasActiveUnit, canMoveFurther, movedThisTurn);

        Debug.Log(condition.ToString());

        List<ResultingAction> actions = RuleEngine.PerformEvaluation(condition);

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


    public void ServiceUiButtonClick(string tag)
    {
        //Debug.Log("O: Cancel attack button");
    }


    public void ServiceCombatFinish()
    {

    }


    private void ServiceActionShowOverlay(UnitManager.Unit unitAtHex)
    {
        List<UnitManager.Unit> enemies = UnitManager.Instance.UnitList.Where(u => u.Player != SceneState.Instance.ActivePlayer).ToList();

        List<HexCoords> hexesInAttackRange = UnitStatsData.InteractionArea(unitAtHex.HexCoords, unitAtHex.Direction, unitAtHex.UnitType, InteractionAreaType.AttackRange);

        List<HexCoords> targets = new List<HexCoords>();

        foreach (UnitManager.Unit e in enemies)
        {
            if (hexesInAttackRange.Contains(e.HexCoords))
            {
                targets.Add(e.HexCoords);
            }
        }

        OverlayManager.Instance.DisplayOverlay(unitAtHex.HexCoords, unitAtHex.Direction, targets);
    }

    private void ServiceActionShowMoveOverlay(UnitManager.Unit unitAtHex)
    {
        OverlayManager.Instance.DisplayOverlay(unitAtHex.HexCoords, unitAtHex.Direction, new List<HexCoords> { });
    }

    private void ServiceActionHideOverlay()
    {
        OverlayManager.Instance.HideOverlay();
    }

    private void ServiceActionMoveUnitForward(UnitManager.Unit unit)
    {
        SceneState.Instance.UnitMovementInProgress = true;
        NetworkBridge.Instance.SubmitMoveForwardServerRpc(unit.ID);
    }

    private void ServiceActionMoveUnitLeft(UnitManager.Unit unit)
    {
        SceneState.Instance.UnitMovementInProgress = true;
        NetworkBridge.Instance.SubmitMoveLeftServerRpc(unit.ID);
    }

    private void ServiceActionMoveUnitRight(UnitManager.Unit unit)
    {
        SceneState.Instance.UnitMovementInProgress = true;
        NetworkBridge.Instance.SubmitMoveRightServerRpc(unit.ID);
    }

    private void ServiceActionInitializeCombat(UnitManager.Unit attackedUnit, UnitManager.Unit activeUnit)
    {
        //chyba do zastapienia powiadomieniem do combat managera i niech on ogarnia wyswietlanie interfejsow
        //i to powinno byc nie show combat menu, tylko initalize combat - obudzenie managera walki
        //cancel combat tez niech idzie przez silnik, bo trzeba m.in. przywrocic overlay; dodatkowo finish i przy finish combat nie przywracac overlay
        //do tego bedzie potrzebne service ui click

        //scene.CombatOverlay.ShowCombatOverlayPanel();
        //scene.CombatOverlay.DisplayArrows(activeUnit.HexCoords, activeUnit.Direction);
        //scene.CombatOverlay.DisplayOwnHighlight(activeUnit.HexCoords);
        //scene.CombatOverlay.DisplayEnemyHighlight(attackedUnit.HexCoords);

        CombatManager.Instance.Initialize(activeUnit, attackedUnit);
    }

    private void ServiceActionSwapUnitsLeft(UnitManager.Unit unit)
    {
        SceneState.Instance.UnitMovementInProgress = true;
        NetworkBridge.Instance.SubmitMoveLeftServerRpc(unit.ID);
        
        HexCoords unitOrigin = unit.HexCoords;
        HexCoords unitDest = HexTools.Neighbor(unit.HexCoords, unit.Direction);
        UnitManager.Unit unitToMove = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(unitDest));
        NetworkBridge.Instance.SubmitBeMovedServerRpc(unitToMove.ID, unitOrigin.q, unitOrigin.r);
    }

    private void ServiceActionSwapUnitsForward(UnitManager.Unit unit)
    {
        SceneState.Instance.UnitMovementInProgress = true;
        NetworkBridge.Instance.SubmitMoveForwardServerRpc(unit.ID);

        HexCoords unitOrigin = unit.HexCoords;
        HexCoords unitDest = HexTools.Neighbor(unit.HexCoords, unit.Direction);
        UnitManager.Unit unitToMove = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(unitDest));
        NetworkBridge.Instance.SubmitBeMovedServerRpc(unitToMove.ID, unitOrigin.q, unitOrigin.r);
    }

    private void ServiceActionSwapUnitsRight(UnitManager.Unit unit)
    {
        SceneState.Instance.UnitMovementInProgress = true;
        NetworkBridge.Instance.SubmitMoveRightServerRpc(unit.ID);

        HexCoords unitOrigin = unit.HexCoords;
        HexCoords unitDest = HexTools.Neighbor(unit.HexCoords, unit.Direction);
        UnitManager.Unit unitToMove = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(unitDest));
        NetworkBridge.Instance.SubmitBeMovedServerRpc(unitToMove.ID, unitOrigin.q, unitOrigin.r);
    }

    private void ServiceActionDestroyUnit(UnitManager.Unit unit)
    {
        //UnitManager.Instance.DestroyUnit(unit);
        NetworkBridge.Instance.SubmitDestroyUnitServerRpc(unit.ID);
    }

    private void Reset()
    {
        //te dane sa do wyczyszczenia po zakonczeniu ostatniej akcji, bo byly zwiazane wylacznie z obsluga tego jednego inputu gracza
        //z rozwazan o przypadkach: ukrycie overlay nie dezaktywuje jednostki
        if (actionQueue.Count == 0)
        {
            activeUnit = null;
            attackedUnit = null;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    
    // Update is called once per frame
    void Update()
    {
        if (!SceneState.Instance.UnitMovementInProgress && actionQueue.Count > 0)
        {
            ResultingAction currentAction = actionQueue.Dequeue();

            
            //wykonujê to w update, aby unikn¹æ zagniezdzonych korutyn
            switch (currentAction)
            {
                case ResultingAction.HideOverlay:
                    ServiceActionHideOverlay();
                    break;
                case ResultingAction.MoveUnitLeft:
                    ServiceActionMoveUnitLeft(activeUnit);
                    break;
                case ResultingAction.MoveUnitForward:
                    ServiceActionMoveUnitForward(activeUnit);
                    break;
                case ResultingAction.MoveUnitRight:
                    ServiceActionMoveUnitRight(activeUnit);
                    break;
                case ResultingAction.ShowOverlay:
                    ServiceActionShowOverlay(activeUnit);
                    break;
                case ResultingAction.ShowMoveOverlay:
                    ServiceActionShowMoveOverlay(activeUnit);
                    break;
                case ResultingAction.InitializeCombat:
                    ServiceActionInitializeCombat(attackedUnit, activeUnit);
                    break;
                case ResultingAction.SwapUnitsLeft:
                    ServiceActionSwapUnitsLeft(activeUnit);
                    break;
                case ResultingAction.SwapUnitsForward:
                    ServiceActionSwapUnitsForward(activeUnit);
                    break;
                case ResultingAction.SwapUnitsRight:
                    ServiceActionSwapUnitsRight(activeUnit);
                    break;
                case ResultingAction.DestroyUnit:
                    ServiceActionDestroyUnit(activeUnit);
                    break;

                default:
                    break;
            }

            //stan orkiestratora ma zostac wyczyszczony po zakonczeniu przetwarzania jednego inputu / zdarzenia
            Reset();


        }
    }
}
