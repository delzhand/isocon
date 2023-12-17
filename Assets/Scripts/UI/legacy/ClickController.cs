using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class ClickController : MonoBehaviour
// {
//     public static Block FocusedBlock = null;
//     public static Block SelectedBlock = null;

//     void Update()
//     {
//         if (UI.ClicksSuspended) {
//             return;
//         }

//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//         RaycastHit hit;
//         if (Physics.Raycast(ray, out hit, 100f)) {
//             if (hit.collider.gameObject) {
//                 FocusedBlock = hit.collider.GetComponent<Block>();
//                 if (FocusedBlock) {
//                     if (!FocusedBlock.Focused) {
//                         FocusedBlock.Focus();
//                     }
//                     if (Input.GetMouseButtonDown(1)) {
//                         FocusedBlock.HandleClicks(1);
//                     }
//                     if (Input.GetMouseButtonDown(0)) {
//                         FocusedBlock.HandleClicks(0);
//                     }
//                 }
//             }
//         }
//     }
// }
