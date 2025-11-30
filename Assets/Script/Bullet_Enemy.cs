using Unity.VisualScripting;
using UnityEngine;

public class Bullet_Enemy : MonoBehaviour
{
    private Vector3 Player;
    [SerializeField]private float speed;
    void Start()
    {
        Destroy(gameObject,3f);
        Transform playerpos=GameObject.FindWithTag("Player").GetComponent<Transform>();
        Player=playerpos.position;
        Player.y-=2f;
    }
    void Update()
    {
        if (Player == null) return;
        Vector3 dir = (Player - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }
}
