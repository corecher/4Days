using UnityEngine;
using Unity.Netcode;

public class GameSceneSpawner : MonoBehaviour
{
    public GameObject hostPlayerPrefab;
    public GameObject clientPlayerPrefab;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer) // 서버만 실행
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                GameObject prefabToSpawn;

                if (clientId == NetworkManager.Singleton.LocalClientId)
                    prefabToSpawn = hostPlayerPrefab; // 호스트 자신
                else
                    prefabToSpawn = clientPlayerPrefab; // 나머지 클라이언트

                GameObject obj = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
                obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            }
        }
    }
}

