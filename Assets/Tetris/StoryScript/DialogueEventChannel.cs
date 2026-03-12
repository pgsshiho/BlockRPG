using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Dialogue/Event Channel")]
public class DialogueEventChannel : ScriptableObject
{
    // 대화 시작 요청을 보낼 이벤트
    public Action<DialogueLine[], Action> OnDialogueRequested;

    public void RaiseEvent(DialogueLine[] lines, Action onComplete)
    {
        // 구독자(Manager)가 있으면 실행
        OnDialogueRequested?.Invoke(lines, onComplete);
    }
}