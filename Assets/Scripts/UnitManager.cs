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
    SceneManagerReferences scene;

    public GameObject UnitObjectsRoot;
    public GameObject UsFighterPrefab;
    public GameObject JpFighterPrefab;

    //unit state: fresh, done, can_shoot
    //unit manager state: animation_in_progress (moving, shooting), - to chyba juz raczej turn manager/game state manager

    public enum UnitType
    {
        US_FIGHTER,
        JP_FIGHTER,
        US_BOMBER,
        JP_BOMBER
    }






    public class Unit //https://x.com/i/grok?conversation=2005345584077517125
    {
        public Unit(HexCoords _hexCoords, UnitType _unitType, HexTools.HexDirectionPT _direction, string _id, SceneState.PlayerE _player, GameObject _gameObject = null) //GameObject jest opcjonalny
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
        public HexTools.HexDirectionPT Direction;
        public string ID;
        public SceneState.PlayerE Player;
        public int MovesThisTurn;
        public bool AttackedThisTurn;
    }


    private List<Unit> unitList = new List<Unit>();

    public IReadOnlyList<Unit> UnitList => unitList;



    private void SetInitialPositions() //tempshit?
    {
        unitList.Add(new Unit(new HexCoords(-3, 3), UnitType.US_FIGHTER, HexTools.HexDirectionPT.EAST, "usf1", SceneState.PlayerE.PLAYER_1));
        unitList.Add(new Unit(new HexCoords(-2, 3), UnitType.US_FIGHTER, HexTools.HexDirectionPT.NORTH_WEST, "usf2", SceneState.PlayerE.PLAYER_1));
        unitList.Add(new Unit(new HexCoords(3, -1), UnitType.US_FIGHTER, HexTools.HexDirectionPT.SOUTH_WEST, "usf3", SceneState.PlayerE.PLAYER_1));

        unitList.Add(new Unit(new HexCoords(1, 0), UnitType.JP_FIGHTER, HexTools.HexDirectionPT.SOUTH_EAST, "jpf1", SceneState.PlayerE.PLAYER_2));
        unitList.Add(new Unit(new HexCoords(2, -3), UnitType.JP_FIGHTER, HexTools.HexDirectionPT.SOUTH_EAST, "jpf2", SceneState.PlayerE.PLAYER_2));
        unitList.Add(new Unit(new HexCoords(0, -2), UnitType.JP_FIGHTER, HexTools.HexDirectionPT.SOUTH_WEST, "jpf3", SceneState.PlayerE.PLAYER_2));
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
                case UnitType.US_FIGHTER:
                    prefab = UsFighterPrefab;
                    break;
                case UnitType.JP_FIGHTER:
                    prefab = JpFighterPrefab;
                    break;
                //case UnitType.US_BOMBER:
                //case UnitType.JP_BOMBER:
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
    IEnumerator MoveObject(Transform obj, Vector3 from, Vector3 to, HexTools.HexDirectionChangeE directionChange, bool IsMoved, System.Action onComplete = null) //opcjonalnie moze byc callback - kod do wykonania po
    {
        int direction = 0;
        int movedModifier = 0;

        switch (directionChange)
        {
            case HexTools.HexDirectionChangeE.TO_LEFT:
                direction = 1;
                break;
            case HexTools.HexDirectionChangeE.TO_RIGHT:
                direction = -1;
                break;
            case HexTools.HexDirectionChangeE.NA:
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

    public void MoveForward(Unit unit)
    {
        //ustaliæ docelowy hex
        HexCoords targetHex = HexTools.Neighbor(unit.HexCoords, unit.Direction);

        //ustalic docelow¹ pozycjê
        Vector3 targetPosition = new Vector3(HexTools.HexCoordsToCart(targetHex).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(targetHex).y);


        scene.SceneState.UnitMovementInProgress = true;
        StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, targetPosition, HexTools.HexDirectionChangeE.NA, false, () =>
        {
            scene.SceneState.UnitMovementInProgress = false;
            unit.MovesThisTurn++;
            unit.HexCoords = targetHex;
        }        
        ));
    }

    public void MoveLeft(Unit unit)
    {
        //ustaliæ docelowy hex
        HexCoords targetHex = HexTools.Neighbor(unit.HexCoords, unit.Direction);

        //ustalic docelow¹ pozycjê
        Vector3 targetPosition = new Vector3(HexTools.HexCoordsToCart(targetHex).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(targetHex).y);


        scene.SceneState.UnitMovementInProgress = true;
        StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, targetPosition, HexTools.HexDirectionChangeE.TO_LEFT, false, () =>
        {
            scene.SceneState.UnitMovementInProgress = false;
            unit.MovesThisTurn++;
            unit.HexCoords = targetHex;
            //dodaæ zmianê direction
            unit.Direction = HexTools.AdjacentDirection(unit.Direction, HexTools.HexDirectionChangeE.TO_LEFT);
        }
        ));
    }

    public void MoveRight(Unit unit)
    {
        //ustaliæ docelowy hex
        HexCoords targetHex = HexTools.Neighbor(unit.HexCoords, unit.Direction);

        //ustalic docelow¹ pozycjê
        Vector3 targetPosition = new Vector3(HexTools.HexCoordsToCart(targetHex).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(targetHex).y);


        scene.SceneState.UnitMovementInProgress = true;
        StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, targetPosition, HexTools.HexDirectionChangeE.TO_RIGHT, false, () =>
        {
            scene.SceneState.UnitMovementInProgress = false;
            unit.MovesThisTurn++;
            unit.HexCoords = targetHex;
            //dodaæ zmianê direction
            unit.Direction = HexTools.AdjacentDirection(unit.Direction, HexTools.HexDirectionChangeE.TO_RIGHT);
        }
        ));
    }


    public void BeMoved(Unit unit, HexCoords destination)
    {
        Vector3 destinationPosition = new Vector3(HexTools.HexCoordsToCart(destination).x, unit.GmObject.transform.position.y, HexTools.HexCoordsToCart(destination).y);

        scene.SceneState.UnitMovementInProgress = true;
        StartCoroutine(MoveObject(unit.GmObject.transform, unit.GmObject.transform.position, destinationPosition, HexTools.HexDirectionChangeE.NA, true, () =>
        {
            scene.SceneState.UnitMovementInProgress = false;
            unit.HexCoords = destination;
        }
        ));


    }

    public void OnTurnStart(SceneState.PlayerE player)
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scene = SceneManagerReferences.Instance;

        SetInitialPositions();
        SpawnUnitGameObjects();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
