using UnityEngine;

[CreateAssetMenu(fileName = "FindEnemy", menuName = "Scriptable Objects/FindEnemy")]
public class FindEnemy : ScriptableObject
{
    public bool slime = false;
    public bool goblin = false;
    public bool ouger = false;
    public bool siren = false;
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
        slime = goblin = ouger = siren = golem = chraken =
        ghost = dragon = crown = shaman = knight_night = boss = false;
    }
}
