using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct GuideSlide
{
    public Sprite image;
    [TextArea(3, 10)]
    public string description;
}

public class GuideUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private Image guideImage;
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button closeButton;

    [Header("Slides Configuration")]
    [SerializeField] private List<GuideSlide> slides;

    private int currentIndex = 0;

    private void Start()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextPage);
        }

        if (prevButton != null)
        {
            prevButton.onClick.AddListener(PreviousPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseGuide);
        }
    }

    public System.Action OnClose;

    /// <summary>Opens the guide panel and shows the first slide.</summary>
    public void OpenGuide()
    {
        if (slides == null || slides.Count == 0)
        {
            Debug.LogWarning("[GuideUI] No slides configured in the Guide UI component!");
            return;
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }

        currentIndex = 0;
        ShowPage(currentIndex);
    }

    /// <summary>Closes the guide panel.</summary>
    public void CloseGuide()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
        OnClose?.Invoke();
    }

    private void ShowPage(int index)
    {
        if (index < 0 || index >= slides.Count) return;

        currentIndex = index;
        GuideSlide currentSlide = slides[currentIndex];

        if (guideText != null)
        {
            guideText.text = currentSlide.description;
        }

        if (guideImage != null)
        {
            if (currentSlide.image != null)
            {
                guideImage.gameObject.SetActive(true);
                guideImage.sprite = currentSlide.image;
            }
            else
            {
                guideImage.gameObject.SetActive(false);
            }
        }

        if (prevButton != null)
        {
            prevButton.interactable = (currentIndex > 0);
        }

        if (nextButton != null)
        {
            nextButton.interactable = (currentIndex < slides.Count - 1);
        }
    }

    /// <summary>Navigates to the next slide.</summary>
    public void NextPage()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (currentIndex < slides.Count - 1)
        {
            ShowPage(currentIndex + 1);
        }
    }

    /// <summary>Navigates to the previous slide.</summary>
    public void PreviousPage()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (currentIndex > 0)
        {
            ShowPage(currentIndex - 1);
        }
    }
}
