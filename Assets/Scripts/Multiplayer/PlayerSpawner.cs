using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Only server controls positioning
        if (!NetworkManager.Singleton.IsServer) return;

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            return;

        var playerObject = NetworkManager.Singleton
            .ConnectedClients[clientId]
            .PlayerObject;

        if (playerObject == null) return;

        Transform spawnPoint;

        if (clientId == NetworkManager.ServerClientId) // ✅ FIXED HERE
        {
            spawnPoint = hostSpawnPoint;
        }
        else
        {
            spawnPoint = clientSpawnPoint;
        }

        playerObject.transform.position = spawnPoint.position;
        playerObject.transform.rotation = spawnPoint.rotation;
    }
}