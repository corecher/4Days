using Unity.Netcode;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject cameraA; // 호스트용 카메라 (씬에 배치됨)
    public GameObject cameraB; // 클라이언트용 카메라 (씬에 배치됨)
    public GameObject canvasA; // 호스트용 카메라 (씬에 배치됨)
    public GameObject canvasB; // 클라이언트용 카메라 (씬에 배치됨)

    void Start()
    {
        // 네트워크가 시작되지 않은 경우 방지
        if (!NetworkManager.Singleton.IsListening)
            return;

        if (NetworkManager.Singleton.IsHost)
        {
            // 호스트는 A 카메라만 켬
            cameraA.SetActive(true);
            cameraB.SetActive(false);
            canvasA.SetActive(true);
            canvasB.SetActive(false);
        }
        else
        {
            // 클라이언트는 B 카메라만 켬
            cameraA.SetActive(false);
            cameraB.SetActive(true);
            canvasA.SetActive(false);
            canvasB.SetActive(true);
        }
    }
}
