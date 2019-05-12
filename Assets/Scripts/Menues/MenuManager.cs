using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public CreditsMenu creditsMenuPrefab;
    public GameOverMenu gameOverMenuPrefab;
    public InstructionsMenu instructionsMenuPrefab;
    public MainMenu mainMenuPrefab;
    public OptionsMenu optionsMenuPrefab;
    public PauseMenu pauseMenuPrefab;

    [Space(20)]
    public Menu initialMenu;

    private Stack<Menu> _menuStack = new Stack<Menu>();
    private bool pushedThisFrame = false;

    private static MenuManager _instance;
    public static MenuManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<MenuManager>();
            }
            return _instance;
        }
    }

    private void LateUpdate()
    {
        if (!pushedThisFrame && Input.GetKeyDown(KeyCode.Escape))
        {
            if (_menuStack.Count > 0)
            {
                _menuStack.Peek().OnPressedEscape();
            }
        }
        pushedThisFrame = false;
    }

    private void Start()
    {
        if(initialMenu != null)
        {
            PushMenu(initialMenu.GetType());
        }
    }

    public bool IsOpen()
    {
        return _menuStack.Count > 0;
    }

    public void PushMenu(System.Type menuType)
    {
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().gameObject.SetActive(false);
        }
        Menu instance = Instantiate(GetPrefab(menuType));
        ActivateMenu(instance);

        _menuStack.Push(instance);
        _menuStack.Peek().OnEnter();

        pushedThisFrame = true;
    }

    public void PushMenu<T>() where T : Menu
    {
        PushMenu(typeof(T));
    }

    public void PopMenu()
    {
        if(_menuStack.Count > 0)
        {
            _menuStack.Peek().OnExit();
            Destroy(_menuStack.Pop().gameObject);
        }

        if (_menuStack.Count > 0)
        {
            Menu top = _menuStack.Peek();
            top.gameObject.SetActive(true);
            ActivateMenu(top);
            top.OnEnter();
        }
    }

    private void ActivateMenu(Menu menu)
    {
        RectTransform rectTransform = menu.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        menu.transform.localScale = Vector3.one;
        menu.transform.localPosition = Vector3.zero;
        menu.transform.SetParent(transform, false);
    }

    private Menu GetPrefab(System.Type menuType)
    {
        if(menuType == typeof(CreditsMenu))
        {
            return creditsMenuPrefab;
        }
        if (menuType == typeof(GameOverMenu))
        {
            return gameOverMenuPrefab;
        }
        if (menuType == typeof(InstructionsMenu))
        {
            return instructionsMenuPrefab;
        }
        if (menuType == typeof(MainMenu))
        {
            return mainMenuPrefab;
        }
        if (menuType == typeof(OptionsMenu))
        {
            return optionsMenuPrefab;
        }
        if (menuType == typeof(PauseMenu))
        {
            return pauseMenuPrefab;
        }

        throw new MissingReferenceException("Tried to instantiate missing prefab " + menuType.ToString());
    }
}
