using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class forstorytriggrt : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private DialogueEventChannel eventChannel;

    [Header("Dialogue Data")]
    public List<DialogueGroup> dialogueGroups = new List<DialogueGroup>();

    private int phase = 0;
    public GameObject move, rotate, drop, hold, enemy, stat;
    public bool istuto = false;

    void Start()
    {
        // 씬 시작 시 첫 번째 대화 실행 (참조 없이 채널에 쏘기)
        TriggerNextPhaseDialogue("first");
    }

    public void TriggerNextPhaseDialogue(string groupName)
    {
        DialogueGroup group = dialogueGroups.Find(g => g.groupName == groupName);

        if (group != null && eventChannel != null)
        {
            // [최적화] 대화가 끝났을 때 실행될 행동을 델리게이트로 바로 전달
            eventChannel.RaiseEvent(group.lines, () => {
                StartCoroutine(WaitForKeyStep());
            });
        }
    }

    IEnumerator WaitForKeyStep()
    {
        istuto = true;

        switch (phase)
        {
            case 0:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                move.SetActive(true);
                // 필요하다면 다음 대화 실행
                // TriggerNextPhaseDialogue("after_move"); 
                break;

            case 1:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
                rotate.SetActive(true);
                break;
        }

        phase++;
    }
}