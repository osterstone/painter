using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wing.uPainter
{
    public class Constants
    {
        public const string UPAINTER_POINT_MODE = "UPAINTER_POINT_MODE";
        public const string UPAINTER_LINE_MODE = "UPAINTER_LINE_MODE";

        public const string UPAINTER_CAP_FLAT = "UPAINTER_CAP_FLAT";
        public const string UPAINTER_CAP_ROUND = "UPAINTER_CAP_ROUND";

        public const string UPAINTER_ENABLE_BEZIER = "UPAINTER_ENABLE_BEZIER";
        public const string UPAINTER_NEQ_WIDTH_LINE = "UPAINTER_NEQ_WIDTH_LINE";

        public const string UPAINTER_WARP_STRETCH = "UPAINTER_WARP_STRETCH";

        public const string UPAINTER_CORNER_FLAT = "UPAINTER_CORNER_FLAT";
        public const string UPAINTER_CORNER_SHARP = "UPAINTER_CORNER_SHARP";
        public const string UPAINTER_CORNER_ROUND = "UPAINTER_CORNER_ROUND";

        public const string UPAINTER_ENABLE_REMAP_UV = "UPAINTER_ENABLE_REMAP_UV";
    }

    public enum EPictureType
    {
        JPG,
        PNG,
    }

    public enum EBrushCapStyle
    {
        Flat,
        Round,
    }

    public enum EBrushJointStyle
    {
        Flat,
        Round,
    }

    public enum EBlendMode
    {
        Normal ,
        Restore ,
        Replace ,

        Darken ,
        Mutipy ,
        ColorBurn ,
        LinearDark ,

        Lighten ,
        ColorScreen ,
        ColorDodge ,
        LinearDodge ,

        Overlay ,
        HardLight ,
        SoftLight ,
        VividLight ,
        PinLight ,
        LinearLight ,
        HardMix ,

        Difference ,
        Exclusion ,
        Subtract ,
        Add ,

        //Dissolve,
    }

    public enum EPaintStatus
    {
        Success,
        Failed,
        NoBrushError,
        NoLayerError,
        BrushCheckFailed,
        NoCanvasError,
        NoMeshOperatorError,
        NoColliderError,
    }
    
    public enum EPaintMode
    {
        Dash,
        Line,
    }

    public enum ETextureWarpMode
    {
        Repeat,
        Stretch,
    }

    public enum EDrawStatus
    {
        Preview,
        DrawTemp,
        Draw,
    }
}
