using UnityEngine;

public class reincarnationSkill : MonoBehaviour // 상속을 MonoBehaviour로 변경
{
    Stat st;
    public bool isshiled = false; // 철자 수정 (isshiled -> isshield)

    void Start()
    {
        st = FindAnyObjectByType<Stat>();
    }

    public bool CheckSkillActivation(int level, float chancePerLevel = 0.25f)
    {
        if (level <= 0) return false;

        float currentChance = level * chancePerLevel;
        float randomValue = Random.Range(0f, 100f);

        return randomValue <= currentChance;
    }

    public void TrySkill()
    {
        // Reincarnation.instance가 있는지 먼저 확인 (안전장치)
        if (Reincarnation.instance == null) return;

        TryActivateShield();
        TryActivateLightning();
    }

    public void TryActivateShield()
    {
        // 싱글톤 인스턴스의 레벨 데이터를 참조
        if (CheckSkillActivation(Reincarnation.instance.shieldLevel))
        {
            isshiled = true;
            // TODO: 실제 대미지 감소 로직이나 이펙트 연결
        }
    }

    public void TryActivateLightning()
    {
        if (CheckSkillActivation(Reincarnation.instance.lightningLevel))
        {
            Enemybase[] enemies = FindObjectsByType<Enemybase>(FindObjectsSortMode.None);

            foreach (Enemybase enemy in enemies)
            {
                if (!enemy.IsDead)
                {
                    enemy.ApplyStun(2.0f);
                    // 여기에 번개 이펙트 코드를 추가하면 좋습니다.
                }
            }
        }
    }

    public void TryActivateSelfHeal(int lines) // 매개변수를 lines만 받음
    {
        // 데이터 창고(Reincarnation)가 없거나 레벨이 0이면 리턴
        if (Reincarnation.instance == null || Reincarnation.instance.selfHealLevel <= 0) return;

        if (st == null) st = FindAnyObjectByType<Stat>();

        // 질문하신 공식: 레벨 * 지워진 라인 * 0.25
        float healAmount = Reincarnation.instance.selfHealLevel * lines * 0.25f;

        st.hp += healAmount;

        // 최대 체력 초과 방지
        if (st.hp > st.maxhp)
        {
            st.hp = st.maxhp;
        }

        // 체력 바 UI 업데이트 함수가 있다면 호출
        st.hpcal();

    }
}