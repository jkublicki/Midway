using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class MenuCanvasManager : MonoBehaviour
{
    

    public static MenuCanvasManager Instance { get; private set; }

    public TextMeshProUGUI ServerCount;
    public TextMeshProUGUI CentralMessage;

    public Button PreviousServerButton;
    public Button NextServerButton;
    public Button JoinButton;
    public Button CreateServerButton;

    public GameObject ServerPanel;
    public TextMeshProUGUI ServerName;
    public TextMeshProUGUI ServerDescription;

    private int currentServer = 0;

    private List<Lobby> serverListCache = new List<Lobby>();

    private bool IsAnimatonInProgress = false;


    public void RefreshCanvas()
    {
        CentralMessage.text = "";
        JoinButton.interactable = true;
        CreateServerButton.interactable = true;
        ServerPanel.SetActive(true);

        if (serverListCache.Count == 0)
        {
            currentServer = 0;
            ServerCount.text = "0/0";
            PreviousServerButton.interactable = false;
            NextServerButton.interactable = false;
            ServerPanel.SetActive(false);
            JoinButton.interactable = false;
            CentralMessage.text = "No games available. Create a game.";
            return;
        }
        
        ServerCount.text = (currentServer + 1).ToString() + "/" + serverListCache.Count.ToString();

        if (currentServer > serverListCache.Count - 1)
        {
            currentServer = serverListCache.Count - 1;
        }

        if (currentServer == 0)
        {
            PreviousServerButton.interactable = false;
        }
        else
        {
            PreviousServerButton.interactable = true;
        }

        if (currentServer < serverListCache.Count - 1)
        {
            NextServerButton.interactable = true;
        }
        else
        {
            NextServerButton.interactable = false;
        }

        if (serverListCache[currentServer] != null)
        {
            ServerName.text = serverListCache[currentServer].Name;
            ServerDescription.text = ServerDescription.text = $"ID: {serverListCache[currentServer].Id} | " +
                $"  {serverListCache[currentServer].Players.Count}/{serverListCache[currentServer].MaxPlayers} | Host: {serverListCache[currentServer].HostId} | " +
                $"Private: {serverListCache[currentServer].IsPrivate} | Locked: {serverListCache[currentServer].IsLocked}";
        }
    }


    public void UpdateServers(List<Lobby> serverList)
    {
        if (serverList == null)
        {
            return;
        }

        serverListCache.Clear();
        serverListCache.AddRange(serverList);
        RefreshCanvas();
    }

    private void OnPreviousClick()
    {
        if (IsAnimatonInProgress)
        {
            return;
        }
        currentServer--;
        StartCoroutine(SlideServerPanel(false));
    }

    private void OnNextClick()
    {
        if (IsAnimatonInProgress)
        {
            return;
        }
        currentServer++;
        StartCoroutine(SlideServerPanel(true));
    }

    private void OnCreateServerClick()
    {
        MenuSceneManager.Instance.CreateGame(CentralMessage, CreateServerButton);
    }

    private void OnJoinClick()
    {
        MenuSceneManager.Instance.JoinGame(serverListCache[currentServer].Id, CentralMessage, CreateServerButton);
    }

    private IEnumerator SlideServerPanel(bool upwards)
    {
        IsAnimatonInProgress = true;
        float duration = 0.3f;
        float maxDistance = 250.0f;
        float pow = 1.5f;
        Vector3 startPosition = ServerPanel.transform.position;
        float sign = 1.0f;

        if (!upwards)
        {
            sign = -1.0f;
        }        

        for (float t = 0.0f; t < duration; t += Time.deltaTime)
        {
            float deltaY = sign * maxDistance * Mathf.Pow((t / duration), pow);
            Vector3 v = new Vector3(startPosition.x, startPosition.y + deltaY, startPosition.z);
            ServerPanel.transform.position = v; 
            yield return null;
        }

        ServerPanel.transform.position = startPosition;
        IsAnimatonInProgress = false;

        RefreshCanvas();
    }





    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }



    async void Start()
    {
        PreviousServerButton.onClick.AddListener(OnPreviousClick);
        NextServerButton.onClick.AddListener(OnNextClick);

        CreateServerButton.onClick.AddListener(OnCreateServerClick);
        JoinButton.onClick.AddListener(OnJoinClick);

        CentralMessage.text = "Refreshing lobby…";
        JoinButton.interactable = false;
        CreateServerButton.interactable = false;
        ServerPanel.SetActive(false);
        PreviousServerButton.interactable = false;
        NextServerButton.interactable = false;



    }

    
    /*
    void Update()
    {
        
    }
    */
}
