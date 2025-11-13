using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace ShoulderSurfing
{
    public static class Util
    {
        public static string? DirectoryName = null;

        public static string GetDirectory() {
            if (DirectoryName == null) {
                DirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
            return DirectoryName;
        }

        public static Shader LoadShader(string shaderName) {
            string shaderPath = Path.Combine(GetDirectory(), "shaders", shaderName);
            if (!File.Exists(shaderPath)) {
                Debug.Log("[ShoulderSurfingCamera] Shader not found in " + shaderPath);
                return null;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(shaderPath);
            ; // TODO
            return null;
        }

        public static Sprite LoadSprite(string textureName)
        {
            string directoryName = GetDirectory();
            string path = Path.Combine(directoryName, "textures");
            string text = Path.Combine(path, textureName);
            bool flag2 = File.Exists(text);
            if (flag2)
            {
                byte[] data = File.ReadAllBytes(text);
                Texture2D texture2D = new Texture2D(2, 2);
                bool flag3 = texture2D.LoadImage(data);
                if (flag3)
                {
                    return Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            return null;
        }
    }
}


