using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject healthMarkerPrefab;

    [Space(20)]
    public Image maxHealthForegroundImage;
    public Image maxHealthBackgroundImage;
    public Image currentHealthImage;
    public RectTransform healthMarkersObject;
    public RectTransform healthBarObject;

    public int healthPerPixel = 2;

    private int _targetHealth = 100;
    private int _targetMaxHealth = 100;

    private int _currentHealth = 0;
    private int _currentMaxHealth = 0;

    private Vector3 _originalPosition;

    private void Start()
    {
        _originalPosition = healthBarObject.anchoredPosition;
    }

    private void FixedUpdate()
    {
        bool shake = false;
        if (_currentHealth != _targetHealth)
        {
            if (_currentHealth < _targetHealth)
            {
                _currentHealth += 1;
                if (_currentHealth > _targetHealth)
                {
                    _currentHealth = _targetHealth;
                }
            }
            if (_currentHealth > _targetHealth)
            {
                shake = true;
                _currentHealth -= 1;
                if (_currentHealth < _targetHealth)
                {
                    _currentHealth = _targetHealth;
                }
            }
            SetHealth(_currentHealth);
        }

        if (_currentMaxHealth != _targetMaxHealth)
        {
            if (_currentMaxHealth < _targetMaxHealth)
            {
                _currentMaxHealth += 1;
                if (_currentMaxHealth > _targetMaxHealth)
                {
                    _currentMaxHealth = _targetMaxHealth;
                }
            }
            if (_currentMaxHealth > _targetMaxHealth)
            {
                shake = true;
                _currentMaxHealth -= 1;
                if (_currentMaxHealth < _targetMaxHealth)
                {
                    _currentMaxHealth = _targetMaxHealth;
                }
            }
            SetMaxHealth(_currentMaxHealth);
        }

        if (shake)
        {
            float s = 8f;
            healthBarObject.anchoredPosition = _originalPosition + new Vector3((int)Random.Range(-s, s), (int)Random.Range(-s, s), 0f);
        }
        else
        {
            healthBarObject.anchoredPosition = _originalPosition;
        }
    }

    public void SetHealthbar(int health, int maxHealth)
    {
        _targetHealth = health;
        _targetMaxHealth = maxHealth;
    }

    private void SetHealth(int health)
    {
        _currentHealth = health;
        health = health * healthPerPixel;
        Vector2 sizeDelta = currentHealthImage.rectTransform.sizeDelta;

        sizeDelta.x = health;

        currentHealthImage.rectTransform.sizeDelta = sizeDelta;
    }

    private void SetMaxHealth(int maxHealth)
    {
        _currentMaxHealth = maxHealth;
        maxHealth = maxHealth * healthPerPixel;
        Vector2 foregroundSizeDelta = maxHealthForegroundImage.rectTransform.sizeDelta;
        Vector2 backgroundSizeDelta = maxHealthBackgroundImage.rectTransform.sizeDelta;

        foregroundSizeDelta.x = maxHealth - 2;
        backgroundSizeDelta.x = maxHealth - 4;

        maxHealthForegroundImage.rectTransform.sizeDelta = foregroundSizeDelta;
        maxHealthBackgroundImage.rectTransform.sizeDelta = backgroundSizeDelta;

        int numMarkersWanted = maxHealth / 20;
        int currentMarkerCount = healthMarkersObject.childCount;
        if (numMarkersWanted > currentMarkerCount)
        {
            int newMarkerCount = numMarkersWanted - currentMarkerCount;
            for (int i = 0; i < newMarkerCount; i++)
            {
                Instantiate(healthMarkerPrefab, healthMarkersObject);
            }
        }

        for (int i = 0; i < healthMarkersObject.childCount; i++)
        {
            if (i < numMarkersWanted)
            {
                healthMarkersObject.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                healthMarkersObject.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
