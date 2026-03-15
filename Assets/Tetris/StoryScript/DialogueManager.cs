using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueEventChannel eventChannel;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private Action onDialogueEnd;

    void OnEnable() { if (eventChannel != null) eventChannel.OnDialogueRequested += StartDialogue; }
    void OnDisable() { if (eventChannel != null) eventChannel.OnDialogueRequested -= StartDialogue; }

    public void StartDialogue(DialogueLine[] lines, Action onComplete = null)
    {
        onDialogueEnd = onComplete;
        dialoguePanel.SetActive(true);
        linesQueue.Clear();

        foreach (var line in lines) linesQueue.Enqueue(line);
        DisplayNextSentence();
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        // 대사가 더 남아있을 때만 클릭으로 넘김
        if (linesQueue.Count > 0 && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            DisplayNextSentence();
        }
    }

    public void DisplayNextSentence()
    {
        if (linesQueue.Count == 0) return;

        DialogueLine line = linesQueue.Dequeue();
        dialogueText.text = ProcessDialogueText(line.text);

        if (linesQueue.Count == 0)
        {
            // [수정] 즉시 호출하지 않고 코루틴으로 한 프레임 뒤에 호출하여 
            // 현재 프레임의 클릭/키 입력을 초기화함
            StartCoroutine(DelayedRaiseEvent());
        }
    }

    IEnumerator DelayedRaiseEvent()
    {
        yield return null; // 현재 프레임 끝날 때까지 대기
        var callback = onDialogueEnd;
        onDialogueEnd = null;
        if (callback != null) callback.Invoke();
    }
    public void ClosePanel()
    {
        dialoguePanel.SetActive(false);
    }

    private string ProcessDialogueText(string originalText)
    {
        var keys = KeyBinding.instance;
        if (keys == null) return originalText;

        return originalText
            .Replace("{Left}", $"<color=#FFD700>{keys.left}</color>")
            .Replace("{Right}", $"<color=#FFD700>{keys.right}</color>")
            .Replace("{Rotate}", $"<color=#FFD700>{keys.rotate}</color>")
            .Replace("{ZRotate}", $"<color=#FFD700>{keys.zRotate}</color>")
            .Replace("{ARotate}", $"<color=#FFD700>{keys.aRotate}</color>")
            .Replace("{Hold}", $"<color=#FFD700>{keys.hold}</color>")
            .Replace("{Hold2}", $"<color=#FFD700>{keys.hold2}</color>")
            .Replace("{Down}", $"<color=#FFD700>{keys.down}</color>")
            .Replace("{HardDrop}", $"<color=#FFD700>{keys.hardDrop}</color>")
            .Replace("{Stat}", $"<color=#FFD700>{keys.openstat}</color>");
    }
}