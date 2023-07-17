using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReserveController : MonoBehaviour
{
    public static int Size;
    private Vector3 anchorPos = new Vector3(0, 65, 4);

    // Start is called before the first frame update
    void Start()
    {
         Adjust();   
    }

    // Update is called once per frame
    void Update()
    {   
        anchorPos.x = Screen.width * .9f;
        anchorPos.y = Screen.height * .1f;
        Vector3 worldPos = GameObject.Find("ReserveCamera").GetComponent<Camera>().ScreenToWorldPoint(anchorPos);
        GameObject.Find("SpotRoot").transform.position = worldPos;
    }

    public static void Adjust() {
        // Delete empty spots
        GameObject[] spots = GameObject.FindGameObjectsWithTag("Reserve");
        for (int i = 0; i < spots.Length; i++) {
            ReserveSpot rs = spots[i].GetComponent<ReserveSpot>();
            if (rs.Token == null) {
                GameObject.DestroyImmediate(spots[i]);   
            }
        }

        // Ensure at least one empty spot at the end
        GameObject newSpot = Instantiate(Resources.Load("Prefabs/ReserveTile") as GameObject);
        newSpot.transform.parent = GameObject.Find("SpotRoot").transform;

        // Reposition spots
        spots = GameObject.FindGameObjectsWithTag("Reserve");
        for (int i = 0; i < spots.Length; i++) {
            spots[i].name = "Reserve " + (i+1);
            Vector3 position = new Vector3(i, .2f, -.5f * (i%2));
            spots[i].transform.localPosition = position;
            // Token t = spots[i].GetComponent<ReserveSpot>().Token;
            // if (t) {
            //     spots[i].GetComponent<ReserveSpot>().PlaceAtReserveSpot(t);
            // }
            spots[i].GetComponent<ReserveSpot>().SetTokenPosition();
        }
    }
}
