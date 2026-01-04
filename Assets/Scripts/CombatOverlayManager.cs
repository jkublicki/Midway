using UnityEngine;
using UnityEngine.UI;

public class CombatOverlayManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GameObject combatOverlayPanel;
    [SerializeField] private Button closeButton;

    public void ShowCombatOverlay()
    {
        combatOverlayPanel.SetActive(true);
    }

    public void HideCombatOverlay()
    {
        combatOverlayPanel.SetActive(false);
    }



    void Start()
    {
        closeButton.onClick.AddListener(HideCombatOverlay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
