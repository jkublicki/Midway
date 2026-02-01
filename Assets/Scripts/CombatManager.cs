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


    private UnitManager.Unit ownUnit;
    private UnitManager.Unit enemyUnit;
    private UnitManager.Unit ownUnitBackup;
    private UnitManager.Unit enemyUnitBackup;

    private CombatRole? ownCombatRole;

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



    private bool moveSequenceIsDone = false;

    public HexDirectionChange? AttackerFirstMove => attackerFirstMove;
    public HexDirectionChange? AttackerSecondMove => attackerSecondMove;
    public HexDirectionChange? DefenderFirstMove => defenderFirstMove;
    public HexDirectionChange? DefenderSecondMove => defenderSecondMove;



    private Dictionary<CombatState, float> StateTimeouts = new Dictionary<CombatState, float>
    {
        { CombatState.AutofilingMoves, 2.0f },
        { CombatState.AwaitingMoves, 15.0f },
        { CombatState.AwaitingDefenderCard, 10.0f },
        { CombatState.AwaitingAttackerCard, 10.0f }
    };

    /*
    public void Initialize_old(UnitManager.Unit _ownUnit, UnitManager.Unit _enemyUnit)
    {
        //dane
        ownUnit = _ownUnit;
        ownUnitBackup = new UnitManager.Unit(_ownUnit);
        enemyUnit = _enemyUnit;
        enemyUnitBackup = new UnitManager.Unit(_enemyUnit);

        //stan poczatkowy
        ChangeCombatState(CombatState.AwaitingMoves);
    }
    */

    public void Initialize(string attackerUnitId, string defenderUnitId)
    {
        UnitManager.Unit attackerUnit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID == attackerUnitId);
        UnitManager.Unit defenderUnit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID == defenderUnitId);

        if (attackerUnit.Player == LobbyManager.Instance.LocalPlayer)
        {
            ownCombatRole = CombatRole.Attacker;
            ownUnit = attackerUnit;
            enemyUnit = defenderUnit;

            Debug.Log($"Your unit {ownUnit.ID} is attacking enemy unit {enemyUnit.ID}");

        }
        else
        {
            ownCombatRole = CombatRole.Defender;
            ownUnit = defenderUnit;
            enemyUnit = attackerUnit;

            Debug.Log($"Enemy unit {enemyUnit.ID} is attacking your unit {ownUnit.ID}");
        }

        ownUnitBackup = new UnitManager.Unit(ownUnit);
        enemyUnitBackup = new UnitManager.Unit(enemyUnit);

        ChangeCombatState(CombatState.AwaitingMoves);


    }


    public void SetMove(HexDirectionChange direction, CombatRole combatRole)
    {
        if (combatRole == CombatRole.Attacker)
        {
            if (attackerFirstMove == null)
            {
                attackerFirstMove = direction;
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
            }
            else
            {
                defenderSecondMove = direction;
            }
        }
    }


    void TempshitTestSetEnemyMoves()
    {
        static HexDirectionChange RandomDir()
        {
            var values = Enum.GetValues(typeof(HexDirectionChange));
            return (HexDirectionChange)values.GetValue(UnityEngine.Random.Range(0, values.Length));
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
        CombatOverlayManager.Instance.HideArrow(1);
        UnitManager.Unit unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(ownUnit.HexCoords, ownUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, ownUnit.HexCoords);
            unitToSwap = null;
        }
        if (attackerFirstMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(ownUnit);
        }
        else if (attackerFirstMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(ownUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(ownUnit);
        }
        ownUnit.MovesThisTurn--;

        //defender 1
        unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(enemyUnit.HexCoords, enemyUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, enemyUnit.HexCoords);
            unitToSwap = null;
        }
        if (defenderFirstMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(enemyUnit);
        }
        else if (defenderFirstMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(enemyUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(enemyUnit);
        }
        enemyUnit.MovesThisTurn--;

        //attacker 2
        CombatOverlayManager.Instance.HideArrow(2);
        unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(ownUnit.HexCoords, ownUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, ownUnit.HexCoords);
            unitToSwap = null;
        }        
        if (attackerSecondMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(ownUnit);
        }
        else if (attackerSecondMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(ownUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(ownUnit);
        }
        ownUnit.MovesThisTurn--;

        //defender 2
        unitToSwap = UnitManager.Instance.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(enemyUnit.HexCoords, enemyUnit.Direction)));
        if (unitToSwap != null)
        {
            UnitManager.Instance.BeMoved(unitToSwap, enemyUnit.HexCoords);
            unitToSwap = null;
        }
        if (defenderSecondMove == HexDirectionChange.NA)
        {
            yield return UnitManager.Instance.MoveForward(enemyUnit);
        }
        else if (defenderSecondMove == HexDirectionChange.ToLeft)
        {
            yield return UnitManager.Instance.MoveLeft(enemyUnit);
        }
        else
        {
            yield return UnitManager.Instance.MoveRight(enemyUnit);
        }
        enemyUnit.MovesThisTurn--;

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

        ownCombatRole = null;

        ownUnit = null;
        ownUnitBackup = null;
        enemyUnit = null;
        enemyUnitBackup = null;

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



    }



    private void OnStateEnter(CombatState entering)
    {
        switch (state)
        {
            case CombatState.AwaitingMoves:

                ownUnit.AttackedThisTurn = true;
                //interfejsy
                CombatOverlayManager.Instance.ShowCombatOverlayPanel();
                CombatOverlayManager.Instance.DisplayArrows(ownUnit.HexCoords, ownUnit.Direction);
                CombatOverlayManager.Instance.DisplayOwnHighlight(ownUnit.HexCoords);
                CombatOverlayManager.Instance.DisplayEnemyHighlight(enemyUnit.HexCoords);
                //pokazac iterfejsy obroncy
                //tymczasowy brudny test
                TempshitTestSetEnemyMoves();
                break;
            case CombatState.AutofilingMoves:
                //tempshit do testow, docelowo to przyjdzie od klienta drugiego gracza, z nieistniejacego jeszcze interfejsu
                AutofillMoves();
                CombatOverlayManager.Instance.HideArrows();
                break;
            case CombatState.RevealingMoves:
                //tu odpalic MoveSequence
                StartCoroutine(MoveSequence());
                break;
            case CombatState.Dogfight:
                //sprawdzic czy ktorys (lub oba) wylecial za mape
                if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(ownUnit.HexCoords) || !TerrainManager.Instance.TerrainHexCoordsList.Contains(enemyUnit.HexCoords))
                {
                    if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(ownUnit.HexCoords))
                    {
                        UnitManager.Instance.DestroyUnit(ownUnit);
                        Debug.Log("Napastnik wylecia³ poza mapê i spad³ z braku paliwa.");
                    }
                    if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(enemyUnit.HexCoords))
                    {
                        UnitManager.Instance.DestroyUnit(enemyUnit);
                        Debug.Log("Zaatakowany wylecia³ poza mapê i spad³ z braku paliwa.");
                    }
                    ChangeCombatState(CombatState.Finished);
                    break;
                }

                //sprawdzic odleglosc, skonczyc jesli > 3
                if (HexTools.HexDistance(ownUnit.HexCoords, enemyUnit.HexCoords) > 3)
                {
                    Debug.Log("Uczestnicy starcia oddalili siê od siebie.");
                    ChangeCombatState(CombatState.Finished);
                    break;
                }


                InteractionAreaType? attackerInteraction = InteractionZone(ownUnit.HexCoords, ownUnit.Direction, ownUnit.UnitType, enemyUnit.HexCoords);
                InteractionAreaType? defenderInteraction = InteractionZone(enemyUnit.HexCoords, enemyUnit.Direction, enemyUnit.UnitType, ownUnit.HexCoords);
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

                List<int> attackerDices = new List<int>();
                for (int i = 0; i < attackerDiceCount; i++)
                {
                    attackerDices.Add(UnityEngine.Random.Range(1, 8));
                }

                List<int> defenderDices = new List<int>();
                for (int i = 0; i < defenderDiceCount; i++)
                {
                    defenderDices.Add(UnityEngine.Random.Range(1, 8));
                }


                Debug.Log("Walka powietrzna; interakcja napastnika: " + attackerInteraction + ", zaatakowanego: " + defenderInteraction + ". Kosci/karty napastnika: "
                    + string.Join(", ", attackerDices) + ". Kosci/karty zaatakowanego: " + string.Join(", ", defenderDices) + ".");

                if (attackerDices.Max() > defenderDices.Max() && defenderCanDie)
                {
                    //smierc zaatakowanego
                    UnitManager.Instance.DestroyUnit(enemyUnit);
                    Debug.Log("Zaatakowany zostal zestrzelony!");
                }
                else
                if (defenderDices.Max() > attackerDices.Max() && attackerCanDie)
                {
                    //smierc napastnika
                    UnitManager.Instance.DestroyUnit(ownUnit);
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
                    ChangeCombatState(CombatState.Dogfight);
                }
                




                break;
        }

    }
}
