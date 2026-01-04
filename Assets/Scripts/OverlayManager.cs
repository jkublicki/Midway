using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class OverlayManager : MonoBehaviour
{
    public GameObject OverlayElementsRoot;

    public GameObject ActiveUnitOverlayPrefab;
    public GameObject TargetOverlayPrefab;

    public bool IsOverlayVisible = false;
    public HexCoords ActiveHex; 
    public HexTools.HexDirectionPT ActiveDirection;
    public List<HexCoords> TargetHexes;

    


    public void DisplayOverlay(HexCoords activeHex, HexTools.HexDirectionPT activeDirection, List<HexCoords> targets)
    {
        float yRotation = HexTools.HexDirectionToRotation(activeDirection);
        Quaternion rotation = Quaternion.Euler(ActiveUnitOverlayPrefab.transform.eulerAngles.x, yRotation, ActiveUnitOverlayPrefab.transform.eulerAngles.z);
                
        Vector2 v2 = HexTools.HexCoordsToCart(activeHex);
        GameObject obj = Instantiate(ActiveUnitOverlayPrefab, new Vector3(v2.x, 0.0f, v2.y), rotation, OverlayElementsRoot.transform);
        obj.name = "ActiveHexOverlay_at_" + activeHex.q.ToString() + "_" + activeHex.r.ToString();        

        IsOverlayVisible = true;
        ActiveHex = activeHex;
        ActiveDirection = activeDirection;

        foreach (HexCoords targetHex in targets)
        {
            Vector2 v2t = HexTools.HexCoordsToCart(targetHex);
            GameObject tgt = Instantiate(TargetOverlayPrefab, new Vector3(v2t.x, 0.0f, v2t.y), TargetOverlayPrefab.transform.rotation, OverlayElementsRoot.transform);
            tgt.name = "TargetOverlay_at_" + targetHex.q.ToString() + "_" + targetHex.r.ToString();
            tgt.GetComponentInChildren<OverlayElement>().Hex = targetHex; //aby orkiestrator wiedzial, ktory target zostal klikniety
            TargetHexes.Add(targetHex);
        }

    }

    public void HideOverlay()
    {
        foreach (Transform child in OverlayElementsRoot.transform)
        {
            Destroy(child.gameObject);
        }

        IsOverlayVisible = false;
        TargetHexes.Clear();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TargetHexes = new List<HexCoords>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
