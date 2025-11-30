using UnityEngine;
using System.Collections; // IEnumerator 사용을 위해 추가
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TestAttack : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] float rotationSpeed;
    [SerializeField] float minX = -70f;
    [SerializeField] float maxX = 20f;
    
    [Header("Combat")]
    [SerializeField] GameObject bullet;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private List<Transform> muzzleTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float bulletLifeTime = 5f;
    private int index = 0;

    [Header("References")]
    [SerializeField] private Transform body;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Image hpUi;
    [SerializeField] private Text bullets;
    [SerializeField] private string gameSceneName = "LobbyScene"; 

    public NetworkVariable<int> ammunition = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> hp = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        hp.OnValueChanged += OnHealthChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (IsServer)
        {
            hp.Value = 1000;
            ammunition.Value = 100;
        }

        UpdateUI();
    }

    public override void OnNetworkDespawn()
    {
        hp.OnValueChanged -= OnHealthChanged;
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer) return; 

        Debug.Log($"연결 끊김 감지 (ID: {clientId}). 로비로 이동합니다.");
        if (SoundManager.Instance != null) SoundManager.Instance.state = true;
        
        SceneManager.LoadScene(gameSceneName);
    }

    // ==================================================================================
    // [수정] HP 변경 감지 및 우아한 종료 처리
    // ==================================================================================
    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (newValue <= 0 && IsOwner)
        {
            Debug.Log("사망: 게임 종료 프로세스 시작");

            if (IsServer)
            {
                // 내가 호스트라면: 클라이언트들을 먼저 내보내고 나중에 나간다.
                StartCoroutine(EndGameSequence());
            }
            else
            {
                // 내가 일반 클라이언트라면: 그냥 나간다.
                LeaveGameLocal();
            }
        }
    }

    // [추가] 호스트가 주도하는 종료 시퀀스
    private IEnumerator EndGameSequence()
    {
        // 1. 모든 클라이언트에게 나가라고 명령 (호스트 자신은 제외됨)
        LeaveGameClientRpc();

        // 2. 클라이언트들이 메시지를 받고 처리할 시간을 줌 (네트워크 지연 고려 0.5~1초)
        yield return new WaitForSeconds(1.0f);

        // 3. 호스트 종료
        LeaveGameLocal();
    }

    // [추가] 클라이언트들에게 실행되는 RPC (호스트가 호출)
    [ClientRpc]
    private void LeaveGameClientRpc()
    {
        // 호스트가 이 RPC를 받으면 무시 (호스트는 코루틴에서 별도로 처리)
        if (IsServer) return;

        LeaveGameLocal();
    }

    // [추가] 실제 연결 해제 및 씬 이동 로직 (공통)
    private void LeaveGameLocal()
    {
        // 셧다운 (연결 해제)
        NetworkManager.Singleton.Shutdown();
        
        if (SoundManager.Instance != null) SoundManager.Instance.state = true;
        
        // 씬 이동
        SceneManager.LoadScene(gameSceneName);
    }

    // ==================================================================================
    // 기존 로직 유지
    // ==================================================================================
    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("EnemyBulletbig") || collision.gameObject.CompareTag("EnemyBulletself"))
        {
            int damage;
            if(collision.gameObject.CompareTag("EnemyBullet")) damage = 1;
            else if(collision.gameObject.CompareTag("EnemyBulletbig")) damage = 30;
            else damage = 100;
            
            hp.Value = Mathf.Max(0, hp.Value - damage);

            if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                netObj.Despawn();
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
    }

    void Start()
    {
        if (GameObject.FindWithTag("body") != null)
            body = GameObject.FindWithTag("body").GetComponent<Transform>();
        if(IsOwner && SoundManager.Instance != null) SoundManager.Instance.StopBGM();
    }

    void Update()
    {
        if (body != null) FixBody();

        if (IsOwner)
        {
            UpdateUI();
            Attack();
        }
    }

    void UpdateUI()
    {
        if (hpUi != null) hpUi.fillAmount = hp.Value / 1000f;
        if (bullets != null) bullets.text = $"{ammunition.Value}/500";
    }

    void Attack()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.UpArrow))
            RotationClamp(-1);
        if (Input.GetKey(KeyCode.DownArrow))
            RotationClamp(1);

        if (Input.GetKeyDown(KeyCode.F) && ammunition.Value > 0)
        {
            RequestFireServerRpc();
        }
    }

    [ServerRpc]
    void RequestFireServerRpc()
    {
        if (ammunition.Value <= 0) return;
        ammunition.Value--;
        index = (index == 0) ? 1 : 0;
        Shot(index);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayNetworkSound("shot", transform.position);
            SoundManager.Instance.PlayNetworkSound("gunimpact", transform.position);
        }
    }

    void Shot(int i)
    {
        Vector3 spawnPos;
        Vector3 forwardDir;

        if (muzzleTransform != null && muzzleTransform.Count > i)
        {
            spawnPos = muzzleTransform[i].position;
            forwardDir = cameraTransform.forward;
        }
        else
        {
            spawnPos = cameraTransform.position + cameraTransform.forward * 0.6f;
            forwardDir = cameraTransform.forward;
        }

        GameObject bulletInstance = Instantiate(bullet, spawnPos, Quaternion.LookRotation(forwardDir));
        Rigidbody rb = bulletInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = forwardDir.normalized * bulletSpeed;
#else
            rb.velocity = forwardDir.normalized * bulletSpeed;
#endif
        }

        NetworkObject bulletNetObj = bulletInstance.GetComponent<NetworkObject>();
        if (bulletNetObj != null) bulletNetObj.Spawn();
        
        Destroy(bulletInstance, bulletLifeTime);
    }

    void FixBody()
    {
        transform.position = offset + body.position;
    }

    void RotationClamp(int input)
    {
        foreach (Transform child in transform)
        {
            Vector3 angles = child.localEulerAngles;
            float x = angles.x;
            if (x > 180f) x -= 360f;
            x += input * rotationSpeed * Time.deltaTime;
            x = Mathf.Clamp(x, minX, maxX);
            angles.x = x;
            child.localEulerAngles = angles;
        }
    }
}

