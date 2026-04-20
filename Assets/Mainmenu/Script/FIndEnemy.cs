using UnityEngine;

[CreateAssetMenu(fileName = "FindEnemy", menuName = "Scriptable Objects/FindEnemy")]
public class FindEnemy : ScriptableObject
{
    public bool slime = false;
    public bool goblin = false;
    public bool ouger = false;
    public bool golem = false;
    public bool chraken = false;
    public bool ghost = false;
    public bool dragon = false;
    public bool crown = false;
    public bool shaman = false;
    public bool knight_night = false;
    public bool boss = false;
    public void ResetAll()
    {
        slime = goblin = ouger =  golem = chraken =
        ghost = dragon = crown = shaman = knight_night = boss = false;
    }
    public void Save()
    {
        PlayerPrefs.SetInt("Found_Goblin", goblin ? 1 : 0);
        PlayerPrefs.SetInt("Found_Slime", slime ? 1 : 0);
        PlayerPrefs.SetInt("Found_Ouger", ouger ? 1 : 0);
        PlayerPrefs.SetInt("Found_Golem", golem ? 1 : 0);
        PlayerPrefs.SetInt("Found_Chraken", chraken ? 1 : 0);
        PlayerPrefs.SetInt("Found_Ghost", ghost ? 1 : 0);
        PlayerPrefs.SetInt("Found_Dragon", dragon ? 1 : 0);
        PlayerPrefs.SetInt("Found_Crown", crown ? 1 : 0);
        PlayerPrefs.SetInt("Found_Shaman", shaman ? 1 : 0);
        PlayerPrefs.SetInt("Found_Knight_Night", knight_night ? 1 : 0);
        PlayerPrefs.SetInt("Found_Boss", boss ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void Load()
    {
        goblin = PlayerPrefs.GetInt("Found_Goblin", 0) == 1;
        slime = PlayerPrefs.GetInt("Found_Slime", 0) == 1;
        ouger = PlayerPrefs.GetInt("Found_Ouger", 0) == 1;
        golem = PlayerPrefs.GetInt("Found_Golem", 0) == 1;
        chraken = PlayerPrefs.GetInt("Found_Chraken", 0) == 1;
        ghost = PlayerPrefs.GetInt("Found_Ghost", 0) == 1;
        dragon = PlayerPrefs.GetInt("Found_Dragon", 0) == 1;
        crown = PlayerPrefs.GetInt("Found_Crown", 0) == 1;
        shaman = PlayerPrefs.GetInt("Found_Shaman", 0) == 1;
        knight_night = PlayerPrefs.GetInt("Found_Knight_Night", 0) == 1;
        boss = PlayerPrefs.GetInt("Found_Boss", 0) == 1;
    }
}
