using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class EnemyAttack1 : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform enemy; // 필요 없다면 제거 가능
    
    public bool playAttack;
    public Enemy2 enemy2;

    // Start 대신 OnNetworkSpawn 사용 권장 (네트워크 객체 초기화 시점)
    public override void OnNetworkSpawn()
    {
        // 클라이언트에서는 공격 로직을 돌릴 필요가 없으므로 바로 리턴
        if (!IsServer) return;

        enemy2 = GetComponent<Enemy2>();
        playAttack = true;
        StartCoroutine(AttackReturn());
    }

    public void Attack(GameObject targetPlayer)
    {
        // 이미 위에서 막았지만 이중 안전 장치
        if (!IsServer) return; 

        // 1. 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // 2. 총알 스크립트 주입
        Bullet_Enemy bulletScript = bullet.GetComponent<Bullet_Enemy>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(targetPlayer.transform.position);
        }

        // 3. 네트워크 스폰
        bullet.GetComponent<NetworkObject>().Spawn();
    }

    IEnumerator AttackReturn()
    {
        // 무한 루프 시작
        while (playAttack)
        {
            // 방어 코드: enemy2가 없거나, center(타겟)가 아직 없으면 대기
            if (enemy2 == null || enemy2.center == null)
            {
                // 타겟을 찾을 때까지 잠시 대기 (프레임 드랍 방지)
                yield return null; 
                continue; 
            }

            // 안전하게 확인 후 공격 시도
            if (enemy2.center.gameObject != null)
            {
                Attack(enemy2.center.gameObject);
            }

            // 1초 대기
            yield return new WaitForSeconds(1f);
        }
    }
}
