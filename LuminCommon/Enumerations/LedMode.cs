using LuminCommon.Attributes;

namespace LuminCommon.Enumerations
{
    public enum LedMode
    {
        [LedMode(false, false)]
        None = 0,
        [LedMode(false, true)]
        Spin = 1,
        [LedMode(false, true)]
        KnightRider = 2,
        [LedMode(true, true)]
        Disco = 3,
    }
}
