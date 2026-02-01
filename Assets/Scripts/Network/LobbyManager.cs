using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private Lobby currentLobby;
    public Lobby CurrentLobby => currentLobby; //read only
    public Player? LocalPlayer = null;


    private float heartbeatTimer = 0f;
    private float lobbyUpdateTimer = 0f;
    private const float heartbeatInterval = 15f;
    private const float lobbyUpdateInterval = 1.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {

        HandleLobbyHeartbeat();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer >= heartbeatInterval)
            {
                heartbeatTimer = 0f;
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }

    public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, string relayJoinCode)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            LocalPlayer = Player.PLAYER_1; //tworzacy serwe jest graczem 1

            Debug.Log($"Lobby created: {currentLobby.Name} (ID: {currentLobby.Id})");
            return currentLobby;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return null;
        }
    }

    public async Task<List<Lobby>> GetAvailableLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            return response.Results;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to query lobbies: {e.Message}");
            return new List<Lobby>();
        }
    }

    public async Task<bool> JoinLobbyById(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            LocalPlayer = Player.PLAYER_2; //dolaczajacy klient jest graczem 2

            Debug.Log($"Joined lobby: {currentLobby.Name}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            return false;
        }
    }

    public string GetRelayJoinCode()
    {
        if (currentLobby != null && currentLobby.Data.ContainsKey("RelayJoinCode"))
        {
            return currentLobby.Data["RelayJoinCode"].Value;
        }
        return null;
    }

    public async void LeaveLobby()
    {
        if (currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
                currentLobby = null;

                LocalPlayer = null;

            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to leave lobby: {e.Message}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        LeaveLobby();
    }
}