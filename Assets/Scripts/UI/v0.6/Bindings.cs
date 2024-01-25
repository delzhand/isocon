using UnityEngine;

public class Bindings : MonoBehaviour
{
    void Awake()
    {
        Bind();
    }

    void Bind()
    {
        CameraControl.CreateBindings();

    }
}
