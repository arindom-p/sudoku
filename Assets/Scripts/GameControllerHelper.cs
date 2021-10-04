using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class GameControllerHelper : MonoBehaviour
{
    [System.NonSerialized]
    public GameData gameData;
    public DataKeyCollection dataKeyCollection = DataKeyCollection.GetObject();
    public SudokuCreator sudokuCreator;

    public void Start1()
    {
        gameData = GameData.GetObject();
        sudokuCreator = SudokuCreator.GetObject();
        gameData.UpperPanelCenter = UpperTransform.position;
        gameData.LowerPanelCenter = LowerTransform.position;
    }

    #region sudoku ground creation
    public GameObject UpperPrefab, LowerPrefab, SmallLinePrefab;
    public Transform UpperTransform, LowerTransform, LineTransform;
    public Material LineMat;
    public void CreateGrid()
    {
        int i, j;
        gameData.UpperObjects[0] = Instantiate(UpperPrefab);
        gameData.UpperObjects[0].transform.parent = UpperTransform;
        gameData.UpperObjects[0].transform.localScale = gameData.UpperSquareScale;
        gameData.UpperObjects[0].transform.position = gameData.UpperPanelCenter + Vector3.up * gameData.UpperSquareSize * 4 + Vector3.left * gameData.UpperSquareSize * 4;
        gameData.UpperPanelObjectTexts[0].first =  gameData.UpperObjects[0].transform.Find("First").GetComponent<Text>();
        gameData.UpperPanelObjectTexts[0].second =  gameData.UpperObjects[0].transform.Find("Second").GetComponent<Text>();
        gameData.UpperObjectBackgrounds[0] = gameData.UpperObjects[0].transform.Find("Background").gameObject;
        gameData.UpperObjectBGSpriteRenderers[0] = gameData.UpperObjectBackgrounds[0].GetComponent<SpriteRenderer>();
        gameData.UpperObjects[0].name = "e-00";
        for(i = 1; i < 81; i++)
        {
            gameData.UpperObjects[i] = Instantiate(gameData.UpperObjects[0]);
            gameData.UpperObjects[i].transform.parent = UpperTransform;
            gameData.UpperObjects[i].transform.position = gameData.UpperPanelCenter + Vector3.up * gameData.UpperSquareSize * (4 - i / 9) + Vector3.left * gameData.UpperSquareSize * (4 - i % 9);
            gameData.UpperPanelObjectTexts[i].first = gameData.UpperObjects[i].transform.Find("First").GetComponent<Text>();
            gameData.UpperPanelObjectTexts[i].second = gameData.UpperObjects[i].transform.Find("Second").GetComponent<Text>();
            gameData.UpperObjectBackgrounds[i] = gameData.UpperObjects[i].transform.Find("Background").gameObject;
            gameData.UpperObjectBGSpriteRenderers[i] = gameData.UpperObjectBackgrounds[i].GetComponent<SpriteRenderer>();
            gameData.UpperObjects[i].name = "e-" + i.ToString().PadLeft(2, '0');
        }

        gameData.LowerObjects[0] = Instantiate(LowerPrefab);
        gameData.LowerObjects[0].transform.parent = LowerTransform;
        gameData.LowerObjects[0].transform.localScale = gameData.LowerSquareScale;
        gameData.LowerObjects[0].transform.position = gameData.LowerPanelCenter + Vector3.down * gameData.LowerSquareSize * 0.5f + Vector3.right * gameData.LowerSquareSize * 2;
        gameData.LowerPanelObjectTexts[0].first = gameData.LowerObjects[0].transform.Find("First").GetComponent<Text>();
        gameData.LowerPanelObjectTexts[0].second = gameData.LowerObjects[0].transform.Find("Second").GetComponent<Text>();
        gameData.LowerObjectBackgrounds[0] = gameData.LowerObjects[0].transform.Find("Background").gameObject;
        gameData.LowerObjectBGSpriteRenderers[0] = gameData.LowerObjectBackgrounds[0].GetComponent<SpriteRenderer>();
        gameData.BorderSpriteRenderers[0] = gameData.LowerObjects[0].transform.Find("Circle Border").GetComponent<SpriteRenderer>();
        gameData.LowerObjects[0].name = "i-0";
        gameData.LowerPanelObjectTexts[0].first.text = "X";
        gameData.LowerPanelObjectTexts[0].second.text = "";
        for (i = 0; i < 9; i++)
        {
            j = i + 1;
            gameData.LowerObjects[j] = Instantiate(gameData.LowerObjects[0]);
            gameData.LowerObjects[j].transform.parent = LowerTransform;
            gameData.LowerObjects[j].transform.position = gameData.LowerPanelCenter + Vector3.up * gameData.LowerSquareSize * (0.5f - i / 5) + Vector3.left * gameData.LowerSquareSize * (2 - i % 5);
            gameData.LowerPanelObjectTexts[j].first = gameData.LowerObjects[j].transform.Find("First").GetComponent<Text>();
            gameData.LowerPanelObjectTexts[j].second = gameData.LowerObjects[j].transform.Find("Second").GetComponent<Text>();
            gameData.LowerObjectBackgrounds[j] = gameData.LowerObjects[j].transform.Find("Background").gameObject;
            gameData.LowerObjectBGSpriteRenderers[j] = gameData.LowerObjectBackgrounds[j].GetComponent<SpriteRenderer>();
            gameData.BorderSpriteRenderers[j] = gameData.LowerObjects[j].transform.Find("Circle Border").GetComponent<SpriteRenderer>();
            gameData.LowerObjects[j].name = "i-" + j;
            gameData.LowerPanelObjectTexts[j].first.text = j.ToString();
        }

        for (i = 0; i < 4; i++)
        {
            gameData.lineRenderers[i] = new GameObject("Line-" + (i + 1)).AddComponent<LineRenderer>();
            gameData.lineRenderers[i].transform.position = Vector3.zero;
            gameData.lineRenderers[i].positionCount = 2;
            gameData.lineRenderers[i].transform.parent = LineTransform;
            gameData.lineRenderers[i].startWidth = 0.02f;
            gameData.lineRenderers[i].endWidth = 0.02f;
            gameData.lineRenderers[i].material = LineMat;
        }
        gameData.lineRenderers[0].SetPosition(0, gameData.UpperPanelCenter + Vector3.up * gameData.UpperSquareSize * 1.5f + Vector3.left * gameData.UpperSquareSize * 4.5f);
        gameData.lineRenderers[1].SetPosition(0, gameData.UpperPanelCenter + Vector3.down * gameData.UpperSquareSize * 4.5f + Vector3.left * gameData.UpperSquareSize * 1.5f);
        gameData.lineRenderers[2].SetPosition(0, gameData.UpperPanelCenter + Vector3.down * gameData.UpperSquareSize * 1.5f + Vector3.right * gameData.UpperSquareSize * 4.5f);
        gameData.lineRenderers[3].SetPosition(0, gameData.UpperPanelCenter + Vector3.up * gameData.UpperSquareSize * 4.5f + Vector3.right * gameData.UpperSquareSize * 1.5f);
        gameData.LinePos[0] = gameData.UpperPanelCenter + Vector3.up * gameData.UpperSquareSize * 1.5f + Vector3.right * gameData.UpperSquareSize * 4.5f;
        gameData.LinePos[1] = gameData.UpperPanelCenter + Vector3.up * gameData.UpperSquareSize * 4.5f + Vector3.left * gameData.UpperSquareSize * 1.5f;
        gameData.LinePos[2] = gameData.UpperPanelCenter + Vector3.down * gameData.UpperSquareSize * 1.5f + Vector3.left * gameData.UpperSquareSize * 4.5f;
        gameData.LinePos[3] = gameData.UpperPanelCenter + Vector3.down * gameData.UpperSquareSize * 4.5f + Vector3.right * gameData.UpperSquareSize * 1.5f;

        Vector2 posA, posB;
        int ind;
        GameObject a, b;
        for (i = 0; i < 8; i++)
        {
            if (i % 3 == 2) continue;
            posA = gameData.UpperPanelCenter + Vector3.left * gameData.UpperSquareSize * 4 + Vector3.up * gameData.UpperSquareSize * (3.5f - i);
            posB = gameData.UpperPanelCenter + Vector3.up * gameData.UpperSquareSize * 4 + Vector3.left * gameData.UpperSquareSize * (3.5f - i);
            for (j = 0; j < 9; j++)
            {
                ind = (i - i / 3) * 9 + j;
                a = Instantiate(SmallLinePrefab, posA, Quaternion.identity);
                b = Instantiate(SmallLinePrefab, posB, Quaternion.Euler(0, 0, 90));
                gameData.SmallLineSpriteRenderers[0, ind] = a.GetComponent<SpriteRenderer>();
                gameData.SmallLineSpriteRenderers[1, ind] = b.GetComponent<SpriteRenderer>();
                a.transform.localScale = Vector2.one;
                b.transform.localScale = Vector2.one;
                a.transform.parent = LineTransform;
                b.transform.parent = LineTransform;
                posA += Vector2.right * gameData.UpperSquareSize;
                posB += Vector2.down * gameData.UpperSquareSize;
            }
        }
    }
    #endregion

    #region load game
    private int _numberOfFixedNumbers = 40;
    [HideInInspector] public bool[] isFixed = new bool[81];
    [HideInInspector] public int[] Solution = new int[81];
    public void LoadNewGame(int numberOfFixedNumbers)
    {
        int i;
        if (numberOfFixedNumbers >= 0)
        {
            int[] numArr = new int[81];
            int randomIndex;
            _numberOfFixedNumbers = numberOfFixedNumbers;

            for (i = 0; i < 81; i++)
            {
                isFixed[i] = false;
                numArr[i] = i;
            }
            for (i = 0; i < _numberOfFixedNumbers; i++)
            {
                randomIndex = Random.Range(i, 81);
                (numArr[i], numArr[randomIndex]) = (numArr[randomIndex], numArr[i]);
                isFixed[numArr[i]] = true;
            }
            Solution = sudokuCreator.GetRandomSudoku();
        }
        
        for (i = 0; i < 81; i++)
        {
            if (isFixed[i])
            {
                gameData.UpperPanelObjectTexts[i].first.text = Solution[i] + "";
            }
            else
            {
                gameData.UpperPanelObjectTexts[i].first.text = "";
            }
            gameData.UpperPanelObjectTexts[i].second.text = "";
        }
    }
    #endregion

    #region line animation
    public void AnimateLine(float duration)
    {
        int i;
        for (i = 0; i < 81; i++)
        {
            gameData.UpperPanelObjectTexts[i].first.color = Color.clear;
            gameData.UpperPanelObjectTexts[i].second.color = Color.clear;
            gameData.UpperObjectBackgrounds[i].transform.localScale = Vector2.zero;
        }
        for (i = 0; i < 4; i++)
            StartCoroutine(Co_AnimateLine(i, duration));
    }
    private IEnumerator Co_AnimateLine(int ind, float duration)
    {
        float processed, startTime = Time.time;
        while (true)
        {
            processed = (Time.time - startTime) / duration;
            gameData.lineRenderers[ind].SetPosition(1, Vector3.Lerp(gameData.lineRenderers[ind].GetPosition(0), gameData.LinePos[ind], processed));
            if (processed >= 1) break;
            yield return null;
        }
        if(ind == 0) AnimateUpperGround();
    }
    #endregion

    #region sudoku upper ground animation
    public void AnimateUpperGround()
    {
        for(int i = 0; i < 81; i++)
        {
            if(isFixed[i])
            {
                gameData.UpperObjectBackgrounds[i].transform.localScale = Vector3.zero;
                gameData.UpperPanelObjectTexts[i].first.color = Color.clear;
                StartCoroutine(AnimateAfterWaitingPeriod(i, int.Parse(gameData.UpperPanelObjectTexts[i].first.text) / 15f));
            }
            else if(gameData.UpperPanelObjectTexts[i].first.text + gameData.UpperPanelObjectTexts[i].second.text != "")
                StartCoroutine(AnimateAfterWaitingPeriod(i, 0.5f));
        }
    }
    private IEnumerator AnimateAfterWaitingPeriod(int index, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (isFixed[index])
            gameData.UpperObjectBackgrounds[index].transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack, 2f).OnComplete(() => gameData.UpperPanelObjectTexts[index].first.DOColor(gameData.CurrentTheme.upText[1], 0.2f));
        else
        {
            gameData.UpperPanelObjectTexts[index].first.DOColor(gameData.CurrentTheme.upText[0], 0.5f);
            gameData.UpperPanelObjectTexts[index].second.DOColor(gameData.CurrentTheme.upText[0], 0.5f);
        }
    }
    #endregion

    #region time counter coroutine
    [HideInInspector] public int currentGamePlayingTime;
    public IEnumerator TimeCounter(Text gamePanelTime, int sec = 0)
    {
        currentGamePlayingTime = sec;
        int hour = sec / 3600;      sec %= 3600;
        int min = sec / 60;         sec %= 60;
        while (true)
        {
            gamePanelTime.text = "TIME " + (hour > 0 ?
                hour + ":" + min.ToString().PadLeft(2, '0') + ":" + sec.ToString().PadLeft(2, '0'):
                min + ":" + sec.ToString().PadLeft(2, '0'));
            yield return new WaitForSeconds(1);
            currentGamePlayingTime++;
            sec++;
            if (sec == 60)
            {
                min++;
                sec = 0;
                if(min == 60)
                {
                    hour++;
                    min = 0;
                }
            }
        }
    }
    #endregion

    #region function of adding data in edit mode
    public string AddData(string currentString, int n, ref bool contains)
    {
        contains = false;
        if (currentString.Contains(n.ToString()))
        {
            string newStr = "";
            foreach (char ch in currentString)
            {
                if (ch != ' ' && ch != n + 48)
                    newStr += ch;
            }
            return (newStr.Length < 5 ? newStr : newStr.Substring(0, 2) + ' ' + newStr.Substring(2));
        }
        if (currentString.Length > 8) return currentString;
        contains = true;
        if (currentString.Length != 4)
            return (currentString + n);
        return (currentString.Substring(0, 2) + ' ' + currentString.Substring(2) + n);
    }
    #endregion

    #region highlight helper function
    public void HighlightHelper(int index, bool isUpperPanelObj, int brightnessLevel)
    {
        if (isUpperPanelObj)
        {
            if (isFixed[index])
            {
                if (brightnessLevel == 0)
                {
                    gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[1];
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[1];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[1];
                }
                else
                {
                    gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[2];
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[2];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[2];
                }
            }
            else
            {
                if (brightnessLevel == 0)
                {
                    gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[0];
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[0];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[0];
                }
                else if (brightnessLevel == 1)
                {
                    gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[2];
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[2];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[2];
                }
                else
                {
                    gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[3];
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[3];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[3];
                }
            }
        }
        else
        {
            if (brightnessLevel == 0)
            {
                gameData.LowerObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[0];
                gameData.BorderSpriteRenderers[index].color = gameData.CurrentTheme.upText[0];
                gameData.LowerPanelObjectTexts[index].first.color = gameData.CurrentTheme.downText[0];
                gameData.LowerPanelObjectTexts[index].second.color = gameData.CurrentTheme.downText[0];
            }
            else
            {
                gameData.LowerObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[3];
                gameData.BorderSpriteRenderers[index].color = Color.clear;
                gameData.LowerPanelObjectTexts[index].first.color = gameData.CurrentTheme.downText[1];
                gameData.LowerPanelObjectTexts[index].second.color = gameData.CurrentTheme.downText[1];
            }
        }
    }
    //----------------------------------------------------------------------------both class are same axcept animation duration
    public IEnumerator HighlightHelper(int index, bool isUpper, int brightnessLevel, float waitingPeriod)
    {
        yield return new WaitForSeconds(waitingPeriod);
        float animationDuration = 0.2f;
        if (isUpper)
        {
            if (isFixed[index])
            {
                if (brightnessLevel == 0)
                {
                    gameData.UpperObjectBGSpriteRenderers[index].DOColor(gameData.CurrentTheme.circleBg[1], animationDuration * 2);
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[1];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[1];
                }
                else
                {
                    gameData.UpperObjectBGSpriteRenderers[index].DOColor(gameData.CurrentTheme.circleBg[2], animationDuration * 2);
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[2];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[2];
                }
            }
            else
            {
                if (brightnessLevel == 0)
                {
                    gameData.UpperObjectBackgrounds[index].transform.DOScale(Vector2.zero, animationDuration).OnComplete(() => {
                        gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[0];
                        gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[0];
                        gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[0];
                    });
                }
                else if (brightnessLevel == 1)
                {
                    gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[2];
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[2];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[2];
                    gameData.UpperObjectBackgrounds[index].transform.localScale = Vector3.zero;
                    gameData.UpperObjectBackgrounds[index].transform.DOScale(Vector2.one, animationDuration);
                }
                else
                {
                    gameData.UpperObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[3];
                    gameData.UpperPanelObjectTexts[index].first.color = gameData.CurrentTheme.upText[3];
                    gameData.UpperPanelObjectTexts[index].second.color = gameData.CurrentTheme.upText[3];
                    gameData.UpperObjectBackgrounds[index].transform.localScale = Vector3.zero;
                    gameData.UpperObjectBackgrounds[index].transform.DOScale(Vector2.one, animationDuration);
                }
            }
        }
        else
        {
            if (brightnessLevel == 0)
            {
                gameData.LowerObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[0];
                gameData.BorderSpriteRenderers[index].color = gameData.CurrentTheme.upText[0];
                gameData.LowerPanelObjectTexts[index].first.color = gameData.CurrentTheme.downText[0];
                gameData.LowerPanelObjectTexts[index].second.color = gameData.CurrentTheme.downText[0];
            }
            else
            {
                gameData.LowerObjectBGSpriteRenderers[index].color = gameData.CurrentTheme.circleBg[3];
                gameData.BorderSpriteRenderers[index].color = Color.clear;
                gameData.LowerPanelObjectTexts[index].first.color = gameData.CurrentTheme.downText[1];
                gameData.LowerPanelObjectTexts[index].second.color = gameData.CurrentTheme.downText[1];
            }
        }
    }
    #endregion
}
