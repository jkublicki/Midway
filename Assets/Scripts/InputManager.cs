using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

//to jest jedyna klasa w calym projekcie, ktora moze przyjmowac input - klikniecia itp

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask uiLayer;
    [SerializeField] private LayerMask hexLayer;
    [SerializeField] private Camera mainCamera;
    //public SceneManagerReferences scene;
    private SceneManagerReferences scene;

    private void Start()
    {
        scene = SceneManagerReferences.Instance;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !scene.SceneState.UnitMovementInProgress)
        {
            HandleClick();
        }


        //proste interakcje obs³u¿one w prosty sposob
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            scene.CameraManager.Pan(CameraManager.PanDirectionE.Left);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            scene.CameraManager.Pan(CameraManager.PanDirectionE.Right);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            scene.CameraManager.Pan(CameraManager.PanDirectionE.Up);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            scene.CameraManager.Pan(CameraManager.PanDirectionE.Down);
        }


    }

    private void HandleClick()
    {

        //PANELE UI
        // First, check for Canvas UI hits (blocks further raycasts if over UI)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("IM: Canvas UI clicked");
            return;
        }









        //POZOSTALE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check UI first
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, uiLayer))
        {
                    Debug.Log("IM: UI clicked");


            IClickable clickableUi = hit.collider.GetComponent<IClickable>();
            clickableUi.OnClick();
            return;
        }

        // Check hexes/water
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, hexLayer))
        {
                    Debug.Log("IM: hex plane clicked");


            HexCoords hexClicked = HexTools.CartToHexCoords(new Vector2(hit.point.x, hit.point.z));
            scene.Orchestrator.ServiceHexPlaneClick(hexClicked);
        }
    }
}