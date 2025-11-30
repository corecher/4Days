using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement; // 일반 씬 매니지먼트 필요

public class Timer : NetworkBehaviour
{
    public static Timer Instance { get; private set; }
    
    [SerializeField] private string gameSceneName = "GamePlayer";    
    public float dayLengthInMinutes = 10f;

    // 네트워크 변수 (서버만 쓰고, 모두가 읽음)
    public NetworkVariable<float> netGameTime = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int currentDay { get; private set; }
    public int currentHour { get; private set; }
    public int currentMinute { get; private set; }

    private bool isSceneLoading = false; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        // 1. 시간 계산 (서버만)
        if (IsServer)
        {
            CalculateTimeServer();
        }

        // 2. UI 표시 업데이트 (모두)
        UpdateDisplayVars();
    }

    private void CalculateTimeServer()
    {
        float timeMultiplier = 86400f / (dayLengthInMinutes * 60f);
        netGameTime.Value += Time.deltaTime * timeMultiplier;

        int dayCheck = (int)(netGameTime.Value / 86400f);

        // 5일차가 되었고, 아직 로딩 명령을 안 내렸다면
        if (dayCheck >= 5 && !isSceneLoading)
        {
            isSceneLoading = true;
            // [중요] 서버가 모든 클라이언트(자신 포함)에게 "게임 끝, 연결 끊고 이동해!"라고 명령
            EndGameAndLeaveRoomClientRpc();
        }
    }

    // [핵심 변경 사항] ClientRpc: 호스트가 호출하면, 모든 클라이언트에서 실행됨
    [ClientRpc]
    private void EndGameAndLeaveRoomClientRpc()
    {
        Debug.Log("4일이 지났습니다. 방을 나가고 씬을 이동합니다.");

        // 1. 네트워크 연결 끊기 (방에서 나가기)
        // 호스트는 서버를 닫고, 클라이언트는 연결을 끊습니다.
        NetworkManager.Singleton.Shutdown();
        SoundManager.Instance.state=false;
        // 2. 로컬 씬 이동 (각자 컴퓨터에서 혼자 이동)
        // Unity.Netcode의 SceneManager가 아니라, 기본 UnityEngine의 SceneManager를 씁니다.
        SceneManager.LoadScene(gameSceneName);
    }

    private void UpdateDisplayVars()
    {
        float time = netGameTime.Value;
        currentDay = (int)(time / 86400f); 
        float timeOfDay = time % 86400f; 

        currentHour = (int)(timeOfDay / 3600f);
        currentMinute = (int)((timeOfDay % 3600f) / 60f);
    }

    public string GetTimeString()
    {
        return $"Day {currentDay:D2} - {currentHour:D2}/{currentMinute:D2}";
    }
}
