using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
    public static MenuSceneManager Instance;


    public async void JoinGame(string lobbyId, TextMeshProUGUI messageTextField, Button joinGameButton)
    {
        joinGameButton.interactable = false;
        messageTextField.text = "Joining game...";

        bool joined = await LobbyManager.Instance.JoinLobbyById(lobbyId);
        if (!joined)
        {
            messageTextField.text = "Failed to join lobby";
            return;
        }

        string relayJoinCode = LobbyManager.Instance.GetRelayJoinCode();
        if (string.IsNullOrEmpty(relayJoinCode))
        {
            messageTextField.text = "No relay code found";
            return;
        }

        bool relayJoined = await RelayManager.Instance.JoinRelay(relayJoinCode);
        if (!relayJoined)
        {
            messageTextField.text = "Failed to join relay";
            return;
        }

        NetworkManager.Singleton.StartClient(); //Netcode automatically synchronizes the scene
        messageTextField.text = "Connecting...";
    }


    public async void CreateGame(TextMeshProUGUI messageTextField, Button createGameButton)
    {
        messageTextField.text = "Creating game...";
        createGameButton.interactable = false;

        // Create Relay
        string relayJoinCode = await RelayManager.Instance.CreateRelay(2);
        if (string.IsNullOrEmpty(relayJoinCode))
        {
            messageTextField.text = "Failed to create relay";
            createGameButton.interactable = true;
            return;
        }

        // Create Lobby
        string lobbyName = $"Game_{UnityEngine.Random.Range(1000, 9999)}";
        var lobby = await LobbyManager.Instance.CreateLobby(lobbyName, 2, relayJoinCode);
        if (lobby == null)
        {
            messageTextField.text = "Failed to create lobby";
            createGameButton.interactable = true;
            return;
        }

        // Start as Host
        NetworkManager.Singleton.StartHost();
        messageTextField.text = "Starting as host...";

        // Load game scene
        NetworkManager.Singleton.SceneManager.LoadScene("BattleScene", LoadSceneMode.Single);
    }




    private async System.Threading.Tasks.Task GetLobbyList()
    {
        List<Lobby> lobbies = await LobbyManager.Instance.GetAvailableLobbies();
        Debug.Log($"Found {lobbies.Count} games");

        MenuCanvasManager.Instance.UpdateServers(lobbies);
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
        while (!UnityServicesInitializer.Instance.IsReady)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        InvokeRepeating(nameof(GetLobbyList), 0.0f, 5.0f);
    }


    
    void Update()
    {
        
    }
}
