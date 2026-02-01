using UnityEngine;

public class BattleSceneState : MonoBehaviour
{
    public static BattleSceneState Instance { get; private set; }


    //public Player? LocalPlayer = null;



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








    public bool UnitMovementInProgress = false;


    private Player activePlayer;
    public Player ActivePlayer => activePlayer;


    public void EndTurn()
    {
        if (activePlayer == Player.PLAYER_1)
        {
            activePlayer = Player.PLAYER_2;
        }
        else
        {
            activePlayer = Player.PLAYER_1;
        }

        UnitManager.Instance.OnTurnStart(activePlayer);
    }

    void Start()
    {
        activePlayer = Player.PLAYER_1;
    }

    
    void Update()
    {
        
    }
}
