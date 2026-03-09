using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkDebugUI : MonoBehaviour
{
    public TMP_Text debugText;

    private UnityTransport transport;
    private string lastEvent = "None";
    private float pingCheckTimer;

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;

            LogEvent("Debug UI Initialized");
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
    }

    void Update()
    {
        if (NetworkManager.Singleton == null)
        {
            debugText.text = "NetworkManager not found";
            return;
        }

        var nm = NetworkManager.Singleton;

        string info =
            $"=== NETWORK STATE ===\n" +
            $"IsHost: {nm.IsHost}\n" +
            $"IsClient: {nm.IsClient}\n" +
            $"IsServer: {nm.IsServer}\n" +
            $"IsListening: {nm.IsListening}\n" +
            $"LocalClientId: {nm.LocalClientId}\n" +
            $"ConnectedClients: {nm.ConnectedClients.Count}\n\n";

        if (transport != null)
        {
            info += $"=== TRANSPORT ===\n";
            info += $"Protocol: {transport.Protocol}\n";
            info += $"Address: {transport.ConnectionData.Address}\n";
            info += $"Port: {transport.ConnectionData.Port}\n\n";
        }

        info += $"=== LAST EVENT ===\n{lastEvent}\n\n";

        info += $"FPS: {(1f / Time.unscaledDeltaTime):0}";

        debugText.text = info;
    }

    private void OnClientConnected(ulong clientId)
    {
        LogEvent($"Client Connected: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        LogEvent($"Client Disconnected: {clientId}");
    }

    private void OnTransportFailure()
    {
        LogEvent("Transport FAILURE detected!");
    }

    private void LogEvent(string message)
    {
        Debug.Log("[NetworkDebug] " + message);
        lastEvent = message;
    }
}