using UnityEngine;

public class CombatManager : MonoBehaviour
{
    private SceneManagerReferences scene;


    private UnitManager.Unit ownUnit;
    private UnitManager.Unit enemyUnit;
    private UnitManager.Unit ownUnitBackup;
    private UnitManager.Unit enemyUnitBackup;
    private CombatStateE combatState;


    public enum CombatStateE
    {
        NO_COMBAT,
        INITIALIZED,
        AWAITING_OWN_CARD,
        AWAITING_ENEMY_CARD,
        DOGFIGHT        //animacje kart-kosci i ostrzalu
    }

    public void Initialize(UnitManager.Unit _ownUnit, UnitManager.Unit _enemyUnit)
    {
        //stan
        combatState = CombatStateE.INITIALIZED;

        //dane
        ownUnit = _ownUnit;
        ownUnitBackup = new UnitManager.Unit(_ownUnit);
        enemyUnit = _enemyUnit;
        enemyUnitBackup = new UnitManager.Unit(_enemyUnit);

        //interfejsy
        scene.CombatOverlay.ShowCombatOverlayPanel();
        scene.CombatOverlay.DisplayArrows(ownUnit.HexCoords, ownUnit.Direction);
        scene.CombatOverlay.DisplayOwnHighlight(ownUnit.HexCoords);
        scene.CombatOverlay.DisplayEnemyHighlight(enemyUnit.HexCoords);
    }

    public void Cancel()
    {
        //stan
        combatState = CombatStateE.NO_COMBAT;

        //dane - czyszczenie wszystkich danych
        ownUnit = null;
        ownUnitBackup = null;
        enemyUnit = null;
        enemyUnitBackup = null;

        //ukrycie interfejsow
        //chamskie bez sprawdzenia czy sa, ale powinny byc odporne na to - do refaktoryzacji?
        scene.CombatOverlay.HideCombatOverlayPanel();
        scene.CombatOverlay.HideArrows();
        scene.CombatOverlay.HideEnemyHighlight();
        scene.CombatOverlay.HideOwnHighlight();
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
