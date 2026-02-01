using Unity.VisualScripting;
using UnityEngine;

public class CombatOverlayElement : MonoBehaviour, IClickable
{


    public void OnClick()
    {
        CombatOverlayManager.Instance.ServiceCombatOverlayClick(this.gameObject.tag);
    }

    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
