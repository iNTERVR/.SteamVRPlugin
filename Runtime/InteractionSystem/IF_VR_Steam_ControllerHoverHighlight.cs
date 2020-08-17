//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Highlights the controller when hovering over interactables
//
//=============================================================================

using UnityEngine;
using System.Collections;
using Valve.VR;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class IF_VR_Steam_ControllerHoverHighlight : MonoBehaviour
    {
        public Material highLightMaterial;
        public bool fireHapticsOnHightlight = true;

        protected IF_VR_Steam_Hand hand;

        protected IF_VR_Steam_RenderModel renderModel;

        protected SteamVR_Events.Action renderModelLoadedAction;

        protected void Awake()
        {
            hand = GetComponentInParent<IF_VR_Steam_Hand>();
        }

        protected void OnHandInitialized(int deviceIndex)
        {
            GameObject renderModelGameObject = GameObject.Instantiate(hand.renderModelPrefab);
            renderModelGameObject.transform.parent = this.transform;
            renderModelGameObject.transform.localPosition = Vector3.zero;
            renderModelGameObject.transform.localRotation = Quaternion.identity;
            renderModelGameObject.transform.localScale = hand.renderModelPrefab.transform.localScale;


            renderModel = renderModelGameObject.GetComponent<IF_VR_Steam_RenderModel>();

            renderModel.SetInputSource(hand.handType);
            renderModel.OnHandInitialized(deviceIndex);
            renderModel.SetMaterial(highLightMaterial);

            hand.SetHoverRenderModel(renderModel);
            renderModel.onControllerLoaded += RenderModel_onControllerLoaded;
            renderModel.Hide();
        }

        private void RenderModel_onControllerLoaded()
        {
            renderModel.Hide();
        }


        //-------------------------------------------------
        protected void OnParentHandHoverBegin(IF_VR_Steam_Interactable other)
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }

            if (other.transform.parent != transform.parent)
            {
                ShowHighlight();
            }
        }


        //-------------------------------------------------
        private void OnParentHandHoverEnd(IF_VR_Steam_Interactable other)
        {
            HideHighlight();
        }


        //-------------------------------------------------
        private void OnParentHandInputFocusAcquired()
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }

            if (hand.hoveringInteractable && hand.hoveringInteractable.transform.parent != transform.parent)
            {
                ShowHighlight();
            }
        }


        //-------------------------------------------------
        private void OnParentHandInputFocusLost()
        {
            HideHighlight();
        }


        //-------------------------------------------------
        public void ShowHighlight()
        {
            if (renderModel == null)
            {
                return;
            }

            if (fireHapticsOnHightlight)
            {
                hand.TriggerHapticPulse(500);
            }

            renderModel.Show();
        }


        //-------------------------------------------------
        public void HideHighlight()
        {
            if (renderModel == null)
            {
                return;
            }

            if (fireHapticsOnHightlight)
            {
                hand.TriggerHapticPulse(300);
            }

            renderModel.Hide();
        }
    }
}