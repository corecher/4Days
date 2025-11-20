using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer Instance { get; private set; }

    public float dayLengthInMinutes = 10f;

    public float gameTime = 0f;
    private int startDay = 1;
    private int startHour = 0;
    private int startMinute = 0;
    private int startSecond = 0;

    public int currentDay { get; private set; }
    public int currentHour { get; private set; }
    public int currentMinute { get; private set; }
    public int currentSecond { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        gameTime = startHour * 3600f + startMinute * 60f + startSecond;
        currentDay = startDay;
        UpdateTimeFromGameTime();
    }

    // Update is called once per frame
    void Update()
    {
        float timeMultiplier = 86400f / (dayLengthInMinutes * 60f);

        gameTime += Time.deltaTime * timeMultiplier;

        while (gameTime >= 86400f)
        {
            gameTime -= 86400f;
            currentDay += 1;
        }

        UpdateTimeFromGameTime();

    }

    private void UpdateTimeFromGameTime()
    {
        currentHour = (int)(gameTime / 3600f);
        currentMinute = (int)((gameTime % 3600f) / 60f);
        currentSecond = (int)(gameTime % 60f);
    }

    public string GetTimeString()
    {
        return $"Day {currentDay:D2} - {currentHour:D2}:{currentMinute:D2}:{currentSecond:D2}";
    }
}
