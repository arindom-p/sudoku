using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class GameController : GameControllerHelper
{
    public GameObject WorldSpace, GamePanel, StartPanel, GameoverPanel, GamePausePanel, RestartPanel, BottomWindowSelectionPanel, StatisticsPanel, ResumeButton;
    public ThemeHandler themeHandler;
    public Text GamePanelTimeText, HintText, MistakeText, ScoreText;
    [HideInInspector] public bool InEditMode, GamePaused;
    [HideInInspector] public int SelectedMode, SelectedIndex, NumberOfCompleteGames = 0;
    public struct ThemeObject
    {
        public Transform primaryTransform, secondaryTransform, tempTransform;
        public SpriteRenderer primarySR;
    }
    public ThemeObject[] ThemeObjects = new ThemeObject[9];
    public struct ActivityTracker
    {
        public int type, index, score;
        public string data;
    }
    private ActivityTracker _activityTracker;
    public Stack activityTrackerStack = new Stack();

    private int[] _levels = new int[] { -1, 55, 40, 25 };
    private int[] _scoresInGrid = new int[81];
    private string[] _levelNames = new string[] { null, "_easy", "_medium", "_hard" };
    private int _tappedButtonIndex, _numberOfMistakes, _mistakeLimit = 3, _availableHint, _currentLevel;
    private bool _gameOver, _gridDataAvailable, _noMistake;
    public Coroutine co_Time;

    private void Awake() => Start1(); //as a Start() method of parent class
    private void Start()
    {
        float worldHeightScale = Camera.main.orthographicSize * 2;
        RectTransform rt = WorldSpace.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(Screen.width / (float) Screen.height * worldHeightScale, worldHeightScale);

        rt = UpperTransform.GetComponent<RectTransform>();
        gameData.UpperSquareSize = Mathf.Min(rt.rect.width, rt.rect.height) / 9;
        rt = LowerTransform.GetComponent<RectTransform>();
        gameData.LowerSquareSize = Mathf.Min(rt.rect.width / 5, rt.rect.height / 2);
        gameData.UpperSquareScale = new Vector3(gameData.UpperSquareSize, gameData.UpperSquareSize);
        gameData.LowerSquareScale = new Vector2(gameData.LowerSquareSize, gameData.LowerSquareSize);

        CreateGrid();
        CurrentWindow("Home");
        SetTheme(false);

        _gridDataAvailable = false;
        _currentLevel = -1;
    }

    public void StartGame(int n)
    {
        _gameOver = false;
        _noMistake = true;
        _availableHint = 1;
        _numberOfMistakes = 0;
        _gridDataAvailable = true;
        currentGamePlayingTime = 0;
        SelectedMode = 0;
        InEditMode = false;
        if (co_Time != null) StopCoroutine(co_Time);
        GamePaused = true;
        TogglePausePhase();
        UpdateHintAndMistake();
        GameoverPanel.SetActive(false);
        RestartPanel.SetActive(false);
        CurrentWindow("Game");
        activityTrackerStack.Clear();

        if (n > 0)
        {
            int levelIndex = PlayerPrefs.GetInt(dataKeyCollection.gameLevel, 0);
            if (PlayerPrefs.GetInt(dataKeyCollection.resumeAvailable, 0) != 0 && levelIndex != 0)
            {
                PlayerPrefs.SetInt(dataKeyCollection.numberOfGamesStarted + _levelNames[levelIndex],
                    PlayerPrefs.GetInt(dataKeyCollection.numberOfGamesStarted + _levelNames[levelIndex], 0) + 1);
                UpdateThisWeekGameData(1, 0);
                PlayerPrefs.SetInt(dataKeyCollection.currentWinStreak + _levelNames[levelIndex], 0);
            }
            _currentLevel = n;

            if (PlayerPrefs.GetInt(dataKeyCollection.gameStarted + _levelNames[1], 0) != 0) PlayerPrefs.SetInt(dataKeyCollection.currentWinStreak + _levelNames[1], 0);
            else if (PlayerPrefs.GetInt(dataKeyCollection.gameStarted + _levelNames[2], 0) != 0) PlayerPrefs.SetInt(dataKeyCollection.currentWinStreak + _levelNames[2], 0);
            else if (PlayerPrefs.GetInt(dataKeyCollection.gameStarted + _levelNames[3], 0) != 0) PlayerPrefs.SetInt(dataKeyCollection.currentWinStreak + _levelNames[3], 0);
        }
        LoadNewGame(_levels[n]);
        SetTheme(false);

        if (n > 0) AnimateLine(0.5f);
        else AnimateUpperGround();

        for (int i = 0; i < 81; i++)
            _scoresInGrid[i] = 0;
        UpdateScore();
    }

    public void CurrentWindow(string panelName)
    {
        switch (panelName)
        {
            case "Home":
                if (co_Time != null) StopCoroutine(co_Time);
                StartPanel.SetActive(true);
                GamePanel.SetActive(false);
                StatisticsPanel.SetActive(false);
                BottomWindowSelectionPanel.SetActive(true);
                if (_gridDataAvailable) SaveData();
                ResumeButton.SetActive(PlayerPrefs.GetInt(dataKeyCollection.resumeAvailable, 0) != 0);
                break;
            case "Game":
                StartPanel.SetActive(false);
                GamePanel.SetActive(true);
                StatisticsPanel.SetActive(false);
                BottomWindowSelectionPanel.SetActive(false);
                break;
            case "Statistics":
                StartPanel.SetActive(false);
                GamePanel.SetActive(false);
                StatisticsPanel.SetActive(true);
                BottomWindowSelectionPanel.SetActive(true);
                break;
        }
    }

    public void ResumeLastGame()
    {
        _gameOver = false;
        _gridDataAvailable = true;
        _availableHint = PlayerPrefs.GetInt(dataKeyCollection.availableHint, -1);
        _numberOfMistakes = PlayerPrefs.GetInt(dataKeyCollection.numberOfMistake, -1);
        _currentLevel = PlayerPrefs.GetInt(dataKeyCollection.gameLevel, -1);
        currentGamePlayingTime = PlayerPrefs.GetInt(dataKeyCollection.timeLeft, -1);
        SelectedMode = 0;
        InEditMode = false;
        if (co_Time != null) StopCoroutine(co_Time);
        GamePaused = true;
        TogglePausePhase();
        UpdateHintAndMistake();
        GameoverPanel.SetActive(false);
        RestartPanel.SetActive(false);
        CurrentWindow("Game");
        activityTrackerStack.Clear();
        RetrivePreviousGameData();
        UpdateScore();
        SetTheme(false);
        AnimateLine(0.5f);
    }

    private void RetrivePreviousGameData()
    {
        string data = PlayerPrefs.GetString(dataKeyCollection.previousGridData);
        int i, ind;
        for (i = ind = 0; i < data.Length; ind++)
        {
            if (data[i] == '0') //nothing at position 'ind'
            {
                gameData.UpperPanelObjectTexts[ind].first.text = "";
                gameData.UpperPanelObjectTexts[ind].second.text = "";
                i++;
            }
            else if (data[i] == '1') //data available
            {
                gameData.UpperPanelObjectTexts[ind].first.text = data[i + 1] + "";
                gameData.UpperPanelObjectTexts[ind].second.text = "";
                i += 2;
            }
            else if (data[i] == '2') //data as note (in edit mode)
            {
                gameData.UpperPanelObjectTexts[ind].first.text = "";
                gameData.UpperPanelObjectTexts[ind].second.text = data.Substring(i + 2, data[i + 1] - '0');
                i += data[i + 1] - '0' + 2;
            }
            else { print("error in type, where i = " + i + ", ind = " + ind + ", the invalid ascii is " + (int)data[i]); break; }
        }

        data = PlayerPrefs.GetString(dataKeyCollection.fixedGridPositions);
        for (i = 0; i < data.Length; i++)
            isFixed[i] = (data[i] == '1');

        data = PlayerPrefs.GetString(dataKeyCollection.stackData);
        for (i = 0; i < data.Length;)
        {
            _activityTracker.type = data[i] - '0';
            _activityTracker.index = int.Parse(data.Substring(i + 1, 2));
            _activityTracker.data = data.Substring(i + 5, data[i + 3] - '0');
            _activityTracker.score = int.Parse(data.Substring(i + 5 + data[i + 3] - '0', data[i + 4] - '0'));
            activityTrackerStack.Push(_activityTracker);
            i += 5 + data[i + 3] - '0' + data[i + 4] - '0';
        }

        data = PlayerPrefs.GetString(dataKeyCollection.solusion);
        for (i = 0; i < data.Length; i++)
            Solution[i] = data[i] - '0';

        data = PlayerPrefs.GetString(dataKeyCollection.currentScores, "");
        for(i = ind = 0; i < data.Length; ind++)
        {
            _scoresInGrid[ind] = int.Parse(data.Substring(i + 1, data[i] - '0'));
            i += data[i] - '0' + 1;
        }

        _noMistake = PlayerPrefs.GetInt(dataKeyCollection.noMistakeFlag, 0) == 1;
    }
    
    private void SaveData()
    {
        PlayerPrefs.SetInt(dataKeyCollection.resumeAvailable, 1);
        string data = "";
        int i;
        for (i = 0; i < 81; i++)
        {
            if (gameData.UpperPanelObjectTexts[i].first.text == "")
            {
                if (gameData.UpperPanelObjectTexts[i].second.text == "") data += '0';
                else data += "2" + gameData.UpperPanelObjectTexts[i].second.text.Length + gameData.UpperPanelObjectTexts[i].second.text;
            }
            else data += "1" + gameData.UpperPanelObjectTexts[i].first.text;
        }
        PlayerPrefs.SetString(dataKeyCollection.previousGridData, data);

        data = "";
        for (i = 0; i < 81; i++)
        {
            data += (isFixed[i] == true ? "1" : "0");
        }
        PlayerPrefs.SetString(dataKeyCollection.fixedGridPositions, data);

        data = "";
        while(activityTrackerStack.Count != 0)
        {
            _activityTracker = (ActivityTracker) activityTrackerStack.Pop();
            data = _activityTracker.type + _activityTracker.index.ToString().PadLeft(2, '0') + _activityTracker.data.Length + _activityTracker.score.ToString().Length +
                _activityTracker.data + _activityTracker.score + data;
        }
        PlayerPrefs.SetString(dataKeyCollection.stackData, data);

        data = "";
        for (i = 0; i < 81; i++)
            data += Solution[i];
        PlayerPrefs.SetString(dataKeyCollection.solusion, data);
        
        data = "";
        for (i = 0; i < 81; i++)
        {
            data += _scoresInGrid[i].ToString().Length + _scoresInGrid[i].ToString();
          //  print("int " + _scoresInGrid[i] + ", and " + _scoresInGrid[i].ToString().Length + _scoresInGrid[i]);
        }
        PlayerPrefs.SetString(dataKeyCollection.currentScores, data);

        PlayerPrefs.SetInt(dataKeyCollection.availableHint, _availableHint);
        PlayerPrefs.SetInt(dataKeyCollection.numberOfMistake, _numberOfMistakes);
        PlayerPrefs.SetInt(dataKeyCollection.timeLeft, currentGamePlayingTime);
        PlayerPrefs.SetInt(dataKeyCollection.gameLevel, _currentLevel);
        PlayerPrefs.SetInt(dataKeyCollection.noMistakeFlag, (_noMistake == true ? 1 : 0));
    }

    public void Hint()
    {
        if (GamePaused || SelectedMode != 1 || _availableHint < 1 || gameData.UpperPanelObjectTexts[SelectedIndex].first.text != "") return;
        _availableHint--;
        UpdateHintAndMistake();
        gameData.UpperPanelObjectTexts[SelectedIndex].first.text = Solution[SelectedIndex].ToString();
        HighlightBlocks(Solution[SelectedIndex], SelectedIndex, true);
        if (ShouldCheckForCompletion()) CongratulateUser();
    }

    public void WatchAd()
    {
        _gameOver = false;
        GameoverPanel.SetActive(false);
        ResumeAfterWatchingAd();
    }

    private void ResumeAfterWatchingAd()
    {
        _numberOfMistakes = 0;
        UpdateHintAndMistake();
        co_Time = StartCoroutine(TimeCounter(GamePanelTimeText, currentGamePlayingTime));
    }

    private void UpdateHintAndMistake()
    {
        HintText.text = "Hint\n(" + _availableHint + ")";
        MistakeText.text = "Mistakes : " + _numberOfMistakes + "/" + _mistakeLimit;
    }

    private void WarnForWrongInput(int ind)
    {
        _numberOfMistakes++;
        if (_numberOfMistakes >= _mistakeLimit) GameOver();
        UpdateHintAndMistake();
        gameData.UpperObjectBGSpriteRenderers[ind].color = Color.red;
        gameData.UpperObjectBackgrounds[ind].transform.localScale = Vector2.zero;
        gameData.UpperObjectBackgrounds[ind].transform.DOScale(Vector2.one, 0.2f);
        _noMistake = false;
    }

    private void GameOver()
    {
        _gameOver = true;
        StopCoroutine(co_Time);
        GameoverPanel.SetActive(true);
    }

    private void CongratulateUser()
    {
        for(int i = 0; i < 81; i++)
            if (gameData.UpperPanelObjectTexts[i].first.text != Solution[i].ToString()) return;

        _gridDataAvailable = false;
        StopAllCoroutines();
        GamePanelTimeText.text = "Congratulations!";
        NumberOfCompleteGames++;
        
        PlayerPrefs.SetInt(dataKeyCollection.numberOfGamesStarted + _levelNames[_currentLevel],
            PlayerPrefs.GetInt(dataKeyCollection.numberOfGamesStarted + _levelNames[_currentLevel], 0) + 1);
        PlayerPrefs.SetInt(dataKeyCollection.gamesWon + _levelNames[_currentLevel],
            PlayerPrefs.GetInt(dataKeyCollection.gamesWon + _levelNames[_currentLevel], 0) + 1);
        PlayerPrefs.SetInt(dataKeyCollection.resumeAvailable, 0);

        int time = PlayerPrefs.GetInt(dataKeyCollection.bestTime + _levelNames[_currentLevel], -1);
        if(time < 0 || time > currentGamePlayingTime)
            PlayerPrefs.SetInt(dataKeyCollection.bestTime + _levelNames[_currentLevel], currentGamePlayingTime);
        PlayerPrefs.SetInt(dataKeyCollection.totalGameWonTime + _levelNames[_currentLevel],
            PlayerPrefs.GetInt(dataKeyCollection.totalGameWonTime + _levelNames[_currentLevel], 0) + currentGamePlayingTime);

        int streak = PlayerPrefs.GetInt(dataKeyCollection.currentWinStreak + _levelNames[_currentLevel], 0) + 1;
        PlayerPrefs.SetInt(dataKeyCollection.currentWinStreak + _levelNames[_currentLevel], streak);
        if (streak > PlayerPrefs.GetInt(dataKeyCollection.besttWinStreak + _levelNames[_currentLevel], 0))
            PlayerPrefs.SetInt(dataKeyCollection.besttWinStreak + _levelNames[_currentLevel], streak);

        if(_noMistake)
            PlayerPrefs.SetInt(dataKeyCollection.winsWithNoMistakes + _levelNames[_currentLevel],
                PlayerPrefs.GetInt(dataKeyCollection.winsWithNoMistakes + _levelNames[_currentLevel], 0) + 1);

        int score = 0;
        foreach(int aScore in _scoresInGrid)
            score += aScore;
        int bestScore = PlayerPrefs.GetInt(dataKeyCollection.bestScore + _levelNames[_currentLevel], 0);print(bestScore + " and current score is " + score);
        if(score > bestScore) PlayerPrefs.SetInt(dataKeyCollection.bestScore + _levelNames[_currentLevel], score);
        PlayerPrefs.SetInt(dataKeyCollection.totalScore + _levelNames[_currentLevel], score + PlayerPrefs.GetInt(dataKeyCollection.totalScore + _levelNames[_currentLevel], 0));

        UpdateThisWeekGameData(1, 1);

        _currentLevel = 0;
    }

    private void UpdateThisWeekGameData(int increaseStartedGame, int increaseWonGame)
    {
        if (_currentLevel < 1) _currentLevel = PlayerPrefs.GetInt(dataKeyCollection.gameLevel);
        string playingData = PlayerPrefs.GetString(dataKeyCollection.dataOfThisWeek + _levelNames[_currentLevel], "");
        if (playingData.Length < 10 || playingData.Length > 9 && DateTime.Now.ToString().Substring(10) != playingData.Substring(10))
        {
            playingData = DateTime.Now.ToString().Substring(0, 10) + increaseStartedGame.ToString().PadLeft(3, '0') + increaseWonGame.ToString().PadLeft(3, '0') + playingData;
        }
        else
        {
            int played = int.Parse(playingData.Substring(10, 3)) + increaseStartedGame;
            int won = int.Parse(playingData.Substring(13, 3)) + increaseWonGame;
            PlayerPrefs.SetString(dataKeyCollection.dataOfThisWeek,
                playingData.Substring(0, 10) + played.ToString().PadLeft(3, '0') + won.ToString().PadLeft(3, '0') + playingData.Substring(16));
        }
        int i;
        for (i = 0; i < playingData.Length; i+= 16)
        {
            if ((DateTime.Now - DateTime.ParseExact(playingData.Substring(i, 10), "MM/dd/yyyy", null)).Days > 7) break;
        }

        PlayerPrefs.SetString(dataKeyCollection.dataOfThisWeek + _levelNames[_currentLevel], playingData.Substring(0, i));
    }

    private int GenerateScore() => (int)(5 + (50 / (1 + currentGamePlayingTime / 5000f) + 40 / (1 + currentGamePlayingTime / 500f) + 30 / (1 + currentGamePlayingTime / 50f) + 20 / (1 + currentGamePlayingTime / 5f)));

    public void EditButtonPressed()
    {
        InEditMode = !InEditMode;
        if (InEditMode)
            editButtonBgImg.color = gameData.CurrentTheme.circleBg[3];
        else editButtonBgImg.color = Color.clear;
    }

    public void Undo()
    {
        if (GamePaused || activityTrackerStack.Count == 0) return;
        _activityTracker = (ActivityTracker) activityTrackerStack.Pop();
        if (_activityTracker.type == 1)
        {
            gameData.UpperPanelObjectTexts[_activityTracker.index].first.text = _activityTracker.data;
            gameData.UpperPanelObjectTexts[_activityTracker.index].second.text = "";
        }
        else
        {
            gameData.UpperPanelObjectTexts[_activityTracker.index].first.text = "";
            gameData.UpperPanelObjectTexts[_activityTracker.index].second.text = _activityTracker.data;
        }
        _scoresInGrid[_activityTracker.index] = _activityTracker.score;
        print("actual index " + _activityTracker.index + ", and " + _scoresInGrid[_activityTracker.index]);

        UpdateScore();
        HighlightBlocks(0, -1, true);
        SelectedMode = 0;
    }

    private void UpdateScore()
    {
        int i, total = 0;
        for(i = 0; i < 81; i++)
            total += _scoresInGrid[i];
        ScoreText.text = "Score : " + total;
    }

    public void TogglePausePhase()
    {
        GamePaused = !GamePaused;

        if (GamePaused)
        {
            pauseButtonImg.gameObject.SetActive(false);
            playButtonImg.gameObject.SetActive(true);
            GamePausePanel.SetActive(true);
            if(co_Time != null) StopCoroutine(co_Time);
        }
        else
        {
            pauseButtonImg.gameObject.SetActive(true);
            playButtonImg.gameObject.SetActive(false);
            GamePausePanel.SetActive(false);
            co_Time = StartCoroutine(TimeCounter(GamePanelTimeText, currentGamePlayingTime));
        }
    }

    public void ToggleRestartPanel()
    {
        RestartPanel.SetActive(!RestartPanel.activeSelf);
    }

    private bool ShouldCheckForCompletion()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (gameData.LowerPanelObjectTexts[i].second.text != "")
                return false;
        }
        return true;
    }

    public void ButtonClicked(string buttonName)
    {
        if (themeHandler.themeSelectionIconPressed || _gameOver || GamePaused) return;
        _tappedButtonIndex = int.Parse(buttonName.Substring(2));
        bool needToUpdateStack = false;
        switch (buttonName[0])
        {
            case 'e':
                if (SelectedMode == 2)
                {
                    if (isFixed[_tappedButtonIndex]) return;
                    bool contains = false;
                    if (InEditMode)
                    {
                        if (gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text != "")
                        {
                            _activityTracker.data = gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text;
                            _activityTracker.type = 2;
                            _activityTracker.index = _tappedButtonIndex;
                            _activityTracker.score = _scoresInGrid[_activityTracker.index];
                            _scoresInGrid[_activityTracker.index] = 0;
                            needToUpdateStack = true;
                            //      activityTrackerStack.Push(_activityTracker);
                            gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text = "";
                            gameData.UpperPanelObjectTexts[_tappedButtonIndex].second.text = SelectedIndex.ToString();
                            gameData.UpperObjectBGSpriteRenderers[_tappedButtonIndex].color = gameData.CurrentTheme.circleBg[2];
                        }
                        else
                        {
                            string str = gameData.UpperPanelObjectTexts[_tappedButtonIndex].second.text;
                            _activityTracker.data = str;
                            str = AddData(str, SelectedIndex, ref contains);
                            gameData.UpperPanelObjectTexts[_tappedButtonIndex].second.text = str;
                            gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text = "";
                            if (str != _activityTracker.data)
                            {
                                _activityTracker.type = 2;
                                _activityTracker.index = _tappedButtonIndex;
                                _activityTracker.score = _scoresInGrid[_activityTracker.index];
                                _scoresInGrid[_activityTracker.index] = 0;
                                needToUpdateStack = true;
                                //      activityTrackerStack.Push(_activityTracker);
                            }
                        }
                    }
                    else
                    {
                        if (gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text == "")
                        {
                            _activityTracker.data = gameData.UpperPanelObjectTexts[_tappedButtonIndex].second.text;
                            _activityTracker.type = 2;
                        }
                        else
                        {
                            _activityTracker.data = gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text;
                            _activityTracker.type = 1;

                            Text aText = gameData.LowerPanelObjectTexts[int.Parse(_activityTracker.data)].second;
                            aText.text = (aText.text == "-1" ? "" : ((aText.text == "" ? 0 : int.Parse(aText.text)) + 1).ToString());
                        }
                        _activityTracker.index = _tappedButtonIndex;
                        needToUpdateStack = true;
                        //    activityTrackerStack.Push(_activityTracker);

                        if (gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text == SelectedIndex.ToString())
                        {
                            gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text = "";
                            contains = false;
                        }
                        else
                        {
                            gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text = SelectedIndex.ToString();
                            contains = true;
                        }
                        gameData.UpperPanelObjectTexts[_tappedButtonIndex].second.text = "";

                        if (gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text != "")
                        {
                            Text dummyText = gameData.LowerPanelObjectTexts[SelectedIndex].second;
                            dummyText.text = (dummyText.text == "1" ? "" : ((dummyText.text == "" ? 0 : int.Parse(dummyText.text)) - 1).ToString());
                        }

                        _activityTracker.score = _scoresInGrid[_activityTracker.index];
                        if (gameData.UpperPanelObjectTexts[_activityTracker.index].first.text == Solution[_activityTracker.index].ToString()) _scoresInGrid[_activityTracker.index] = GenerateScore();
                        else _scoresInGrid[_activityTracker.index] = 0;
                    }
                    if (contains)
                    {
                        if (!InEditMode && gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text != Solution[_tappedButtonIndex].ToString())
                            WarnForWrongInput(_tappedButtonIndex);
                        else
                        {
                            StartCoroutine(HighlightHelper(_tappedButtonIndex, true, 1, 0));
                            if (!InEditMode && ShouldCheckForCompletion()) CongratulateUser();
                        }
                    }
                    else StartCoroutine(HighlightHelper(_tappedButtonIndex, true, 0, 0));
                }
                else
                {
                    if ((SelectedMode != 0 && gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text != "" &&
                        gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text == gameData.UpperPanelObjectTexts[SelectedIndex].first.text) ||
                        (SelectedMode == 1 && SelectedIndex == _tappedButtonIndex))
                    {
                        SelectedMode = 0;
                        HighlightBlocks(0, -1, true);
                    }
                    else
                    {
                        if (gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text != "")
                        {
                            if (isFixed[_tappedButtonIndex])
                            {
                                SelectedMode = 3;
                                StartCoroutine(HighlightHelper(_tappedButtonIndex, true, 2, 0));
                            }
                            else
                            {
                                SelectedMode = 1;
                                StartCoroutine(HighlightHelper(_tappedButtonIndex, true, 3, 0));
                            }
                            SelectedIndex = _tappedButtonIndex;
                            HighlightBlocks(int.Parse(gameData.UpperPanelObjectTexts[_tappedButtonIndex].first.text), _tappedButtonIndex, true);
                        }
                        else
                        {
                            SelectedMode = 1;
                            SelectedIndex = _tappedButtonIndex;
                            StartCoroutine(HighlightHelper(_tappedButtonIndex, true, 3, 0));
                            HighlightBlocks(0, _tappedButtonIndex, true);
                        }
                    }
                }
                break;

            case 'i':
                if (_tappedButtonIndex == 0)
                {
                    if (SelectedMode != 1 || (SelectedMode == 1 && gameData.UpperPanelObjectTexts[SelectedIndex].first.text == "" &&
                        gameData.UpperPanelObjectTexts[SelectedIndex].second.text == "")) return;

                    if (gameData.UpperPanelObjectTexts[SelectedIndex].first.text == "")
                    {
                        _activityTracker.data = gameData.UpperPanelObjectTexts[SelectedIndex].second.text;
                        _activityTracker.type = 2;
                    }
                    else
                    {
                        _activityTracker.data = gameData.UpperPanelObjectTexts[SelectedIndex].first.text;
                        _activityTracker.type = 1;
                        int ind = int.Parse(gameData.UpperPanelObjectTexts[SelectedIndex].first.text);
                        string str = gameData.LowerPanelObjectTexts[ind].second.text;
                        int newValue = (str == "" ? 0 : int.Parse(str));
                        gameData.LowerPanelObjectTexts[ind].second.text = "" + (newValue == -1 ? "" : (newValue + 1).ToString());
                        gameData.UpperObjectBGSpriteRenderers[SelectedIndex].color = gameData.CurrentTheme.circleBg[3];
                    }
                    _activityTracker.index = SelectedIndex;
                    _activityTracker.score = _scoresInGrid[_activityTracker.index];
                    _scoresInGrid[_activityTracker.index] = 0;
                    needToUpdateStack = true;
                    //   activityTrackerStack.Push(_activityTracker);

                    gameData.UpperPanelObjectTexts[SelectedIndex].first.text = "";
                    gameData.UpperPanelObjectTexts[SelectedIndex].second.text = "";
                }
                else //---------------- for all lower ground buttons
                {
                    if (SelectedMode == 1)
                    {
                        gameData.UpperObjectBGSpriteRenderers[SelectedIndex].transform.DOScale(Vector2.one * 1.2f, 0.1f).OnComplete(() =>
                            gameData.UpperObjectBGSpriteRenderers[SelectedIndex].transform.DOScale(Vector2.one, 0.1f));
                        bool contains = false;
                        if (InEditMode)
                        {
                            string str = gameData.UpperPanelObjectTexts[SelectedIndex].second.text;
                            _activityTracker.data = str;
                            str = AddData(str, _tappedButtonIndex, ref contains);
                            gameData.UpperPanelObjectTexts[SelectedIndex].second.text = str;
                            gameData.UpperPanelObjectTexts[SelectedIndex].first.text = "";
                            if (str != _activityTracker.data)
                            {
                                _activityTracker.type = 2;
                                _activityTracker.index = SelectedIndex;
                                _activityTracker.score = _scoresInGrid[_activityTracker.index];
                                _scoresInGrid[_activityTracker.index] = 0;
                                needToUpdateStack = true;
                                //        activityTrackerStack.Push(_activityTracker);
                            }
                        }
                        else
                        {
                            if (gameData.UpperPanelObjectTexts[SelectedIndex].first.text == "")
                            {
                                _activityTracker.data = gameData.UpperPanelObjectTexts[SelectedIndex].second.text;
                                _activityTracker.type = 2;
                            }
                            else
                            {
                                _activityTracker.data = gameData.UpperPanelObjectTexts[SelectedIndex].first.text;
                                _activityTracker.type = 1;
                            }
                            _activityTracker.index = SelectedIndex;
                            needToUpdateStack = true;
                     //       activityTrackerStack.Push(_activityTracker);

                            if (gameData.UpperPanelObjectTexts[SelectedIndex].first.text == _tappedButtonIndex.ToString())
                            {
                                gameData.UpperPanelObjectTexts[SelectedIndex].first.text = "";
                                contains = false;
                            }
                            else
                            {
                                gameData.UpperPanelObjectTexts[SelectedIndex].first.text = _tappedButtonIndex.ToString();
                                contains = true;
                            }
                            gameData.UpperPanelObjectTexts[SelectedIndex].second.text = "";
                        }
                        if (contains) HighlightBlocks(_tappedButtonIndex, SelectedIndex, true);
                        else HighlightBlocks(0, SelectedIndex, true);

                        if (contains && !InEditMode && gameData.UpperPanelObjectTexts[SelectedIndex].first.text != Solution[SelectedIndex].ToString())
                            WarnForWrongInput(SelectedIndex);
                        else
                        {
                            gameData.UpperObjectBGSpriteRenderers[SelectedIndex].color = gameData.CurrentTheme.circleBg[3];
                            if (ShouldCheckForCompletion()) CongratulateUser();
                        }

                        _activityTracker.score = _scoresInGrid[_activityTracker.index];
                        if (gameData.UpperPanelObjectTexts[_activityTracker.index].first.text == Solution[_activityTracker.index].ToString()) _scoresInGrid[_activityTracker.index] = GenerateScore();
                        else _scoresInGrid[_activityTracker.index] = 0;
                    }
                    else if (SelectedMode == 2)
                    {
                        HighlightHelper(SelectedIndex, false, 0);
                        if (SelectedIndex != _tappedButtonIndex)
                        {
                            SelectedIndex = _tappedButtonIndex;
                            HighlightHelper(_tappedButtonIndex, false, 1);
                            HighlightBlocks(_tappedButtonIndex, -1, true);
                        }
                        else
                        {
                            SelectedMode = 0;
                            HighlightBlocks(0, -1, true);
                        }
                    }
                    else
                    {
                        SelectedMode = 2;
                        SelectedIndex = _tappedButtonIndex;
                        HighlightHelper(_tappedButtonIndex, false, 1);
                        HighlightBlocks(_tappedButtonIndex, -1, true);
                    }
                }
                break;
        }
        if (needToUpdateStack)
        {
            print("stack pushed");
            activityTrackerStack.Push(_activityTracker);
            UpdateScore();
            print("selectedIndex " + SelectedIndex + ", and " + _scoresInGrid[SelectedIndex]);
            print("tappedIndex " + _tappedButtonIndex + ", and " + _scoresInGrid[_tappedButtonIndex]);
            print("actual index " + _activityTracker.index + ", and " + _scoresInGrid[_activityTracker.index]);
        }
    }

    private void HighlightBlocks(int n, int index, bool animate)
    {
        char ch = n.ToString().ElementAt(0);
        int i, j, len;
        int[] cnt = new int[10];
        string str1, str2;
        for (i = 0; i < 10; i++) cnt[i] = 9;
        for (i = 0; i < 81; i++)
        {
            str1 = gameData.UpperPanelObjectTexts[i].first.text;
            if(str1 != "") cnt[int.Parse(str1)]--;
            if (i == index) continue;
            if (n == 0)
            {
                if (animate) StartCoroutine(HighlightHelper(i, true, 0, i % 6 / 15f));
                else
                {
                    HighlightHelper(i, true, 0);
                }
                continue;
            }
            str2 = gameData.UpperPanelObjectTexts[i].second.text;
            len = str2.Length;
            if(str1 == n.ToString())
            {
                if (animate) StartCoroutine(HighlightHelper(i, true, 1, i % 6 / 15f));
                else HighlightHelper(i, true, 1);
                continue;
            }
            for(j = 0; j < len; j++)
            {
                if(str2[j] == ch)
                {
                    if (animate) StartCoroutine(HighlightHelper(i, true, 1, i % 6 / 15f));
                    else HighlightHelper(i, true, 1);
                    break;
                }
            }
            if (j == len)
            {
                if (animate) StartCoroutine(HighlightHelper(i, true, 0, i % 6 / 15f));
                else HighlightHelper(i, true, 0);
            }
        }

        for (i = 1; i < 10; i++)
        {
            if (cnt[i] == 0) gameData.LowerPanelObjectTexts[i].second.text = "";
            else gameData.LowerPanelObjectTexts[i].second.text = cnt[i].ToString();
            if (n == 0) HighlightHelper(i, false, 0);
        }
    }

    #region SetTheme
    [Header("For Theme")]
    public Image backButtonImg;
    public Image retryButtonImg;
    public Image pauseButtonImg;
    public Image playButtonImg;
    public Image editButtonImg;
    public Image editButtonBgImg;
    public Image undoButtonImg;
    public Image themeSelectorPanelBG;
    public Image themeIcon;
    public Image hintBorderImg;
    private Vector2 primaryLarge = Vector2.one * 0.9f;
    private Vector2 primarySmall = Vector2.one * 0.8f;
    private Vector2 secondaryLarge = Vector2.one * 0.8f;
    private Vector2 secondarySmall = Vector2.one * 0.45f;
    private Vector2 tempLarge = Vector2.one * 0.45f;
    private Vector2 tempSmall = Vector2.zero;
    private Color selectedThemeBackground = new Color(0.675f, 0.318f, 0.216f);


    public void AnimateNewThemePanel(bool selected)
    {
        float duration = 0.2f;
        if (selected)
        {
            ThemeObjects[gameData.CurrentThemeIndex].primarySR.DOColor(selectedThemeBackground, duration);
            ThemeObjects[gameData.CurrentThemeIndex].primaryTransform.DOScale(primaryLarge, duration).SetEase(Ease.OutBack);
            ThemeObjects[gameData.CurrentThemeIndex].secondaryTransform.DOScale(secondaryLarge, duration).SetEase(Ease.OutBack);
            ThemeObjects[gameData.CurrentThemeIndex].tempTransform.DOScale(tempLarge, duration * 3).SetEase(Ease.OutBack);
        }
        else
        {
            ThemeObjects[gameData.CurrentThemeIndex].primarySR.DOColor(gameData.Themes[gameData.CurrentThemeIndex].background, duration);
            ThemeObjects[gameData.CurrentThemeIndex].primaryTransform.DOScale(primarySmall, duration);
            ThemeObjects[gameData.CurrentThemeIndex].secondaryTransform.DOScale(secondarySmall, duration);
            ThemeObjects[gameData.CurrentThemeIndex].tempTransform.DOScale(tempSmall, duration);
        }
    }

    public void SetTheme(bool animate)
    {
        gameData.CurrentTheme = gameData.Themes[gameData.CurrentThemeIndex];
        SelectedMode = 0;
        int i, j;
        if (animate)
        {
            float duration = 0.5f;
            Camera.main.DOColor(gameData.CurrentTheme.background, duration);
            GamePanelTimeText.DOColor(gameData.CurrentTheme.upText[0], duration);
            backButtonImg.DOColor(gameData.CurrentTheme.upText[0], duration);
            pauseButtonImg.DOColor(gameData.CurrentTheme.upText[0], duration);
            playButtonImg.DOColor(gameData.CurrentTheme.upText[0], duration);
            retryButtonImg.DOColor(gameData.CurrentTheme.upText[0], duration);
            editButtonImg.DOColor(gameData.CurrentTheme.upText[0], duration);
            hintBorderImg.DOColor(gameData.CurrentTheme.upText[0], duration);
            if (InEditMode) editButtonBgImg.DOColor(gameData.CurrentTheme.circleBg[3], duration);
            else editButtonBgImg.color = gameData.CurrentTheme.circleBg[0];
            themeSelectorPanelBG.DOColor(gameData.CurrentTheme.circleBg[1], duration);
            undoButtonImg.DOColor(gameData.CurrentTheme.upText[0], duration);
            LineMat.DOColor(gameData.CurrentTheme.circleBg[3], duration);
            themeIcon.DOColor(gameData.CurrentTheme.upText[0], duration);
            MistakeText.DOColor(gameData.CurrentTheme.upText[0], duration);
            HintText.DOColor(gameData.CurrentTheme.upText[0], duration);
            ScoreText.DOColor(gameData.CurrentTheme.upText[0], duration);

            HighlightBlocks(0, -1, true);
            for (i = 0; i < 10; i++)
            {
                HighlightHelper(i, false, 0);
            }
        }
        else
        {
            Camera.main.backgroundColor = gameData.CurrentTheme.background;
            GamePanelTimeText.color = gameData.CurrentTheme.upText[0];
            backButtonImg.color = gameData.CurrentTheme.upText[0];
            pauseButtonImg.color = gameData.CurrentTheme.upText[0];
            playButtonImg.color = gameData.CurrentTheme.upText[0];
            retryButtonImg.color = gameData.CurrentTheme.upText[0];
            editButtonImg.color = gameData.CurrentTheme.upText[0];
            hintBorderImg.color = gameData.CurrentTheme.upText[0];
            if (InEditMode) editButtonBgImg.color = gameData.CurrentTheme.circleBg[3];
            else editButtonBgImg.color = gameData.CurrentTheme.circleBg[0];
            themeSelectorPanelBG.color = gameData.CurrentTheme.circleBg[1];
            undoButtonImg.color = gameData.CurrentTheme.upText[0];
            LineMat.color = gameData.CurrentTheme.circleBg[3];
            //   startButtonBG.color = gameData.CurrentTheme.circleBg[2];
            //   startButtonText.color = gameData.CurrentTheme.upText[3];
            themeIcon.color = gameData.CurrentTheme.upText[0];
            MistakeText.color = gameData.CurrentTheme.upText[0];
            HintText.color = gameData.CurrentTheme.upText[0];
            ScoreText.color = gameData.CurrentTheme.upText[0];

            HighlightBlocks(0, -1, false);
            for (i = 0; i < 10; i++)
            {
                HighlightHelper(i, false, 0);
            }
        }

        for (i = 0; i < 2; i++)
            for (j = 0; j < 54; j++)
                gameData.SmallLineSpriteRenderers[i, j].color = gameData.CurrentTheme.circleBg[1];
    }
    #endregion
}