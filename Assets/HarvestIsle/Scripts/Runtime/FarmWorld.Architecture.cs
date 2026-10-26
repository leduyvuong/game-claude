using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmWorld
    {
        public void DetailEstateShop(Transform shop)
        {
            var roofData=Resources.Load<NatureModelAsset>("Architecture/ShopRoof");
            if(roofData!=null&&roofData.meshes!=null&&roofData.meshes.Length>0)
            {
                foreach(Transform child in shop)if(child.name=="Shop roof"||child.name=="Roof ridge")child.GetComponent<Renderer>().enabled=false;
                var roof=new GameObject("Laid tile roof",typeof(MeshFilter),typeof(MeshRenderer));roof.transform.SetParent(shop,false);
                roof.GetComponent<MeshFilter>().sharedMesh=roofData.meshes[0];
                var palette=new Material[roofData.meshes[0].subMeshCount];System.Array.Copy(roofData.materials,palette,palette.Length);roof.GetComponent<MeshRenderer>().sharedMaterials=palette;
            }
            var root=new GameObject("Shop architectural details").transform;root.SetParent(shop,false);
            Color wood=Hex("896343"),dark=Hex("5C4635"),teal=Hex("668C6C"),stone=Hex("D3C7AC");
            for(int i=0;i<2;i++)Shape(PrimitiveType.Cube,"Door step",root,new Vector3(-.5f,.035f+i*.04f,-1.22f+i*.12f),new Vector3(.78f,.07f+i*.08f,.27f),stone);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(PrimitiveType.Cube,"Porch foot",root,new Vector3(side*1.04f,.09f,-1.18f),new Vector3(.21f,.18f,.21f),stone);
                Shape(PrimitiveType.Cube,"Porch column",root,new Vector3(side*1.04f,.67f,-1.18f),new Vector3(.095f,1.17f,.095f),dark);
                var brace=Shape(PrimitiveType.Cube,"Porch brace",root,new Vector3(side*.91f,1.09f,-1.18f),new Vector3(.07f,.40f,.075f),wood);brace.transform.localRotation=Quaternion.Euler(0,0,side*-42);
                for(int row=0;row<3;row++)Shape(PrimitiveType.Cube,"Side stone course",root,new Vector3(side*1.36f,.13f+row*.13f,.08f),new Vector3(.045f,.10f,1.7f),stone);
                for(int window=0;window<2;window++)
                {
                    float x=window==0?-.55f:.55f;
                    Shape(PrimitiveType.Cube,"Upper shutter",root,new Vector3(x+side*.34f,1.76f,-.76f),new Vector3(.17f,.47f,.06f),teal);
                    for(int slat=0;slat<4;slat++)Shape(PrimitiveType.Cube,"Shutter slat",root,new Vector3(x+side*.34f,1.60f+slat*.10f,-.80f),new Vector3(.18f,.026f,.025f),dark);
                }
                Shape(PrimitiveType.Cube,"Eave gutter",root,new Vector3(side*1.46f,2.07f,.12f),new Vector3(.08f,.08f,2.18f),dark);
                for(int row=0;row<7;row++)
                {Shape(PrimitiveType.Cube,"Corner masonry",root,new Vector3(side*1.32f,.18f+row*.27f,-.92f),new Vector3(.23f,.19f,.14f),row%3==0?stone:Hex("E5D6B5"));}
                for(int i=0;i<5;i++)Shape(PrimitiveType.Cube,"Balcony carved baluster",root,new Vector3(side*(.12f+i*.19f),1.46f,-1.28f),new Vector3(.032f,.25f,.04f),wood);
            }
            for(int row=0;row<3;row++)for(int i=0;i<6;i++)Shape(PrimitiveType.Cube,"Porch paving",root,new Vector3(-.96f+i*.38f,.018f,-1.02f-row*.19f),new Vector3(.35f,.03f,.17f),Hex((i+row)%4==0?"965236":"B86D4C"));
            for(int side=-1;side<=1;side+=2)
            {Shape(PrimitiveType.Cube,"Door hinge",root,new Vector3(-.72f,.48f+side*.28f,-1.00f),new Vector3(.16f,.055f,.035f),dark);}
            Shape(PrimitiveType.Cube,"Door handle",root,new Vector3(-.31f,.45f,-1.02f),new Vector3(.04f,.14f,.045f),Hex("D4B36C"));
            Shape(PrimitiveType.Cube,"Rain pipe",root,new Vector3(1.46f,1.12f,.94f),new Vector3(.07f,1.92f,.07f),dark);
            Shape(PrimitiveType.Cube,"Rear door",root,new Vector3(.6f,.59f,.966f),new Vector3(.57f,1.13f,.035f),wood);
            Shape(PrimitiveType.Cube,"Rear lintel",root,new Vector3(.6f,1.18f,.99f),new Vector3(.72f,.10f,.08f),dark);
            Shape(PrimitiveType.Cube,"Rear vent",root,new Vector3(-.55f,1.71f,.96f),new Vector3(.72f,.42f,.04f),dark);
            for(int i=0;i<5;i++)Shape(PrimitiveType.Cube,"Rear vent louver",root,new Vector3(-.83f+i*.14f,1.71f,.99f),new Vector3(.09f,.38f,.035f),teal);
            BatchTownHouse(root);
        }
    }
}
