using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour, IClickable
{
    private SceneManagerReferences scene;

    public void OnClick()
    {
        Orchestrator.Instance.ServiceUiButtonClick(this.gameObject.tag);

    }

    void Start()
    {
        scene = SceneManagerReferences.Instance;
    }
}