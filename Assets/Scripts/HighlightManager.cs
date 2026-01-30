using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//zakladam, ze overlay jest odrebny od terenu, aby nie bylo stanow terenu - teren ma powstac i byc i tyle


public class HighlightManager : MonoBehaviour
{
    public static HighlightManager Instance {get; private set;}

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


    //[SerializeField] private SceneManagerReferences scene;
    private SceneManagerReferences scene;

    public GameObject HighlightObjectsRoot;
    public GameObject HighlightPrefab;

    public List<HexCoords> OverlayHexCoordsList = new List<HexCoords>(); //do przerbienia, powinno byæ tylko do odczytu; nie wolno zmieniac innym skryptem

    public void AddOverlay(List<HexCoords> coords)
    {

        foreach (HexCoords hex in coords)
        {
            //pomin jezeli poza terenem, pomin jesli juz istnieje
            if (!TerrainManager.Instance.TerrainHexCoordsList.Contains(hex) 
                || OverlayHexCoordsList.Contains(hex))
            {
                continue;
            }


            Vector2 v2 = HexTools.HexCoordsToCart(hex);
            GameObject obj = Instantiate(HighlightPrefab, new Vector3(v2.x, 0.0f, v2.y), HighlightPrefab.transform.rotation, HighlightObjectsRoot.transform);



            obj.name = "HexHighlight_at_" + hex.q.ToString() + "_" + hex.r.ToString();


            OverlayHexCoordsList.Add(hex);
        }
    }

    public void DeleteOverlay()
    {
        OverlayHexCoordsList.Clear();
        
        foreach (Transform child in HighlightObjectsRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scene = SceneManagerReferences.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
