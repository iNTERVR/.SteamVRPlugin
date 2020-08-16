using UnityEngine;
using InterVR.IF.VR.Plugin.Steam.InteractionSystem;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public interface IF_VR_Steam_IComponentBuilder
    {
        void Build<T>(T go) where T : MonoBehaviour;
        void BuildTracker(IF_VR_Steam_Hand hand);
    }
}