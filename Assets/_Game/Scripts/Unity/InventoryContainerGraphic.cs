using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // A native Canvas mesh rather than a stretched bitmap. All decoration stays inside its hit rectangle.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class InventoryContainerGraphic : UnityEngine.UI.MaskableGraphic
    {
        public InventoryContainerSkin Skin;
        public bool FrontLoop;
        private UnityEngine.UI.VertexHelper mesh;
        private Rect bounds;
        private Color cloth,shade,binding,thread,patch,interior,interiorShade;
        // Bag and pocket insides get no light, so their cloth is drawn darker than the lit rim, binding and patches.
        private const float InteriorDarkening=.45f;
        public void Bind(InventoryContainerSkin skin,bool front=false)
        { Skin=skin;FrontLoop=front;raycastTarget=false;SetVerticesDirty(); }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear();if(Skin==null)return;mesh=vh;bounds=rectTransform.rect;
            cloth=Skin.Cloth;shade=Skin.Shade;binding=Skin.Binding;thread=Skin.Thread;patch=Skin.Patch;
            interior=Color.Lerp(cloth,Color.black,InteriorDarkening);interiorShade=Color.Lerp(shade,Color.black,InteriorDarkening);
            switch(Skin.Kind)
            {
                case InventoryContainerKind.Rags: ClothBag(true);break;
                case InventoryContainerKind.Backpack: ClothBag(false);break;
                case InventoryContainerKind.Pocket: Pocket();break;
                case InventoryContainerKind.Belt: Belt();break;
                case InventoryContainerKind.ToolLoop: Loop();break;
                case InventoryContainerKind.Opening: Opening();break;
            }
            mesh=null;
        }
        private Vector2 P(float x,float y)=>new Vector2(bounds.xMin+x*bounds.width,bounds.yMin+y*bounds.height);
        private void Poly(Color c,params Vector2[] p)
        {
            int start=mesh.currentVertCount;Vector2 center=Vector2.zero;
            foreach(var v in p)center+=v;center/=p.Length;
            mesh.AddVert(center,c,Vector2.zero);foreach(var v in p)mesh.AddVert(v,c,Vector2.zero);
            for(int i=0;i<p.Length;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%p.Length);
        }
        private void Quad(float x,float y,float w,float h,Color c)=>Poly(c,P(x,y),P(x+w,y),P(x+w,y+h),P(x,y+h));
        private void Line(Vector2 a,Vector2 b,float pixels,Color c)
        {
            var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*pixels*.5f;
            Poly(c,a-n,b-n,b+n,a+n);
        }
        private void Seam(Vector2 a,Vector2 b,float spacing=12)
        {
            int count=Mathf.Clamp(Mathf.FloorToInt(Vector2.Distance(a,b)/spacing),2,120);
            for(int i=0;i<count;i++)Line(Vector2.Lerp(a,b,(i+.15f)/count),Vector2.Lerp(a,b,(i+.57f)/count),1.5f,thread);
        }
        private void Weave(float inset=.05f)
        {
            Color ink=Color.Lerp(interior,interiorShade,.22f);ink.a=.33f;
            float dx=18/Mathf.Max(1,bounds.width),dy=16/Mathf.Max(1,bounds.height);
            for(float x=inset;x<1-inset;x+=dx)Line(P(x,inset),P(x,1-inset),.6f,ink);
            for(float y=inset;y<1-inset;y+=dy)Line(P(inset,y),P(1-inset,y),.6f,ink);
        }
        private void ClothBag(bool rags)
        {
            if(rags)
            {
                Poly(shade,P(0,.06f),P(.035f,.025f),P(.14f,.045f),P(.22f,0),P(.38f,.03f),P(.54f,.012f),P(.69f,.04f),P(.83f,.016f),P(.97f,.05f),P(1,.16f),P(.985f,.43f),P(1,.7f),P(.97f,.98f),P(.82f,.955f),P(.7f,1),P(.52f,.98f),P(.32f,1),P(.15f,.97f),P(.025f,.99f),P(0,.79f),P(.016f,.54f));
                Poly(interior,P(.025f,.09f),P(.22f,.035f),P(.51f,.05f),P(.79f,.04f),P(.97f,.085f),P(.96f,.94f),P(.7f,.96f),P(.34f,.96f),P(.04f,.95f));
                Weave();
                Color innerPatch=Color.Lerp(patch,Color.black,InteriorDarkening);
                Poly(innerPatch,P(.055f,.12f),P(.23f,.1f),P(.25f,.33f),P(.07f,.35f));
                Seam(P(.08f,.14f),P(.21f,.13f));Seam(P(.08f,.32f),P(.22f,.3f));Seam(P(.07f,.14f),P(.08f,.31f));Seam(P(.23f,.14f),P(.23f,.3f));
                Poly(innerPatch,P(.8f,.72f),P(.94f,.7f),P(.95f,.91f),P(.79f,.92f));
                Seam(P(.81f,.74f),P(.93f,.73f));Seam(P(.81f,.9f),P(.93f,.89f));
                for(int i=0;i<27;i++)
                {
                    float x=.035f+i*.035f;Line(P(x,.035f+.009f*(i%3)),P(x+.006f,.008f),1,thread);
                }
                Seam(P(.045f,.075f),P(.96f,.075f));Seam(P(.035f,.1f),P(.035f,.93f));Seam(P(.965f,.1f),P(.965f,.93f));
                // Mended tear; no actual hole that could imply unusable bag cells.
                Line(P(.57f,.8f),P(.7f,.85f),2,interiorShade);
                for(int i=0;i<6;i++)Line(P(.58f+i*.02f,.78f+i*.008f),P(.59f+i*.02f,.82f+i*.008f),1.5f,thread);
            }
            else
            {
                Poly(binding,P(0,.05f),P(.025f,0),P(.975f,0),P(1,.05f),P(1,.96f),P(.97f,1),P(.03f,1),P(0,.96f));
                Quad(.027f,.045f,.946f,.91f,interior);Weave();
                // Side reinforcement and a bound open mouth, echoing the crafted hemp/leather pack.
                Quad(.012f,.04f,.035f,.92f,binding);Quad(.953f,.04f,.035f,.92f,binding);
                Quad(.025f,.91f,.95f,.065f,patch);Quad(.025f,.9f,.95f,.018f,shade);
                Quad(.028f,.035f,.944f,.045f,binding);
                for(int i=0;i<2;i++)
                {
                    float x=i==0?.09f:.86f;Quad(x,.91f,.045f,.085f,binding);Quad(x+.009f,.927f,.026f,.031f,thread);
                }
                Seam(P(.055f,.09f),P(.945f,.09f));Seam(P(.055f,.89f),P(.945f,.89f));
                Seam(P(.055f,.1f),P(.055f,.88f));Seam(P(.945f,.1f),P(.945f,.88f));
                // Fold shadows stay at the perimeter so inventory silhouettes remain readable.
                Poly(interiorShade,P(.048f,.1f),P(.075f,.25f),P(.048f,.85f));
                Poly(interiorShade,P(.952f,.1f),P(.93f,.25f),P(.952f,.85f));
            }
        }
        private void Pocket()
        {
            Poly(binding,P(.03f,.95f),P(.97f,.95f),P(.97f,.17f),P(.8f,.03f),P(.2f,.03f),P(.03f,.17f));
            Poly(interior,P(.075f,.9f),P(.925f,.9f),P(.925f,.2f),P(.77f,.075f),P(.23f,.075f),P(.075f,.2f));
            Weave(.12f);Quad(.045f,.85f,.91f,.09f,patch);Quad(.065f,.825f,.87f,.025f,shade);
            Seam(P(.11f,.8f),P(.11f,.23f),9);Seam(P(.89f,.8f),P(.89f,.23f),9);
            Seam(P(.11f,.23f),P(.26f,.13f),9);Seam(P(.26f,.13f),P(.74f,.13f),9);Seam(P(.74f,.13f),P(.89f,.23f),9);
            Seam(P(.08f,.9f),P(.92f,.9f),9);
        }
        private void Belt()
        {
            Quad(0,.03f,1,.9f,binding);Quad(0,.16f,1,.68f,patch);
            Seam(P(.01f,.23f),P(.99f,.23f),10);Seam(P(.01f,.78f),P(.99f,.78f),10);
            for(float x=.045f;x<.98f;x+=.12f)Quad(x,.43f,.006f,.13f,shade);
        }
        private void Loop()
        {
            if(!FrontLoop)
            {
                // Retaining strap joins the pants belt; no generic metal hook or weapon-specific socket.
                Quad(.42f,.69f,.16f,.31f,binding);Quad(.45f,.72f,.1f,.26f,patch);
                Seam(P(.46f,.75f),P(.46f,.96f),10);Seam(P(.54f,.75f),P(.54f,.96f),10);
                Quad(.31f,.67f,.38f,.12f,shade);
            }
            else
            {
                Line(P(.33f,.79f),P(.38f,.69f),Mathf.Min(8,bounds.width*.055f),binding);
                Line(P(.38f,.69f),P(.62f,.69f),Mathf.Min(10,bounds.width*.07f),binding);
                Line(P(.62f,.69f),P(.67f,.79f),Mathf.Min(8,bounds.width*.055f),binding);
                Line(P(.37f,.69f),P(.63f,.69f),2,thread);
                Quad(.32f,.77f,.035f,.04f,thread);Quad(.645f,.77f,.035f,.04f,thread);
            }
        }
        private void Opening()
        {
            Quad(0,0,1,1,binding);Quad(.015f,.2f,.97f,.53f,shade);
            Seam(P(.015f,.82f),P(.985f,.82f));
        }
        // Bake the exact same UI geometry for designers to inspect as a mesh asset.
        public Mesh BakeMesh()
        {
            var result=new Mesh { name=Skin!=null?Skin.name:"Container" };
            using(var vh=new UnityEngine.UI.VertexHelper()){OnPopulateMesh(vh);vh.FillMesh(result);}
            return result;
        }
    }
}
