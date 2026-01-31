

using UnityEngine;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    [SerializeField]
    private Camera camera;

    public float MaxDeviationHorizontal = 0.0f;
    public float MaxDeviationVertical = 0.0f;
    public float PanSpeed = 0.0f;

    private float deviationHorizontal = 0;
    private float deviationVertical = 0;
    private Vector3 cameraDefaultPositon;


    public void Pan(CardinalDirection direction)
    {
        float d = PanSpeed * Time.deltaTime;


        Vector3 origin = camera.transform.position;
        


        //kiedyœ poprawic: niech nie akumuluja sie bledy i niech bedzie kompatybilne z dowolnym ustawieniem kamery
        switch (direction)
        {
            case CardinalDirection.Right:                
                if (deviationHorizontal < -MaxDeviationHorizontal)
                {
                    return;
                }
                camera.transform.position = new Vector3(origin.x + d, origin.y, origin.z - d);
                deviationHorizontal -= d;                
                break;
            case CardinalDirection.Left:
                if (deviationHorizontal > MaxDeviationHorizontal)
                {
                    return;
                }
                camera.transform.position = new Vector3(origin.x - d, origin.y, origin.z + d);
                deviationHorizontal += d;
                break;
            case CardinalDirection.Down:
                if (deviationVertical > MaxDeviationVertical)
                {
                    return;
                }
                camera.transform.position = new Vector3(origin.x - d, origin.y, origin.z - d);
                deviationVertical += d;
                break;
            case CardinalDirection.Up:
                if (deviationVertical < -MaxDeviationVertical)
                {
                    return;
                }
                camera.transform.position = new Vector3(origin.x + d, origin.y, origin.z + d);
                deviationVertical -= d;
                break;
        }


        


    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraDefaultPositon = camera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
