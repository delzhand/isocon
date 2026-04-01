using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class StartSessionModal
{
    public static void Open(ConnectMode mode)
    {
        Modal2.SetCurrentDialog("ShunDialog1");

        Modal2.AddDialogHeader($"Configure {mode.ToString()} Mode");

        if (mode == ConnectMode.Host)
        {
            Modal2.AddAlert("VPN Information", "VPNs interfere with host connections and must be disabled for clients to connect successfully.", ShunUI.AlertVariant.Attention);
        }

        Modal2.AddInlineTextField("PlayerName", "Player Name", Preferences.Current.PlayerName, "How you appear to other players");

        if (mode == ConnectMode.Solo || mode == ConnectMode.Host)
        {
            Modal2.AddInlineSelectField("GridType", "Grid Type", Preferences.Current.Grid, new List<string> { "Square", "Hex" }, "Choose 4 or 6 sided map tiles");
        }
        if (mode == ConnectMode.Host)
        {
            Modal2.AddInlineIntField("PlayerCount", "Max Players", 4);
        }
        if (mode == ConnectMode.Client)
        {
            Modal2.AddInlineTextField("HostIP", "Host IP", Preferences.Current.HostIP, "The IP address of the hosting player");
        }

        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm(mode == ConnectMode.Client ? "Join Session" : "Start Session", () =>
        {
            StartSession(mode);
        });

        Modal2.Open();
    }

    private static void StartSession(ConnectMode mode)
    {
        Modal2.SetValueOrigin("ShunDialog1");
        string playerName = Modal2.GetTextFieldValue("PlayerName");
        Preferences.Current.PlayerName = playerName;

        if (mode == ConnectMode.Solo || mode == ConnectMode.Host)
        {
            string gridType = Modal2.GetSelectFieldValue("GridType");
            Preferences.Current.Grid = gridType;
            TerrainController.GridType = gridType;
        }

        if (mode == ConnectMode.Client)
        {
            Preferences.Current.HostIP = Modal2.GetTextFieldValue("HostIP");
        }

        NetworkManager netManager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();
        switch (mode)
        {
            case ConnectMode.Solo:
                netManager.maxConnections = 1;
                netManager.StartHost();
                break;
            case ConnectMode.Host:
                int playerCount = Modal2.GetIntFieldValue("PlayerCount");
                Preferences.Current.PlayerCount = playerCount;
                netManager.maxConnections = playerCount;
                netManager.StartHost();
                break;
            case ConnectMode.Client:
                netManager.networkAddress = Preferences.Current.HostIP;
                netManager.StartClient();
                break;
        }
        LauncherState.ListenForConnection();
        UI.ToggleDisplay("StartupOptions", false);
        UI.ToggleDisplay("ConnectingMessage", true);
        Preferences.Save();
    }

}