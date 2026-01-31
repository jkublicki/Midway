using Unity.VisualScripting;
using UnityEngine;

public class OverlayElement : MonoBehaviour, IClickable
{


    public HexCoords? Hex;

    public void OnClick()
    {
        Orchestrator.Instance.ServiceOverlayClick(this.gameObject.tag, Hex);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
