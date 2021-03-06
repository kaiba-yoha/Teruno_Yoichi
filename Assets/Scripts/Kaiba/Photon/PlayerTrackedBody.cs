using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerTrackedBody : MonoBehaviour, IPunInstantiateMagicCallback
{
    public static Dictionary<TrackedBodyPart, PlayerTrackedBody> TrackedBodyDict = new Dictionary<TrackedBodyPart, PlayerTrackedBody>();
    public static event Action<TrackedBodyPart> OnTrackedPartsUpdated;


    [SerializeField]
    TrackedBodyPart bodyPart;

    [SerializeField]
    PhotonAnchoredTransformView view;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var part = (TrackedBodyPart)info.photonView.InstantiationData[0];
        Initialize(part);
    }

    public void Initialize(TrackedBodyPart part)
    {
        bodyPart = part;
        TrackedBodyDict.Add(part, this);
        if (PlayerBody_NetSync.HandDict.TryGetValue(part, out GameObject obj))
        {
            transform.SetParent(obj.transform);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            var anchor = (int)part > 2 ? TerunoManager.catcherAnchor : TerunoManager.shooterAnchor;
            transform.SetParent(anchor);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }

        view.Init();

        OnTrackedPartsUpdated?.Invoke(part);
    }

}

public enum TrackedBodyPart : int
{
    PlayerOne_Head = 0,
    PlayerOne_RightHand = 1,
    PlayerOne_LeftHand = 2,
    PlayerTwo_Head = 3,
    PlayerTwo_RightHand = 4,
    PlayerTwo_LeftHand = 5,
}
