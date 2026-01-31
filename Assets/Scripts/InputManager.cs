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

    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !SceneState.Instance.UnitMovementInProgress)
        {
            HandleClick();
        }


        //proste interakcje obs³u¿one w prosty sposob
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            CameraManager.Instance.Pan(CardinalDirection.Left);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            CameraManager.Instance.Pan(CardinalDirection.Right);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            CameraManager.Instance.Pan(CardinalDirection.Up);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            CameraManager.Instance.Pan(CardinalDirection.Down);
        }


    }

    private void HandleClick()
    {

        //PANELE UI
        // First, check for Canvas UI hits (blocks further raycasts if over UI)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            

            // SprawdŸ co zosta³o klikniête
            GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
            if (clickedObject != null)
            {
                IClickable clickable = clickedObject.GetComponent<IClickable>();
                if (clickable != null)
                {
                    Debug.Log("IM: Clickable canvas UI clicked");
                    clickable.OnClick();
                }
                else
                {
                    Debug.Log("IM: Non-clickable canvas UI clicked");
                }
            }
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
            Orchestrator.Instance.ServiceHexPlaneClick(hexClicked);
        }
    }
}