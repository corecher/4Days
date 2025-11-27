using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScenePlayerManager : NetworkBehaviour
{
    public NetworkObject playerA; // Host
    public List<NetworkObject> playerB; // Client

    private void Start()
    {
        // 서버에서만 실행
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadMode)
    {
        // Host(서버)는 무조건 한 번만 실행되므로 여기서 변경 가능
        AssignOwnership();
    }

    private void AssignOwnership()
    {
        SoundManager.Instance.StopBGM();
        ulong serverId = NetworkManager.ServerClientId;

        Debug.Log("AssignOwnership(): Scene objects spawned. Assigning ownership...");

        if (playerA != null && playerA.IsSpawned)
        {
            playerA.ChangeOwnership(serverId);
            Debug.Log($"playerA -> Host({serverId})");
        }

        foreach (var kv in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = kv.Key;

            if (clientId != serverId)
            {
                for(int i=0;i<playerB.Count;i++)
                {
                    if (playerB != null && playerB[i].IsSpawned)
                    {
                        playerB[i].ChangeOwnership(clientId);
                        Debug.Log($"playerB -> Client({clientId})");
                    }
                }
                break;
            }
        }
    }
}







