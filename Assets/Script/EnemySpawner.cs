using UnityEngine;

public class EnermyS : MonoBehaviour
{
    public GameObject[] enemy;
    private SpawnSpot[] spawnSpots;
    void Start()
    {
        spawnSpots = FindObjectsOfType<SpawnSpot>();
    }
    void Update()
    {
        if (spawnSpots.Length == 0)
        {
            Debug.LogWarning("SpawnSpot!");
            return;
        }

        int index = Random.Range(0, spawnSpots.Length);
        Transform spot = spawnSpots[index].transform;

        Instantiate(enemy[0], spot.position, spot.rotation);
    }

}
