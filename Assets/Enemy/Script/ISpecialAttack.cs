public interface ISpecialAttack
{
    void ApplyEffect();  // 효과 시작 (공격 시 호출)
    void RemoveEffect(); // 효과 종료 (지속시간 종료 혹은 사망 시 호출)
}