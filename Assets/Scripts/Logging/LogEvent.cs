// ============================================================
//  LogEvent.cs
//  The data model for a single logged event.
//  Every tracker (Gaze, Movement, Grab) creates one of these
//  and sends it to VRLogger.
// ============================================================

using UnityEngine;

[System.Serializable]          // Makes it visible in Inspector & serializable to JSON
public class LogEvent
{
    // ── What kind of event this is ───────────────────────────
    // Examples: "GazeEnter", "GazeExit", "Move", "Grab", "Release"
    public string eventType;

    // ── Which object was involved ────────────────────────────
    // The name of the VR object (e.g. "AlcoholicBottle", "GlassOfWine")
    // Leave empty "" for movement events that aren't object-specific
    public string objectID;

    // ── When did it happen ───────────────────────────────────
    // Seconds since the session started (not wall-clock time)
    public float timestamp;

    // ── Where was the player ─────────────────────────────────
    // Head (camera) position in the VR world at the moment of the event
    public float posX;
    public float posY;
    public float posZ;

    // ── How long did it last (optional) ─────────────────────
    // Used for: gaze duration, grab duration
    // Leave at 0 for instant events (like "Grab started")
    public float duration;

    // ── Extra detail (optional) ──────────────────────────────
    // Free-text field for anything extra, e.g. "speed=1.23"
    public string extra;

    // ────────────────────────────────────────────────────────
    //  Constructor — the easy way to create a LogEvent
    // ────────────────────────────────────────────────────────
    public LogEvent(string eventType, string objectID, float timestamp,
                    Vector3 position, float duration = 0f, string extra = "")
    {
        this.eventType = eventType;
        this.objectID  = objectID;
        this.timestamp = timestamp;
        this.posX      = position.x;
        this.posY      = position.y;
        this.posZ      = position.z;
        this.duration  = duration;
        this.extra     = extra;
    }

    // ────────────────────────────────────────────────────────
    //  CSV row — one line for this event
    //  Column order matches VRLogger.CSV_HEADER
    // ────────────────────────────────────────────────────────
    public string ToCSVRow()
    {
        return $"{eventType};{objectID};{timestamp:F3};" +
               $"{posX:F3};{posY:F3};{posZ:F3};" +
               $"{duration:F3};{extra}";
    }
}
