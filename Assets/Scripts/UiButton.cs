using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour, IClickable
{

    public void OnClick()
    {
        Orchestrator.Instance.ServiceUiButtonClick(this.gameObject.tag);

    }

    void Start()
    {

    }
}