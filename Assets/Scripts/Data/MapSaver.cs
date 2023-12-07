using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class MapSaver
{
    public static void StegLoad(string fullPath) {
        Debug.Log(fullPath);
        byte[] imageData = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        string json = Decode(texture);
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        Toast.Add("Map loaded.");
    }

    public static void LegacyLoad(string fullPath) {
        string json = File.ReadAllText(fullPath);
        State state = JsonUtility.FromJson<State>(json);
        State.SetSceneFromState(state);
        Toast.Add("Map loaded.");
    }

    public static void StegSave(string fullPath) {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);

        fullPath = fullPath.Replace(".json", ".png");
        GameObject.Find("Screenshot Camera").GetComponent<Camera>().orthographicSize = Camera.main.orthographicSize;
        RenderTexture rt = Resources.Load<RenderTexture>("Textures/ScreenshotTexture");
        Block.UnfocusAll();
        Texture2D screenshot = toTexture2D(rt);
        Texture2D encoded = Encode(screenshot, json);
        byte[] bytes = encoded.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, bytes);
        Toast.Add($"Map written to {fullPath.Replace("\\", "/")}");
    }

    private static Texture2D toTexture2D(RenderTexture rt) {
        Texture2D tex = new Texture2D(640, 480, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        return tex;
    }

    private static Texture2D Encode(Texture2D image, string message){

        //Create a new texture to copy encoded pixels to
        Texture2D newTexture = new Texture2D(image.width,image.height);
        
        //This variable holds the total amount of bits in the message.
        int totalBits = 0;

        System.Text.Encoding encoding = System.Text.Encoding.ASCII;
        byte[] strBytes = encoding.GetBytes (message);
        
        
        BitArray strBits = new BitArray(strBytes);
        totalBits = strBits.Length;
        
        BitArray strLength = new BitArray(System.BitConverter.GetBytes(totalBits));
        
        //Create a new BitArray to hold the length of the message + the message itself.
        BitArray finalBits = new BitArray(strLength.Length + totalBits);
        int index = 0;
        for (int lb = 0;lb<strLength.Length;lb++){
            finalBits[lb] = strLength[lb];
            index++;
        }
        for (int sb = 0;sb<strBits.Length;sb++){
            finalBits[index] = strBits[sb];
            index++;
        }
        
        
        //Get the pixels for the image...
        Color[] imagePixels = image.GetPixels();
        for (int i=0;i<finalBits.Length;i++){
            if (finalBits[i] == true) {
                imagePixels[i].a = 1.0f;
            }
            else {
                imagePixels[i].a = 0.99f;
            }
        }

        newTexture.SetPixels(imagePixels);
        newTexture.Apply();
        return newTexture;
    }

    private static string Decode(Texture2D image) {
        
        //Get the pixels for the image...
        Color[] imagePixels = image.GetPixels();
        
        //Go Through the First 32 Pixels and create a 4 byte array. 
        //This array should give us the message's length.	
        BitArray newBits = new BitArray(32);
        for (int i=0;i<32;i++){
            if(imagePixels[i].a == 1){
                newBits[i] = true;
            }
            else {
                newBits[i] = false;
            }
        }
        
        int total = System.BitConverter.ToInt32(ToByteArray(newBits), 0);
        BitArray messageBits = new BitArray(total);
        for (int j=32;j<total + 32;j++){
            if(imagePixels[j].a == 1){
                messageBits[j-32] = true;
            }
            else {
                messageBits[j-32] = false;
            }
        }
        
        return System.Text.Encoding.ASCII.GetString(ToByteArray(messageBits));		
    }

    public static byte[] ToByteArray(BitArray bits){
        var bytes = new byte[bits.Length / 8];
        bits.CopyTo(bytes,0);
        return bytes;
    }

}
