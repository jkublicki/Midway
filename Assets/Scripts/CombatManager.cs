using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HexTools;
using static UnitStatsData;
using static UnityEngine.Rendering.DebugUI;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private UnitManager.Unit attackerUnit;
    private UnitManager.Unit defenderUnit;
    private UnitManager.Unit attackerUnitBackup;
    private UnitManager.Unit defenderUnitBackup;

    public CombatRole? OwnCombatRole;

    private CombatState state;
    private float stateEnterTime;
    private HexDirectionChange? attackerFirstMove;
    private HexDirectionChange? attackerSecondMove;
    private HexDirectionChange? defenderFirstMove;
    private HexDirectionChange? defenderSecondMove;


    private bool attackerCanDie = false;
    private bool defenderCanDie = false;
    private int attackerDiceCount = 0;
    private int defenderDiceCount = 0;

    List<int> attackerDices = new List<int>();
    List<int> defenderDices = new List<int>();



    private bool moveSequenceIsDone = false;

    public HexDirectionChange? AttackerFirstMove => attackerFirstMove;
    public HexDirectionChange? AttackerSecondMove => attackerSecondMove;
    public HexDirectionChange? DefenderFirstMove => defenderFirstMove;
    public HexDirectionChange? DefenderSecondMove => defenderSecondMove;



    private Dictionary<CombatState, float> StateTimeouts = new Dictionary<CombatState, float>
    {
        { CombatState.AutofilingMoves, 2.0f },
        { CombatState.AwaitingMoves, 60.0f }, //domyslnie 15.0f?
        { CombatState.AwaitingDefenderCard, 10.0f },
        { CombatState.AwaitingAttackerCard, 10.0f }
    };

    public void Initialize(string attackerUnitId, string defenderUnitId)
    {
        attackerUnit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID == attackerUnitId);
        defenderUnit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID == defenderUnitId);

        if (attackerUnit.Player == LobbyManager.Instance.LocalPlayer)
        {
            OwnCombatRole = CombatRole.Attacker;
            Debug.Log($"Your unit {attackerUnit.ID} is attacking enemy unit {defenderUnit.ID}");
        }
        else
        {
            OwnCombatRole = CombatRole.Defender;
            Debug.Log($"Enemy unit {attackerUnit.ID} is attacking your unit {defenderUnit.ID}");
        }

        attackerUnitBackup = new UnitManager.Unit(attackerUnit);
        defenderUnitBackup = new UnitManager.Unit(defenderUnit);

        ChangeCombatState(CombatState.AwaitingMoves);
    }


    public void SetMove(HexDirectionChange direction, CombatRole combatRole) //zast¹piæ send
    {
        if (combatRole == CombatRole.Attacker)
        {
            if (attackerFirstMove == null)
            {
                attackerFirstMove = direction; //zastapic submit
                Debug.Log("AttackerFirstMove set: " + AttackerFirstMove);
            }
            else
            {
                attackerSecondMove = direction;
                Debug.Log("AttackerSecondMove set: " + AttackerSecondMove);
            }
        }
        else
        {
            if (defenderFirstMove == null)
            {
                defenderFirstMove = direction;
                Debug.Log("DefenderFirstMove set: " + DefenderFirstMove);
            }
            else
            {
                defenderSecondMove = direction;
                Debug.Log("DefenderSecondMove set: " + DefenderSecondMove);
            }
        }
    }


    public void SetDices(int d1, int? d2, int? d3, CombatRole combatRole)
    {
        if (combatRole == CombatRole.Attacker)
        {
            attackerDices.Add(d1);
            if (d2 != null)
            {
                attackerDices.Add((int)d2);
            }
            if (d3 != null)
            {
                attackerDices.Add((int)d3);
            }
        }
        else
        {
            defenderDices.Add(d1);
            if (d2 != null)
            {
                defenderDices.Add((int)d2);
            }
            if (d3 != null)
            {
                defenderDices.Add((int)d3);
            }
        }
    }

    private void AutofillMoves()
    {
        //przeniesc do hextools?
        static HexDirectionChange RandomDir()
        {
            var values = Enum.GetValues(typeof(HexDirectionChange));
            return (HexDirectionChange)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        //kiedys jakie madrzejszy algorytm, teraz cokolwiek dla testow
        if (attackerFirstMove == null)
        {
            attackerFirstMove = RandomDir();
        }
        if (attackerSecondMove == null)
        {
            attackerSecondMove = RandomDir();
        }
        if (defenderFirstMove == null)
        {
            defenderFirstMove = RandomDir();
        }
        if (defenderSecondMove == null)
        {
            defenderSecondMove = RandomDir();
        }
    }


    //odtworzenie 4 ruchów po kolei
    IEnumerator MoveSequence()
    {
        CombatOverlayManager.Instance.HideOwnHighlight();
        CombatOverlayManager.Instance.HideEnemyHighlight();

        //attacker 1
        if (OwnCombatRole == CombatRole.Attacker)
        {
            CombatOverlayManager.Instance.HideArrow(1);
        }
        UnitManager.Unit unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(attackerUnit.HexCoords, attackerUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, attackerUnit.HexCoords);
            unitToSwap = null;
        }

        if (attackerFirstMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(attackerUnit);
        }
        else if (attackerFirstMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(attackerUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(attackerUnit);
        }
        attackerUnit.MovesThisTurn--;

        //defender 1
        if (OwnCombatRole == CombatRole.Defender)
        {
            CombatOverlayManager.Instance.HideArrow(1);
        }
        unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(defenderUnit.HexCoords, defenderUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, defenderUnit.HexCoords);
            unitToSwap = null;
        }
        if (defenderFirstMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(defenderUnit);
        }
        else if (defenderFirstMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(defenderUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(defenderUnit);
        }
        defenderUnit.MovesThisTurn--;

        //attacker 2
        if (OwnCombatRole == CombatRole.Attacker)
        {
            CombatOverlayManager.Instance.HideArrow(2);
        }
        unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(attackerUnit.HexCoords, attackerUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, attackerUnit.HexCoords);
            unitToSwap = null;
        }        
        if (attackerSecondMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(attackerUnit);
        }
        else if (attackerSecondMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(attackerUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(attackerUnit);
        }
        attackerUnit.MovesThisTurn--;

        //defender 2
        if (OwnCombatRole == CombatRole.Defender)
        {
            CombatOverlayManager.Instance.HideArrow(2);
        }
        unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(defenderUnit.HexCoords, defenderUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, defenderUnit.HexCoords);
            unitToSwap = null;
        }
        if (defenderSecondMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(defenderUnit);
        }
        else if (defenderSecondMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(defenderUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(defenderUnit);
        }
        defenderUnit.MovesThisTurn--;

        moveSequenceIsDone = true;
    }

    InteractionAreaType? InteractionZone(HexCoords unitHex, HexDirection unitDirection, UnitType unitType, HexCoords banditHex)
    {
        InteractionAreaType? result = null;
       
        foreach (InteractionAreaType areaType in Enum.GetValues(typeof(InteractionAreaType)))
        {
            List<HexCoords> areaHexes = InteractionArea(unitHex, unitDirection, unitType, areaType);
            if (areaHexes.Contains(banditHex))
            {
                result = areaType;
            }
        }

        return result;
    }


    //wyczyszczenie stanu combat managera - ma byc zawsze robione na koniec
    private void Reset()
    {
        ChangeCombatState(CombatState.NoCombat);

        OwnCombatRole = null;

        attackerUnit = null;
        attackerUnitBackup = null;
        defenderUnit = null;
        defenderUnitBackup = null;

        attackerCanDie = false;
        defenderCanDie = false;
        attackerDiceCount = 0;
        defenderDiceCount = 0;

        moveSequenceIsDone = false;

        stateEnterTime = 0.0f;

        attackerFirstMove = null;
        attackerSecondMove = null;
        defenderFirstMove = null;
        defenderSecondMove = null;

        attackerDices.Clear();
        defenderDices.Clear();
    }



    private void OnStateEnter(CombatState entering)
    {
        UnitManager.Unit ownUnit = null;
        UnitManager.Unit enemyUnit = null;
        if (OwnCombatRole == CombatRole.Attacker)
        {
            ownUnit = attackerUnit;
            enemyUnit = defenderUnit;
        }
        else
        {
            ownUnit = defenderUnit;
            enemyUnit = attackerUnit;
        }


        switch (state)
        {
            case CombatState.AwaitingMoves:

                attackerUnit.AttackedThisTurn = true;
                //interfejsy
                CombatOverlayManager.Instance.DisplayArrows(ownUnit.HexCoords, ownUnit.Direction);
                CombatOverlayManager.Instance.DisplayOwnHighlight(ownUnit.HexCoords);
                CombatOverlayManager.Instance.DisplayEnemyHighlight(enemyUnit.HexCoords);
                break;
            case CombatState.AutofilingMoves:
                AutofillMoves();
                CombatOverlayManager.Instance.HideArrows();
                break;
            case CombatState.RevealingMoves:
                //tu odpalic MoveSequence
                StartCoroutine(MoveSequence());
                break;
            case CombatState.AwaitingDices:
                //sprawdzic czy ktorys (lub oba) wylecial za mape
                if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(attackerUnit.HexCoords) || !TerrainManager.Instance.TerrainHexCoordsList.Contains(defenderUnit.HexCoords))
                {
                    if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(attackerUnit.HexCoords))
                    {
                        UnitManager.Instance.DestroyUnit(attackerUnit);
                        Debug.Log("Napastnik wylecia³ poza mapê i spad³ z braku paliwa.");
                    }
                    if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(defenderUnit.HexCoords))
                    {
                        UnitManager.Instance.DestroyUnit(defenderUnit);
                        Debug.Log("Zaatakowany wylecia³ poza mapê i spad³ z braku paliwa.");
                    }
                    ChangeCombatState(CombatState.Finished);
                    break;
                }

                //sprawdzic odleglosc, skonczyc jesli > 3
                if (HexTools.HexDistance(attackerUnit.HexCoords, defenderUnit.HexCoords) > 3)
                {
                    Debug.Log("Uczestnicy starcia oddalili siê od siebie.");
                    ChangeCombatState(CombatState.Finished);
                    break;
                }


                InteractionAreaType? attackerInteraction = InteractionZone(attackerUnit.HexCoords, attackerUnit.Direction, attackerUnit.UnitType, defenderUnit.HexCoords);
                InteractionAreaType? defenderInteraction = InteractionZone(defenderUnit.HexCoords, defenderUnit.Direction, defenderUnit.UnitType, attackerUnit.HexCoords);
                if (attackerInteraction == null || defenderInteraction == null)
                {
                    throw new Exception("Nie znaleziono interakcji");
                }
                switch (attackerInteraction)
                {
                    case InteractionAreaType.FireThree:
                        defenderCanDie = true;
                        attackerDiceCount = 3;
                        break;
                    case InteractionAreaType.FireTwo:
                        defenderCanDie = true;
                        attackerDiceCount = 2;
                        break;
                    case InteractionAreaType.DodgeTwo:
                        defenderCanDie = false;
                        attackerDiceCount = 2;
                        break;
                    case InteractionAreaType.DodgeOne:
                        defenderCanDie = false;
                        attackerDiceCount = 1;
                        break;
                }
                switch (defenderInteraction)
                {
                    case InteractionAreaType.FireThree:
                        attackerCanDie = true;
                        defenderDiceCount = 3;
                        break;
                    case InteractionAreaType.FireTwo:
                        attackerCanDie = true;
                        defenderDiceCount = 2;
                        break;
                    case InteractionAreaType.DodgeTwo:
                        attackerCanDie = false;
                        defenderDiceCount = 2;
                        break;
                    case InteractionAreaType.DodgeOne:
                        attackerCanDie = false;
                        defenderDiceCount = 1;
                        break;
                }

                List<int> dices = new List<int>();
                for (int i = 0; i < attackerDiceCount; i++)
                {
                    dices.Add(UnityEngine.Random.Range(1, 8));
                }
            
                NetworkBridge.Instance.SubmitCombatDicesRpc(dices[0], dices.Count > 1 ? dices[1] : -1, dices.Count > 2 ? dices[2] : -1, (CombatRole)OwnCombatRole);

                Debug.Log("Walka powietrzna; interakcja napastnika: " + attackerInteraction + ", zaatakowanego: " + defenderInteraction);

                break;

            case CombatState.CombatResolution:

                Debug.Log("Kosci/karty napastnika: " + string.Join(", ", attackerDices) + ". Kosci/karty zaatakowanego: " + string.Join(", ", defenderDices) + ".");


                if (attackerDices.Max() > defenderDices.Max() && defenderCanDie)
                {
                    //smierc zaatakowanego
                    UnitManager.Instance.DestroyUnit(defenderUnit);
                    Debug.Log("Zaatakowany zostal zestrzelony!");
                }
                else
                if (defenderDices.Max() > attackerDices.Max() && attackerCanDie)
                {
                    //smierc napastnika
                    UnitManager.Instance.DestroyUnit(attackerUnit);
                    Debug.Log("Napastnik zostal zestrzelony!");
                }
                else
                {
                    Debug.Log("Nikt nie zostal zestrzelony.");
                }


                ChangeCombatState(CombatState.Finished);
                break;

            case CombatState.Finished:
                Reset();
                Orchestrator.Instance.ServiceCombatFinish();
                break;


            default: //dla NO_COMBAT itp.
                break;
        }
    }

    private void OnStateExit(CombatState exiting)
    {

    }

    private void ChangeCombatState(CombatState newState)
    {
        Debug.Log("Combat state: " + state + " -> " + newState);

        OnStateExit(state);
        state = newState;
        stateEnterTime = Time.time;
        OnStateEnter(state);
    }


    void Start()
    {
        
    }

 
    void Update()
    {
        switch (state)
        {
            case CombatState.AwaitingMoves:
                if (defenderFirstMove != null && defenderSecondMove != null && attackerFirstMove != null && attackerSecondMove != null)
                {
                    ChangeCombatState(CombatState.RevealingMoves);
                }
                else if (Time.time - stateEnterTime > StateTimeouts[CombatState.AwaitingMoves])
                {
                    //automatycznie ustawic ruchy
                    ChangeCombatState(CombatState.AutofilingMoves);
                }
                break;
            case CombatState.AutofilingMoves:
                if (Time.time - stateEnterTime > StateTimeouts[CombatState.AutofilingMoves])
                {
                    //automatycznie ustawic ruchy; ma³a pauza i isc dalej
                    ChangeCombatState(CombatState.RevealingMoves);
                }
                break;
            case CombatState.RevealingMoves:
                if (moveSequenceIsDone)
                {
                    ChangeCombatState(CombatState.AwaitingDices);
                }
                break;
            case CombatState.AwaitingDices:
                if (defenderDices.Count > 0 && attackerDices.Count > 0)
                {
                    ChangeCombatState(CombatState.CombatResolution);
                }
                break;
        }

    }
}
