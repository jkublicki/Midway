using Unity.VisualScripting;
using UnityEngine;

public class CombatOverlayElement : MonoBehaviour, IClickable
{
    private SceneManagerReferences scene;

    public void OnClick()
    {
        scene.CombatOverlay.ServiceCombatOverlayClick(this.gameObject.tag);
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
