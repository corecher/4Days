using UnityEngine;
using Unity.Netcode;

public class Bullet_Enemy : NetworkBehaviour
{
    private Vector3 targetPos; // SerializeField 제거 (코드에서 주입하므로)
    
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 3f;
    
    private bool isInitialized = false; // 타겟 설정 여부 확인

    // 1. 외부(적 스크립트)에서 이 함수를 호출해 타겟을 정해줍니다.
    public void Initialize(Vector3 targetPosition)
    {
        targetPos = targetPosition;
        targetPos.y -= 2f; // 기존 로직 유지
        isInitialized = true;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Invoke(nameof(DespawnSelf), lifeTime);
    }

    void Update()
    {
        if (!IsServer || !isInitialized) return; // 타겟이 없으면 움직이지 않음

        // 타겟 위치(고정점)를 향해 이동
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        
        // (선택 사항) 목표 지점에 거의 도달했으면 삭제하고 싶다면 여기에 거리 체크 로직 추가
    }

    void DespawnSelf()
    {
        if (IsSpawned) GetComponent<NetworkObject>().Despawn(true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (IsSpawned) GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
