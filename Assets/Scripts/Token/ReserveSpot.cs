using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReserveSpot : MonoBehaviour
{
    public Token Token;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {

        if (ModeController.ClickMode == ClickMode.Play) {
            if (Token.TokenHeld != null && !Token.TokenHeld.InReserve) {
                PlaceAtReserveSpot(Token.TokenHeld);
            }
            else if (Token.TokenHeld == null) {
                GameObject.Find("Engine").GetComponent<ModeController>().ActivateElementByName("AddTokenModal");
                ModeController.ClickMode = ClickMode.Other;
            }
        }
    }

    public void PlaceAtReserveSpot(Token token) {
        Token = token;
        Token.InReserve = true;
        Token.transform.Find("Base").gameObject.SetActive(false);
        SetTokenPosition();
        Token.ChangeHeld(null);
        ReserveController.Adjust();
    }

    public void SetTokenPosition() {
        if (Token) {
            Token.transform.position = this.transform.position + new Vector3(0, .2f, 0);
            if (Token.Size == 2) {
                Token.transform.position += new Vector3(.5f, 0, .5f);
            }
        }
    }

    public static ReserveSpot GetReserveSpot(Token token) {
        GameObject[] spots = GameObject.FindGameObjectsWithTag("Reserve");
        for (int i = 0; i < spots.Length; i++) {
            ReserveSpot rs = spots[i].GetComponent<ReserveSpot>();
            if (rs != null && rs.Token == token) {
                return rs;
            }
        }
        return null;
    }

    public static ReserveSpot LastReserveSpot() {
        GameObject[] spots = GameObject.FindGameObjectsWithTag("Reserve");
        for (int i = 0; i < spots.Length; i++) {
            ReserveSpot rs = spots[i].GetComponent<ReserveSpot>();
            if (rs != null && rs.Token == null) {
                return rs;
            }
        }
        return null;
    }
}
