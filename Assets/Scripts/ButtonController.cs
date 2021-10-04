using UnityEngine;

public class ButtonController : MonoBehaviour
{
    private GameData gameData;
    private GameController gameController;
    private DataKeyCollection dataKeyCollection = DataKeyCollection.GetObject();

    private bool _isThemeComponent;
    private int _themeIndex;

    void Start()
    {
        gameController = GameObject.Find("Game Controller").GetComponent<GameController>();

        if (gameObject.name.Contains("Theme"))
        {
            gameData = GameData.GetObject();
            _themeIndex = int.Parse(name.Substring(name.Length - 1));
            _isThemeComponent = true;

            gameController.ThemeObjects[_themeIndex].primaryTransform   = transform.Find("Primary Color");
            gameController.ThemeObjects[_themeIndex].secondaryTransform = transform.Find("Secondary Color");
            gameController.ThemeObjects[_themeIndex].tempTransform      = transform.Find("Temp Color");
            gameController.ThemeObjects[_themeIndex].primarySR          = gameController.ThemeObjects[_themeIndex].primaryTransform.GetComponent<SpriteRenderer>();
            gameController.ThemeObjects[_themeIndex].primarySR.color    = gameData.Themes[_themeIndex].background;
            gameController.ThemeObjects[_themeIndex].tempTransform.GetComponent<SpriteRenderer>().color = gameData.Themes[_themeIndex].background;
            transform.Find("Secondary Color").GetComponent<SpriteRenderer>().color = gameData.Themes[_themeIndex].circleBg[3];
            if (_themeIndex == gameController.gameData.Themes.Length - 1) gameController.AnimateNewThemePanel(true);
        }
        else _isThemeComponent = false;
    }

    private void OnMouseDown()
    {
        if (_isThemeComponent && gameData.CurrentThemeIndex != _themeIndex)
        {
            gameController.AnimateNewThemePanel(false);
            gameData.CurrentThemeIndex = _themeIndex;
            PlayerPrefs.SetInt(dataKeyCollection.currentThemeIndex, _themeIndex);
            gameController.SetTheme(true);
            gameController.AnimateNewThemePanel(true);
        }
        else gameController.ButtonClicked(name);
    }
}
