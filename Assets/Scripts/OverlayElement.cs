using Unity.VisualScripting;
using UnityEngine;

public class OverlayElement : MonoBehaviour, IClickable
{


    public HexCoords? Hex;

    public void OnClick()
    {
        Orchestrator.Instance.ServiceOverlayClick(this.gameObject.tag, Hex);
    }

    
    void Start()
    {

    }

    
    void Update()
    {

    }
}
