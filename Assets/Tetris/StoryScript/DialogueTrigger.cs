using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueEventChannel eventChannel; // 동일한 무전기 연결
    public List<DialogueGroup> dialogueGroups = new List<DialogueGroup>();

    public void TriggerDialogueByName(string name)
    {
        DialogueGroup group = dialogueGroups.Find(g => g.groupName == name);
        if (group != null && eventChannel != null)
        {
            // 매니저를 찾지 않고 에셋에 신호를 보냄 (매우 빠름)
            eventChannel.RaiseEvent(group.lines, () => {
                Debug.Log("대화 끝!");
            });
        }
    }
}