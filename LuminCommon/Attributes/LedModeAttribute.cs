using System;

namespace LuminCommon.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class LedModeAttribute : Attribute
    {
        public bool CanSetColor { get; }
        public bool UseSmoothing { get; }

        public LedModeAttribute(bool canSetColor, bool useSmoothing)
        {
            CanSetColor = canSetColor;
            UseSmoothing = useSmoothing;
        }
    }
}