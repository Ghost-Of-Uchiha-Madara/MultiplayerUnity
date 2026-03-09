using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    private UnityTransport transport;

    private void Awake()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public async Task<string> CreateRelay(int maxConnections)
    {
        await UnityServicesInitializer.InitializationTask;

        ShutdownIfRunning();

        Debug.Log("Creating Relay Allocation...");

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData,
                false // Use false first (DTLS off for stability)
            );

            bool started = NetworkManager.Singleton.StartHost();
            Debug.Log("Host Started: " + started);

            if (!started)
            {
                Debug.LogError("Host failed to start!");
                return null;
            }

            Debug.Log("Relay Created. Join Code: " + joinCode);
            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Create Relay Failed: " + e);
            return null;
        }
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
        await UnityServicesInitializer.InitializationTask;

        ShutdownIfRunning();

        Debug.Log("Joining Relay with code: " + joinCode);

        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Join Allocation Success");

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData,
                false // DTLS off for testing stability
            );

            bool started = NetworkManager.Singleton.StartClient();
            Debug.Log("Client Started: " + started);

            if (!started)
            {
                Debug.LogError("Client failed to start!");
                return false;
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Join Relay Failed: " + e);
            return false;
        }
    }

    private void ShutdownIfRunning()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.Log("Shutting down previous session...");
            NetworkManager.Singleton.Shutdown();
        }
    }
}