using Unity.VisualScripting;
using UnityEngine;

public class OverlayElement : MonoBehaviour, IClickable
{
    private SceneManagerReferences scene;

    public HexCoords? Hex;

    public void OnClick()
    {
        scene.Orchestrator.ServiceOverlayClick(this.gameObject.tag, Hex);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scene = SceneManagerReferences.Instance; //tu ustawiac, inaczej moze nie istniec
    }

    // Update is called once per frame
    void Update()
    {

    }
}
