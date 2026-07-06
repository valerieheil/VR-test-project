// ============================================================
//  GazeTracker.cs
//  Detects which objects the player is looking at using a
//  raycast from the camera (head direction).
//
//  HOW TO USE:
//    1. Attach this script to the OVRCameraRig's CenterEyeAnchor
//       (or your main VR Camera GameObject)
//    2. On every trigger object (bottles etc.) add:
//         - A Collider component (any shape)
//         - Tag the object as "VRTrigger" (or set the layer below)
//    3. That's it — gaze events are logged automatically
// ============================================================

using UnityEngine;

public class GazeTracker : MonoBehaviour
{
    // ── Inspector settings ───────────────────────────────────
    [Header("Raycast Settings")]
    [Tooltip("How far the gaze ray reaches (meters)")]
    public float gazeDistance = 10f;

    [Tooltip("Only detect objects on these layers. Set to 'Everything' to detect all.")]
    public LayerMask gazeLayer = Physics.DefaultRaycastLayers;

    [Header("Performance")]
    [Tooltip("How often to check gaze (seconds). 0 = every frame. Recommended: 0.05")]
    public float checkInterval = 0.05f;   // 20 checks per second — good balance

    [Header("Debug")]
    [Tooltip("Draw the gaze ray in the Scene view while playing")]
    public bool showDebugRay = true;

    [Header("Filter")]
    [Tooltip("Only treat objects with this tag as gaze targets")]
    public string gazeTag = "gazeTrack";

    // ── Internal state ───────────────────────────────────────
    private float  _timer         = 0f;
    private string _currentObject = "";   // Name of object currently being looked at
    private float  _gazeStartTime = 0f;   // When the current gaze started

    // ── Unity lifecycle ──────────────────────────────────────

    private void Update()
    {
        // Throttle: only run the check every `checkInterval` seconds
        _timer += Time.deltaTime;
        if (_timer < checkInterval) return;
        _timer = 0f;

        PerformGazeCheck();
    }

    // ── Core logic ───────────────────────────────────────────

    private void PerformGazeCheck()
    {
        // Cast a ray from the camera forward (= where player is looking)
        Ray ray = new Ray(transform.position, transform.forward);

        if (showDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * gazeDistance, Color.cyan, checkInterval);

        if (Physics.Raycast(ray, out RaycastHit hit, gazeDistance, gazeLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (!hitObject.CompareTag(gazeTag))
            {
                // Ignore objects that are not valid gaze targets.
                if (!string.IsNullOrEmpty(_currentObject))
                {
                    LogGazeExit(_currentObject);
                    _currentObject = "";
                }
                return;
            }

            string hitName = hitObject.name;

            // ── Player started looking at a NEW object ───────
            if (hitName != _currentObject)
            {
                // First: log exit for the previous object (if there was one)
                if (!string.IsNullOrEmpty(_currentObject))
                    LogGazeExit(_currentObject);

                // Then: log entry for the new object
                _currentObject = hitName;
                _gazeStartTime = VRLogger.Instance.SessionTime;
                LogGazeEnter(_currentObject);
            }
            // (If same object: do nothing — we log duration only on exit)
        }
        else
        {
            // ── Ray hit nothing — player looked away ─────────
            if (!string.IsNullOrEmpty(_currentObject))
            {
                LogGazeExit(_currentObject);
                _currentObject = "";
            }
        }
    }

    // ── Logging helpers ──────────────────────────────────────

    private void LogGazeEnter(string objectName)
    {
        VRLogger.Instance.Log(new LogEvent(
            eventType : "GazeEnter",
            objectID  : objectName,
            timestamp : VRLogger.Instance.SessionTime,
            position  : transform.position   // head position
        ));
    }

    private void LogGazeExit(string objectName)
    {
        float duration = VRLogger.Instance.SessionTime - _gazeStartTime;

        VRLogger.Instance.Log(new LogEvent(
            eventType : "GazeExit",
            objectID  : objectName,
            timestamp : VRLogger.Instance.SessionTime,
            position  : transform.position,
            duration  : duration,            // How long they looked at it
            extra     : $"gazeDuration={duration:F3}s"
        ));
    }

    // ── Cleanup ──────────────────────────────────────────────
    // If the scene ends while looking at something, log the exit
    private void OnDisable()
    {
        if (!string.IsNullOrEmpty(_currentObject))
            LogGazeExit(_currentObject);
    }
}
