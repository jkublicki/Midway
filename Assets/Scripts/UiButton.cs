using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour, IClickable
{
    private SceneManagerReferences scene;

    public void OnClick()
    {
        scene.Orchestrator.ServiceUiButtonClick(this.gameObject.tag);

    }

    void Start()
    {
        scene = SceneManagerReferences.Instance;
    }
}