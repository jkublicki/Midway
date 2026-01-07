using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static RuleEngine;

public class CombatOverlayManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private SceneManagerReferences scene;

    [SerializeField] private GameObject combatOverlayPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject combatOverlayElementsRoot;
    [SerializeField] private GameObject ownHighlightPrefab;
    [SerializeField] private GameObject enemyHighlightPrefab;
    [SerializeField] private GameObject arrowsPrefab;
    [SerializeField] private GameObject arrowLeftPrefab;
    [SerializeField] private GameObject arrowForwardPrefab;
    [SerializeField] private GameObject arrowRightPrefab;



    public bool IsCombatOverlayPanelVisible = false;
    public HexCoords OwnUnitOverlayHex;
    public HexTools.HexDirectionPT OwnUnitOverlayDirection;
    public HexCoords EnemyUnitOverlayHex;

    private GameObject ownHighlight;
    private GameObject enemyHighlight;
    private GameObject arrows;
    private GameObject arrowFirstMove;
    private GameObject arrowSecondMove;



    private void HelperServiceCombatOverlayClick(HexTools.HexDirectionChangeE turn)
    {
        //zmiana combat overlay
        //ustawiam na wypadek wyczyszczenie tego w hide
        HexCoords ownHex = OwnUnitOverlayHex;
        HexTools.HexDirectionPT ownDirection = OwnUnitOverlayDirection;
        HideArrows();

        if (scene.CombatManager.AttackerSecondMove == null)
        {
            DisplayArrows(HexTools.Neighbor(ownHex, ownDirection), HexTools.AdjacentDirection(ownDirection, turn));
        }

        GameObject obj = DisplayArrow(ownHex, ownDirection, turn);

        //strzalka pokazujaca wybor
        if (scene.CombatManager.AttackerSecondMove == null)
        {
            arrowFirstMove = obj;
        }
        else
        {
            arrowSecondMove = obj;
        }
    }



    public void ServiceCombatOverlayClick(string tag)
    {
        Debug.Log("COM: " + tag + " clicked");

        switch (tag)
        {
            case "UiCoOvArrowLeft":
                //przekazanie decyzji uzytkownika
                scene.CombatManager.SetMove(HexTools.HexDirectionChangeE.TO_LEFT, CombatManager.CombatRoleE.ATTACKER);
                HelperServiceCombatOverlayClick(HexTools.HexDirectionChangeE.TO_LEFT);
                break;
            case "UiCoOvArrowForward":
                //jw.
                scene.CombatManager.SetMove(HexTools.HexDirectionChangeE.NA, CombatManager.CombatRoleE.ATTACKER);
                HelperServiceCombatOverlayClick(HexTools.HexDirectionChangeE.NA);
                break;
            case "UiCoOvArrowRight":
                scene.CombatManager.SetMove(HexTools.HexDirectionChangeE.TO_RIGHT, CombatManager.CombatRoleE.ATTACKER);
                HelperServiceCombatOverlayClick(HexTools.HexDirectionChangeE.TO_RIGHT);
                break;
            default:
                Debug.Log("Pusty lub nieprawidlowy tag");
                throw new ArgumentOutOfRangeException(nameof(tag), tag, "Nieobs³ugiwany tag");
        }

    }

    


    public void ShowCombatOverlayPanel()
    {
        combatOverlayPanel.SetActive(true);
        IsCombatOverlayPanelVisible = true;
    }

    public void HideCombatOverlayPanel()
    {
        combatOverlayPanel.SetActive(false);
        IsCombatOverlayPanelVisible = false;
    }

    public void DisplayArrows(HexCoords activeHex, HexTools.HexDirectionPT direction)
    {
        float yRotation = HexTools.HexDirectionToRotation(direction);
        Quaternion rotation = Quaternion.Euler(arrowsPrefab.transform.eulerAngles.x, yRotation, arrowsPrefab.transform.eulerAngles.z);

        Vector2 v2 = HexTools.HexCoordsToCart(activeHex);
        arrows = Instantiate(arrowsPrefab, new Vector3(v2.x, 0.0f, v2.y), rotation, combatOverlayElementsRoot.transform);
        arrows.name = "CombatArrows_at_" + activeHex.q.ToString() + "_" + activeHex.r.ToString();

        IsCombatOverlayPanelVisible = true;
        OwnUnitOverlayHex = activeHex;
        OwnUnitOverlayDirection = direction;
    }

    public GameObject DisplayArrow(HexCoords hex, HexTools.HexDirectionPT direction, HexTools.HexDirectionChangeE arrowType)
    {
        GameObject prefab = null;

        switch (arrowType)
        {
            case HexTools.HexDirectionChangeE.TO_LEFT:
                prefab = arrowLeftPrefab;
                break;
            case HexTools.HexDirectionChangeE.NA:
                prefab = arrowForwardPrefab; 
                break;
            case HexTools.HexDirectionChangeE.TO_RIGHT:
                prefab = arrowRightPrefab;
                break;
        }

        float yRotation = HexTools.HexDirectionToRotation(direction);
        Quaternion rotation = Quaternion.Euler(arrowsPrefab.transform.eulerAngles.x, yRotation, arrowsPrefab.transform.eulerAngles.z);

        Vector2 v2 = HexTools.HexCoordsToCart(hex);        

        GameObject obj = Instantiate(prefab, new Vector3(v2.x, 0.0f, v2.y), rotation, combatOverlayElementsRoot.transform);
        obj.name = "CombatArrow_at_" + hex.q.ToString() + "_" + hex.r.ToString();

        return obj;
    }

    public void HideArrow(int firstOrSecond)
    {
        if (firstOrSecond == 1 && arrowFirstMove != null)
        {
            Destroy(arrowFirstMove);
            arrowFirstMove = null;
            Debug.Log(Time.time.ToString() + ": destroying arrow 1");
        }
        else if (firstOrSecond == 2 && arrowSecondMove != null)
        {
            Destroy(arrowSecondMove);
            arrowSecondMove = null;
            Debug.Log(Time.time.ToString() + ": destroying arrow 2");
        }
    }


    public void HideArrows()
    {
        if (arrows != null)
        {
            Destroy(arrows);
            arrows = null; //tak sie robi, bo destroy jest na koniec ramki/klatki, co teoretycznie grozi odwolaniem do zniszczonego, ale jeszcze nie znulowanego obiektu; plus wskazuje intencje
        }
    }

    public void DisplayOwnHighlight(HexCoords activeHex)
    {
        Vector2 v2 = HexTools.HexCoordsToCart(activeHex);
        ownHighlight = Instantiate(ownHighlightPrefab, new Vector3(v2.x, 0.0f, v2.y), ownHighlightPrefab.transform.rotation, combatOverlayElementsRoot.transform);
        ownHighlight.name = "OwnHighlight_at_" + activeHex.q.ToString() + "_" + activeHex.r.ToString();
    }

    public void HideOwnHighlight()
    {
        if (ownHighlight != null)
        {
            Destroy(ownHighlight);
            ownHighlight = null; 
        }
    }


    public void DisplayEnemyHighlight(HexCoords enemyHex)
    {
        Vector2 v2 = HexTools.HexCoordsToCart(enemyHex);
        enemyHighlight = Instantiate(enemyHighlightPrefab, new Vector3(v2.x, 0.0f, v2.y), enemyHighlightPrefab.transform.rotation, combatOverlayElementsRoot.transform);
        enemyHighlight.name = "OwnHighlight_at_" + enemyHex.q.ToString() + "_" + enemyHex.r.ToString();
    }

    public void HideEnemyHighlight()
    {
        if (enemyHighlight != null)
        {
            Destroy(enemyHighlight);
            enemyHighlight = null;
        }
    }


    void Start()
    {
        scene = SceneManagerReferences.Instance;
        closeButton.onClick.AddListener(HideCombatOverlayPanel); //tempshit, docelowo tu ma byc przekazanie informacji o cancl do managera stanow walki, a on pozleca schowanie roznych menu przy cancel
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
