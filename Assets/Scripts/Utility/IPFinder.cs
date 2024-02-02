using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class IPFinder
{
    private static string _publicIP;

    [Serializable]
    private class IpifyResponse
    {
        public string ip;
    }

    private static readonly string IpifyApiUrl = "https://api.ipify.org/?format=json";

    private static async Task AsyncGetIP() {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(IpifyApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to get the IP address
                    IpifyResponse ipifyResponse = JsonUtility.FromJson<IpifyResponse>(json);
                    // Debug.Log(ipifyResponse);
                    _publicIP = ipifyResponse.ip;
                    // Debug.Log(_publicIP);
                }
                else
                {
                    // Handle the error or log it
                    Debug.Log($"Error: {response.StatusCode}");
                    _publicIP = "[unknown]";
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Debug.Log($"Exception: {ex.Message}");
            _publicIP = "[unknown]";
        }
    }
    
    private static string LocalIP() {
        #if UNITY_WEBGL
            return "";
        #endif

        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(
            f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString();
    }

    public static async void ReplaceTokens(Label label) {
        label.text = label.text.Replace("<LocalIP>", LocalIP());
        string original = label.text;
        label.text = original.Replace("<GlobalIP>", "(checking...)");
        await AsyncGetIP();
        label.text = original.Replace("<GlobalIP>", _publicIP);
    }
}
