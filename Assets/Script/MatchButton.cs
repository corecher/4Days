using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MatchButton : MonoBehaviour
{
    private bool isMatching = false;

    // 버튼 클릭 시 실행할 함수
    public void StartRandomMatch()
    {
        if (isMatching) return; // 중복 방지
        isMatching = true;
        
        StartCoroutine(RandomMatchRoutine());
    }

    private IEnumerator RandomMatchRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("🎲 랜덤 매칭 시작!");

        // 클라이언트로 먼저 접속 시도
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                Debug.Log("✅ 클라이언트로 접속 성공!");
            }
            else
            {
                Debug.Log("❌ 클라이언트 접속 실패, 호스트로 전환합니다...");
                NetworkManager.Singleton.StartHost();
                Debug.Log("🏠 호스트 시작 (방 생성)");
            }
        }
    }

    // 클라이언트가 성공적으로 연결된 경우 자동 호출
    private void OnEnable()
{
    // ✅ 안전하게 Null 체크 추가
    if (NetworkManager.Singleton != null)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    else
    {
        Debug.LogWarning("⚠️ NetworkManager.Singleton이 아직 초기화되지 않았습니다.");
    }
}

private void OnDisable()
{
    if (NetworkManager.Singleton != null)
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"🎯 클라이언트 {clientId} 연결 완료!");

        // 예시로 씬 이동
        if (NetworkManager.Singleton.IsHost)
        {
            SceneManager.LoadScene("GamePlay"); // 이름은 직접 지정
        }
    }
}


