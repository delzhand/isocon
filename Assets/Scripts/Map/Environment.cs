using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Palette {
    GREENFIELD,
    BLUECAVE,
}

public enum Background {
    SUNSHINE,
    NIGHTTIME,
    SEASIDE,
}

public class Environment : MonoBehaviour
{

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ChangeBackground(Background background) {
        Color c1 = Color.blue;
        Color c2 = Color.blue;
        switch (background) {
            case Background.NIGHTTIME:
                c1 = Color.black;
                c2 = new Color(0, 38/255f, 60/255f);
                break;
            case Background.SUNSHINE:
                c1 = new Color(177/255f, 214/255f, 128/255f);
                c2 = new Color(38/255f, 113/255f, 156/255f);
                break;
            case Background.SEASIDE:
                c1 = new Color(38/255f, 113/255f, 156/255f);
                c2 = new Color(234/255f, 215/255f, 129/255f);
                break;
        }
        MeshRenderer mr = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
        mr.material.SetColor("_Color1", c1);
        mr.material.SetColor("_Color2", c2);
    }

}
