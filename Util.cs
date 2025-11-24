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

		public static Material LoadMaterial(string matName) {
			string platformStr = "Win";
			if (Application.platform == RuntimePlatform.OSXPlayer) {
				platformStr = "OSX";
			}
            string matPath = Path.Combine(GetDirectory(), "shaders/" + platformStr + "/shaders");
			AssetBundle bundle = AssetBundle.LoadFromFile(matPath);
			if (!bundle) {
				Debug.Log("[shoulder camera] can't load bundle from path " + matPath);
				return null;
			}
			Material mat = bundle.LoadAsset<Material>(matName);
			if (!mat) {
				Debug.Log("[shoulder camera] failed to load material, no material named " + matName);
			}
			return mat;
		}


		public static Shader LoadShader(string shaderName) {
            string platformStr = "Win";
            if (Application.platform == RuntimePlatform.OSXPlayer) {
                platformStr = "OSX";
			}
            string shaderPath = Path.Combine(GetDirectory(), "shaders/" + platformStr + "/shaders");
            AssetBundle bundle = AssetBundle.LoadFromFile(shaderPath);
            if (!bundle) {
                Debug.Log("[shoulder camera] can't load bundle from path " + shaderPath);
                return null;
            }
			Shader shader = bundle.LoadAsset<Shader>(shaderName);
            if (shader == null) {
                Debug.Log("[shoulder camera] failed to load shader, no shader named " + shaderName);
            }
            return shader;
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


