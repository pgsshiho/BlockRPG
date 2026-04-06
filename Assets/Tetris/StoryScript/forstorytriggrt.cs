using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class forstorytriggrt : MonoBehaviour
{
    [Header("Event Channel")]
    [SerializeField] private DialogueEventChannel eventChannel;
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Dialogue Data")]
    public List<DialogueGroup> dialogueGroups = new List<DialogueGroup>();

    private int phase = 0;
    public GameObject hold, enemy, stat;
    public bool istuto = false;

    void Start()
    {
        if (dialogueManager == null) dialogueManager = FindFirstObjectByType<DialogueManager>();
        StartPhase(0);
    }

    public void StartPhase(int index)
    {
        phase = index;
        if (phase < dialogueGroups.Count)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.isON = true;
                Time.timeScale = 0;
            }

            eventChannel.RaiseEvent(dialogueGroups[phase].lines, () => {
                StartCoroutine(WaitForKeyStep());
            });
        }
    }

    IEnumerator WaitForKeyStep()
    {
        istuto = true;
        var keys = KeyBinding.instance;
        if (keys == null) yield break;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isON = false;
            Time.timeScale = 1;
        }

        // 대사창을 넘길 때 썼던 키가 로직에 영향을 주지 않도록 한 프레임 대기
        yield return null;

        bool proceed = false;
        while (!proceed)
        {
            switch (phase)
            {
                case 0: if (Input.GetKeyDown(keys.right) || Input.GetKeyDown(keys.left)) proceed = true; break;
                case 1: if (Input.GetKeyDown(keys.rotate) || Input.GetKeyDown(keys.zRotate)) proceed = true; break;
                case 2:
                    if (Input.GetKeyDown(keys.aRotate))
                    {
                        if (hold != null) hold.SetActive(true);
                        proceed = true;
                    }
                    break;
                case 3:
                    if (Input.GetKeyDown(keys.hold) || Input.GetKeyDown(keys.hold2))
                    {
                        if (hold != null) hold.SetActive(false);
                        proceed = true;
                    }
                    break;
                case 4: if (Input.GetKeyDown(keys.down)) proceed = true; break;
                case 5:
                    if (Input.GetKeyDown(keys.hardDrop))
                    {
                        if (stat != null) stat.SetActive(true);
                        proceed = true;
                    }
                    break;
                case 6:
                    // [수정] 이전 단계(하드드롭)의 입력이 남아있을 수 있으므로 
                    // GetKeyDown 대신 조금 더 안전한 조건이나 프레임 분리 필요
                    if (Input.anyKeyDown && !Input.GetKeyDown(keys.hardDrop))
                    {
                        if (stat != null) stat.SetActive(false);
                        proceed = true;
                    }
                    break;
                case 7:
                    // 마우스 클릭이나 스페이스바 대기
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) proceed = true;
                    break;
            }
            yield return null;
        }

        if (dialogueManager != null) dialogueManager.ClosePanel();

        // [중요] 다음 단계 대사가 나오기 전에 실시간 시간으로 약간의 여유를 줌
        // 이렇게 해야 하드드롭 후 바로 대사가 뜨고 사라지는 것을 막을 수 있음
        yield return new WaitForSecondsRealtime(0.5f);

        int nextPhase = phase + 1;
        if (nextPhase < dialogueGroups.Count) StartPhase(nextPhase);
        else SceneChanger.BG("Mainmenu");
    }
}