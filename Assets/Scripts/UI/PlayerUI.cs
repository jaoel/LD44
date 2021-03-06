﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerUI : MonoBehaviour
{
    public GameObject healthMarkerPrefab;
    public TMPro.TextMeshProUGUI currentLevelText;
    public Image weaponImage;
    public Image goldKeyImage;
    public Image skeletonKeyImage;
    public TMPro.TextMeshProUGUI weaponText;
    public GameObject skeletonKeyContainer;
    public Slider chargeMeter;

    [Space(20)]
    public Image maxHealthForegroundImage;
    public Image maxHealthBackgroundImage;
    public Image currentHealthImage;
    public RectTransform healthMarkersObject;
    public RectTransform healthBarObject;
    public RawImage minimapObject;
    public RectTransform minimapMinimizedObject;
    public RectTransform minimapExpandedObject;

    public int healthPerPixel = 2;

    private int _targetHealth = 100;
    private int _targetMaxHealth = 100;

    private int _currentHealth = 0;
    private int _currentMaxHealth = 0;
    private Color _keyMissingColor = Utility.RGBAColor(17, 17, 17, 1.0f);

    private Vector3 _originalPosition;
    private CanvasScaler _scaler;
    private Image _chargeMeterFill;

    private int _currentLevel = -1;

    private bool minimapExpanded = false;
    private float minimapExpandedAmount = 0f;
    private float minimapExpandDuration = 0.25f;

    public Image test => _chargeMeterFill;

    private void Awake()
    {
        _scaler = GetComponent<CanvasScaler>();
        _chargeMeterFill = chargeMeter.GetComponentInChildren<Image>();
    }

    private void CopyRectTransform(RectTransform target, RectTransform from, RectTransform to, float amount = 1f)
    {
        target.anchoredPosition = Vector2.Lerp(from.anchoredPosition, to.anchoredPosition, amount);
        target.anchorMin = Vector2.Lerp(from.anchorMin, to.anchorMin, amount);
        target.anchorMax = Vector2.Lerp(from.anchorMax, to.anchorMax, amount);
        target.offsetMin = Vector2.Lerp(from.offsetMin, to.offsetMin, amount);
        target.offsetMax = Vector2.Lerp(from.offsetMax, to.offsetMax, amount);
        target.pivot = Vector2.Lerp(from.pivot, to.pivot, amount);
        target.sizeDelta = Vector2.Lerp(from.sizeDelta, to.sizeDelta, amount);
    }

    private void Start()
    {
        _originalPosition = healthBarObject.anchoredPosition;
    }

    private void OnTweenUpdate()
    {
        CopyRectTransform(minimapObject.rectTransform, minimapMinimizedObject, minimapExpandedObject, minimapExpandedAmount);
    }

    private void Update()
    {
        if (FogOfWar.Instance.FoWTexture == null)
        {
            return;
        }
        if (Keybindings.UIExpandMinimap)
        {
            minimapExpanded = !minimapExpanded;
            if (minimapExpanded)
            {
                Keybindings.DeactivateInputMode(Keybindings.InputMode.Player);
                Keybindings.ActivateInputMode(Keybindings.InputMode.Minimap);
                DOTween.To(() => minimapExpandedAmount, x => minimapExpandedAmount = x, 1f, minimapExpandDuration).OnUpdate(OnTweenUpdate).SetEase(Ease.InOutSine);
            }
            else
            {
                Keybindings.DeactivateInputMode(Keybindings.InputMode.Minimap);
                Keybindings.ActivateInputMode(Keybindings.InputMode.Player);
                DOTween.To(() => minimapExpandedAmount, x => minimapExpandedAmount = x, 0f, minimapExpandDuration).OnUpdate(OnTweenUpdate).SetEase(Ease.InOutSine);
            }
        }

        if (minimapObject.texture != FogOfWar.Instance.FoWTexture)
        {
            minimapObject.texture = FogOfWar.Instance.FoWTexture;
        }

        Rect uvRect = minimapObject.uvRect;
        uvRect.width = minimapObject.rectTransform.rect.width / minimapObject.texture.width;
        uvRect.height = minimapObject.rectTransform.rect.height / minimapObject.texture.height;
        Vector3Int playerTilePosition = FogOfWar.Instance.WorldToTile(Main.Instance.player.transform.position);
        uvRect.x = playerTilePosition.x / (float)minimapObject.texture.width - uvRect.width / 2f;
        uvRect.y = playerTilePosition.y / (float)minimapObject.texture.height - uvRect.height / 2f;

        minimapObject.uvRect = uvRect;

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

        if (Main.Instance.gameState == GameState.Gameplay)
        {
            if (MapManager.Instance.CurrentLevel != _currentLevel)
            {
                _currentLevel = MapManager.Instance.CurrentLevel;
                currentLevelText.text = "Level " + _currentLevel.ToString();
            }
        }
        else
        {
            if (currentLevelText.text != MapManager.Instance.GetLevelName())
            {
                currentLevelText.text = MapManager.Instance.GetLevelName();
            }
        }
    }

    public void SetChargeMeterPosition(Vector2 targetPosition)
    {
        chargeMeter.transform.position = Camera.main.WorldToScreenPoint(targetPosition) - new Vector3(0.0f, 16.0f  * _scaler.scaleFactor, 0.0f);
    }

    public void SetChargeMeterColor(Color newColor)
    {
        _chargeMeterFill.color = newColor;
    }

    public void SetChargeMeterColor(Color startColor, Color endColor, float value)
    {
        _chargeMeterFill.color = Color.Lerp(startColor, endColor, value);
    }

    public Tween FlashChargeMeter(Color flashColor, Color endColor, float time, int flashes)
    {
        return _chargeMeterFill.DOColor(flashColor, time / flashes).SetLoops(flashes, LoopType.Yoyo);
    }

    public void AddSkeletonKey(int count)
    {
        if (count == 1)
        {
            skeletonKeyImage.color = Color.white;
        }
        else
        {
            GameObject.Instantiate(skeletonKeyImage.gameObject, skeletonKeyContainer.transform);
        }
    }

    public void RemoveSkeletonKey(int count)
    {
        if (count == 0)
        {
            skeletonKeyImage.color = _keyMissingColor;

            for (int i = 1; i < skeletonKeyContainer.transform.childCount; i++)
            {
                Destroy(skeletonKeyContainer.transform.GetChild(i).gameObject);
            }
        }
        else
        {
            Destroy(skeletonKeyContainer.transform.GetChild(skeletonKeyContainer.transform.childCount - 1).gameObject);
        }
    }

    public void SetGoldKey(bool hasKey)
    {
        if(hasKey)
        {
            goldKeyImage.color = Color.white;
        }
        else
        {
            goldKeyImage.color = _keyMissingColor;
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
