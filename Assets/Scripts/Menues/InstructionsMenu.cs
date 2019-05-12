using UnityEngine;

public class InstructionsMenu : Menu
{
    public GameObject pageOne;
    public GameObject pageTwo;
    public GameObject pageThree;
    public GameObject nextButton;
    public GameObject previousButton;

    private int _currentPage = 0;

    public override void OnEnter()
    {
        base.OnEnter();
        previousButton.SetActive(false);
        _currentPage = 0;
        OnPageChanged();
    }

    public override void OnPressedEscape()
    {
        OnMainMenuClick();
    }

    public void OnMainMenuClick()
    {
        _currentPage = 0;
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
    }

    public void OnNextClick()
    {
        _currentPage++;
        OnPageChanged();
        SoundManager.Instance.PlayUIButtonClick();
    }

    public void OnPreviousClick()
    {
        _currentPage--;
        OnPageChanged();
        SoundManager.Instance.PlayUIButtonClick();
    }

    public void OnPageChanged()
    {
        if (_currentPage == 0)
        {
            pageOne.SetActive(true);
            pageTwo.SetActive(false);
            pageThree.SetActive(false);
            previousButton.SetActive(false);
            nextButton.SetActive(true);
        }
        else if (_currentPage == 1)
        {
            pageOne.SetActive(false);
            pageTwo.SetActive(true);
            pageThree.SetActive(false);
            previousButton.SetActive(true);
            nextButton.SetActive(true);
        }
        else if (_currentPage == 2)
        {
            pageTwo.SetActive(false);
            pageThree.SetActive(true);
            nextButton.SetActive(false);
        }
    }
}
