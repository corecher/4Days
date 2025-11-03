using UnityEngine;

public class EnermyS : MonoBehaviour
{
    // 0 : fighter, 1 : bomber, 2 : suicide
    public GameObject[] enemy;
    private SpawnSpot[] spawnSpots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnSpots = FindObjectsOfType<SpawnSpot>();
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnSpots.Length == 0)
        {
            Debug.LogWarning("SpawnSpot이 없습니다!");
            return;
        }

        int index = Random.Range(0, spawnSpots.Length);
        Transform spot = spawnSpots[index].transform;

        Instantiate(enemy[0], spot.position, spot.rotation);
    }

}
