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
    private SceneManagerReferences scene;


    private UnitManager.Unit ownUnit;
    private UnitManager.Unit enemyUnit;
    private UnitManager.Unit ownUnitBackup;
    private UnitManager.Unit enemyUnitBackup;
    private CombatStateE state;
    private float stateEnterTime;
    private HexTools.HexDirectionChangeE? attackerFirstMove;
    private HexTools.HexDirectionChangeE? attackerSecondMove;
    private HexTools.HexDirectionChangeE? defenderFirstMove;
    private HexTools.HexDirectionChangeE? defenderSecondMove;


    private bool attackerCanDie = false;
    private bool defenderCanDie = false;
    private int attackerDiceCount = 0;
    private int defenderDiceCount = 0;



    private bool moveSequenceIsDone = false;

    public HexTools.HexDirectionChangeE? AttackerFirstMove => attackerFirstMove;
    public HexTools.HexDirectionChangeE? AttackerSecondMove => attackerSecondMove;
    public HexTools.HexDirectionChangeE? DefenderFirstMove => defenderFirstMove;
    public HexTools.HexDirectionChangeE? DefenderSecondMove => defenderSecondMove;

    public enum CombatStateE
    {
        NO_COMBAT,
        AWAITING_MOVES, //nie przewiduje mozliwosci anulowania, bo to by oglupialo przeciwnika
        AUTOFILING_MOVES,
        REVEALING_MOVES,
        AWAITING_DEFENDER_CARD,
        AWAITING_ATTACKER_CARD,
        DOGFIGHT,
        FINISHED
    }

    public enum CombatRoleE
    {
        ATTACKER,
        DEFENDER
    }

    private Dictionary<CombatStateE, float> StateTimeouts = new Dictionary<CombatStateE, float>
    {
        { CombatStateE.AUTOFILING_MOVES, 2.0f },
        { CombatStateE.AWAITING_MOVES, 15.0f },
        { CombatStateE.AWAITING_DEFENDER_CARD, 10.0f },
        { CombatStateE.AWAITING_ATTACKER_CARD, 10.0f }
    };

    public void Initialize(UnitManager.Unit _ownUnit, UnitManager.Unit _enemyUnit)
    {
        //dane
        ownUnit = _ownUnit;
        ownUnitBackup = new UnitManager.Unit(_ownUnit);
        enemyUnit = _enemyUnit;
        enemyUnitBackup = new UnitManager.Unit(_enemyUnit);

        //stan poczatkowy
        ChangeCombatState(CombatStateE.AWAITING_MOVES);
    }

    public void Cancel()
    {
        if (defenderFirstMove != null)
        {
            return;
        }

        //stan
        state = CombatStateE.NO_COMBAT;

        //dane - czyszczenie wszystkich danych
        Reset();

        //ukrycie interfejsow
        //chamskie bez sprawdzenia czy sa, ale powinny byc odporne na to - do refaktoryzacji?
        scene.CombatOverlay.HideCombatOverlayPanel();
        scene.CombatOverlay.HideArrows();
        scene.CombatOverlay.HideEnemyHighlight();
        scene.CombatOverlay.HideOwnHighlight();
        //dodac ukrywanie interfejsow przeciwnika
    }




    public void SetMove(HexTools.HexDirectionChangeE direction, CombatRoleE combatRole)
    {
        if (combatRole == CombatRoleE.ATTACKER)
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
        static HexDirectionChangeE RandomDir()
        {
            var values = Enum.GetValues(typeof(HexDirectionChangeE));
            return (HexDirectionChangeE)values.GetValue(UnityEngine.Random.Range(0, values.Length));
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
        static HexDirectionChangeE RandomDir()
        {
            var values = Enum.GetValues(typeof(HexDirectionChangeE));
            return (HexDirectionChangeE)values.GetValue(UnityEngine.Random.Range(0, values.Length));
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
        scene.CombatOverlay.HideOwnHighlight();
        scene.CombatOverlay.HideEnemyHighlight();



        //attacker 1
        scene.CombatOverlay.HideArrow(1);
        UnitManager.Unit unitToSwap = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(ownUnit.HexCoords, ownUnit.Direction)));
        if (unitToSwap != null)
        {
            scene.Unit.BeMoved(unitToSwap, ownUnit.HexCoords);
            unitToSwap = null;
        }
        if (attackerFirstMove == HexTools.HexDirectionChangeE.NA)
        {
            yield return scene.Unit.MoveForward(ownUnit);
        }
        else if (attackerFirstMove == HexTools.HexDirectionChangeE.TO_LEFT)
        {
            yield return scene.Unit.MoveLeft(ownUnit);
        }
        else
        {
            yield return scene.Unit.MoveRight(ownUnit);
        }
        ownUnit.MovesThisTurn--;

        //defender 1
        unitToSwap = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(enemyUnit.HexCoords, enemyUnit.Direction)));
        if (unitToSwap != null)
        {
            scene.Unit.BeMoved(unitToSwap, enemyUnit.HexCoords);
            unitToSwap = null;
        }
        if (defenderFirstMove == HexTools.HexDirectionChangeE.NA)
        {
            yield return scene.Unit.MoveForward(enemyUnit);
        }
        else if (defenderFirstMove == HexTools.HexDirectionChangeE.TO_LEFT)
        {
            yield return scene.Unit.MoveLeft(enemyUnit);
        }
        else
        {
            yield return scene.Unit.MoveRight(enemyUnit);
        }
        enemyUnit.MovesThisTurn--;

        //attacker 2
        scene.CombatOverlay.HideArrow(2);
        unitToSwap = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(ownUnit.HexCoords, ownUnit.Direction)));
        if (unitToSwap != null)
        {
            scene.Unit.BeMoved(unitToSwap, ownUnit.HexCoords);
            unitToSwap = null;
        }        
        if (attackerSecondMove == HexTools.HexDirectionChangeE.NA)
        {
            yield return scene.Unit.MoveForward(ownUnit);
        }
        else if (attackerSecondMove == HexTools.HexDirectionChangeE.TO_LEFT)
        {
            yield return scene.Unit.MoveLeft(ownUnit);
        }
        else
        {
            yield return scene.Unit.MoveRight(ownUnit);
        }
        ownUnit.MovesThisTurn--;

        //defender 2
        unitToSwap = scene.Unit.UnitList.FirstOrDefault(u => u.HexCoords.Equals(HexTools.Neighbor(enemyUnit.HexCoords, enemyUnit.Direction)));
        if (unitToSwap != null)
        {
            scene.Unit.BeMoved(unitToSwap, enemyUnit.HexCoords);
            unitToSwap = null;
        }
        if (defenderSecondMove == HexTools.HexDirectionChangeE.NA)
        {
            yield return scene.Unit.MoveForward(enemyUnit);
        }
        else if (defenderSecondMove == HexTools.HexDirectionChangeE.TO_LEFT)
        {
            yield return scene.Unit.MoveLeft(enemyUnit);
        }
        else
        {
            yield return scene.Unit.MoveRight(enemyUnit);
        }
        enemyUnit.MovesThisTurn--;

        moveSequenceIsDone = true;
    }

    UnitStatsData.InteractionAreaTypeE? InteractionZone(HexCoords unitHex, HexTools.HexDirectionPT unitDirection, UnitManager.UnitType unitType, HexCoords banditHex)
    {
        UnitStatsData.InteractionAreaTypeE? result = null;
       
        foreach (InteractionAreaTypeE areaType in Enum.GetValues(typeof(InteractionAreaTypeE)))
        {
            List<HexCoords> areaHexes = UnitStatsData.InteractionArea(unitHex, unitDirection, unitType, areaType);
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
        ChangeCombatState(CombatStateE.NO_COMBAT);


        ownUnit = null;
        ownUnitBackup = null;
        enemyUnit = null;
        enemyUnitBackup = null;

        attackerCanDie = false;
        defenderCanDie = false;
        attackerDiceCount = 0;
        defenderDiceCount = 0;

        moveSequenceIsDone = false;

        //private float stateEnterTime ??

        attackerFirstMove = null;
        attackerSecondMove = null;
        defenderFirstMove = null;
        defenderSecondMove = null;

        attackerCanDie = false;
        defenderCanDie = false;
        attackerDiceCount = 0;
        defenderDiceCount = 0;
        moveSequenceIsDone = false;


    }



    private void OnStateEnter(CombatStateE entering)
    {
        switch (state)
        {
            case CombatStateE.AWAITING_MOVES:
                //interfejsy
                scene.CombatOverlay.ShowCombatOverlayPanel();
                scene.CombatOverlay.DisplayArrows(ownUnit.HexCoords, ownUnit.Direction);
                scene.CombatOverlay.DisplayOwnHighlight(ownUnit.HexCoords);
                scene.CombatOverlay.DisplayEnemyHighlight(enemyUnit.HexCoords);
                //pokazac iterfejsy obroncy
                //tymczasowy brudny test
                TempshitTestSetEnemyMoves();
                break;
            case CombatStateE.AUTOFILING_MOVES:
                //tempshit do testow, docelowo to przyjdzie od klienta drugiego gracza, z nieistniejacego jeszcze interfejsu
                AutofillMoves();
                scene.CombatOverlay.HideArrows();
                break;
            case CombatStateE.REVEALING_MOVES:
                //tu odpalic MoveSequence
                StartCoroutine(MoveSequence());
                break;
            case CombatStateE.DOGFIGHT:
                //sprawdzic czy ktorys wylecial za mape
                if (!scene.Terrain.TerrainHexCoordsList.Contains(ownUnit.HexCoords))
                {
                    scene.Unit.DestroyUnit(ownUnit);
                    ChangeCombatState(CombatStateE.FINISHED);
                    break;
                }
                if (!scene.Terrain.TerrainHexCoordsList.Contains(enemyUnit.HexCoords))
                {
                    scene.Unit.DestroyUnit(enemyUnit);
                    ChangeCombatState(CombatStateE.FINISHED);
                    break;
                }

                //sprawdzic odleglosc, skonczyc jesli > 3
                if (HexTools.HexDistance(ownUnit.HexCoords, enemyUnit.HexCoords) > 3)
                {
                    ChangeCombatState(CombatStateE.FINISHED);
                    break;
                }


                UnitStatsData.InteractionAreaTypeE? attackerInteraction = InteractionZone(ownUnit.HexCoords, ownUnit.Direction, ownUnit.UnitType, enemyUnit.HexCoords);
                UnitStatsData.InteractionAreaTypeE? defenderInteraction = InteractionZone(enemyUnit.HexCoords, enemyUnit.Direction, enemyUnit.UnitType, ownUnit.HexCoords);
                if (attackerInteraction == null || defenderInteraction == null)
                {
                    throw new Exception("Nie znaleziono interakcji");
                }
                switch (attackerInteraction)
                {
                    case InteractionAreaTypeE.FIRE_3:
                        defenderCanDie = true;
                        attackerDiceCount = 3;
                        break;
                    case InteractionAreaTypeE.FIRE_2:
                        defenderCanDie = true;
                        attackerDiceCount = 2;
                        break;
                    case InteractionAreaTypeE.DODGE_2:
                        defenderCanDie = false;
                        attackerDiceCount = 2;
                        break;
                    case InteractionAreaTypeE.DODGE_1:
                        defenderCanDie = false;
                        attackerDiceCount = 1;
                        break;
                }
                switch (defenderInteraction)
                {
                    case InteractionAreaTypeE.FIRE_3:
                        attackerCanDie = true;
                        defenderDiceCount = 3;
                        break;
                    case InteractionAreaTypeE.FIRE_2:
                        attackerCanDie = true;
                        defenderDiceCount = 2;
                        break;
                    case InteractionAreaTypeE.DODGE_2:
                        attackerCanDie = false;
                        defenderDiceCount = 2;
                        break;
                    case InteractionAreaTypeE.DODGE_1:
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


                Debug.Log("Dogfight! Interakcja napastnika: " + attackerInteraction + ", zaatakowanego: " + defenderInteraction + ". Kosci/karty napastnika: "
                    + string.Join(", ", attackerDices) + ". Kosci/karty zaatakowanego: " + string.Join(", ", defenderDices) + ".");

                if (attackerDices.Max() > defenderDices.Max() && defenderCanDie)
                {
                    //smierc zaatakowanego
                    scene.Unit.DestroyUnit(enemyUnit);
                    Debug.Log("Zaatakowany zostal zestrzelony!");
                }
                else
                if (defenderDices.Max() > attackerDices.Max() && attackerCanDie)
                {
                    //smierc napastnika
                    scene.Unit.DestroyUnit(ownUnit);
                    Debug.Log("Napastnik zostal zestrzelony!");
                }
                else
                {
                    Debug.Log("Nikt nie zostal zestrzelony.");
                }


                ChangeCombatState(CombatStateE.FINISHED);
                break;

            case CombatStateE.FINISHED:
                Reset();
                scene.Orchestrator.ServiceCombatFinish();
                break;


            default: //dla NO_COMBAT itp.
                break;
        }
    }

    private void OnStateExit(CombatStateE exiting)
    {

    }

    private void ChangeCombatState(CombatStateE newState)
    {
        Debug.Log("Combat state: " + state + " -> " + newState);

        OnStateExit(state);
        state = newState;
        stateEnterTime = Time.time;
        OnStateEnter(state);
    }




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scene = SceneManagerReferences.Instance;
    }

    

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case CombatStateE.AWAITING_MOVES:
                if (defenderFirstMove != null && defenderSecondMove != null && attackerFirstMove != null && attackerSecondMove != null)
                {
                    ChangeCombatState(CombatStateE.REVEALING_MOVES);
                }
                else if (Time.time - stateEnterTime > StateTimeouts[CombatStateE.AWAITING_MOVES])
                {
                    //automatycznie ustawic ruchy
                    ChangeCombatState(CombatStateE.AUTOFILING_MOVES);
                }
                break;
            case CombatStateE.AUTOFILING_MOVES:
                if (Time.time - stateEnterTime > StateTimeouts[CombatStateE.AUTOFILING_MOVES])
                {
                    //automatycznie ustawic ruchy; ma³a pauza i isc dalej
                    ChangeCombatState(CombatStateE.REVEALING_MOVES);
                }
                break;
            case CombatStateE.REVEALING_MOVES:
                if (moveSequenceIsDone)
                {
                    ChangeCombatState(CombatStateE.DOGFIGHT);
                }
                




                break;
        }

    }
}
