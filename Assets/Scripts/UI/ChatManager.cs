using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    public TMP_InputField messageInput;
    public TMP_Text chatContent;

    public void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(messageInput.text))
            return;

        string message = messageInput.text;
        messageInput.text = "";

        SendMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        // Get the player object of the sender
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(senderClientId, out var client))
        {
            PlayerController player = client.PlayerObject.GetComponent<PlayerController>();

            if (player != null)
            {
                string senderName = player.GetPlayerName();
                ReceiveMessageClientRpc(senderName, message);
            }
        }
    }

    [ClientRpc]
    private void ReceiveMessageClientRpc(string senderName, string message)
    {
        chatContent.text += $"\n<b>{senderName}:</b> {message}";
    }
}