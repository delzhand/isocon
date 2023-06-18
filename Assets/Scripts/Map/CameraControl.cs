using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public static Transform CameraTransform;

    private float lerpTimer;
    private float lerpDuration;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Quaternion reserveRotation;

    private Vector3 originPosition;
    private Vector3 targetPosition;

    private bool isLocked;
    private bool overhead;

    // Start is called before the first frame update
    void Start()
    {
        isLocked = false;
        overhead = false;
        CameraTransform = GameObject.Find("CameraOrigin").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocked) {
            if (lerpTimer <= lerpDuration) {
                lerpTimer += Time.deltaTime;
                float percent = Mathf.Clamp(lerpTimer / lerpDuration, 0, 1);
                CameraTransform.rotation = Quaternion.Lerp(originalRotation, targetRotation, percent);
                CameraTransform.position = Vector3.Lerp(originPosition, targetPosition, percent);
            }
            if (lerpTimer > lerpDuration) {
                isLocked = false;
            }
        }
    }

    public static void RotateLeft() {
        CameraControl.CameraTransform.GetComponent<CameraControl>().Rotate(90);
    }

    public static void RotateRight() {
        CameraControl.CameraTransform.GetComponent<CameraControl>().Rotate(-90);
    }

    public void Rotate(float value) {
        if (!isLocked && !overhead) {
            initializeTransition(.35f);
            targetRotation = originalRotation * Quaternion.Euler(0, value, 0);
        }
    }

    public static void GoToBlock(Block block) {
        CameraControl.CameraTransform.GetComponent<CameraControl>().Translate(block.transform.position);
    }

    public void Translate(Vector3 value) {
        if (!isLocked && !overhead) {
            initializeTransition(.35f);
            targetPosition = value;
        }
    }

    public static void Zoom(float value) {
        Camera.main.GetComponent<Camera>().orthographicSize = value;
    }

    public static void ToggleOverhead() {
        CameraControl.CameraTransform.GetComponent<CameraControl>().Overhead();
    }

    public void Overhead() {
        if (!isLocked) {
            if (!overhead) {
                overhead = true;
                reserveRotation = GameObject.Find("CameraOrigin").transform.rotation;
                initializeTransition(.35f);
                targetRotation = Quaternion.Euler(0, 0, 30);
            }
            else {
                overhead = false;
                initializeTransition(.35f);
                targetRotation = reserveRotation;
            }
        }
    }

    private void initializeTransition(float duration) {
        targetPosition = GameObject.Find("CameraOrigin").transform.position;
        originPosition = targetPosition;
        targetRotation = GameObject.Find("CameraOrigin").transform.rotation;
        originalRotation = targetRotation;
        lerpTimer = 0;
        lerpDuration = duration;
        isLocked = true;
    }

}
