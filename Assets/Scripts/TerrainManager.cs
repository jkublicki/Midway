using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }

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




    public GameObject TerrainObjectsRoot;
    public GameObject HexPrefab;

    private List<HexCoords> terrainHexCoordsList = new List<HexCoords>(); 
    public IReadOnlyList<HexCoords> TerrainHexCoordsList => terrainHexCoordsList;

    private void CreateTerrain()
    {
        HexCoords center = new HexCoords(0, 0);
        List<HexCoords> hexList = new List<HexCoords>();
        HexTools.HexNeighbors(center, 3, false, hexList);

        foreach (HexCoords hex in hexList)
        {
            
            Vector2 v2 = HexTools.HexCoordsToCart(hex);
            GameObject obj = Instantiate(HexPrefab, new Vector3(v2.x, 0.0f, v2.y), HexPrefab.transform.rotation, TerrainObjectsRoot.transform);

            

            obj.name = "Hex_at_" + hex.q.ToString() + "_" + hex.r.ToString();


            terrainHexCoordsList.Add(hex);
        }
    }

    
    void Start()
    {
        CreateTerrain();
    }

    
    void Update()
    {
        
    }
}
