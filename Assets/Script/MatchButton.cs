using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GamePlayer"; // 이동할 씬 이름

    private Button startButton;

    private void Awake()
    {
        startButton = GetComponent<Button>();
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        // 호스트만 씬 전환 가능
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can start the game!");
            return;
        }

        // 모든 클라이언트가 해당 씬으로 넘어가도록 씬 전환
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}





