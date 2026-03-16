using System.Collections.Generic;
using UnityEngine;

public class Shadow
{
    public static Dictionary<string, Material> Materials;

    public static Material SquareShadow;
    public static Material Hex1Shadow;
    public static Material Hex2Shadow;
    public static Material Hex3Shadow;
    public static Material Hex4Shadow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitMaterials()
    {
        Materials = new();
        SquareShadow = Resources.Load<Material>("Materials/Token/BorderBase");
        Hex1Shadow = Resources.Load<Material>("Materials/Token/HexBase1");
        Hex2Shadow = Resources.Load<Material>("Materials/Token/HexBase2");
        Hex3Shadow = Resources.Load<Material>("Materials/Token/HexBase3");
        Hex4Shadow = Resources.Load<Material>("Materials/Token/HexBase4");
    }

    public static Material GetMaterial(string shape, string colorhex)
    {
        string key = $"{shape}|{colorhex}";
        if (Materials.ContainsKey(key))
        {
            return Materials[key];
        }
        else
        {
            Material baseMaterial = SquareShadow;
            switch (shape)
            {
                case "Hex1":
                    baseMaterial = Hex1Shadow;
                    break;
                case "Hex2":
                    baseMaterial = Hex2Shadow;
                    break;
                case "Hex3":
                    baseMaterial = Hex3Shadow;
                    break;
                case "Hex4":
                    baseMaterial = Hex4Shadow;
                    break;
            }
            Material m = Object.Instantiate(baseMaterial);
            Color color = ColorUtility.GetColor(colorhex);
            m.SetColor("_Border", color);
            Materials[key] = m;
            return m;
        }
    }
}
