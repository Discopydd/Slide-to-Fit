using UnityEngine;

[System.Serializable]
public class CarConfig
{
    public string id;
    public int x;
    public int y;
    public int length = 2;
    public VehicleOrientation orientation;
    public bool isTarget;
    public Color color = Color.gray;
}

[CreateAssetMenu(menuName = "Rush Hour/Level Config")]
public class LevelConfig : ScriptableObject
{
    public int width = 6;
    public int height = 6;
    public int exitRow = 2;

    public CarConfig[] cars;
}