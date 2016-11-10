/*
 * ViveEventManager - A small utility class to easily add listeners to hmd/controllers events
 *
 * Author:
 *    Romain Cazier
 *    romaincazier.com
 *    @romaincazier
 *
 * Project page:
 *    http://github.com/romaincazier/ViveEventManager
 *
 */

using UnityEngine;
using System.Collections;

public struct ControllerEventObject {
    public string side;
    public Vector3 position;
    public Quaternion rotation;
    public float pressure;
    public bool isTriggerClicked;
    public Vector2 touchPoint;
    public string swipeDirection;
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
    private GameObject leftControllerObject;
    private LineRenderer leftRay;
    private SteamVR_Controller.Device rightController;
    private GameObject rightControllerObject;
    private LineRenderer rightRay;

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
    Vector2 leftTouchStartPoint;
    Vector2 rightTouchStartPoint;
    public event ControllerEventHandler OnTouchpadPressStart;
    float leftTouchpadPressStartTime;
    float rightTouchpadPressStartTime;
    public event ControllerEventHandler OnTouchpadPress;
    public event ControllerEventHandler OnTouchpadPressEnd;
    public event ControllerEventHandler OnTouchpadClick;

    void Awake() {
        hmd = FindObjectOfType(typeof(SteamVR_Camera)) as SteamVR_Camera;

        var controllerManager = FindObjectOfType(typeof(SteamVR_ControllerManager)) as SteamVR_ControllerManager;

        leftControllerObject  = controllerManager.left;

        leftRay = leftControllerObject.AddComponent<LineRenderer>();
        leftRay.material = new Material(Shader.Find("Particles/Additive"));
        leftRay.useWorldSpace = false;
        leftRay.SetWidth(0.001f, 0.001f);
        leftRay.SetPosition(1, Vector3.forward * 100.0f);

        rightControllerObject = controllerManager.right;

        rightRay = rightControllerObject.AddComponent<LineRenderer>();
        rightRay.material = new Material(Shader.Find("Particles/Additive"));
        rightRay.useWorldSpace = false;
        rightRay.SetWidth(0.001f, 0.001f);
        rightRay.SetPosition(1, Vector3.forward * 100.0f);

        hasLeftTriggerPressStarted  = false;
        hasLeftTriggerBeenClicked   = false;
        hasRightTriggerPressStarted = false;
        hasRightTriggerBeenClicked  = false;

        clickTimeThreshold = 0.3f;
        swipeTimeThreshold = 0.5f;
        swipeDistanceThreshold = 0.5f;
    }

    void FixedUpdate() {
        var leftIndex = leftControllerObject.GetComponent<SteamVR_TrackedObject>().index;
        if(leftIndex != SteamVR_TrackedObject.EIndex.None) {
            leftController = SteamVR_Controller.Input((int)leftIndex);
        } else {
            leftController = null;
        }

        var rightIndex = rightControllerObject.GetComponent<SteamVR_TrackedObject>().index;
        if(rightIndex != SteamVR_TrackedObject.EIndex.None) {
            rightController = SteamVR_Controller.Input((int)rightIndex);
        } else {
            rightController = null;
        }

        if(leftController != null) {
            ControllerEventObject eventObj = new ControllerEventObject();
            eventObj.side = "left";
            eventObj.position = leftControllerObject.transform.position;
            eventObj.rotation = leftControllerObject.transform.rotation;
            eventObj.pressure = leftController.GetState().rAxis1.x;
            eventObj.isTriggerClicked = hasLeftTriggerBeenClicked;
            eventObj.touchPoint = Vector2.zero;
            eventObj.swipeDirection = "none";
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
                leftTouchStartPoint = eventObj.touchPoint;
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
                if(Time.time - leftTouchStartTime < swipeTimeThreshold) {
                    Vector2 deltaPoint = eventObj.touchPoint - leftTouchStartPoint;
                    if(Mathf.Abs(deltaPoint.x) > Mathf.Abs(deltaPoint.y)) {
                        if(Mathf.Abs(deltaPoint.x) > swipeDistanceThreshold) {
                            eventObj.swipeDirection = deltaPoint.x > 0 ? "+X" : "-X";
                            if(OnTouchSwipe != null) {
                                OnTouchSwipe(eventObj);
                            }
                            if(DebugMode) {
                                Debug.Log("Left Swipe " + eventObj.swipeDirection);
                            }
                        }
                    } else {
                        if(Mathf.Abs(deltaPoint.y) > swipeDistanceThreshold) {
                            eventObj.swipeDirection = deltaPoint.y > 0 ? "+Y" : "-Y";
                            if(OnTouchSwipe != null) {
                                OnTouchSwipe(eventObj);
                            }
                            if(DebugMode) {
                                Debug.Log("Left Swipe " + eventObj.swipeDirection);
                            }
                        }
                    }
                }
                if(Time.time - leftTouchStartTime < clickTimeThreshold && eventObj.swipeDirection == "none") {
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

            if(DebugMode) {
                //(leftControllerTracker.transform.position, leftControllerTracker.transform.forward, Color.white);
            }
        }

        if(rightController != null) {
            ControllerEventObject eventObj = new ControllerEventObject();
            eventObj.side = "right";
            eventObj.position = rightControllerObject.transform.position;
            eventObj.rotation = rightControllerObject.transform.rotation;
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
                rightTouchStartPoint = eventObj.touchPoint;
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
                if(Time.time - rightTouchStartTime < swipeTimeThreshold) {
                    Vector2 deltaPoint = eventObj.touchPoint - rightTouchStartPoint;
                    if(Mathf.Abs(deltaPoint.x) > Mathf.Abs(deltaPoint.y)) {
                        if(Mathf.Abs(deltaPoint.x) > swipeDistanceThreshold) {
                            eventObj.swipeDirection = deltaPoint.x > 0 ? "+X" : "-X";
                            if(OnTouchSwipe != null) {
                                OnTouchSwipe(eventObj);
                                Debug.Log(OnTouchSwipe);
                            }
                            if(DebugMode) {
                                Debug.Log("Right Swipe " + eventObj.swipeDirection);
                            }
                        }
                    } else {
                        if(Mathf.Abs(deltaPoint.y) > swipeDistanceThreshold) {
                            eventObj.swipeDirection = deltaPoint.y > 0 ? "+Y" : "-Y";
                            if(OnTouchSwipe != null) {
                                OnTouchSwipe(eventObj);
                            }
                            if(DebugMode) {
                                Debug.Log("Right Swipe " + eventObj.swipeDirection);
                            }
                        }
                    }
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