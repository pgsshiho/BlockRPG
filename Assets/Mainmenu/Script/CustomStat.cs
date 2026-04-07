using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class CustomStat : MonoBehaviour
{
    [Header("Monster Counts")]
    public int Slime;
    public int Goblin, Ouger, Siren, Golem, Chraken, Ghost, Dragon, Crown, Shaman, Knight_night,Boss;

    [Header("UI Text References")]
    // 각 숫자를 표시할 텍스트 객체들을 인스펙터에서 드래그해서 넣어주세요.
    public TextMeshProUGUI txtSlime;
    public TextMeshProUGUI txtGoblin, txtOuger, txtSiren, txtGolem, txtChraken, txtGhost, txtDragon, txtCrown, txtShaman, txtKnight,txtBoss;

    void Start()
    {
        // 시작할 때 모든 텍스트를 현재 값(0)으로 초기화
        UpdateAllTexts();
    }

    public void ChangeMonsterCount(int index, int amount)
    {
        switch (index)
        {
            case 0: Slime = Mathf.Clamp(Slime + amount, 0, 10); break;
            case 1: Goblin = Mathf.Clamp(Goblin + amount, 0, 10); break;
            case 2: Ouger = Mathf.Clamp(Ouger + amount, 0, 10); break;
            case 3: Siren = Mathf.Clamp(Siren + amount, 0, 10); break;
            case 4: Golem = Mathf.Clamp(Golem + amount, 0, 10); break;
            case 5: Chraken = Mathf.Clamp(Chraken + amount, 0, 10); break;
            case 6: Ghost = Mathf.Clamp(Ghost + amount, 0, 10); break;
            case 7: Dragon = Mathf.Clamp(Dragon + amount, 0, 10); break;
            case 8: Crown = Mathf.Clamp(Crown + amount, 0, 10); break;
            case 9: Shaman = Mathf.Clamp(Shaman + amount, 0, 10); break;
            case 10: Knight_night = Mathf.Clamp(Knight_night + amount, 0, 10); break;
            case 11: Boss = Mathf.Clamp(Boss + amount, 0, 10); break;
        }

        // 값이 바뀌었으므로 텍스트 갱신
        UpdateAllTexts();
    }
    // CustomStat.cs 안에 추가
    // 인덱스를 양수(+)로 넣으면 +1, 음수(-)로 넣으면 -1로 처리하는 방식

    public void Btn_Slime(int amount) => ChangeMonsterCount(0, amount);
    public void Btn_Goblin(int amount) => ChangeMonsterCount(1, amount);
    public void Btn_Ouger(int amount) => ChangeMonsterCount(2, amount);
    public void Btn_Siren(int amount) => ChangeMonsterCount(3, amount);
    public void Btn_Golem(int amount) => ChangeMonsterCount(4, amount);
    public void Btn_Chraken(int amount) => ChangeMonsterCount(5, amount);
    public void Btn_Ghost(int amount) => ChangeMonsterCount(6, amount);
    public void Btn_Dragon(int amount) => ChangeMonsterCount(7, amount);
    public void Btn_Crown(int amount) => ChangeMonsterCount(8, amount);
    public void Btn_Shaman(int amount) => ChangeMonsterCount(9, amount);
    public void Btn_Knight(int amount) => ChangeMonsterCount(10, amount);
    public void Btn_Boss(int amount) => ChangeMonsterCount(11, amount);
    // 모든 텍스트 UI를 변수값에 맞춰 업데이트하는 함수
    public void UpdateAllTexts()
    {
        if (txtSlime != null) txtSlime.text = Slime.ToString();
        if (txtGoblin != null) txtGoblin.text = Goblin.ToString();
        if (txtOuger != null) txtOuger.text = Ouger.ToString();
        if (txtSiren != null) txtSiren.text = Siren.ToString();
        if (txtGolem != null) txtGolem.text = Golem.ToString();
        if (txtChraken != null) txtChraken.text = Chraken.ToString();
        if (txtGhost != null) txtGhost.text = Ghost.ToString();
        if (txtDragon != null) txtDragon.text = Dragon.ToString();
        if (txtCrown != null) txtCrown.text = Crown.ToString();
        if (txtShaman != null) txtShaman.text = Shaman.ToString();
        if (txtKnight != null) txtKnight.text = Knight_night.ToString();
        if (txtBoss != null) txtBoss.text = Boss.ToString();
    }

    public void enter()
    {
        if (DataHolder.instance != null)
        {
            int[] counts = { Slime, Goblin, Ouger, Siren, Golem, Chraken, Ghost, Dragon, Crown, Shaman, Knight_night, Boss };

            for (int i = 0; i < counts.Length; i++)
            {
                DataHolder.instance.monsterCounts[i] = counts[i];
            }
            Debug.Log($"[데이터 전송 완료] 0번 슬라임: {DataHolder.instance.monsterCounts[0]}마리");

            SceneChanger.BG("custom"); 
        }
        else
        {
            Debug.LogError("DataHolder 인스턴스가 없습니다! 메인메뉴부터 시작했는지 확인하세요.");
        }
    }
}