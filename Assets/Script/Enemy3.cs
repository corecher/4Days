using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    public Transform center;
    public float rotateSpeed = 50f;
    [SerializeField]private Transform Player;
    [SerializeField]private float speed;
    void Start()
    {
        Player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        center = GameObject.FindWithTag("center").GetComponent<Transform>();
    }
    void Update()
    {
        if(Vector3.Distance(Player.position,transform.position)<150) Dash();
        else MoveCircle();
    }
    void MoveCircle()
    {
        transform.RotateAround(center.position, Vector3.up, rotateSpeed * Time.deltaTime);
    }
    void Dash()
    {
        if (Player == null) return;
        Vector3 dir = (Player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
        transform.position += dir * speed * Time.deltaTime;
    }
}
