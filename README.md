# Simply Climbing
 A simple velocity based climbing system script for vrchat.
 
 Video example:

https://github.com/user-attachments/assets/7fc8da9c-0a2d-4ee2-95c4-9ee97441dfa4

 ## How it works
 
 * Grab Radius scales with avatar height. Default height is 1.64m.
 * Grab Layers checks if colliders or rigidbodies excludes the localPlayer layer.
 * Climbing moving objects only works if that object has a Rigidbody.
 * Keep Velocity lets the player fling themselves when releaseing the climbing.
 * Can grapple up ledges to make getting up easier.
 * Designed for VR users.

 ## How to setup

* Add the [Simply Climbing prefab](Simply%20Climbing.prefab) or [SimplyClimbing script](Scripts/SimplyClimbing.cs) to your scene.
* Specify how far you can grab with "GrabRadius" float variable.
* Select what layers that are climbable in the "GrabLayers" layer mask variable.
* Optional if you want to grab trigger colliders with "GrabVolumes" bool variable.
* Playerse can fling themselves with "KeepVelocity" bool variable.
* Can boost the velocity with "VelocityMultiplier" float variable.
* Can teleport up ledges with "GrappleLedge" bool variable.

## Example

Made a place you can test [in this world](https://vrchat.com/home/world/wrld_b9e38d09-bdf0-4a53-a90e-f94d21094b60)

## Requirements

* [VRChat Sdk - Worlds](https://vrchat.com/home/download) 3.7.0+
* Unity 2022.3.22f1+
