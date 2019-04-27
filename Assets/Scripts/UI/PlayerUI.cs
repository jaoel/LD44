using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    public GameObject healthMarkerPrefab;

    [Space(20)]
    public Image maxHealthForegroundImage;
    public Image maxHealthBackgroundImage;
    public Image currentHealthImage;
    public RectTransform healthMarkersObject;

    public int healthPerPixel = 2;

    private int targetHealth = 100;
    private int targetMaxHealth = 100;

    private int currentHealth = 0;
    private int currentMaxHealth = 0;

    private void FixedUpdate() {
        if(currentHealth != targetHealth) {
            if(currentHealth < targetHealth) {
                currentHealth += 5;
                if (currentHealth > targetHealth) {
                    currentHealth = targetHealth;
                }
            }
            if (currentHealth > targetHealth) {
                currentHealth -= 5;
                if (currentHealth < targetHealth) {
                    currentHealth = targetHealth;
                }
            }
            SetHealth(currentHealth);
        }

        if (currentMaxHealth != targetMaxHealth) {
            if (currentMaxHealth < targetMaxHealth) {
                currentMaxHealth += 5;
                if (currentMaxHealth > targetMaxHealth) {
                    currentMaxHealth = targetMaxHealth;
                }
            }
            if (currentMaxHealth > targetMaxHealth) {
                currentMaxHealth -= 5;
                if (currentMaxHealth < targetMaxHealth) {
                    currentMaxHealth = targetMaxHealth;
                }
            }
            SetMaxHealth(currentMaxHealth);
        }
    }

    public void SetHealthbar(int health, int maxHealth) {
        targetHealth = health;
        targetMaxHealth = maxHealth;
    }

    private void SetHealth(int health) {
        currentHealth = health;
        health = health * healthPerPixel;
        Vector2 sizeDelta = currentHealthImage.rectTransform.sizeDelta;

        sizeDelta.x = health;

        currentHealthImage.rectTransform.sizeDelta = sizeDelta;
    }

    private void SetMaxHealth(int maxHealth) {
        currentMaxHealth = maxHealth;
        maxHealth = maxHealth * healthPerPixel;
        Vector2 foregroundSizeDelta = maxHealthForegroundImage.rectTransform.sizeDelta;
        Vector2 backgroundSizeDelta = maxHealthBackgroundImage.rectTransform.sizeDelta;

        foregroundSizeDelta.x = maxHealth - 2;
        backgroundSizeDelta.x = maxHealth - 4;

        maxHealthForegroundImage.rectTransform.sizeDelta = foregroundSizeDelta;
        maxHealthBackgroundImage.rectTransform.sizeDelta = backgroundSizeDelta;

        int numMarkersWanted = maxHealth / 20;
        int currentMarkerCount = healthMarkersObject.childCount;
        if (numMarkersWanted > currentMarkerCount) {
            int newMarkerCount = numMarkersWanted - currentMarkerCount;
            for(int i = 0; i < newMarkerCount; i++) {
                Instantiate(healthMarkerPrefab, healthMarkersObject);
            }
        }

        for(int i = 0; i < healthMarkersObject.childCount; i++) {
            if (i < numMarkersWanted) {
                healthMarkersObject.GetChild(i).gameObject.SetActive(true);
            } else {
                healthMarkersObject.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
