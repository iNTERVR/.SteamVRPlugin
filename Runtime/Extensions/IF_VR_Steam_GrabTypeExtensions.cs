using InterVR.IF.VR.Defines;
using InterVR.IF.VR.Plugin.Steam.InteractionSystem;

namespace InterVR.IF.VR.Plugin.Steam.Extensions
{
    public static class IF_VR_Steam_GrabTypeExtensions
    {
        public static IF_VR_Steam_GrabTypes ConvertTo(this IF_VR_GrabType type)
        {
            if (type == IF_VR_GrabType.Grip)
            {
                return IF_VR_Steam_GrabTypes.Grip;
            }
            else if (type == IF_VR_GrabType.Pinch)
            {
                return IF_VR_Steam_GrabTypes.Pinch;
            }
            else if (type == IF_VR_GrabType.Scripted)
            {
                return IF_VR_Steam_GrabTypes.Scripted;
            }
            return IF_VR_Steam_GrabTypes.None;
        }

        public static IF_VR_GrabType ConvertTo(this IF_VR_Steam_GrabTypes type)
        {
            if (type == IF_VR_Steam_GrabTypes.Grip)
            {
                return IF_VR_GrabType.Grip;
            }
            else if (type == IF_VR_Steam_GrabTypes.Pinch)
            {
                return IF_VR_GrabType.Pinch;
            }
            else if (type == IF_VR_Steam_GrabTypes.Scripted)
            {
                return IF_VR_GrabType.Scripted;
            }
            return IF_VR_GrabType.None;
        }
    }
}