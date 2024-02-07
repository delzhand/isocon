using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenLibrary : MonoBehaviour
{
    public static TokenMeta[] Tokens;
    public static List<(TokenMeta, VisualElement)> ElementMap;

    private static TokenLibrary Find()
    {
        return GameObject.Find("AppState").GetComponent<TokenLibrary>();
    }

    public static void Setup()
    {
        ElementMap = new();
        Find().DebugSetup();
    }

    private void DebugSetup()
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
            FPS = 2,
            Frames = 4,
            Hash = "d8df59ff15f1a68794ae2549d2305d1cbda518be8b0d1d9e0c7ff999303597ae",
            Name = "Ada Animated",
        });
        list.Add(new TokenMeta
        {
            FPS = 12,
            Frames = 13,
            Hash = "6914b4c0d81213c8cae0ddbcf62a0e334e83c9de0e70ecf607a691e56415ce78",
            Name = "Hamon Bladeseeker",
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

        foreach (var tokenMeta in Tokens)
        {
            AddToUI(tokenMeta);
        }

    }

    void Update()
    {
        foreach ((TokenMeta, VisualElement) item in ElementMap)
        {
            var meta = item.Item1;
            var element = item.Item2;
            int currentFrameIndex = Mathf.FloorToInt(Time.time * meta.FPS) % meta.Frames;
            int offset = Mathf.RoundToInt(-100 * currentFrameIndex);
            element.Q("Sprite").style.left = Length.Percent(offset);
        }
    }

    private void AddToUI(TokenMeta meta)
    {
        int rawSize = 200;

        Texture2D graphic = TextureSender.LoadImageFromFile(meta.Hash, true);
        float aspectRatio = graphic.width / meta.Frames / (float)graphic.height;

        VisualElement tokenDisplay = new();
        tokenDisplay.AddToClassList("item");
        tokenDisplay.style.height = rawSize;
        tokenDisplay.style.width = rawSize;

        VisualElement frame = new();
        frame.AddToClassList("frame");
        int width = rawSize;
        int height = rawSize;
        if (aspectRatio >= 1)
        {
            height = Mathf.RoundToInt(rawSize / aspectRatio);
        }
        else
        {
            width = Mathf.RoundToInt(rawSize * aspectRatio);
        }
        frame.style.width = width;
        frame.style.height = height;
        Debug.Log($"{meta.Name} w:{graphic.width / meta.Frames} h:{graphic.height} r:{aspectRatio} w2:{width} h2:{height}");

        Label label = new();
        label.AddToClassList("panel-text");
        label.text = meta.Name;
        label.style.backgroundColor = new Color(0, 0, 0, .5f);

        VisualElement sprite = new();
        sprite.name = "Sprite";
        sprite.AddToClassList("sprite");
        sprite.style.width = width * meta.Frames;
        sprite.style.backgroundImage = graphic;

        frame.Add(sprite);
        tokenDisplay.Add(frame);
        tokenDisplay.Add(label);
        UI.System.Q("TokenLibrary").Q("LibraryGrid").Add(tokenDisplay);

        ElementMap.Add((meta, tokenDisplay));
    }
}
