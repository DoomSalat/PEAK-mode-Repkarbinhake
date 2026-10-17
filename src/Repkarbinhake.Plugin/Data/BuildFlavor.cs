namespace Repkarbinhake.Plugin.Data
{
    public static class BuildFlavor // Static: compile-time build flavor read by setup code
    {
#if REPKA_PUBLIC
        public static readonly bool HasDebugTools = false; // Static: fixed per build
#else
        public static readonly bool HasDebugTools = true; // Static: fixed per build
#endif
    }
}
