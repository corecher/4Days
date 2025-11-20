using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]private float maxHp;
    private float hp;
    private Rigidbody rb;
    void Start()
    {
        hp=maxHp;
        rb=GetComponent<Rigidbody>();
    }
    void Damage()
    {
        hp--;
        if(hp<=0)
        {
            rb.useGravity = true;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bullet"))
        {
            Damage();
        }
    }
}
