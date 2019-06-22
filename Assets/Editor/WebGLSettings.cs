using UnityEditor;

namespace SSM
{
    public static class WebGLSettings
    {
        [MenuItem("SSM/Project/Load WebGL Settings")]
        private static void SetWebGLSettings()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.memorySize = 512;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        }
    }
}