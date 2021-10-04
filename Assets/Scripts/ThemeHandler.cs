using DG.Tweening;
using UnityEngine;

public class ThemeHandler : MonoBehaviour
{
    public GameObject themePrefab;
    public GameController gameController;
    public RectTransform themePanelRT, themePanelBGRT;
    [HideInInspector] public bool themeSelectionIconPressed = false;
    private bool _screenTapped = false;
    private Vector3 _startPosition;
    private float y, minHeight, maxHeight, lastActionTime = 0;

    void Start()
    {
        int numberOfThemes = gameController.gameData.Themes.Length;
        _startPosition = new Vector3(4.5f + 10 * Screen.width / Screen.height, 0, -0.2f);
        themePanelBGRT.position = new Vector3(0, 0, -0.2f);
        themePanelBGRT.sizeDelta = new Vector2(numberOfThemes, 1.5f);
        themePanelBGRT.transform.localScale = new Vector2(1, 0);
        themePanelRT.gameObject.SetActive(false);
        maxHeight = Screen.height * 0.575f;
        minHeight = Screen.height * 0.425f;

        GameObject obj;
        RectTransform rt;
        float themeWidth = 1f / numberOfThemes;
        for (int i = 0; i < numberOfThemes; i++)
        {
            obj = Instantiate(themePrefab);
            obj.name = "Theme-" + i;
            obj.transform.SetParent(themePanelRT);
            rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.right * i * themeWidth;
            rt.anchorMax = Vector2.right * (i + 1) * themeWidth + Vector2.up;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    private void Update()
    {
        if(themeSelectionIconPressed && !_screenTapped && Input.touchCount > 0)
        {
            y = Input.GetTouch(0).position.y;
            if (y < minHeight || y > maxHeight)
                OnClick();
        }
        _screenTapped = Input.touchCount > 0;
    }

    public void OnClick()
    {
        if (Time.time - lastActionTime < 0.5f) return;
        lastActionTime = Time.time;
        themeSelectionIconPressed = !themeSelectionIconPressed;
        if (themeSelectionIconPressed)
        {
            themePanelBGRT.DOScaleY(1, 0.3f).OnComplete(() => {
                themePanelRT.gameObject.SetActive(true);
                themePanelRT.transform.position = _startPosition;
            });
        }
        else
        {
            themePanelRT.DOMoveX(-_startPosition.x, 0.5f).OnComplete(() =>
                {
                    themePanelRT.gameObject.SetActive(false);
                    themePanelBGRT.DOScaleY(0, 0.3f);
                });
        }
    }
}
