using UnityEngine;
using System.Collections;

public struct ControllerEventObject {
    public string side;
    public Vector3 position;
    public Quaternion rotation;
    public float pressure;
    public bool isTriggerClicked;
    public Vector2 touchPoint;
    public SteamVR_Controller.Device controller;
}

public delegate void ControllerEventHandler(ControllerEventObject evt);

public struct HMDEventObject {
    public Vector3 position;
    public Quaternion rotation;
}

public delegate void HMDEventHandler(HMDEventObject evt);

public class ViveEventManager : MonoBehaviour {

    public bool DebugMode;

    private static ViveEventManager instance;
    public  static ViveEventManager Instance {
        get {
            if(instance == null) instance = FindObjectOfType(typeof(ViveEventManager)) as ViveEventManager;
            if(instance == null) instance = new GameObject("Event Manager").AddComponent<ViveEventManager>();
            return instance;
        }
    }

    private SteamVR_Camera hmd;
    private SteamVR_Controller.Device leftController;
    private SteamVR_TrackedObject leftControllerTracker;
    private SteamVR_Controller.Device rightController;
    private SteamVR_TrackedObject rightControllerTracker;
    
    public event ControllerEventHandler OnTriggerPressStart;
    bool hasLeftTriggerPressStarted;
    bool hasLeftTriggerBeenClicked;
    bool hasRightTriggerPressStarted;
    bool hasRightTriggerBeenClicked;
    public event ControllerEventHandler OnTriggerPress;
    public event ControllerEventHandler OnTriggerPressEnd;
    public event ControllerEventHandler OnTriggerClick;

    float clickTimeThreshold;
    float swipeTimeThreshold;
    float swipeDistanceThreshold;
    
    public event ControllerEventHandler OnGripPressStart;
    float leftGripPressStartTime;
    float rightGripPressStartTime;
    public event ControllerEventHandler OnGripPress;
    public event ControllerEventHandler OnGripPressEnd;
    public event ControllerEventHandler OnGripClick;
    
    public event ControllerEventHandler OnTouchStart;
    float leftTouchStartTime;
    float rightTouchStartTime;
    public event ControllerEventHandler OnTouchMove;
    public event ControllerEventHandler OnTouchEnd;
    public event ControllerEventHandler OnTouchTap;
    public event ControllerEventHandler OnTouchSwipe;
    public event ControllerEventHandler OnTouchpadPressStart;
    float leftTouchpadPressStartTime;
    float rightTouchpadPressStartTime;
    public event ControllerEventHandler OnTouchpadPress;
    public event ControllerEventHandler OnTouchpadPressEnd;
    public event ControllerEventHandler OnTouchpadClick;

    void Awake() {
        hmd = FindObjectOfType(typeof(SteamVR_Camera)) as SteamVR_Camera;

        hasLeftTriggerPressStarted  = false;
        hasLeftTriggerBeenClicked   = false;
        hasRightTriggerPressStarted = false;
        hasRightTriggerBeenClicked  = false;

        clickTimeThreshold = 0.3f;
        swipeTimeThreshold = 0.5f;
        swipeDistanceThreshold = 0.5f;
    }

    void FixedUpdate() {
        SteamVR_TrackedObject[] objects = FindObjectsOfType<SteamVR_TrackedObject>();

        foreach(SteamVR_TrackedObject obj in objects) {
            if(obj.name.Contains("left")) {
                leftController = SteamVR_Controller.Input((int)obj.index);
                leftControllerTracker = obj;
            } else if(obj.name.Contains("right")) {
                rightController = SteamVR_Controller.Input((int)obj.index);
                rightControllerTracker = obj;
            }
        }

        if(leftController != null) {
            ControllerEventObject eventObj = new ControllerEventObject();
            eventObj.side = "left";
            eventObj.position = leftControllerTracker.transform.position;
            eventObj.rotation = leftControllerTracker.transform.rotation;
            eventObj.pressure = leftController.GetState().rAxis1.x;
            eventObj.isTriggerClicked = hasLeftTriggerBeenClicked;
            eventObj.touchPoint = Vector2.zero;
            eventObj.controller = leftController;

            /* Trigger Events */
            if(leftController.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)) {
                if(OnTriggerPressStart != null) {
                    OnTriggerPressStart(eventObj);
                }
                hasLeftTriggerPressStarted = true;
                if(DebugMode) {
                    Debug.Log("Left Trigger Press Start");
                }
            }

            if(leftController.GetTouch(SteamVR_Controller.ButtonMask.Trigger)) {
                if(!hasLeftTriggerPressStarted) {
                    if(OnTriggerPressStart != null) {
                        OnTriggerPressStart(eventObj);
                    }
                    hasLeftTriggerPressStarted = true;
                    if(DebugMode) {
                        Debug.Log("Left Trigger Press Start");
                    }
                } else if(!hasLeftTriggerBeenClicked && leftController.GetState().rAxis1.x == 1.0f) {
                    if(OnTriggerClick != null) {
                        OnTriggerClick(eventObj);
                    }
                    hasLeftTriggerBeenClicked = true;
                    if(DebugMode) {
                        Debug.Log("Left Trigger Click");
                    }
                } else {
                    if(OnTriggerPress != null) {
                        OnTriggerPress(eventObj);
                    }
                    if(leftController.GetState().rAxis1.x < 1.0f) hasLeftTriggerBeenClicked = false;
                    if(DebugMode) {
                        Debug.Log("Left Trigger Press");
                    }
                }
            }

            if(leftController.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger)) {
                if(OnTriggerPressEnd != null) {
                    OnTriggerPressEnd(eventObj);
                }
                hasLeftTriggerPressStarted = false;
                if(DebugMode) {
                    Debug.Log("Left Trigger Press End");
                }
            }

            /* Grip Events */
            if(leftController.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
                if(OnGripPressStart != null) {
                    OnGripPressStart(eventObj);
                }
                leftGripPressStartTime = Time.time;
                if(DebugMode) {
                    Debug.Log("Left Grip Press Start");
                }
            }

            if(leftController.GetPress(SteamVR_Controller.ButtonMask.Grip)) {
                if(OnGripPress != null) {
                    OnGripPress(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Left Grip Press");
                }
            }

            if(leftController.GetPressUp(SteamVR_Controller.ButtonMask.Grip)) {
                if(OnGripPressEnd != null) {
                    OnGripPressEnd(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Left Grip Press End");
                }
                if(Time.time - leftGripPressStartTime < clickTimeThreshold) {
                    if(OnGripClick != null) {
                        OnGripClick(eventObj);
                    }
                    if(DebugMode) {
                        Debug.Log("Left Grip Click");
                    }
                }
            }

            /* Touchpad Events */
            if(leftController.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad)) {
                eventObj.touchPoint = new Vector2(leftController.GetState().rAxis0.x, leftController.GetState().rAxis0.y);
                if(OnTouchStart != null) {
                    OnTouchStart(eventObj);
                }
                leftTouchStartTime = Time.time;
                if(DebugMode) {
                    Debug.Log("Left Touch Start");
                }
            }

            if(leftController.GetTouch(SteamVR_Controller.ButtonMask.Touchpad)) {
                eventObj.touchPoint = new Vector2(leftController.GetState().rAxis0.x, leftController.GetState().rAxis0.y);
                if(OnTouchMove != null) {
                    OnTouchMove(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Left Touch Move");
                }
            }

            if(leftController.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad)) {
                eventObj.touchPoint = new Vector2(leftController.GetState().rAxis0.x, leftController.GetState().rAxis0.y);
                if(OnTouchEnd != null) {
                    OnTouchEnd(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Left Touch End");
                }
                if(Time.time - leftTouchStartTime < clickTimeThreshold) {
                    if(OnTouchTap != null) {
                        OnTouchTap(eventObj);
                    }
                    if(DebugMode) {
                        Debug.Log("Left Touch Tap");
                    }
                }
            }

            if(leftController.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)) {
                if(OnTouchpadPressStart != null) {
                    OnTouchpadPressStart(eventObj);
                }
                leftTouchpadPressStartTime = Time.time;
                if(DebugMode) {
                    Debug.Log("Left Touchpad Press Start");
                }
            }

            if(leftController.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
                if(OnTouchpadPress != null) {
                    OnTouchpadPress(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Left Touchpad Press");
                }
            }

            if(leftController.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad)) {
                if(OnTouchpadPressEnd != null) {
                    OnTouchpadPressEnd(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Left Touchpad Press End");
                }
                if(Time.time - leftTouchpadPressStartTime < clickTimeThreshold) {
                    if(OnTouchpadClick != null) {
                        OnTouchpadClick(eventObj);
                    }
                    if(DebugMode) {
                        Debug.Log("Left Touchpad Click");
                    }
                }
            }
        }

        if(rightController != null) {
            ControllerEventObject eventObj = new ControllerEventObject();
            eventObj.side = "right";
            eventObj.position = rightControllerTracker.transform.position;
            eventObj.rotation = rightControllerTracker.transform.rotation;
            eventObj.pressure = rightController.GetState().rAxis1.x;
            eventObj.isTriggerClicked = hasRightTriggerBeenClicked;
            eventObj.touchPoint = Vector2.zero;
            eventObj.controller = rightController;

            /* Trigger Events */
            if(rightController.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)) {
                if(OnTriggerPressStart != null) {
                    OnTriggerPressStart(eventObj);
                }
                hasRightTriggerPressStarted = true;
                if(DebugMode) {
                    Debug.Log("Right Trigger Press Start");
                }
            }

            if(rightController.GetTouch(SteamVR_Controller.ButtonMask.Trigger)) {
                if(!hasRightTriggerPressStarted) {
                    if(OnTriggerPressStart != null) {
                        OnTriggerPressStart(eventObj);
                    }
                    hasRightTriggerPressStarted = true;
                    if(DebugMode) {
                        Debug.Log("Right Trigger Press Start");
                    }
                } else if(!hasRightTriggerBeenClicked && rightController.GetState().rAxis1.x == 1.0f) {
                    if(OnTriggerClick != null) {
                        OnTriggerClick(eventObj);
                    }
                    hasRightTriggerBeenClicked = true;
                    if(DebugMode) {
                        Debug.Log("Right Trigger Click");
                    }
                } else {
                    if(OnTriggerPress != null) {
                        OnTriggerPress(eventObj);
                    }
                    if(rightController.GetState().rAxis1.x < 1.0f) hasRightTriggerBeenClicked = false;
                    if(DebugMode) {
                        Debug.Log("Right Trigger Press");
                    }
                }
            }

            if(rightController.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger)) {
                if(OnTriggerPressEnd != null) {
                    OnTriggerPressEnd(eventObj);
                }
                hasRightTriggerPressStarted = false;
                if(DebugMode) {
                    Debug.Log("Right Trigger Press End");
                }
            }

            /* Grip Events */
            if(rightController.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
                if(OnGripPressStart != null) {
                    OnGripPressStart(eventObj);
                }
                rightGripPressStartTime = Time.time;
                if(DebugMode) {
                    Debug.Log("Right Grip Press Start");
                }
            }

            if(rightController.GetPress(SteamVR_Controller.ButtonMask.Grip)) {
                if(OnGripPress != null) {
                    OnGripPress(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Right Grip Press");
                }
            }

            if(rightController.GetPressUp(SteamVR_Controller.ButtonMask.Grip)) {
                if(OnGripPressEnd != null) {
                    OnGripPressEnd(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Right Grip Press End");
                }
                if(Time.time - rightGripPressStartTime < clickTimeThreshold) {
                    if(OnGripClick != null) {
                        OnGripClick(eventObj);
                    }
                    if(DebugMode) {
                        Debug.Log("Right Grip Click");
                    }
                }
            }

            /* Touchpad Events */
            if(rightController.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad)) {
                eventObj.touchPoint = new Vector2(rightController.GetState().rAxis0.x, rightController.GetState().rAxis0.y);
                if(OnTouchStart != null) {
                    OnTouchStart(eventObj);
                }
                rightTouchStartTime = Time.time;
                if(DebugMode) {
                    Debug.Log("Right Touch Start");
                }
            }

            if(rightController.GetTouch(SteamVR_Controller.ButtonMask.Touchpad)) {
                eventObj.touchPoint = new Vector2(rightController.GetState().rAxis0.x, rightController.GetState().rAxis0.y);
                if(OnTouchMove != null) {
                    OnTouchMove(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Right Touch Move");
                }
            }

            if(rightController.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad)) {
                eventObj.touchPoint = new Vector2(rightController.GetState().rAxis0.x, rightController.GetState().rAxis0.y);
                if(OnTouchEnd != null) {
                    OnTouchEnd(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Right Touch End");
                }
                if(Time.time - rightTouchStartTime < clickTimeThreshold) {
                    if(OnTouchTap != null) {
                        OnTouchTap(eventObj);
                    }
                    if(DebugMode) {
                        Debug.Log("Right Touch Tap");
                    }
                }
            }

            if(rightController.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)) {
                if(OnTouchpadPressStart != null) {
                    OnTouchpadPressStart(eventObj);
                }
                rightTouchpadPressStartTime = Time.time;
                if(DebugMode) {
                    Debug.Log("Right Touchpad Press Start");
                }
            }

            if(rightController.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
                if(OnTouchpadPress != null) {
                    OnTouchpadPress(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Right Touchpad Press");
                }
            }

            if(rightController.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad)) {
                if(OnTouchpadPressEnd != null) {
                    OnTouchpadPressEnd(eventObj);
                }
                if(DebugMode) {
                    Debug.Log("Right Touchpad Press End");
                }
                if(Time.time - rightTouchpadPressStartTime < clickTimeThreshold) {
                    if(OnTouchpadClick != null) {
                        OnTouchpadClick(eventObj);
                    }
                    if(DebugMode) {
                        Debug.Log("Right Touchpad Click");
                    }
                }
            }
        }
    }
}