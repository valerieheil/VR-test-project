// ============================================================
//  MovementTracker.cs
//  Logs the player's head and hand positions at regular intervals.
//  Also calculates movement speed.
//
//  HOW TO USE:
//    1. Create an empty GameObject in your scene, name it "MovementTracker"
//    2. Attach this script to it
//    3. In the Inspector, drag in:
//         - cameraTransform  → OVRCameraRig > TrackingSpace > CenterEyeAnchor
//         - leftHandTransform  → OVRCameraRig > TrackingSpace > LeftHandAnchor
//         - rightHandTransform → OVRCameraRig > TrackingSpace > RightHandAnchor
//    4. Movement is logged automatically every `logInterval` seconds
// ============================================================

using UnityEngine;

public class MovementTracker : MonoBehaviour
{
    // ── Inspector settings ───────────────────────────────────
    [Header("Transforms to Track")]
    [Tooltip("Drag in: OVRCameraRig > TrackingSpace > CenterEyeAnchor")]
    public Transform cameraTransform;

    [Tooltip("Drag in: OVRCameraRig > TrackingSpace > LeftHandAnchor")]
    public Transform leftHandTransform;

    [Tooltip("Drag in: OVRCameraRig > TrackingSpace > RightHandAnchor")]
    public Transform rightHandTransform;

    [Header("Performance")]
    [Tooltip("How often to log position (seconds). Recommended: 0.1 (10 times/sec)")]
    public float logInterval = 0.1f;

    [Header("Speed Threshold")]
    [Tooltip("Only log if the player moved more than this distance since last log (meters). " +
             "Set to 0 to always log.")]
    public float minimumMoveDelta = 0.01f;  // Avoids logging when completely still

    // ── Internal state ───────────────────────────────────────
    private float   _timer = 0f;
    private Vector3 _lastHeadPosition;
    private bool    _initialized = false;

    // Cumulative distance walked during session
    private float _totalDistance = 0f;

    // ── Unity lifecycle ──────────────────────────────────────

    private void Start()
    {
        // Safety check — warn if transforms aren't assigned
        if (cameraTransform == null)
            Debug.LogWarning("[MovementTracker] cameraTransform is not assigned! " +
                             "Drag CenterEyeAnchor into the Inspector.");
    }

    private void Update()
    {
    // Grip-Input erkannt?
        Debug.Log("grip input: "+ OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger));


        if (cameraTransform == null) return;

        // Initialize last position on first frame
        if (!_initialized)
        {
            _lastHeadPosition = cameraTransform.position;
            _initialized = true;
        }

        _timer += Time.deltaTime;
        if (_timer < logInterval) return;
        _timer = 0f;

        LogMovement();
    }

    // ── Core logic ───────────────────────────────────────────

    private void LogMovement()
    {
        Vector3 currentHead = cameraTransform.position;

        // Calculate how far the head moved since last log
        float delta = Vector3.Distance(currentHead, _lastHeadPosition);

        // Skip if barely moved (player standing still)
        if (delta < minimumMoveDelta) return;

        // Accumulate total distance
        _totalDistance += delta;

        // Speed = distance / time
        float speed = delta / logInterval;

        // ── Log head position ────────────────────────────────
        VRLogger.Instance.Log(new LogEvent(
             eventType : "HeadMove",
            objectID  : "",                          // Not object-specific
            timestamp : VRLogger.Instance.SessionTime,
            position  : currentHead,
            duration  : logInterval,                 // The interval this covers
            extra     : $"speed={speed:F3}m/s,totalDist={_totalDistance:F3}m"
        ));

        // ── Log left hand position ───────────────────────────
        if (leftHandTransform != null)
        {
            VRLogger.Instance.Log(new LogEvent(
                eventType : "LeftHandPos",
                objectID  : "",
                timestamp : VRLogger.Instance.SessionTime,
                position  : leftHandTransform.position
            ));
        }

        // ── Log right hand position ──────────────────────────
        if (rightHandTransform != null)
        {
            VRLogger.Instance.Log(new LogEvent(
                eventType : "RightHandPos",
                objectID  : "",
                timestamp : VRLogger.Instance.SessionTime,
                position  : rightHandTransform.position
            ));
        }

        _lastHeadPosition = currentHead;
    }

    // ── Public utility ───────────────────────────────────────

    /// <summary>Total meters the player's head has moved this session.</summary>
    public float TotalDistance => _totalDistance;
}
