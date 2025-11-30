

using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingPanel;   // 엔딩 패널 (검은 배경 + 텍스트 등)
    public Text endingText;          // 엔딩 문구 띄울 Text

    [Header("게임 설정")]
    public int maxDay = 4;           // 버텨야 하는 일 수 (4일)

    [Header("사운드 설정")]
    public AudioSource bgmSource;    // 엔딩 BGM 재생용 AudioSource
    public AudioClip badEndingClip;  // 배드 엔딩 BGM
    public AudioClip goodEndingClip; // 해피 엔딩 BGM

    private bool player1Dead = false;
    private bool player2Dead = false;
    private int currentDay = 0;
    private bool isEndingShown = false;

    private void Start()
    {
        // 시작할 때 엔딩 패널 안 보이게
        if (endingPanel != null)
            endingPanel.SetActive(false);
    }

    /// <summary>
    /// 플레이어가 죽었을 때 호출해 줄 함수
    /// playerIndex: 1이면 플레이어1, 2이면 플레이어2
    /// </summary>
    public void OnPlayerDead(int playerIndex)
    {
        if (isEndingShown) return;

        if (playerIndex == 1)
            player1Dead = true;
        else if (playerIndex == 2)
            player2Dead = true;

        // 둘 중 한 명이라도 죽으면 배드엔딩
        if (player1Dead || player2Dead)
        {
            ShowBadEnding();
        }
    }

    /// <summary>
    /// 하루가 지나갈 때 (예: 밤 → 아침이 될 때) 호출해 줄 함수
    /// </summary>
    public void OnDayChanged(int newDay)
    {
        if (isEndingShown) return;

        currentDay = newDay;

        // 4일을 다 버티면 해피엔딩
        if (currentDay >= maxDay && !player1Dead && !player2Dead)
        {
            ShowGoodEnding();
        }
    }

    private void ShowBadEnding()
    {
        isEndingShown = true;

        if (endingPanel != null)
            endingPanel.SetActive(true);

        if (endingText != null)
        {
            endingText.text =
                "나는 끝까지 싸웠어도, 내 전우가 죽으면 아무 의미 없다.\n" +
                "다 끝났다. 난 뭘 위해 싸웠던것일까?";
        }

        PlayEndingBgm(badEndingClip);
        Time.timeScale = 0f; // 게임 멈추고 싶을 때
    }

    private void ShowGoodEnding()
    {
        isEndingShown = true;

        if (endingPanel != null)
            endingPanel.SetActive(true);

        if (endingText != null)
        {
            endingText.text =
                "우리는 유기당한 상태에서 끝끝내 버티는데 성공하였다.\n" +
                "우리를 도와줄 지원군이 도착하였고, 우리는 승리할 수 있었다.";
        }

        PlayEndingBgm(goodEndingClip);
        Time.timeScale = 0f; // 게임 멈추고 싶을 때
    }

    private void PlayEndingBgm(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = false; // 필요하면 true로 변경
        bgmSource.Play();
    }
}
