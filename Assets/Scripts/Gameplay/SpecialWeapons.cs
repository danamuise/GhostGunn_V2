using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SpecialWeaponType
{
    None,
    Nuke,
    Fire,
    // Add more here if needed
}

public class SpecialWeapons : MonoBehaviour
{
    [System.Serializable]
    public class SpecialWeapon
    {
        public SpecialWeaponType type;
        public GameObject icon;
        public GameObject wordBalloon;
        public GameObject hitVFX;
        public SpriteRenderer outline;
        public float chargeRequired = 1000f;
    }

    [Header("Shared References")]
    public Transform chargeBar;
    public List<SpecialWeapon> specialWeapons = new List<SpecialWeapon>();
    public Vector3 consoleTargetPosition = new Vector3(-1.48f, -3.72f, 0f);
    public float travelDuration = 0.75f;

    private SpecialWeapon currentWeapon;
    private float currentCharge = 0f;
    private bool isArmed = false;
    private GameObject balloonInstance;
    private int targetHitScore = -1; //for target hit

    private void Start()
    {
        foreach (var weapon in specialWeapons)
        {
            if (weapon.icon != null) weapon.icon.SetActive(false);
            if (weapon.outline != null) weapon.outline.enabled = false;
        }

        UpdateChargeBar(0f);
    }

    public void OnSpecialWeaponCollected(SpecialWeaponType type, GameObject pickupObject)
    {
        Debug.Log($"💡 Special Weapon Collected: {type}");

        targetHitScore = GameState.Instance.CurrentScore;
        Debug.Log($"🎯 Target {type} was hit at score {targetHitScore}");

        ActivateWeapon(type, pickupObject);
        SFXManager.Instance.Play("PUCollect");
    }


    public void ActivateWeapon(SpecialWeaponType type, GameObject pickupObject)
    {
        currentWeapon = specialWeapons.Find(w => w.type == type);

        if (currentWeapon == null)
        {
            Debug.LogWarning($"⚠️ No special weapon found with type: {type}");
            return;
        }

        StartCoroutine(MoveToConsole(pickupObject));
    }

    private IEnumerator MoveToConsole(GameObject pickupObject)
    {
        Vector3 start = pickupObject.transform.position;
        float elapsed = 0f;

        Collider2D col = pickupObject.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        while (elapsed < travelDuration)
        {
            float t = Mathf.Pow(elapsed / travelDuration, 3f);
            pickupObject.transform.position = Vector3.Lerp(start, consoleTargetPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        pickupObject.transform.position = consoleTargetPosition;

        if (currentWeapon.icon != null)
        {
            currentWeapon.icon.SetActive(true);

            if (currentWeapon.hitVFX != null)
            {
                Instantiate(currentWeapon.hitVFX, currentWeapon.icon.transform.position, Quaternion.identity, currentWeapon.icon.transform);
            }

            SFXManager.Instance.Play("SWIconLand");
        }

        if (currentWeapon.wordBalloon != null)
        {
            balloonInstance = Instantiate(currentWeapon.wordBalloon, new Vector3(0, -2.4f, 0f), Quaternion.identity);
            StartCoroutine(HideWordBalloonAfterDelay(3f));
        }

        foreach (var sr in pickupObject.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = false;
        }

        Destroy(pickupObject, 3f);
    }

    private IEnumerator HideWordBalloonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (balloonInstance != null)
        {
            Destroy(balloonInstance);
        }
    }

    public void AddCharge(float amount)
    {
        if (isArmed || currentWeapon == null)
            return;

        currentCharge += amount;
        currentCharge = Mathf.Clamp(currentCharge, 0f, currentWeapon.chargeRequired);

        float percent = currentCharge / currentWeapon.chargeRequired;
        UpdateChargeBar(percent);

        // Update icon tint
        if (currentWeapon.icon != null)
        {
            SWIconTarget iconTarget = currentWeapon.icon.GetComponent<SWIconTarget>();
            if (iconTarget != null)
            {
                iconTarget.SetChargeProgress(percent);
            }
        }

        // When fully charged
        if (!isArmed && targetHitScore >= 0 && GameState.Instance.CurrentScore >= targetHitScore + currentWeapon.chargeRequired)

        {
            isArmed = true;

            if (currentWeapon.outline != null)
            {
                currentWeapon.outline.gameObject.SetActive(true);
                currentWeapon.outline.enabled = true;
            }

            Debug.Log($"🚀 {currentWeapon.type} is fully charged and armed.");
        }
    }


    private void UpdateChargeBar(float percent)
    {
        if (chargeBar != null)
        {
            float scaledY = Mathf.Lerp(0.00001f, 0.01f, percent);
            chargeBar.localScale = new Vector3(1f, scaledY, 1f);
            Debug.Log($"🔋 Charge bar updated to {percent * 100:F0}%");
        }
    }

    public void TriggerSpecialWeapon(SpecialWeaponType type)
    {
        SpecialWeapon weaponToTrigger = specialWeapons.Find(w => w.type == type);

        if (weaponToTrigger == null)
        {
            Debug.LogWarning($"⚠️ No weapon found for type {type}");
            return;
        }

        if (!isArmed || weaponToTrigger != currentWeapon)
        {
            Debug.LogWarning($"⚠️ Weapon {type} is either not armed or not the current weapon.");
            return;
        }

        Debug.Log($"💥 TRIGGERED: {type} special weapon!");

        if (type == SpecialWeaponType.Nuke)
        {
            Debug.Log("🚨 Activating Nuke Sequence...");

            GameState.Instance.SaveState();
            GameState.Instance.SpecialWeaponUnlocked = SpecialWeaponType.Fire;

            SceneManager.LoadScene("ChallengeLevel1");
        }

        isArmed = false;
        currentCharge = 0f;
        UpdateChargeBar(0f);

        if (weaponToTrigger.outline != null)
            weaponToTrigger.outline.enabled = false;

        if (weaponToTrigger.icon != null)
            weaponToTrigger.icon.SetActive(false);
    }
}
