using UnityEngine;
using UnityEngine.UI;

public class InstructionsMenu : MonoBehaviour
{
    public GameObject pageOne;
    public GameObject pageTwo;
    public GameObject pageThree;
    public GameObject nextButton;
    public GameObject previousButton;

    public GameObject mainMenu;

    private int _currentPage = 0;

    private void OnEnable()
    {
        previousButton.SetActive(false);
        _currentPage = 0;
        OnPageChanged();
    }

    public void OnMainMenuClick()
    {
        _currentPage = 0;
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnNextClick()
    {
        _currentPage++;
        OnPageChanged();
    }

    public void OnPreviousClick()
    {
        _currentPage--;
        OnPageChanged();
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
