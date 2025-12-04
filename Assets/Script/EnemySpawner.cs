using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class EnemyS : NetworkBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Settings")]
    public float waveInterval = 5.0f;
    public float spawnInterval = 1.0f;

    private SpawnSpot[] allSpawnSpots;
    private int currentProcessingDay = -1;
    private bool isNight = false;

    void Start()
    {
        if (!IsServer) return; // ✅ 서버만 스폰 로직 실행

        allSpawnSpots = FindObjectsOfType<SpawnSpot>();

        if (allSpawnSpots.Length == 0)
        {
            Debug.LogError("SpawnSpot이 없습니다!");
        }
    }

    void Update()
    {
        if (Timer.Instance == null) return;
        if (!IsServer) return; // ✅ 서버 전용

        int day = Timer.Instance.currentDay;

        if (day == 2 && Timer.Instance.currentHour > 20)
        {
            day = 21;
        }

        if (currentProcessingDay != day)
        {
            StopAllCoroutines();
            currentProcessingDay = day;
            StartCoroutine(LevelRoutine(day));
        }
    }

    IEnumerator LevelRoutine(int day)
    {
        Debug.Log($"Day {day} 스폰 시작");

        switch (day)
        {
            case 0:
                yield return StartCoroutine(TutorialPattern());
                break;

            case 1:
            case 2:
                yield return StartCoroutine(DayPatternCommon(4));
                break;

            case 21:
                yield return StartCoroutine(NightAmbushPattern());
                break;

            case 3:
                yield return StartCoroutine(Day3ScoutPattern());
                break;

            case 4:
                yield return StartCoroutine(FinalDefensePattern());
                break;
        }
    }

    IEnumerator TutorialPattern()
    {
        Transform centerSpot = GetSpotByDirection("Center");
        if (centerSpot != null)
        {
            SpawnEnemy(0, centerSpot);
        }
        yield break;
    }

    IEnumerator DayPatternCommon(int totalWaves)
    {
        for (int w = 0; w < totalWaves; w++)
        {
            for (int i = 0; i < 5; i++)
            {
                Transform spot = GetRandomSpotFrom(allSpawnSpots);
                int randomType = Random.Range(0, 2);

                SpawnEnemy(randomType, spot);

                yield return new WaitForSeconds(spawnInterval);
            }
            yield return new WaitForSeconds(waveInterval);
        }
    }

    IEnumerator NightAmbushPattern()
    {
        StartCoroutine(SpawnRandomSuicideBombers());

        for (int w = 0; w < 2; w++)
        {
            int count = (w == 0) ? 8 : 4;

            for (int i = 0; i < count; i++)
            {
                Transform spot = GetSpotByDirection("NorthWest");

                int randomType = Random.Range(0, 2);
                SpawnEnemy(randomType, spot);

                yield return new WaitForSeconds(spawnInterval);
            }
            yield return new WaitForSeconds(waveInterval);
        }
    }

    IEnumerator SpawnRandomSuicideBombers()
    {
        while (true)
        {
            float randomTime = Random.Range(5.0f, 10.0f);
            yield return new WaitForSeconds(randomTime);

            Transform spot = GetRandomSpotFrom(allSpawnSpots);
            SpawnEnemy(2, spot);
        }
    }

    IEnumerator Day3ScoutPattern()
    {
        while (true)
        {
            Transform northSpot = GetSpotByDirection("North");

            SpawnEnemy(Random.Range(0, 2), northSpot);
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy(Random.Range(0, 2), northSpot);

            yield return new WaitForSeconds(8.0f);
        }
    }

    IEnumerator FinalDefensePattern()
    {
        for (int i = 0; i < 8; i++)
        {
            SpawnEnemy(2, GetRandomSpotFrom(allSpawnSpots));
        }

        while (true)
        {
            yield return new WaitForSeconds(10.0f);

            int enemyType = Random.Range(0, 2);
            SpawnEnemy(enemyType, GetRandomSpotFrom(allSpawnSpots));
        }
    }

    // ✅ ✅ ✅ 멀티 전용 스폰 방식 ✅ ✅ ✅
    void SpawnEnemy(int index, Transform spot)
    {
        if (!IsServer) return;
        if (spot == null) return;
        if (index < 0 || index >= enemyPrefabs.Length) return;

        GameObject enemy = Instantiate(enemyPrefabs[index], spot.position, spot.rotation);

        NetworkObject netObj = enemy.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();   // ✅ 모든 클라이언트에 동기화
        }
        else
        {
            Debug.LogError("Enemy 프리팹에 NetworkObject가 없습니다!");
        }
    }

    Transform GetRandomSpotFrom(SpawnSpot[] spots)
    {
        if (spots == null || spots.Length == 0) return null;
        return spots[Random.Range(0, spots.Length)].transform;
    }

    Transform GetSpotByDirection(string dir)
    {
        List<SpawnSpot> filtered = new List<SpawnSpot>();

        foreach (var spot in allSpawnSpots)
        {
            Vector3 pos = spot.transform.position;

            switch (dir)
            {
                case "Center":
                    if (pos.magnitude < 10.0f) filtered.Add(spot);
                    break;
                case "North":
                    if (pos.z > 10.0f) filtered.Add(spot);
                    break;
                case "West":
                    if (pos.x < -10.0f) filtered.Add(spot);
                    break;
                case "NorthWest":
                    if (pos.z > 10.0f || pos.x < -10.0f) filtered.Add(spot);
                    break;
            }
        }

        if (filtered.Count > 0)
        {
            return filtered[Random.Range(0, filtered.Count)].transform;
        }

        return GetRandomSpotFrom(allSpawnSpots);
    }
}
