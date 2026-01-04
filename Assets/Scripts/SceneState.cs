using UnityEngine;

public class SceneState : MonoBehaviour
{
    [SerializeField]
    private SceneManagerReferences scene;



    public enum PlayerE
    {
        PLAYER_1,
        PLAYER_2
    }

    public bool UnitMovementInProgress = false;


    private PlayerE activePlayer;
    public PlayerE ActivePlayer => activePlayer;


    public void EndTurn()
    {

        if (activePlayer == PlayerE.PLAYER_1)
        {
            activePlayer = PlayerE.PLAYER_2;
        }
        else
        {
            activePlayer = PlayerE.PLAYER_1;
        }


        scene.Unit.OnTurnStart(activePlayer);
    }

    void Start()
    {
        activePlayer = PlayerE.PLAYER_1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
