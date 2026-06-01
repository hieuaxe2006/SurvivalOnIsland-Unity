using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.04f;

    private Queue<string> sentences = new Queue<string>();
    private string currentSentence = "";
    private bool isTyping = false;
    private Action onDialogueEndCallback;

    private PlayerMovement playerMovement;
    private PlayerLook playerLook;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Find player in scene
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerLook = playerMovement.GetComponent<PlayerLook>();
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(DisplayNextSentence);
        }
    }

    /// <summary>Returns whether the dialogue panel is currently active.</summary>
    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }

    /// <summary>Starts a dialogue sequence with the given speaker and sentences.</summary>
    public void StartDialogue(string speakerName, string[] sentencesList, Action onComplete = null)
    {
        if (dialoguePanel == null) return;

        onDialogueEndCallback = onComplete;
        sentences.Clear();

        foreach (string sentence in sentencesList)
        {
            sentences.Enqueue(sentence);
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
        }

        // Disable player controls and unlock cursor
        SetPlayerControl(false);

        dialoguePanel.SetActive(true);
        DisplayNextSentence();
    }

    /// <summary>Displays the next sentence or finishes typing the current one.</summary>
    public void DisplayNextSentence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        if (isTyping)
        {
            // If typing, show full sentence immediately
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();
        StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Re-enable player controls
        SetPlayerControl(true);

        // Invoke completion callback
        onDialogueEndCallback?.Invoke();
    }

    private void SetPlayerControl(bool state)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = state;
        }

        if (playerLook != null)
        {
            playerLook.enabled = state;
        }

        if (!state)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
