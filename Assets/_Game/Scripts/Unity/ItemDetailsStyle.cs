using UnityEngine;

namespace TheTrailWalker
{
    public static class ItemDetailsStyle
    {
        public static void Apply(GameObject panel,UnityEngine.UI.Text text)
        {
            var image=panel.GetComponent<UnityEngine.UI.Image>();
            Sprite parchment=Resources.Load<Sprite>("UI/ItemDetails/parchment");
            if(parchment!=null){image.sprite=parchment;image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=2;image.color=Color.white;}
            else image.color=new Color(.91f,.82f,.62f);
            text.color=new Color(.19f,.12f,.055f);
            text.rectTransform.anchorMin=new Vector2(.08f,.50f);text.rectTransform.anchorMax=new Vector2(.92f,.91f);
            text.rectTransform.offsetMin=text.rectTransform.offsetMax=Vector2.zero;
            text.resizeTextForBestFit=true;text.resizeTextMinSize=16;text.resizeTextMaxSize=23;
            foreach(UnityEngine.UI.Button button in panel.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                var rect=(RectTransform)button.transform;bool left=rect.anchorMin.x<.5f;
                rect.anchorMin=new Vector2(left ? 0.08f : 0.54f,.09f);rect.anchorMax=new Vector2(left ? 0.46f : 0.92f,.28f);
                rect.offsetMin=rect.offsetMax=Vector2.zero;
                button.GetComponent<UnityEngine.UI.Image>().color=new Color(.55f,.37f,.17f,.88f);
                UnityEngine.UI.Text label=button.GetComponentInChildren<UnityEngine.UI.Text>();
                if(label!=null)label.color=new Color(1,.95f,.80f);
            }
        }
    }
}
