using UnityEngine;

[CreateAssetMenu(fileName = "DefaultKeyData", menuName = "Settings/KeyData")]
public class KeyData : ScriptableObject
{
    public KeyCode rotate = KeyCode.UpArrow;
    public KeyCode right = KeyCode.RightArrow;
    public KeyCode left = KeyCode.LeftArrow;
    public KeyCode down = KeyCode.DownArrow;
    public KeyCode hardDrop = KeyCode.Space;
    public KeyCode hold = KeyCode.LeftShift;
    public KeyCode hold2 = KeyCode.C;
    public KeyCode zRotate = KeyCode.Z;
    public KeyCode aRotate = KeyCode.A;
    public KeyCode openstat = KeyCode.K;
}
