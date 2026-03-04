using Unity.VisualScripting;
using UnityEngine;

public class Stat : MonoBehaviour
{
    public int difficult = 0;
    public int it = 0;
    public int atk = 0;
    public int def = 0;
    public int maxstatpoint = 0;
    public int currentmaxstatpoint = 0;
    public int hp = 0;
    public int maxhp = 0;
    public int level = 1;
    public float ex = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void upit()
    {
        it++;
    }
    public void upatk()
    {
        atk++;

    }
    public void updef()
    {
        def++;
    }
    public void levelip()
    {
        if(level * 30 <= ex)
        {
            level++;
            ex += 3;
        }
    }
}
