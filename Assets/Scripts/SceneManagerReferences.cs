using UnityEngine;

//architektura: https://chatgpt.com/c/6950c53a-2aa0-8330-9f6d-34a7d9e9c50b
//mo¿e kiedyœ do przerobienia - zastapienia przez to, ze wszystkie managery beda singletonami
public class SceneManagerReferences : MonoBehaviour
{
    //to jest singelton
    //nie trzeba go dociagac w inspektorze skryptowi, ktora go potrzebuje
    //zamiast tego uzywa sie tak: SceneManagerReferences.Instance.Terrain.JakasMetoda()
    public static SceneManagerReferences Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }



    public TerrainManager Terrain;
    public UnitManager Unit;
    public OverlayManager Overlay;
    public HighlightManager Highlight;
    public Orchestrator Orchestrator;
    public SceneState SceneState;
    public CombatOverlayManager CombatOverlay;
    public CameraManager CameraManager;
    public CombatManager CombatManager;

}
