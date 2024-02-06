using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenLibrary
{
    public static TokenMeta[] Tokens;

    public static void Setup()
    {
        List<TokenMeta> list = new();
        list.Add(new TokenMeta
        {
            FPS = 0,
            Frames = 1,
            Hash = "f3602264f74d1439d3aafa14a91f74313c7a68b621d5b5feacb461393d4a9c69",
            Name = "Ada",
        });
        list.Add(new TokenMeta
        {
            FPS = 24,
            Frames = 4,
            Hash = "a0165e74822430c06b1c9b9db089e54fa582736e158c654809f83cb73f073be4",
            Name = "Ada Animated",
        });
        list.Add(new TokenMeta
        {
            FPS = 0,
            Frames = 1,
            Hash = "ef9b4c06643152b1feb267f8604666ebe180afb2cdb98a90833365476ad46849",
            Name = "Graddes"
        });
        list.Add(new TokenMeta
        {
            FPS = 0,
            Frames = 1,
            Hash = "80fb9ad785eabb079829a75b6d0f7b648da3f9a5553028da56d282a6e6b5e155",
            Name = "Sae"
        });
        list.Add(new TokenMeta
        {
            FPS = 0,
            Frames = 1,
            Hash = "037305e03e1b75ad7a644422649b5f5456fcaed1f6c7cc66a70febb6d1cd97f5",
            Name = "Red Worm"
        });
        list.Add(new TokenMeta
        {
            FPS = 0,
            Frames = 1,
            Hash = "312a77bf1aae81e42b3a6a6f87b7e7d2cd712bcd0339014486d34c52a53b14d7",
            Name = "Horned Rooter"
        });
        list.Add(new TokenMeta
        {
            FPS = 0,
            Frames = 1,
            Hash = "8b3627c36b26c32aaa2997d2ed422253e9f0e19f896978cd22514514545f3830",
            Name = "Gunwight"
        });
        Tokens = list.ToArray();
    }
}
