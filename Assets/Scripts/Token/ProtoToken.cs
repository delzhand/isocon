using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ProtoToken : NetworkBehaviour
{
    [SyncVar]
    public string Name;

    [SyncVar]
    public string Job;

    [SyncVar]
    public string JClass;
}
