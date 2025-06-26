using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BookPhotoMatcher : MonoBehaviour
{
    [Header("Book Reference")]
    public Book book; // Assign in Inspector

    [Header("UI Book Photo Buttons")]
    public Button[] bookPhotoButtons; // bookPhoto0–bookPhoto3

    [Header("Civilian Photo Animators")]
    public Animator[] civilianAnimators; // 3 animators, top one is current

    [Header("Book Photo Button Roots (Prefabs)")]
    public GameObject[] bookPhotoButtonRoots; // Same order as bookPhotoButtons

    private int[] civilianPhotoIDs = new int[] { 0, 14, 10 };
    private int currentCivilianIndex = 0;
    private int[] bookPhotoIDsOnCurrentPage = new int[4];

    [Header("Parent Animator for Slide Outs")]
    public Animator civilianPhotosParentAnimator;

    void Start()
    {
        currentCivilianIndex = 0;

        // Hook up photo button listeners
        for (int i = 0; i < bookPhotoButtons.Length; i++)
        {
            int index = i;
            bookPhotoButtons[i].onClick.AddListener(() => OnPhotoButtonClicked(index));
        }
    }

    void OnPhotoButtonClicked(int index)
    {
        if (index < 0 || index >= bookPhotoIDsOnCurrentPage.Length)
        {
            Debug.LogWarning($"📸 bookPhoto{index} clicked, but ID array is not ready.");
            return;
        }

        if (currentCivilianIndex >= civilianPhotoIDs.Length)
        {
            Debug.LogWarning($"🚫 Invalid civilian index: {currentCivilianIndex} (max = {civilianPhotoIDs.Length - 1})");
            return;
        }

        int selectedID = bookPhotoIDsOnCurrentPage[index];
        int targetID = civilianPhotoIDs[currentCivilianIndex];

        if (selectedID == targetID)
        {
            Debug.Log($"photo{selectedID}: correct");
            ShowFeedback(index, true);

            SetPhotoButtonsInteractable(false);
            StartCoroutine(HandleCorrectMatchSequence());
        }
        else
        {
            Debug.Log($"photo{selectedID}: incorrect");
            ShowFeedback(index, false);
        }

        Debug.Log($"🧠 Current Civilian Index: {currentCivilianIndex} | Target ID: {targetID} | Selected ID: {selectedID}");
    }

    IEnumerator HandleCorrectMatchSequence()
    {
        // 1. Wait 0.75s after showing "correct" marker
        yield return new WaitForSeconds(0.75f);

        // 2. Close the book
        HideAllBookPhotoMarkers();
        book.CloseBook();

        // 3. Wait for book closing to complete (~0.5s)
        yield return new WaitForSeconds(0.5f);

        // 4. Trigger stamp param on the current civilian photo
        if (currentCivilianIndex < civilianAnimators.Length && civilianAnimators[currentCivilianIndex] != null)
        {
            string stampParam = $"stamp{currentCivilianIndex}";
            civilianAnimators[currentCivilianIndex].SetBool(stampParam, true);
            Debug.Log($"📬 Triggered animation bool '{stampParam}' on civilian photo {currentCivilianIndex}");
        }

        // 5. Wait 0.75s before slide-out
        yield return new WaitForSeconds(0.75f);

        // 6. Trigger slideOutX on parent
        if (civilianPhotosParentAnimator != null)
        {
            string slideOutParam = $"slideOut{currentCivilianIndex}";
            civilianPhotosParentAnimator.SetBool(slideOutParam, true);
            Debug.Log($"📤 Triggered animation bool '{slideOutParam}' on CivilianPhotos");
        }

        // 7. Wait before advancing logic
        yield return new WaitForSeconds(1.2f);
        AdvanceToNextCivilian();
    }

    void AdvanceToNextCivilian()
    {
        currentCivilianIndex++;

        if (currentCivilianIndex >= civilianPhotoIDs.Length)
        {
            Debug.Log("✅ All civilians matched. Unlock power-up!");
            return;
        }

        Debug.Log($"🔍 Now searching for photo ID: {civilianPhotoIDs[currentCivilianIndex]}");
        SetPhotoButtonsInteractable(true);
    }

    public void SetPagePhotoIDs(int id0, int id1, int id2, int id3)
    {
        bookPhotoIDsOnCurrentPage[0] = id0;
        bookPhotoIDsOnCurrentPage[1] = id1;
        bookPhotoIDsOnCurrentPage[2] = id2;
        bookPhotoIDsOnCurrentPage[3] = id3;
    }

    public void UpdateBookPhotosForPage(int currentPage)
    {
        int spreadIndex = (currentPage - 2) / 2;
        int baseID = spreadIndex * 4;

        SetPagePhotoIDs(baseID, baseID + 1, baseID + 2, baseID + 3);
        Debug.Log($"📖 Book page {currentPage} set photo IDs {baseID}–{baseID + 3}");
    }

    void ShowFeedback(int index, bool isCorrect)
    {
        if (index < 0 || index >= bookPhotoButtonRoots.Length) return;

        Transform root = bookPhotoButtonRoots[index].transform;
        Transform correctMark = root.Find("correct");
        Transform incorrectMark = root.Find("incorrect");

        if (correctMark != null) correctMark.gameObject.SetActive(isCorrect);
        if (incorrectMark != null) incorrectMark.gameObject.SetActive(!isCorrect);
    }

    public void SetPhotoButtonsInteractable(bool isOn)
    {
        Debug.Log($"🔧 Setting photo buttons interactable = {isOn}");
        foreach (var btn in bookPhotoButtons)
        {
            if (btn != null)
            {
                btn.interactable = isOn;
                var colors = btn.colors;
                colors.disabledColor = new Color(1f, 1f, 1f, 0f);
                btn.colors = colors;
            }
        }
    }

    void HideAllBookPhotoMarkers()
    {
        foreach (GameObject root in bookPhotoButtonRoots)
        {
            if (root == null) continue;

            Transform correct = root.transform.Find("correct");
            Transform incorrect = root.transform.Find("incorrect");

            if (correct != null) correct.gameObject.SetActive(false);
            if (incorrect != null) incorrect.gameObject.SetActive(false);
        }
    }

}
