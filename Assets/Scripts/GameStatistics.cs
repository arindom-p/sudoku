using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class GameStatistics : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public GameObject easyPanel, contentPanel;
    private GameObject mediumPanel, hardPanel;
    private DataKeyCollection dataKeyCollection = DataKeyCollection.GetObject();
    private ScrollRect scrollRect;
    private int _movingState;
    private float _duration = 0.3f;
    private Transform[] _panelTransforms = new Transform[3];

    private Text[] _gamesStarted = new Text[3];
    private Text[] _gamesWon = new Text[3];
    private Text[] _winRate = new Text[3];
    private Text[] _winRateThisWeek = new Text[3];
    private Text[] _bestTime = new Text[3];
    private Text[] _averageTime = new Text[3];
    private Text[] _bestScore = new Text[3];
    private Text[] _averageScore = new Text[3];
    private Text[] _currentWinStreak = new Text[3];
    private Text[] _bestWinStreak = new Text[3];
    private Text[] _winsWithNoMistakes = new Text[3];
    private string[] _levelNames = new string[] {"_easy", "_medium", "_hard"};

    void Awake()
    {
        RectTransform rt;
        scrollRect = GetComponent<ScrollRect>();

        mediumPanel = Instantiate(easyPanel);
        mediumPanel.name = "Medium";
        mediumPanel.transform.SetParent(easyPanel.transform.parent);
        rt = mediumPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0);
        rt.anchorMax = new Vector2(0.65f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        hardPanel = Instantiate(easyPanel);
        hardPanel.name = "Hard";
        hardPanel.transform.SetParent(easyPanel.transform.parent);
        rt = hardPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.69f, 0);
        rt.anchorMax = new Vector2(0.98f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _panelTransforms[0] = easyPanel.transform;
        _panelTransforms[1] = mediumPanel.transform;
        _panelTransforms[2] = hardPanel.transform;

        contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / (float)Screen.height * Camera.main.orthographicSize * 6, 12.1f);

        _movingState = 0;

        for (int i = 0; i < 3; i++)
        {
            _gamesStarted[i] = _panelTransforms[i].Find("Games/Games Started").GetComponent<Text>();
            _gamesWon[i] = _panelTransforms[i].Find("Games/Games Won").GetComponent<Text>();
            _winRate[i] = _panelTransforms[i].Find("Games/Win Rate").GetComponent<Text>();
            _winRateThisWeek[i] = _panelTransforms[i].Find("Games/Win Rate This Week").GetComponent<Text>();
            _bestTime[i] = _panelTransforms[i].Find("Time/Best Time").GetComponent<Text>();
            _averageTime[i] = _panelTransforms[i].Find("Time/Average Time").GetComponent<Text>();
            _bestScore[i] = _panelTransforms[i].Find("Scores/Best Score").GetComponent<Text>();
            _averageScore[i] = _panelTransforms[i].Find("Scores/Average Score").GetComponent<Text>();
            _currentWinStreak[i] = _panelTransforms[i].Find("Streaks/Current Win Streak").GetComponent<Text>();
            _bestWinStreak[i] = _panelTransforms[i].Find("Streaks/Best Win Streak").GetComponent<Text>();
            _winsWithNoMistakes[i] = _panelTransforms[i].Find("Streaks/Wins With No Mistakes").GetComponent<Text>();
        }
    }
    
    private void OnEnable()
    {
        SetTheme();
        scrollRect.horizontal = true;
        scrollRect.vertical = true;
        contentPanel.transform.position = (Vector3.right + Vector3.down) * 100;
        UpdatePanels();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_movingState == 0) //not moving toward any direction
        {
            if (Mathf.Abs(eventData.pressPosition.x - eventData.position.x) > Mathf.Abs(eventData.pressPosition.y - eventData.position.y))
            {
                _movingState = 1;
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                scrollRect.inertia = false;
            }
            else
            {
                _movingState = 2;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.inertia = true;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_movingState == 1)
        {
            float minDistance = Mathf.Abs(_panelTransforms[0].position.x - transform.position.x);
            int index = 0;
            for(int i = 1; i < _panelTransforms.Length; i++)
            {
                if(minDistance > Mathf.Abs(_panelTransforms[i].position.x - transform.position.x))
                {
                    index = i;
                    minDistance = Mathf.Abs(_panelTransforms[i].position.x - transform.position.x);
                }
            }
            minDistance = _panelTransforms[index].position.x - transform.position.x;
            contentPanel.transform.DOMoveX(contentPanel.transform.position.x - minDistance, _duration);

            LevelTexts[index].DOColor(Color.white, _duration);
            for(int i = 0; i < LevelTexts.Length; i++)
                if(i != index) LevelTexts[i].DOColor(Color.black, _duration);
        }

        _movingState = 0;
    }

    private void UpdatePanels()
    {
        string level;
        for (int i = 0; i < 3; i++)
        {
            if (i == 0) level = dataKeyCollection.easy;
            else if (i == 1) level = dataKeyCollection.medium;
            else level = dataKeyCollection.hard;

            int gameStarted = PlayerPrefs.GetInt(dataKeyCollection.numberOfGamesStarted + level, 0);
            int won = PlayerPrefs.GetInt(dataKeyCollection.gamesWon + level, 0);

            _gamesStarted[i].text = (gameStarted + (PlayerPrefs.GetInt(dataKeyCollection.resumeAvailable, 0) != 0 ?
                (PlayerPrefs.GetInt(dataKeyCollection.gameLevel, 0) == (i + 1) ? 1 : 0) : 0)).ToString();
            _gamesWon[i].text = won.ToString();
            _winRate[i].text = Math.Round((won / (float)(gameStarted == 0 ? 1 : gameStarted)) * 100) + " %";
            _winRateThisWeek[i].text = Math.Round(WinRateThisWeek(i), 1) + " %";

            int sec = PlayerPrefs.GetInt(dataKeyCollection.bestTime + level, 0);
            int hour = sec / 3600;  sec %= 3600;
            int min = sec / 60;  sec %= 60;
            _bestTime[i].text = (hour > 0 ? hour + ":" : "") + (min > 0 ? min.ToString().PadLeft(2, '0') + ":" : "") + (sec.ToString().PadLeft(2, '0'));

            sec = PlayerPrefs.GetInt(dataKeyCollection.totalGameWonTime + level, 0) / (won == 0 ? 1 : won);
            hour = sec / 3600; sec %= 3600;
            min = sec / 60; sec %= 60;
            _averageTime[i].text = (hour > 0 ? hour + ":" : "") + (min > 0 ? min.ToString().PadLeft(2, '0') + ":" : "") + (sec.ToString().PadLeft(2, '0'));

            _bestScore[i].text = PlayerPrefs.GetInt(dataKeyCollection.bestScore + level, 0).ToString();
            _averageScore[i].text = (PlayerPrefs.GetInt(dataKeyCollection.totalScore + level, 0) / (float)(won == 0 ? 1 : won)).ToString();
            _currentWinStreak[i].text = PlayerPrefs.GetInt(dataKeyCollection.currentWinStreak + level, 0).ToString();
            _bestWinStreak[i].text = PlayerPrefs.GetInt(dataKeyCollection.besttWinStreak + level, 0).ToString();
            _winsWithNoMistakes[i].text = PlayerPrefs.GetInt(dataKeyCollection.winsWithNoMistakes + level, 0).ToString();
        }
    }

    private float WinRateThisWeek(int levelIndex)
    {
        int i, started = 0, won = 0;
        string data = PlayerPrefs.GetString(dataKeyCollection.dataOfThisWeek + _levelNames[levelIndex], "");
        for (i = 0; i < data.Length; i += 16)
        {
            if ((DateTime.Now - DateTime.ParseExact(data.Substring(i, 10), "MM/dd/yyyy", null)).Days > 7)
            {
                PlayerPrefs.SetString(dataKeyCollection.dataOfThisWeek + _levelNames[levelIndex], data.Substring(0, i));
                break;
            }
            started += int.Parse(data.Substring(i + 10, 3));
            won += int.Parse(data.Substring(i + 13, 3));
        }
        return won / (float)(started == 0 ? 1 : started) * 100;
    }

    public void ClearStatistics()
    {
        int index;
        if (contentPanel.transform.position.x > 0) index = 0;
        else if (contentPanel.transform.position.x < 0) index = 2;
        else index = 1;

        PlayerPrefs.DeleteKey(dataKeyCollection.numberOfGamesStarted + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.gamesWon            + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.dataOfThisWeek      + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.bestTime            + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.totalGameWonTime    + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.bestScore           + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.totalScore          + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.currentWinStreak    + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.besttWinStreak      + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.winsWithNoMistakes  + _levelNames[index]);
        PlayerPrefs.DeleteKey(dataKeyCollection.gameStarted         + _levelNames[index]);
        UpdatePanels();
    }

    #region seting theme function for statistics panel
    public Text[] LevelTexts;
    private void SetTheme()
    {
        LevelTexts[0].color = Color.white;
        LevelTexts[1].color = Color.black;
        LevelTexts[2].color = Color.black;
    }
    #endregion
}
