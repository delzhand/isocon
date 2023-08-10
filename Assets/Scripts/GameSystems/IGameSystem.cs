using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IGameSystem
{
    string SystemName();
    void Setup();
    void Teardown();
}
