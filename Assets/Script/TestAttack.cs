using UnityEngine;
using System.Collections;
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
    [SerializeField] private GameObject hpUi;
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
        if (IsOwner)
        {
        if (hpUi == null)
        {
            hpUi = GameObject.Find("HpUI"); // ⛔ 자신의 HpUI 정확한 이름으로 수정
        }

        if (hpUi == null)
        {
            Debug.LogError("❌ HpUI를 찾지 못했습니다!");
        }
        else
        {
            // ✅ UI 안전 초기화
            for (int i = 0; i < hpUi.transform.childCount; i++)
            {
                hpUi.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
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

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (newValue <= 0 && IsOwner)
        {
            Debug.Log("사망: 게임 종료 프로세스 시작");

            if (IsServer)
            {
                StartCoroutine(EndGameSequence());
            }
            else
            {
                LeaveGameLocal();
            }
        }
    }

    private IEnumerator EndGameSequence()
    {
        LeaveGameClientRpc();
        yield return new WaitForSeconds(1.0f);
        LeaveGameLocal();
    }

    [ClientRpc]
    private void LeaveGameClientRpc()
    {
        if (IsServer) return;
        LeaveGameLocal();
    }

    private void LeaveGameLocal()
    {
        NetworkManager.Singleton.Shutdown();
        if (SoundManager.Instance != null) SoundManager.Instance.state = true;
        SceneManager.LoadScene(gameSceneName);
    }

    // ==================================================================================
    // [수정 포인트 1] 충돌 처리 시 안전장치 추가
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
                // [중요] 이미 Despawn된 오브젝트를 다시 Despawn 하려 하면 에러 발생함.
                // 따라서 IsSpawned를 확인해야 함.
                if (netObj.IsSpawned) 
                {
                    netObj.Despawn();
                }
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
        if (!IsOwner) return;
    if (hpUi == null) return;

    int max = hpUi.transform.childCount;

    // ✅ 체력을 UI 개수 범위 안으로 강제 제한
    int activeCount = Mathf.Clamp(hp.Value / 100, 0, max);

    for (int i = 0; i < max; i++)
    {
        hpUi.transform.GetChild(i).gameObject.SetActive(i >= activeCount);
    }

    if (bullets != null)
        bullets.text = $"{ammunition.Value}/500";
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

    // ==================================================================================
    // [수정 포인트 2] Destroy() 제거 및 Despawn 로직 변경
    // ==================================================================================
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
        if (bulletNetObj != null) 
        {
            bulletNetObj.Spawn();
            
            // [중요] Destroy(bulletInstance, bulletLifeTime) 삭제!
            // 대신 네트워크 오브젝트를 안전하게 삭제하는 코루틴 실행
            StartCoroutine(DespawnBulletDelay(bulletNetObj, bulletLifeTime));
        }
        else
        {
            // 네트워크 오브젝트가 아니면 그냥 Destroy 해도 됨 (이럴 일은 거의 없겠지만)
            Destroy(bulletInstance, bulletLifeTime);
        }
    }

    // [추가] 총알 수명 관리 코루틴
    IEnumerator DespawnBulletDelay(NetworkObject bulletObj, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 시간이 지났는데 총알이 아직 존재하고, 스폰된 상태라면 Despawn 수행
        // (만약 그 전에 충돌해서 이미 Despawn 되었다면 이 조건문에서 걸러짐)
        if (bulletObj != null && bulletObj.IsSpawned)
        {
            bulletObj.Despawn();
        }
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

