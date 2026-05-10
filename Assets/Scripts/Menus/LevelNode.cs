using UnityEngine;
using UnityEngine.UI;

public class LevelNode : MonoBehaviour
{
    // --- CONFIGURATION ---

    public string sceneToLoad;

    public GameObject lockIcon;

    public Image outlineImage;

    public Button button;

    // --- STATE COLORS ---

    [Header("Couleurs d'état")]
    public Color unlockedColor = Color.white;

    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    // --- STATUS MANAGEMENT ---

    /// Updates level status (locked/unlocked) and visual appearance.
    public void SetStatus(bool unlocked)
    {
        button.interactable = unlocked;

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (outlineImage != null)
            outlineImage.color = unlocked ? unlockedColor : lockedColor;
    }
}