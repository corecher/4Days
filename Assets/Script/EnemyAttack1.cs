using System.Collections;
using UnityEngine;

public class EnemyAttack1 : MonoBehaviour
{
    [SerializeField]private Transform Player;
    [SerializeField]private GameObject bulletPrefab;
    [SerializeField]private Transform enemy;
    void Start()
    {
        StartCoroutine(AttackReturn());
    }
    void Attack()
    {
        Instantiate(bulletPrefab,enemy.position,Quaternion.identity);
    }
    IEnumerator AttackReturn()
    {
        Attack();
        yield return new WaitForSeconds(1f);
        StartCoroutine(AttackReturn());
    }
}
