# VR Exposure Therapy

## Project Description

This project provides the foundation for a virtual reality exposure therapy application. Its purpose is to present users with controlled alcohol-related stimuli in a virtual environment while simultaneously recording their behavior during the exposure session.

Beer bottles are used as exposure stimuli and are distributed throughout the virtual scene. As users navigate the VR environment, various interaction and movement data are collected to analyze their behavior during the therapy session.

## Objectives

The project is intended as a foundation for scientific research and therapeutic applications in VR-based exposure therapy. The primary areas of interest include:

* Attention toward alcohol-related cues
* Interaction behavior with virtual objects
* Movement patterns within the virtual environment
* Gaze behavior (eye tracking)

## Features

* Virtual scene containing multiple beer bottles
* Physics-based interactive objects
* Grabbing and releasing bottles using VR controllers
* Tracking of various user-related data, including:

  * Headset position and rotation
  * Controller positions
  * Gaze tracking
  * Object interactions
  * Movement within the environment
* Foundation for subsequent behavioral data analysis

## Technologies Used

* Unity
* Meta XR SDK
* Meta Interaction SDK
* C#

## Project Structure

```text
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

## Data Tracking

During the application, the following data can be recorded:

* Head position
* Head rotation
* Gaze direction (eye tracking)
* Controller positions
* Controller rotations
* Object interactions
* Movement paths
* Time spent at specific locations
* Timestamp of each interaction event

These data can later be used to analyze user behavior during exposure therapy.

## Usage

1. Open the project in Unity.
2. Ensure that the Meta XR SDK is installed.
3. Connect the Meta Quest via Quest Link or deploy the application directly to the headset.
4. Open the `ExposureScene`.
5. Run or build the application.

## Planned Extensions

* Export tracking data as CSV or JSON
* Heatmap visualization of gaze and movement data
* Multiple exposure scenarios
* Configurable number and placement of beer bottles
* Event logging system
* Optional integration of physiological measurements

## Disclaimer

This project is intended solely for research and development purposes in the field of virtual reality–based exposure therapy. It is **not** a medical treatment or a certified therapeutic system.
