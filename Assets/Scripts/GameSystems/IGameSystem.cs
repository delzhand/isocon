using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IGameSystem
{
    string SystemName();
    void Setup();
    void Teardown();
    string GetTokenParams(string imageHash);
    void InitializeToken(GameObject g, string json);
    string GetTokenImageHash(GameObject g);
}
