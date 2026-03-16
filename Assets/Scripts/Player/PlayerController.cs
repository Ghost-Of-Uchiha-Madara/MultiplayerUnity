using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;

    [Header("Name UI")]
    public TMP_Text nameText;

    private Vector2 moveInput;

    private NetworkVariable<FixedString32Bytes> playerName =
        new NetworkVariable<FixedString32Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Spawned → Owner:{IsOwner}");

        // PREVENT NON-OWNERS FROM STEALING KEYBOARD/MOUSE DEVICE INPUT
        PlayerInput pInput = GetComponent<PlayerInput>();
        if (pInput != null)
        {
            pInput.enabled = IsOwner;
        }

        playerName.OnValueChanged += OnNameChanged;

        if (IsOwner)
        {
            // Set player name
            string myName = PlayerPrefs.GetString("PLAYER_NAME", "Player");
            SubmitNameServerRpc(myName);

            // Assign camera follow
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.SetTarget(transform);
            }
        }

        UpdateNameUI(playerName.Value.ToString());
    }

    void Update()
    {
        if (!IsOwner) return;

        // Start with PlayerInput's moveInput
        Vector2 finalInput = moveInput;

        // Try to read from MobileJoystick if it exists in the scene
        MobileJoystick joystick = Object.FindFirstObjectByType<MobileJoystick>();
        if (joystick != null && joystick.Direction.sqrMagnitude > 0)
        {
            finalInput = joystick.Direction;
        }

        Vector3 move = new Vector3(finalInput.x, 0f, finalInput.y);
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    // Called automatically by PlayerInput component
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        moveInput = context.ReadValue<Vector2>();
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string newName)
    {
        playerName.Value = newName;
    }

    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        UpdateNameUI(newName.ToString());
    }

    private void UpdateNameUI(string newName)
    {
        if (nameText != null)
        {
            nameText.text = newName;
        }
    }

    public string GetPlayerName()
    {
        return playerName.Value.ToString();
    }
}