using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

// [중요] 클래스 외부로 빼고 [System.Serializable]을 붙여야 
// 다른 스크립트(DialogueTrigger)에서 인식하고 인스펙터에 노출됩니다.
[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 10)]
    public string text;
    public Sprite portrait;
}
[System.Serializable]
public class DialogueGroup
{
    public string groupName; // 식별용 이름 (예: "Greeting", "AfterQuest")
    public DialogueLine[] lines; // 해당 그룹의 대사들
}
public class DialogueManager : MonoBehaviour, IDialogueHandler
{
    [SerializeField] private DialogueEventChannel eventChannel; // 무전기 연결
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private bool isTyping = false;
    private Action onDialogueEnd;

    void Awake() => linesQueue = new Queue<DialogueLine>();

    // 인터페이스 구현
    public void StartDialogue(DialogueLine[] lines, Action onComplete = null)
    {
        onDialogueEnd = onComplete;
        dialoguePanel.SetActive(true);
        linesQueue.Clear();

        foreach (var line in lines) linesQueue.Enqueue(line);
        DisplayNextSentence();
    }
    void OnEnable()
    {
        // 이벤트 구독 시작
        if (eventChannel != null) eventChannel.OnDialogueRequested += StartDialogue;
    }

    void OnDisable()
    {
        // 메모리 누수 방지를 위해 해제
        if (eventChannel != null) eventChannel.OnDialogueRequested -= StartDialogue;
    }
    void Update()
    {
        if (dialoguePanel.activeSelf && !isTyping && Input.anyKeyDown)
            DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (linesQueue.Count == 0) { EndDialogue(); return; }
        StartCoroutine(TypeSentence(linesQueue.Dequeue().text));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        isTyping = false;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        onDialogueEnd?.Invoke();
    }
}
