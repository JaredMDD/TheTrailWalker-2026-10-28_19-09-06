using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    // Presentation only: actual grades, pile portions and consumption order stay in the item model.
    public sealed class QualityStarStrip : MonoBehaviour
    {
        private ItemEntry item;
        private bool detailed;
        private readonly int[] counts = new int[6], previous = new int[6];
        private static readonly Sprite[] Stars = new Sprite[6];
        private float nextRefresh;

        public static QualityStarStrip Compact(Transform parent, ItemEntry item, bool centered = false)
        {
            var root = new GameObject("Quality stars", typeof(RectTransform), typeof(QualityStarStrip));
            var rect = root.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin = rect.anchorMax = new Vector2(centered ? .5f : 0,1);
            rect.pivot = new Vector2(centered ? .5f : 0,1);rect.anchoredPosition=new Vector2(centered?0:2,-2);
            var strip=root.GetComponent<QualityStarStrip>();strip.Bind(item,false);return strip;
        }

        public void Bind(ItemEntry entry, bool showCounts)
        {
            item=entry;detailed=showCounts;
            for(int q=1;q<=5;q++) previous[q]=-1;
            Refresh();
        }

        private void LateUpdate()
        {
            if(Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.15f;Refresh();
        }

        private void Refresh()
        {
            if(item==null)return;
            for(int q=1;q<=5;q++)counts[q]=0;
            foreach(PilePortion part in item.Pile)counts[part.Quality]+=part.Quantity;
            bool changed=false;int distinct=0,low=0,high=0;
            for(int q=1;q<=5;q++)
            {
                if(counts[q]!=previous[q])changed=true;
                previous[q]=counts[q];
                if(counts[q]>0){distinct++;if(low==0)low=q;high=q;}
            }
            if(!changed)return;
            foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            if(distinct==0)return;
            if(detailed)
            {
                var layout=GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                if(layout==null)layout=gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                layout.childAlignment=TextAnchor.MiddleCenter;layout.spacing=6;
                layout.childControlWidth=true;layout.childControlHeight=true;
                layout.childForceExpandWidth=false;layout.childForceExpandHeight=true;
                int i=0;
                for(int q=1;q<=5;q++)if(counts[q]>0)
                {
                    RectTransform slot=Rect("Grade "+q,transform,new Vector2((float)i/distinct,0),new Vector2((float)(i+1)/distinct,1));
                    var size=slot.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();size.minWidth=65;size.preferredWidth=100;size.flexibleWidth=0;
                    AddStar(Rect("Gold star",slot,Vector2.zero,new Vector2(.47f,1)),q);
                    var text=Rect("Count",slot,new Vector2(.45f,0),Vector2.one).gameObject.AddComponent<UnityEngine.UI.Text>();
                    StyleText(text,"x"+counts[q],17,new Color(.23f,.15f,.07f));
                    text.resizeTextForBestFit=true;text.resizeTextMinSize=11;text.resizeTextMaxSize=17;i++;
                }
            }
            else
            {
                ((RectTransform)transform).sizeDelta=new Vector2(low==high?30:72,30);
                AddStar(Fixed("Lowest grade",0,30),low);
                if(low!=high)
                {
                    var dash=Fixed("Range",30,12).gameObject.AddComponent<UnityEngine.UI.Text>();StyleText(dash,"–",16,Color.white);
                    AddStar(Fixed("Highest grade",42,30),high);
                }
            }
        }

        private RectTransform Fixed(string name,float x,float width)
        {
            RectTransform rect=Rect(name,transform,Vector2.zero,Vector2.zero);
            rect.pivot=Vector2.zero;rect.anchoredPosition=new Vector2(x,0);rect.sizeDelta=new Vector2(width,30);return rect;
        }
        private static void AddStar(RectTransform rect,int grade)
        {
            if(Stars[grade]==null)Stars[grade]=Resources.Load<Sprite>("UI/ItemDetails/quality_"+grade);
            var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=Stars[grade];image.preserveAspect=true;image.raycastTarget=false;
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
        }
        private static void StyleText(UnityEngine.UI.Text text,string value,int size,Color color)
        {
            text.text=value;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.color=color;
            text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
        }
    }
}
