using UnityEngine;

public class SceneState : MonoBehaviour
{
    public static SceneState Instance { get; private set; }

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






    public enum Player
    {
        PLAYER_1,
        PLAYER_2
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
