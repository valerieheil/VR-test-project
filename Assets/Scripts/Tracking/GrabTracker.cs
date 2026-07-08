// ============================================================
//  GrabTracker.cs
//  Logs when the player grabs or releases a specific object.
//  This script goes ON the grabbable object (e.g. the bottle),
//  NOT on the player.
//
//  HOW TO USE:
//    1. Select a grabbable object in your scene (e.g. "AlcoholicBottle")
//    2. Make sure it has:
//         - A Grabbable component (from the Oculus Interaction SDK)
//         - A Rigidbody component
//         - A Collider component
//    3. Attach THIS script to the same object
//    4. The objectLabel will auto-fill with the GameObject name,
//       or you can set a custom name in the Inspector
// ============================================================
using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Grabbable))]
public class GrabTracker : MonoBehaviour
{
    [Header("Object Identity")]
    [Tooltip("Name used in the log. Defaults to the GameObject name if left empty.")]
    public string objectLabel = "";

    [Tooltip("Is this object considered a therapy trigger?")]
    public bool isTriggerObject = true;

    private Grabbable _grabbable;

    private float _grabStartTime;
    private int _grabCount;

    private Transform _cameraTransform;

    private void Awake()
    {
        _grabbable = GetComponent<Grabbable>();

        if (string.IsNullOrEmpty(objectLabel))
            objectLabel = gameObject.name;

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        _grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    private void OnDisable()
    {
        _grabbable.WhenPointerEventRaised -= HandlePointerEvent;
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:

                _grabCount++;
                _grabStartTime = VRLogger.Instance.SessionTime;
                LogGrab();

                break;

            case PointerEventType.Unselect:

                LogRelease();

                break;
        }
    }

    private void LogGrab()
    {
        Vector3 headPos =
            _cameraTransform != null ?
            _cameraTransform.position :
            Vector3.zero;

        VRLogger.Instance.Log(new LogEvent(
            eventType: "Grab",
            objectID: objectLabel,
            timestamp: VRLogger.Instance.SessionTime,
            position: headPos,
            extra: $"grabCount={_grabCount},isTrigger={isTriggerObject}"
        ));

        Debug.Log($"Grabbed {objectLabel}");
    }

    private void LogRelease()
    {
        float holdDuration =
            VRLogger.Instance.SessionTime - _grabStartTime;

        Vector3 headPos =
            _cameraTransform != null ?
            _cameraTransform.position :
            Vector3.zero;

        VRLogger.Instance.Log(new LogEvent(
            eventType: "Release",
            objectID: objectLabel,
            timestamp: VRLogger.Instance.SessionTime,
            position: headPos,
            duration: holdDuration,
            extra: $"holdDuration={holdDuration:F3}s,isTrigger={isTriggerObject}"
        ));

        Debug.Log($"Released {objectLabel}");
    }

    public int GrabCount => _grabCount;
}