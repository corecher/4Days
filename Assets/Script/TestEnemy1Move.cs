using UnityEngine;

public class TestEnemy1Move : MonoBehaviour
{
    public Transform center;
    public float rotateSpeed = 50f;
    [Header("출렁임 설정")]
    public float waveHeight = 5f;
    public float waveSpeed = 2f;
    [Header("기울임 설정")]
    public float tiltXAngle = 30f;
    public float tiltZAngle = 20f;
    private float baseY;
    [SerializeField]private Transform Player;
    [SerializeField]private float speed;
    public int State=0;
    void Start()
    {
        baseY = transform.position.y;
        Player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        center = GameObject.FindWithTag("center").GetComponent<Transform>();
    }

    void Update()
    {
       switch(State)
       {
            
            case 0: MoveCircle();break;
            case 1: Dash();break;
            case 2: MoveCircle();break;
       }
    }
    void MoveCircle()
    {
        transform.RotateAround(center.position, Vector3.up, rotateSpeed * Time.deltaTime);
        Vector3 pos = transform.position;
        if(State!=2)
        {
            float wave = Mathf.Sin(Time.time * waveSpeed);
            pos.y = baseY + wave * waveHeight;
            transform.position = pos;
            float tiltX = wave * tiltXAngle;
            float tiltZ = Mathf.Cos(Time.time * waveSpeed) * tiltZAngle;
            float currentY = transform.rotation.eulerAngles.y;

            transform.rotation = Quaternion.Euler(tiltX, currentY, tiltZ);
        }
        if(Vector3.Distance(Player.position,transform.position)<110&&State==0)State=1;
        UpPlane(pos);
    }
    void Dash()
    {
        if (Player == null) return;
        Vector3 dir = (Player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
        transform.position += dir * speed * Time.deltaTime;
        if(Vector3.Distance(Player.position,transform.position)<30) State=2;
    }
    void UpPlane(Vector3 pos)
    {
        if (pos.y < 100f)
        {
            pos.y = Mathf.MoveTowards(pos.y, 100f, 50f * Time.deltaTime); 
            transform.position = pos;
        }
        else
        {
            State=0;
        }
    }
}
