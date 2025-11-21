using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 리스트 필터링을 위해 사용

public class EnemyS : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    // 0: Fighter(기본/정찰), 1: Bomber(폭격), 2: Suicide(자폭)
    public GameObject[] enemyPrefabs;

    [Header("Settings")]
    public float waveInterval = 5.0f; // 웨이브 사이 간격
    public float spawnInterval = 1.0f; // 적 생성 사이 간격 (웨이브 내)

    private SpawnSpot[] allSpawnSpots;
    private int currentProcessingDay = -1; // 현재 진행 중인 날짜 체크용
    private bool isNight = false; // 밤/낮 구분 (Timer에서 가져오거나 설정 필요)

    void Start()
    {
        allSpawnSpots = FindObjectsOfType<SpawnSpot>();

        if (allSpawnSpots.Length == 0)
        {
            Debug.LogError("씬에 SpawnSpot이 없습니다!");
        }
    }

    void Update()
    {
        int day = Timer.Instance.currentDay;

        if (day == 2 && Timer.Instance.currentHour > 20)
        {
            day = 21
        }

        if (currentProcessingDay != day)
        {
            StopAllCoroutines();
            currentProcessingDay = day;
            StartCoroutine(LevelRoutine(day));
        }
    }

    // 날짜별 시나리오 분기
    IEnumerator LevelRoutine(int day)
    {
        Debug.Log($"Day {day} 패턴 시작");

        switch (day)
        {
            case 0: // 튜토리얼
                yield return StartCoroutine(TutorialPattern());
                break;

            case 1: // 메인 1일차 (낮)
            case 2: // 메인 2일차 (낮)
                yield return StartCoroutine(DayPatternCommon(4));
                break;

            case 21: // 메인 2-1 (2일차 밤)
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

    // --- [패턴 로직 구현] ---

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
                SpawnEnemy(0, spot);
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
                SpawnEnemy(0, spot);
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

            SpawnEnemy(0, northSpot);
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy(0, northSpot);

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

    // --- [유틸리티 함수] ---

    void SpawnEnemy(int index, Transform spot)
    {
        if (spot == null) return;
        if (index < 0 || index >= enemyPrefabs.Length) return;

        Instantiate(enemyPrefabs[index], spot.position, spot.rotation);
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