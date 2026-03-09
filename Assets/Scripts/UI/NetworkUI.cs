using UnityEngine;
using TMPro;
using Unity.Netcode;

public class NetworkUI : NetworkBehaviour
{
    public RelayManager relayManager;

    [Header("UI References")]
    public TMP_InputField nameInput;
    public TMP_InputField joinInput;
    public TMP_Text statusText;
    public GameObject canvas;

    private string currentJoinCode;

    private const int requiredPlayers = 2;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;

        statusText.text = "Enter name and choose Host or Client";
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
    }

    public async void StartHost()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            statusText.text = "Enter your name!";
            return;
        }

        PlayerPrefs.SetString("PLAYER_NAME", nameInput.text);

        statusText.text = "Creating Relay...";

        currentJoinCode = await relayManager.CreateRelay(1);

        if (!string.IsNullOrEmpty(currentJoinCode))
        {
            statusText.text = $"<b>JOIN CODE</b>\n<size=36>{currentJoinCode}</size>";
            GUIUtility.systemCopyBuffer = currentJoinCode;
        }
        else
        {
            statusText.text = "Relay creation failed.";
        }
    }

    public async void StartClient()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            statusText.text = "Enter your name!";
            return;
        }

        string joinCode = joinInput.text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(joinCode))
        {
            statusText.text = "Enter valid join code!";
            return;
        }

        PlayerPrefs.SetString("PLAYER_NAME", nameInput.text);

        statusText.text = "Joining...";

        bool success = await relayManager.JoinRelay(joinCode);

        if (!success)
        {
            statusText.text = "Join failed!";
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        int connectedCount = NetworkManager.Singleton.ConnectedClients.Count;

        if (connectedCount >= requiredPlayers)
        {
            statusText.text = "Both Players Connected!";
            HideUIClientRpc();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;

        bool isLocalDisconnect = clientId == NetworkManager.Singleton.LocalClientId;

        // Host should stay running if only a remote client left.
        if (NetworkManager.Singleton.IsHost && !isLocalDisconnect)
        {
            statusText.text = "Client disconnected. Waiting for reconnect...";
            canvas.SetActive(true);
            return;
        }

        if (!isLocalDisconnect) return;

        statusText.text = "Disconnected from session.";
        canvas.SetActive(true);

        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void OnTransportFailure()
    {
        statusText.text = "Transport failed!";
        canvas.SetActive(true);
        NetworkManager.Singleton.Shutdown();
    }

    [ClientRpc]
    private void HideUIClientRpc()
    {
        canvas.SetActive(false);
    }
}
