
using Duckov.MiniMaps.UI;
using ShoulderSurfing;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShoulderSurfing
{
    public class PlayerArrow : MonoBehaviour
    {
        private MiniMapDisplay bindDisplay;
        private Image arrowCache;
        private Image rangeCache;

        public static Dictionary<MiniMapDisplay, GameObject> playerArrows = new Dictionary<MiniMapDisplay, GameObject>();
        public Vector3 offset = new Vector3(0, 28, 0);

        public static void SetOffset(MiniMapDisplay display, float offsetScale = 1.0f)
        {
            if (playerArrows.ContainsKey(display))
            {
                var obj = playerArrows[display];
                obj.GetComponent<PlayerArrow>().offset = new Vector3(0, 28 * offsetScale, 0);
            }
        }

        public static GameObject CreateOrGetPlayerArrow(MiniMapDisplay display)
        {
            
            if (playerArrows.ContainsKey(display))
            {
                var preObj = playerArrows[display];
                if(preObj != null)
                    return preObj;
            }
            GameObject arrowObject = new GameObject("PlayerArrow");
            PlayerArrow comp = arrowObject.AddComponent<PlayerArrow>();
            comp.bindDisplay = display;
            // MiniMapCommon.playerArrow = arrowObject;

            Image arrowImage = arrowObject.AddComponent<Image>();
            arrowImage.color = new Color(1, 1, 1, MiniMapCommon.mapIndicatorAlpha);
            arrowImage.sprite = Util.LoadSprite("player.png");
            comp.arrowCache = arrowImage;

            GameObject viewRangeGameobject = new GameObject("ViewRangeImage");
            Image rangeImage = viewRangeGameobject.AddComponent<Image>();
            comp.rangeCache = rangeImage;
            rangeImage.sprite = Util.LoadSprite("range.png");

            viewRangeGameobject.transform.SetParent(arrowObject.transform, false);
            viewRangeGameobject.transform.localPosition = new Vector3(0, 32, 0);
            rangeImage.rectTransform.pivot = new Vector2(0.5f, 0f);
            rangeImage.color = new Color(1, 1, 1, 0.5f * MiniMapCommon.mapIndicatorAlpha);
            viewRangeGameobject.transform.localScale = Vector3.one * 2f;

            arrowObject.transform.SetParent(display.transform, false);
            arrowObject.transform.SetAsLastSibling();
            // MiniMapCommon.playerArrow.transform.localPosition = MiniMapCommon.GetPlayerMinimapPosition();
            arrowObject.transform.position = MiniMapCommon.GetPlayerMinimapGlobalPosition(display);
            arrowObject.transform.localScale = Vector3.one * 0.7f / display.transform.localScale.x;
            RectTransform rectTransform = arrowImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(64, 64);
                // rectTransform.sizeDelta = new Vector2(0.001f, 0.001f) * arrowImage.sprite.texture.width * MiniMapSettings.Instance.PixelSize;
                // 确保锚点和轴心点设置正确
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            playerArrows[display] = arrowObject;
            return arrowObject;
        }

        void Update()
        {
            transform.position = MiniMapCommon.GetPlayerMinimapGlobalPosition(bindDisplay) + offset;
			transform.localRotation = MiniMapCommon.GetPlayerMinimapRotation();
			transform.localScale = Vector3.one * 0.7f / bindDisplay.transform.localScale.x;
			transform.SetAsLastSibling();
			arrowCache.color = new Color(1, 1, 1, MiniMapCommon.mapIndicatorAlpha);
			rangeCache.color = new Color(1, 1, 1, 0.5f * MiniMapCommon.mapIndicatorAlpha);
        }
    }
}
