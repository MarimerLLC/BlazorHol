namespace BlazorRenderModes.Services;

public enum RenderMode
{
    Unknown,
    ServerStatic,
    ServerStaticStreaming,
    ServerInteractive,
    WebAssemblyInteractive
}

public static class RenderModeFlagsExtensions
{
    public static bool IsServer(this RenderMode mode)
    {
        return mode.ToString().Contains("Server");
    }

    public static bool IsWebAssembly(this RenderMode mode)
    {
        return mode == RenderMode.WebAssemblyInteractive;
    }

    public static bool IsInteractive(this RenderMode mode)
    {
        return mode.ToString().Contains("Interactive");
    }

    public static bool IsStreaming(this RenderMode mode)
    {
        return mode == RenderMode.ServerStaticStreaming;
    }
}
