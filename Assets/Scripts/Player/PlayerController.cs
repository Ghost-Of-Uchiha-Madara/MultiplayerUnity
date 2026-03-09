using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;

    [Header("Name UI")]
    public TMP_Text nameText;

    private MobileJoystick joystick;

    private NetworkVariable<FixedString32Bytes> playerName =
        new NetworkVariable<FixedString32Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Spawned → Owner:{IsOwner}");

        playerName.OnValueChanged += OnNameChanged;

        if (IsOwner)
        {
            // Assign joystick for mobile
            joystick = FindObjectOfType<MobileJoystick>();

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

        Vector3 move = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        // Keyboard movement for testing
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        move = new Vector3(x, 0f, z);
#else
        // Mobile joystick movement
        if (joystick != null)
        {
            Vector2 dir = joystick.Direction;
            move = new Vector3(dir.x, 0f, dir.y);
        }
#endif

        transform.position += move * moveSpeed * Time.deltaTime;
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