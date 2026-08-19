using UnityEngine;

public class ResetManager : MonoBehaviour
{
    [SerializeField] private FindEnemy enemyData;
    [SerializeField] private Reincarnation reincarnation;

    public void FullReset()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        ResetStat();
        ResetEnemyData();

        if (reincarnation != null)
            reincarnation.PerformReincarnation();
    }

    private void ResetStat()
    {
        Stat.instance.ResetStat();
    }

    private void ResetEnemyData()
    {
        if (enemyData == null)
            return;

        enemyData.slime = false;
        enemyData.goblin = false;
        enemyData.ouger = false;
        enemyData.golem = false;
        enemyData.chraken = false;
        enemyData.ghost = false;
        enemyData.dragon = false;
        enemyData.crown = false;
        enemyData.shaman = false;
        enemyData.knight_night = false;
        enemyData.boss = false;

        enemyData.Save();
    }
}