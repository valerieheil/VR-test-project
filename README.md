# VR Exposure Therapy

## Projektbeschreibung

Dieses Projekt dient als Grundlage für eine Virtual-Reality-Expositionstherapie. Ziel ist es, Nutzern kontrolliert Reize in einer virtuellen Umgebung bereitzustellen und gleichzeitig deren Verhalten während der Exposition zu erfassen.

Als Expositionsobjekte werden Bierflaschen in einer virtuellen Szene platziert. Während sich der Nutzer in der VR-Umgebung bewegt, werden verschiedene Interaktions- und Bewegungsdaten aufgezeichnet, um das Verhalten während der Therapie analysieren zu können.

## Zielsetzung

Das Projekt soll eine Grundlage für wissenschaftliche Untersuchungen und therapeutische Anwendungen im Bereich der VR-gestützten Exposition bieten. Dabei stehen insbesondere folgende Fragestellungen im Fokus:

- Aufmerksamkeit gegenüber alkoholbezogenen Reizen
- Interaktionsverhalten mit den virtuellen Objekten
- Bewegungsmuster innerhalb der Szene
- Blickverhalten (Gaze Tracking)

## Funktionen

- Virtuelle Szene mit verteilten Bierflaschen
- Physikalisch interagierbare Objekte
- Greifen und Loslassen der Flaschen mittels VR-Controllern
- Tracking verschiedener Nutzerdaten:
  - Headset-Position und -Rotation
  - Controller-Positionen
  - Gaze Tracking
  - Interaktionen mit Objekten
  - Bewegungsdaten innerhalb der Szene
- Grundlage für spätere Datenauswertung

## Verwendete Technologien

- Unity
- Meta XR SDK
- OpenXR
- Oculus/Meta Interaction SDK
- C#

## Projektstruktur

```
Assets/
│
├── Interaction/
│   ├── Grabbable Objects
│   ├── Tracking
│   └── Scripts
│
├── Scenes/
│   └── ExposureScene
│
├── Prefabs/
│
└── Materials/
```

## Tracking

Während der Anwendung können unter anderem folgende Daten erfasst werden:

- Kopfposition
- Kopfrotation
- Blickrichtung (Gaze)
- Controllerpositionen
- Controllerrotationen
- Objektinteraktionen
- Bewegungswege
- Aufenthaltsdauer an bestimmten Positionen
- Zeitpunkt einzelner Interaktionen

Diese Daten können zur späteren Analyse des Nutzerverhaltens verwendet werden.

## Anwendung

1. Projekt in Unity öffnen.
2. Sicherstellen, dass das Meta XR SDK installiert ist.
3. Quest per Link oder Build auf dem Headset starten.
4. Szene `ExposureScene` laden.
5. Anwendung starten.

## Geplante Erweiterungen

- Speicherung der Trackingdaten als CSV oder JSON
- Heatmaps der Blick- und Bewegungsdaten
- Verschiedene Expositionsszenarien
- Anpassbare Anzahl und Position der Bierflaschen
- Ereignisprotokollierung (Events)
- Integration physiologischer Messdaten (optional)

## Hinweis

Dieses Projekt dient ausschließlich Forschungs- und Entwicklungszwecken im Bereich der Virtual-Reality-gestützten Expositionstherapie. Es stellt keine medizinische Behandlung oder ein zertifiziertes Therapiesystem dar.
