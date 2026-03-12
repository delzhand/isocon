using UnityEngine;

public class Shadow
{
    public static Material SquareShadow;
    public static Material Hex1Shadow;
    public static Material Hex2Shadow;
    public static Material Hex3Shadow;
    public static Material Hex4Shadow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitMaterials()
    {
        SquareShadow = Resources.Load<Material>("Materials/Token/BorderBase");
        Hex1Shadow = Resources.Load<Material>("Materials/Token/HexBase1");
        Hex2Shadow = Resources.Load<Material>("Materials/Token/HexBase2");
        Hex3Shadow = Resources.Load<Material>("Materials/Token/HexBase3");
        Hex4Shadow = Resources.Load<Material>("Materials/Token/HexBase4");
    }
}
