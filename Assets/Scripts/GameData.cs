using UnityEngine;
using UnityEngine.UI;

public class DataKeyCollection
{

    public static DataKeyCollection dataKeyCollection;
    private DataKeyCollection() { }
    public static DataKeyCollection GetObject()
    {
        if (dataKeyCollection == null) dataKeyCollection = new DataKeyCollection();
        return dataKeyCollection;
    }

    public string easy = "_easy";
    public string medium = "_medium";
    public string hard = "_hard";

    public string resumeAvailable = "resume";
    public string gameLevel = "level";
    public string previousGridData = "resumeData";
    public string currentThemeIndex = "themeIndex";
    public string fixedGridPositions = "fixed";
    public string stackData = "stack";
    public string availableHint = "hint";
    public string numberOfMistake = "mistake";
    public string timeLeft = "time";
    public string solusion = "solusion";
    public string currentScores = "score";
    public string noMistakeFlag = "noMistake";

    public string numberOfGamesStarted = "gamesStarted";
    public string gamesWon = "won";
    public string dataOfThisWeek = "thisWeek";
    public string bestTime = "bestTime";
    public string totalGameWonTime = "wonTime";
    public string bestScore = "bestScore";
    public string totalScore = "totalScore";
    public string currentWinStreak = "currentWinStreak";
    public string besttWinStreak = "besttWinStreak";
    public string winsWithNoMistakes = "winsWithNoMistakes";
    public string gameStarted = "started";
}

public class GameData
{
    private DataKeyCollection dataKeyCollection = DataKeyCollection.GetObject();
    public static GameData gameData;
    private GameData()
    {
        CurrentThemeIndex = PlayerPrefs.GetInt(dataKeyCollection.currentThemeIndex, 2);
        ThemeInit();
    }

    public static GameData GetObject()
    {
        if (gameData == null) gameData = new GameData();
        return gameData;
    }

    #region theme
    public struct Theme
    {
        public Vector4 background;
        public Vector4[] circleBg, upText, downText;
    }
    public Theme CurrentTheme;
    public Theme[] Themes = new Theme[9];
    public int CurrentThemeIndex;

    private void ThemeInit()
    {
        int i = 0;
        Themes[i].background = new Vector4(255, 255, 255, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(231, 231, 231, 255), new Vector4(188, 185, 166, 255), new Vector4(170, 159, 116, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(122, 124, 121, 255), new Vector4(164, 164, 164, 255), Themes[i].background, Themes[i].background};
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        i = 1;
        Themes[i].background = new Vector4(250, 250, 250, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(220, 220, 220, 255), new Vector4(168, 180, 188, 255), new Vector4(106, 161, 192, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(120, 125, 121, 255), new Vector4(150, 150, 150, 255), Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[1], Themes[i].upText[3] };

        i = 2;
        Themes[i].background = new Vector4(51, 51, 51, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(103, 103, 103, 255), new Vector4(173, 132, 99, 255), new Vector4(242, 129, 36, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(233, 233, 233, 255), Themes[i].background, Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        i = 3;
        Themes[i].background = new Vector4(255, 255, 255, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(239, 239, 239, 255), new Vector4(220, 183, 186, 255), new Vector4(190, 100, 106, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(156, 110, 113, 255), new Vector4(150, 150, 150, 255), Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        i = 4;
        Themes[i].background = new Vector4(25, 25, 25, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(45, 45, 45, 255), new Vector4(100, 98, 84, 255), new Vector4(156, 143, 94, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(214, 214, 214, 255), new Vector4(159, 159, 159, 255), Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        i = 5;
        Themes[i].background = new Vector4(56, 59, 54, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(99, 105, 99, 255), new Vector4(138, 168, 118, 255), new Vector4(175, 201, 159, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(219, 221, 219, 255), Themes[i].background, Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        i = 6;
        Themes[i].background = new Vector4(18, 18, 18, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(34, 36, 34, 255), new Vector4(45, 84, 47, 255), new Vector4(41, 157, 41, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(201, 212, 203, 255), new Vector4(147, 161, 147, 255), Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        i = 7;
        Themes[i].background = new Vector4(15, 22, 29, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(28, 38, 48, 255), new Vector4(57, 106, 127, 255), new Vector4(45, 187, 249, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(91, 173, 207, 255), new Vector4(128, 149, 170, 255), Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        i = 8;
        Themes[i].background = new Vector4(28, 28, 28, 255);
        Themes[i].circleBg = new Vector4[4] { Vector4.zero, new Vector4(48, 48, 48, 255), new Vector4(129, 66, 87, 255), new Vector4(175, 129, 255, 255) };
        Themes[i].upText = new Vector4[4] { new Vector4(228, 217, 121, 255), new Vector4(143, 143, 143, 255), Themes[i].background, Themes[i].background };
        Themes[i].downText = new Vector4[2] { Themes[i].upText[0], Themes[i].upText[3] };

        for(i = 0; i < 9; i++)
        {
            Themes[i].background /= 255f;
            for(int j = 0; j < 4; j++)
            {
                Themes[i].circleBg[j] /= 255f;
                Themes[i].upText[j] /= 255f;
                if (j < 2) Themes[i].downText[j] /= 255f;
            }
        }

        CurrentTheme = Themes[CurrentThemeIndex];
        Camera.main.backgroundColor = CurrentTheme.background;
    }
    #endregion

    #region sudoku_ground
    public Vector3 UpperPanelCenter, LowerPanelCenter, UpperSquareScale, LowerSquareScale;
    public float UpperSquareSize, LowerSquareSize;
    public GameObject[] UpperObjects = new GameObject[81];
    public GameObject[] LowerObjects = new GameObject[10];
    public GameObject[] UpperObjectBackgrounds = new GameObject[81];
    public GameObject[] LowerObjectBackgrounds = new GameObject[10];
    public SpriteRenderer[] UpperObjectBGSpriteRenderers = new SpriteRenderer[81];
    public SpriteRenderer[] LowerObjectBGSpriteRenderers = new SpriteRenderer[10];
    public SpriteRenderer[] BorderSpriteRenderers = new SpriteRenderer[10];
    public struct SudokuGridText
    {
        public Text first, second;
    }
    public SudokuGridText[] UpperPanelObjectTexts = new SudokuGridText[81];
    public SudokuGridText[] LowerPanelObjectTexts = new SudokuGridText[10];

    public LineRenderer[] lineRenderers = new LineRenderer[4];
    public Vector3[] LinePos = new Vector3[4];
    public SpriteRenderer[,] SmallLineSpriteRenderers = new SpriteRenderer[2, 54];
    #endregion
}