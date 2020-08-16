using InterVR.IF.VR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public interface IF_VR_Steam_ISendMessageToEvent
    {
        void OnHandHoverBegin(GameObject sourceGo, GameObject argGo);
        void OnHandHoverEnd(GameObject sourceGo, GameObject argGo);
        void HandHoverUpdate(GameObject sourceGo, GameObject argGo);
        void OnHandFocusAcquired(GameObject sourceGo, GameObject argGo);
        void OnHandFocusLost(GameObject sourceGo, GameObject argGo);
        void OnAttachedToHand(GameObject sourceGo, GameObject argGo);
        void OnDetachedFromHand(GameObject sourceGo, GameObject argGo);
        void HandAttachedUpdate(GameObject sourceGo, GameObject argGo);
        void OnThrowableAttachEaseInCompleted(GameObject sourceGo, GameObject argGo);
    }
}