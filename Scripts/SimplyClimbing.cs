
using System;
using UdonSharp;
using UnityEngine;
using UnityStandardAssets.Utility;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SimplyClimbing : UdonSharpBehaviour
{
    public float grabRadius = 0.2f;
    public LayerMask grabLayers;
    public bool grabVolumes = false;
    public bool keepVelocity = true;
    public float velocityMuliplier = 1.5f;
    public bool grappleLedge = true;

    private bool grabbing_L;
    private Transform grabbingT_L;
    private Vector3 grabbingPos_L;
    private bool grabbing_R;
    private Transform grabbingT_R;
    private Vector3 grabbingPos_R;

    private Collider[] overlapResults = new Collider[32];
    private Vector3[] lastVelocity = new Vector3[5];
    private int localPlayerCollision;
    private VRCPlayerApi localUser;


    private void Start()
    {
        localUser = Networking.LocalPlayer;

        for (int i = 0; i < 32; i++)
        {
            localPlayerCollision = localPlayerCollision | (Physics.GetIgnoreLayerCollision(10, i) ? 0 : (1 << i));
        }
    }
    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        if (value)
        {
            GrabCheck(args.handType);
        }
        else
        {
            bool activeGrab = grabbing_R || grabbing_L;

            if (args.handType == HandType.RIGHT)
            {
                grabbing_R = false;
            }
            else
            {
                grabbing_L = false;
            }

            if (!grabbing_L && !grabbing_R && activeGrab)
            {
                if (grappleLedge)
                {
                    if (!Grapple())
                    {
                        if (keepVelocity) ReleaseVelocity();
                    }
                }
                else
                {
                    if (keepVelocity) ReleaseVelocity();
                }

            }
        }

    }

    private void GrabCheck(HandType hand)
    {
        VRCPlayerApi.TrackingData handTracking = localUser.GetTrackingData(hand == HandType.RIGHT ? VRCPlayerApi.TrackingDataType.RightHand : VRCPlayerApi.TrackingDataType.LeftHand);

        int hitCount = Physics.OverlapSphereNonAlloc(handTracking.position, grabRadius * (localUser.GetAvatarEyeHeightAsMeters() / 1.64f), overlapResults, grabLayers, grabVolumes ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);

        bool validHit = false;
        Rigidbody movingCollider = null;
        int localmask = 0b_0000_0000_0000_0000_0000_0100_0000_0000;

        for (int i = 0; i < hitCount; i++)
        {
            if (!Utilities.IsValid(overlapResults[i]) && ((overlapResults[i].excludeLayers & localmask) != 0)) continue;

            movingCollider = overlapResults[i].attachedRigidbody;

            if (movingCollider != null)
            {
                if ((movingCollider.excludeLayers & localmask) != 0)
                {
                    movingCollider = null;
                    continue;
                }
                else
                {
                    validHit = true;
                    break;
                }
            }
            else
            {
                validHit = true;
            }

        }

        if (!validHit) return;

        switch (hand)
        {
            case HandType.RIGHT:
                if (movingCollider == null)
                {
                    grabbingT_R = null;
                    grabbingPos_R = handTracking.position;
                }
                else
                {
                    grabbingT_R = movingCollider.transform;
                    grabbingPos_R = grabbingT_R.InverseTransformPoint(handTracking.position);
                }

                grabbing_R = true;
                break;
            case HandType.LEFT:
                if (movingCollider == null)
                {
                    grabbingT_L = null;
                    grabbingPos_L = handTracking.position;
                }
                else
                {
                    grabbingT_L = movingCollider.transform;
                    grabbingPos_L = grabbingT_L.InverseTransformPoint(handTracking.position);
                }
                grabbing_L = true;
                break;
        }
    }

    public override void PostLateUpdate()
    {
        if (!grabbing_L && !grabbing_R) return;

        Vector3 destination;
        Vector3 current;
        if (grabbing_L && grabbing_R)
        {
            destination = Vector3.Lerp((grabbingT_L == null) ? grabbingPos_L : grabbingT_L.TransformPoint(grabbingPos_L), (grabbingT_R == null) ? grabbingPos_R : grabbingT_R.TransformPoint(grabbingPos_R), 0.5f);
            current = Vector3.Lerp(localUser.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position, localUser.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position, 0.5f);
        }
        else
        {
            destination = grabbing_L ? ((grabbingT_L == null) ? grabbingPos_L : grabbingT_L.TransformPoint(grabbingPos_L)) : ((grabbingT_R == null) ? grabbingPos_R : grabbingT_R.TransformPoint(grabbingPos_R));
            current = localUser.GetTrackingData(grabbing_L ? VRCPlayerApi.TrackingDataType.LeftHand : VRCPlayerApi.TrackingDataType.RightHand).position;
        }

        Vector3 velocity = (destination - current) * (1f / Time.deltaTime);
        localUser.SetVelocity(velocity);
        lastVelocity[Time.frameCount % lastVelocity.Length] = velocity;

    }

    public bool Grapple()
    {
        if (localUser.IsPlayerGrounded()) return false;
        float height = localUser.GetAvatarEyeHeightAsMeters();
        float userRadius = 0.2f;
        //float castRadius = 0.2f * (height / 1.64f);
        if (!Physics.SphereCast(localUser.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position + (localUser.GetRotation() * ((userRadius * 2) * Vector3.forward)), userRadius, Vector3.down, out RaycastHit hitinfo, height, localPlayerCollision, QueryTriggerInteraction.Ignore)) return false;

        if (Vector3.Dot(Vector3.up, hitinfo.normal) < 0.445) return false;

        localUser.TeleportTo(hitinfo.point, localUser.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
        return true;

    }

    public void ReleaseVelocity()
    {
        Vector3 velocity = Vector3.zero;
        foreach (Vector3 vel in lastVelocity)
        {
            velocity += vel;
        }
        velocity = velocity / lastVelocity.Length;
        localUser.SetVelocity(velocity * velocityMuliplier);
    }
}
