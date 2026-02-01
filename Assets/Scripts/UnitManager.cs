using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static RuleEngine;
using static UnitManager;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

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




    public GameObject UnitObjectsRoot;
    public GameObject UsFighterPrefab;
    public GameObject JpFighterPrefab;

    //unit state: fresh, done, can_shoot
    //unit manager state: animation_in_progress (moving, shooting), - to chyba juz raczej turn manager/game state manager








    public class Unit //https://x.com/i/grok?conversation=2005345584077517125
    {
        public Unit(HexCoords _hexCoords, UnitType _unitType, HexDirection _direction, string _id, Player _player, GameObject _gameObject = null) //GameObject jest opcjonalny
        {
            HexCoords = _hexCoords;
            GmObject = _gameObject;
            UnitType = _unitType;
            Direction = _direction;
            ID = _id;
            Player = _player;
            MovesThisTurn = 0;
            AttackedThisTurn = false;
        }

        //konstruktor kopiujacy
        public Unit(Unit original)
        {
            HexCoords = original.HexCoords; //struct sie skopiuje
            GmObject = original.GmObject; //kopiowanie referencji
            UnitType = original.UnitType;
            Direction = original.Direction;
            ID = original.ID;
            Player = original.Player;
            MovesThisTurn = original.MovesThisTurn;
            AttackedThisTurn = original.AttackedThisTurn;
        }

        public HexCoords HexCoords;
        public GameObject GmObject;
        public UnitType UnitType;
        public HexDirection Direction;
        public string ID;
        public Player Player;
        public int MovesThisTurn;
        public bool AttackedThisTurn;
    }


    private List<Unit> unitList = new List<Unit>();

    public IReadOnlyList<Unit> UnitList => unitList;



    private void SetInitialPositions() //tempshit?
    {
        unitList.Add(new Unit(new HexCoords(-2, 2), UnitType.UsFighter, HexDirection.East, "usf1", Player.PLAYER_1));
        unitList.Add(new Unit(new HexCoords(-2, 3), UnitType.UsFighter, HexDirection.NorthWest, "usf2", Player.PLAYER_1));
        unitList.Add(new Unit(new HexCoords(3, -1), UnitType.UsFighter, HexDirection.SouthWest, "usf3", Player.PLAYER_1));

        unitList.Add(new Unit(new HexCoords(1, 0), UnitType.JpFighter, HexDirection.SouthEast, "jpf1", Player.PLAYER_2));
        unitList.Add(new Unit(new HexCoords(2, -3), UnitType.JpFighter, HexDirection.SouthEast, "jpf2", Player.PLAYER_2));
        unitList.Add(new Unit(new HexCoords(0, -2), UnitType.JpFighter, HexDirection.SouthWest, "jpf3", Player.PLAYER_2));
    }

    private void SpawnUnitGameObjects()
    {
        const float planeAltitude = 0.5f;

        for (int i = 0; i < unitList.Count; i++)
        {
            Vector2 v2 = HexTools.HexCoordsToCart(unitList[i].HexCoords);

            GameObject prefab;

            switch (unitList[i].UnitType)
            {
                case UnitType.UsFighter:
                    prefab = UsFighterPrefab;
                    break;
                case UnitType.JpFighter:
                    prefab = JpFighterPrefab;
                    break;
                //case UnitType.UsBomber:
                //case UnitType.JpBomber:
                default:
                    prefab = UsFighterPrefab;
                    break;
            }
           
            float yRotation = HexTools.HexDirectionToRotation(unitList[i].Direction);
            Quaternion rotation = Quaternion.Euler(prefab.transform.eulerAngles.x, yRotation, prefab.transform.eulerAngles.z);

            GameObject obj = Instantiate(prefab, new Vector3(v2.x, planeAltitude, v2.y), rotation, UnitObjectsRoot.transform);
            obj.name = unitList[i].ID;

            unitList[i].GmObject = obj;
        }
    }

    public AnimationCurve MoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve MoveCurveRoll = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve MoveCurveHorizontalDeviaion = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve MoveCurveYaw = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve BeMovedCurveVerticalDeviaion = AnimationCurve.EaseInOut(0, 0, 1, 1);


    public float MoveDuration = 1.0f;

    

    //claude
    IEnumerator MoveObject(Transform obj, Vector3 from, Vector3 to, HexDirectionChange directionChange, bool IsMoved, System.Action onComplete = null) //opcjonalnie moze byc callback - kod do wykonania po
    {
        int direction = 0;
        int movedModifier = 0;

        switch (directionChange)
        {
            case HexDirectionChange.ToLeft:
                direction = 1;
                break;
            case HexDirectionChange.ToRight:
                direction = -1;
                break;
            case HexDirectionChange.NA:
                direction = 0;
                break;
            default:
                direction = 0;
                break;
        }

        if (IsMoved)
        {
            direction = 0;
            movedModifier = 1;
        }


        //direction: 1 w lewo, 0 prosto, -1 w prawo
        float maxDeviation = 0.1f;
        float maxRoll = 15.0f;

        float elapsed = 0f;

        Vector3 rot0 = obj.transform.localRotation.eulerAngles;


        //wyznaczanie wektora prostopadlego do wektora ruchu
        // Calculate the perpendicular "right" direction relative to movement
        Vector3 moveDirection = (to - from).normalized;
        Vector3 sidewaysDirection = Vector3.Cross(moveDirection, Vector3.up).normalized; //+ w lewo, -w prawo
        Vector3 movedDirection = Vector3.up; //Vector3.Cross(moveDirection, Vector3.right).normalized;


        // If moving purely vertically, fall back to world right
        if (sidewaysDirection == Vector3.zero)
        {
            sidewaysDirection = Vector3.right;
        }



        while (elapsed < MoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / MoveDuration;
            float moveCurveValue = MoveCurve.Evaluate(t);
            float rollCurveValue = MoveCurveRoll.Evaluate(t);
            float yawCurveValue = MoveCurveYaw.Evaluate(t);
            float horizontalDeviationCurveValue = MoveCurveHorizontalDeviaion.Evaluate(t);
            float verticalDeviationCurveValue = BeMovedCurveVerticalDeviaion.Evaluate(t);

            //glowne przesuniecie
            Vector3 basePos = Vector3.Lerp(from, to, moveCurveValue);


            //odchylenie polozenia w prawo 
            //jw w dol
            obj.transform.localPosition = basePos - direction * sidewaysDirection * maxDeviation * horizontalDeviationCurveValue + movedModifier * movedDirection * 0.37f * verticalDeviationCurveValue;


            //przechylenie / roll
            //zmiana kierunku / yaw
            obj.transform.rotation = Quaternion.Euler(new Vector3(rot0.x - direction * maxRoll * rollCurveValue, rot0.y - direction * 60.0f * yawCurveValue, rot0.z));


            yield return null;
        }

        obj.position = to; // Ensure final position

        if (onComplete != null) 
        {
            onComplete.Invoke(); //mogloby byæ onComplete(), ale onComplete.Invoke() robi dokladnie to samo, ale wyraznie pokazuje, ze jest wyknywany delegat
        }

    }

    public Coroutine MoveForward(Unit unit)
    {
        //ustaliæ docelowy hex
        HexCoords targetHex = HexTools.Neighbor(unit.HexCoords, unit.Direction);

        //ustalic docelow¹ pozycjê
        Vector3 targetPosition = new Vector3(HexTools.HexCoordsToCart(targetHex).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(targetHex).y);


        BattleSceneState.Instance.UnitMovementInProgress = true;
        Coroutine coroutine = StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, targetPosition, HexDirectionChange.NA, false, () =>
        {
            BattleSceneState.Instance.UnitMovementInProgress = false;
            unit.MovesThisTurn++;
            unit.HexCoords = targetHex;
        }        
        ));

        return coroutine;
    }

    public Coroutine MoveLeft(Unit unit)
    {
        //ustaliæ docelowy hex
        HexCoords targetHex = HexTools.Neighbor(unit.HexCoords, unit.Direction);

        //ustalic docelow¹ pozycjê
        Vector3 targetPosition = new Vector3(HexTools.HexCoordsToCart(targetHex).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(targetHex).y);


        BattleSceneState.Instance.UnitMovementInProgress = true;
        Coroutine coroutine = StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, targetPosition, HexDirectionChange.ToLeft, false, () =>
        {
            BattleSceneState.Instance.UnitMovementInProgress = false;
            unit.MovesThisTurn++;
            unit.HexCoords = targetHex;
            //dodaæ zmianê direction
            unit.Direction = HexTools.AdjacentDirection(unit.Direction, HexDirectionChange.ToLeft);
        }
        ));

        return coroutine;
    }

    public Coroutine MoveRight(Unit unit)
    {
        //ustaliæ docelowy hex
        HexCoords targetHex = HexTools.Neighbor(unit.HexCoords, unit.Direction);

        //ustalic docelow¹ pozycjê
        Vector3 targetPosition = new Vector3(HexTools.HexCoordsToCart(targetHex).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(targetHex).y);


        BattleSceneState.Instance.UnitMovementInProgress = true;
        Coroutine coroutine = StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, targetPosition, HexDirectionChange.ToRight, false, () =>
        {
            BattleSceneState.Instance.UnitMovementInProgress = false;
            unit.MovesThisTurn++;
            unit.HexCoords = targetHex;
            //dodaæ zmianê direction
            unit.Direction = HexTools.AdjacentDirection(unit.Direction, HexDirectionChange.ToRight);
        }
        ));

        return coroutine;
    }


    public void BeMoved(Unit unit, HexCoords destination)
    {
        Vector3 destinationPosition = new Vector3(HexTools.HexCoordsToCart(destination).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(destination).y);

        BattleSceneState.Instance.UnitMovementInProgress = true;
        StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, destinationPosition, HexDirectionChange.NA, true, () =>
        {
            BattleSceneState.Instance.UnitMovementInProgress = false;
            unit.HexCoords = destination;
        }
        ));


    }

    public void OnTurnStart(Player player)
    {
        foreach (Unit u in unitList.Where(u => u.Player == player))
        {
            u.MovesThisTurn = 0;
            u.AttackedThisTurn = false;
        }
    }   

    public void DestroyUnit(Unit unit)
    {
        Destroy(unit.GmObject);
        unitList.Remove(unit);
    }


    
    void Start()
    {


        SetInitialPositions();
        SpawnUnitGameObjects();

    }

    
    void Update()
    {
        
    }
}
