using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    public string infiniteSceneName = "Tetris";
    public string storySceneName = "StoryTetris";
    public string dungeonSceneName = "Dungeon";
    public string customSceneName = "custom";
    public void infinitestart()
    {
        Time.timeScale = 1f;
        Stat.instance.FullHeal();
        blockclear.currentScore = 0;
        SceneChanger.BG(infiniteSceneName);
    }
    public void StoryStart() => SceneChanger.BG(storySceneName);
    public void DungeonStart()
    {
        Time.timeScale = 1f;
        Stat.instance.FullHeal();
        blockclear.currentScore = 0;
        SceneChanger.BG(dungeonSceneName);
    }
    public void StartCUstom()
    {
        Time.timeScale = 1f;
        Stat.instance.FullHeal();
        blockclear.currentScore = 0;
        SceneChanger.BG(customSceneName);
    }
}
