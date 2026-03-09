using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System;

public class UnityServicesInitializer : MonoBehaviour
{
    public static Task InitializationTask;
    private static string authProfile;

    async void Awake()
    {
        if (InitializationTask == null)
        {
            InitializationTask = InitializeServices();
        }

        await InitializationTask;
    }

    private async Task InitializeServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        // Use a unique auth profile per app instance to avoid host/client auth clashes in local testing.
        if (string.IsNullOrEmpty(authProfile))
        {
            authProfile = $"p{Guid.NewGuid():N}".Substring(0, 30);
        }

        AuthenticationService.Instance.SwitchProfile(authProfile);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log("Unity Services Ready");
    }
}
