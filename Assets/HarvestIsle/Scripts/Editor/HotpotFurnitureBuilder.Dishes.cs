using UnityEngine;

namespace HarvestIsle.Editor
{
    internal sealed partial class HotpotFurnitureBuilder
    {
        void Dish(int type,Transform vessel,Transform contents)
        {
            bool clay=type==3||type==6||type==8;string material=clay?Clay:type==0||type==4||type==11?Steel:Black;
            Vessel(vessel,1.62f,.44f,material);
            C(contents,0,.377f,0,1.44f,.025f,1.44f,type==1?Chili:type==2||type==7?"TomatoFlesh":type==3?Soup:type==8?"Broth":type==10?Dark:"ThaiSoup");
            if(type==1){Shape("Mild broth half",contents,new Vector3(0,.393f,0),new Vector3(1.43f,1,1.43f),Soup,halfDisc);Divider(vessel,1.5f,.45f);}
            if(type==11)
            {
                var chimney=Lathe(new[]{new Vector2(.27f,0),new Vector2(.14f,.72f),new Vector2(.11f,.72f),new Vector2(.23f,0)},24);kitchenMeshes.Add(chimney);
                Shape("Chimney",vessel,new Vector3(0,.35f,0),Vector3.one,Steel,chimney);C(vessel,0,-.045f,0,1.05f,.13f,1.05f,Steel);
            }
            const float y=.395f;
            switch(type)
            {
                case 0:
                    ShrimpPiece(contents,new Vector3(-.30f,y,-.17f),30,1.15f);ShrimpPiece(contents,new Vector3(.28f,y,.14f),210,1.1f);TomatoPiece(contents,new Vector3(.25f,y,-.40f),45,1.05f);MushroomPiece(contents,new Vector3(-.27f,y,.34f),0,.75f);
                    for(int i=0;i<3;i++)Rod(contents,new Vector3(-.56f+i*.08f,y,.15f),new Vector3(-.26f+i*.08f,y+.03f,.59f),.038f,"CabbageStem");break;
                case 1:
                    for(int i=0;i<10;i++)Small(contents,new Vector3(-.24f+Mathf.Sin(i*2.4f)*.23f,y+.035f,-.23f+Mathf.Cos(i*2.4f)*.26f),new Vector3(.10f,.06f,.18f),Chili);
                    MushroomPiece(contents,new Vector3(.27f,y,.35f),1,.60f);for(int i=0;i<3;i++)MeatRoll(contents,new Vector3(-.23f+i*.21f,y+.12f,.22f),false,.75f);for(int i=0;i<2;i++)U(contents,-.32f+i*.32f,y+.07f,-.23f,.20f,.14f,.20f,Tofu);break;
                case 2:
                    for(int i=0;i<5;i++)TomatoPiece(contents,new Vector3(Mathf.Sin(i*1.25f)*.46f,y,Mathf.Cos(i*1.25f)*.46f),i*72,1.05f);CornPiece(contents,new Vector3(-.13f,y,-.26f),.85f);CornPiece(contents,new Vector3(.16f,y,.3f),.82f);U(contents,-.38f,y+.09f,.16f,.25f,.18f,.23f,Tofu);U(contents,.35f,y+.09f,-.08f,.25f,.18f,.23f,Tofu);break;
                case 3:
                    for(int i=0;i<4;i++)MushroomPiece(contents,new Vector3(Mathf.Sin(i*1.6f)*.40f,y,Mathf.Cos(i*1.6f)*.40f),i%3,.8f);MushroomPiece(contents,new Vector3(0,y,-.18f),0,.95f);GreensPiece(contents,new Vector3(-.28f,y,.23f),1,.65f);break;
                case 4:
                    ShrimpPiece(contents,new Vector3(-.30f,y,-.27f),45,1.05f);ShrimpPiece(contents,new Vector3(.31f,y,-.20f),140,.85f);CornPiece(contents,new Vector3(0,y,.03f),.72f);
                    for(int i=0;i<3;i++){var shell=Group("Mussel",contents,new Vector3(-.3f+i*.25f,y+.04f,.36f));E(shell,0,0,0,.21f,.07f,.37f,"Shell");E(shell,0,.065f,0,.11f,.07f,.22f,Orange);Shape("Squid ring",contents,new Vector3(.30f-i*.12f,y+.06f,-.42f),new Vector3(.22f,.5f,.22f),"Squid",ring);}break;
                case 5:
                    for(int i=0;i<5;i++)MeatRoll(contents,new Vector3((i%3-1)*.22f,y+.1f,(i/3-.5f)*.24f),false,.9f);
                    for(int i=0;i<5;i++){float a=i*1.25f;E(contents,Mathf.Sin(a)*.5f,y+.07f,Mathf.Cos(a)*.5f,.22f,.16f,.22f,Mushroom);}break;
                case 6:
                    for(int i=0;i<6;i++){float a=i*1.05f;E(contents,Mathf.Cos(a)*.34f,y+.08f,Mathf.Sin(a)*.34f,.3f,.17f,.24f,"CookedChicken");GreensPiece(contents,new Vector3(Mathf.Cos(a)*.53f,y,Mathf.Sin(a)*.53f),1,.45f);}MushroomPiece(contents,new Vector3(0,y,0),0,.8f);break;
                case 7:
                    for(int i=0;i<4;i++){float a=i*1.57f;var fish=E(contents,Mathf.Sin(a)*.34f,y+.08f,Mathf.Cos(a)*.34f,.34f,.18f,.22f,WhiteFish);fish.localRotation=Quaternion.Euler(0,i*65,0);E(fish,0,.18f,.35f,.75f,.43f,.15f,"Shell");TomatoPiece(contents,new Vector3(Mathf.Sin(a+.8f)*.49f,y,Mathf.Cos(a+.8f)*.49f),i*70,.8f);}break;
                case 8:
                    for(int i=0;i<7;i++){float a=i*2.4f;U(contents,Mathf.Sin(a)*.38f,y+.09f,Mathf.Cos(a)*.38f,.25f,.19f,.23f,i%2==0?"Goat":"Taro");}GreensPiece(contents,new Vector3(-.3f,y,.28f),1,.6f);break;
                case 9:
                    for(int i=0;i<25;i++){float a=i*2.4f,r=.1f+i%5*.055f;Small(contents,new Vector3(Mathf.Sin(a)*r,y+.045f+i%3*.02f,Mathf.Cos(a)*r),new Vector3(.09f,.065f,.08f),"Crab");}
                    for(int i=0;i<4;i++){float a=i*1.57f;TomatoPiece(contents,new Vector3(Mathf.Sin(a)*.5f,y,Mathf.Cos(a)*.5f),i*70,.85f);U(contents,Mathf.Sin(a+.6f)*.40f,y+.075f,Mathf.Cos(a+.6f)*.40f,.23f,.15f,.2f,Yellow);}break;
                case 10:
                    for(int i=0;i<5;i++)MeatRoll(contents,new Vector3(-.45f+i*.21f,y+.09f,-.34f),false,.80f);Noodles(contents,new Vector3(.36f,y,.26f),false,.65f);MushroomPiece(contents,new Vector3(-.40f,y,.06f),0,.80f);GreensPiece(contents,new Vector3(0,y,.36f),1,.7f);for(int i=0;i<3;i++)U(contents,-.43f+i*.2f,y+.08f,.35f,.17f,.16f,.17f,Tofu);break;
                case 11:
                    for(int i=0;i<5;i++){float a=i*1.26f;Vector3 at=new Vector3(Mathf.Sin(a)*.47f,y,Mathf.Cos(a)*.47f);if(i%3==0)MushroomPiece(contents,at,1,.62f);else if(i%3==1)GreensPiece(contents,at,0,.65f);else MeatRoll(contents,at+Vector3.up*.08f,false,.70f);}
                    var side=Group("Side beef tray",contents,new Vector3(1.07f,0,-.18f));B(side,0,.04f,0,.43f,.07f,.65f,Black);for(int i=0;i<3;i++)MeatRoll(side,new Vector3(0,.16f,(i-1)*.20f),false,.70f);break;
            }
            if(type!=11)for(int i=0;i<7;i++){float a=i*2.4f;Small(contents,new Vector3(Mathf.Cos(a)*.43f,y+.065f,Mathf.Sin(a)*.4f),new Vector3(.045f,.025f,.045f),Leaf);}
            Socket(root,new Vector3(0,.53f,0),"Steam");
        }
        void Kitchen(int level,Transform p,Transform contents)
        {
            int burners=level<3?1:level==3?2:level==8?6:4;
            float width=level==1?1.9f:level==2?2.05f:level==3?3.1f:level==8?6.6f:4.8f;
            string cabinet=level==1?Wood:level<=4?Steel:level==5?"Sage":level==6?"Burgundy":level==7?"Teal":Dark;
            float y=1.0f;
            if(level<=3)
            {
                Legs(p,width,1.1f,y,cabinet);B(p,0,.21f,0,width-.1f,.065f,1.0f,cabinet);
                if(level==1)for(int i=0;i<8;i++)B(p,-width*.44f+i*width*.88f/7,y+.012f,0,.012f,.012f,1.12f,Dark);
            }
            else
            {
                Cabinet(p,width,.95f,1.10f,cabinet);
                for(int i=0;i<burners;i++)
                {
                    float x=(i-(burners-1)*.5f)*(width-.3f)/burners;B(p,x,.58f,-.565f,(width-.18f)/burners-.045f,.62f,.045f,cabinet);B(p,x+.14f,.63f,-.598f,.025f,.22f,.035f,level>=5?Gold:Steel);
                    B(p,x,.91f,-.572f,(width-.18f)/burners-.04f,.16f,.055f,cabinet);
                    B(p,x,.92f,-.615f,.21f,.025f,.028f,level>=5?Gold:Steel);
                }
                for(int i=0;i<8;i++)B(p,width*.40f,.24f+i*.035f,-.596f,.31f,.012f,.008f,Black);
            }
            B(p,0,y,0,width+.08f,.12f,1.15f,level==1?Wood:level==5||level==7||level==8?"Marble":level==6?"Stone":Steel);
            if(level>=2)B(p,0,1.18f,.53f,width,.29f,.055f,Steel);
            for(int i=0;i<burners;i++)
            {
                float x=burners==1?.25f:(i-(burners-1)*.5f)*(width-.45f)/burners;
                var stove=Group("Burner "+i,p,new Vector3(x,y+.065f,-.10f));stove.localScale=Vector3.one*.72f;Stove(stove,true);
                var flame=Group("Gas flame "+i,contents,new Vector3(x,y+.26f,-.10f));for(int k=0;k<14;k++){float a=k*Mathf.PI/7;Small(flame,new Vector3(Mathf.Sin(a)*.19f,0,Mathf.Cos(a)*.19f),new Vector3(.035f,.07f,.035f),"Flame");}
                var pot=Group("Stock pot "+i,p,new Vector3(x,y+.29f,-.10f));Vessel(pot,.69f,.39f,Steel);C(contents,x,y+.64f,-.10f,.60f,.018f,.60f,i%2==0?"ThaiSoup":Soup);Socket(root,new Vector3(x,y+.74f,-.10f),"Burner "+i);
                if(level==6&&i>=2){var cover=Group("Lidded simmer pot",p,new Vector3(x,y+.67f,-.10f));PotLid(cover,.70f);}
                if(level==1||level<=3)PlateStack(p,-width*.35f+i*.32f,.25f,0,4);
            }
            if(level==1)
            {B(p,-.57f,1.09f,0,.53f,.035f,.43f,Wood);for(int i=0;i<16;i++)Small(p,new Vector3(-.75f+(i%4)*.09f,1.12f,-.12f+i/4*.07f),new Vector3(.05f,.03f,.045f),Leaf);C(p,-.6f,1.24f,.35f,.13f,.32f,.13f,Wood);}
            else if(level==2){PlateStack(p,-.67f,1.09f,-.05f,4);Bottle(p,-.64f,1.08f,.33f,0);Bottle(p,-.83f,1.08f,.33f,1);}
            if(level>=3)
            {
                foreach(int s in new[]{-1,1})B(p,s*(width*.5f-.05f),1.95f,.53f,.075f,1.98f,.075f,level>=5?Wood:Steel);
                if(level==3||level>=5)
                {
                    B(p,0,1.86f,.43f,width,.06f,.44f,level>=5?Wood:Steel);
                    for(int i=0;i<burners+3;i++)
                    {float x=-width*.44f+i*width*.88f/(burners+2);if(i%3==0)PlateStack(p,x,1.92f,.4f,4);else if(i%3==1){C(p,x,2.0f,.4f,.15f,.19f,.15f,Wood);for(int k=0;k<5;k++)Rod(p,new Vector3(x+(k-2)*.021f,2,.4f),new Vector3(x+(k-2)*.031f,2.25f,.4f),.011f,Dark);}else Plant(p,new Vector3(x,1.91f,.4f),.12f,Cream);}
                }
                if(level>=4)
                {
                    B(p,0,2.98f,.20f,width+.18f,.34f,1.18f,level==8?Dark:Steel);B(p,0,2.79f,.17f,width+.10f,.055f,1.10f,Black);
                    if(level==8){foreach(int s in new[]{-1,1})B(p,0,2.97f,s*.59f+.20f,width+.22f,.35f,.04f,Gold);B(p,0,3.17f,.20f,width+.22f,.04f,1.22f,Gold);}
                    else for(int i=0;i<40;i++)B(p,-width*.46f+i*width*.92f/39,2.85f,-.40f,.025f,.10f,.025f,Black);
                    for(int i=0;i<3;i++){float x=-width*.38f+i*.28f;Rod(p,new Vector3(x,2.78f,.53f),new Vector3(x,1.87f,.53f),.02f,Steel);var scoop=Shape("Hanging ladle",p,new Vector3(x,1.80f,.50f),new Vector3(.14f,.12f,.14f),Steel,bowl);scoop.localRotation=Quaternion.Euler(-70,0,0);}
                }
                var condiments=Group("Prep inserts",p,new Vector3(width*.43f,1.1f,-.06f));
                for(int i=0;i<6;i++){float x=i%2*.15f-.075f,z=i/2*.20f-.2f;B(condiments,x,.035f,z,.14f,.07f,.18f,Steel);for(int j=0;j<4;j++)Small(condiments,new Vector3(x+(j%2-.5f)*.06f,.085f,z+(j/2-.5f)*.07f),new Vector3(.06f,.03f,.06f),i%3==0?Leaf:i%3==1?Orange:Tofu);}
            }
            if(level==8)
            {for(int i=0;i<2;i++){float x=width*.30f+i*.48f;C(p,x,1.60f,.48f,.35f,.85f,.35f,Gold);E(p,x,2.05f,.48f,.38f,.15f,.38f,Gold);C(p,x,2.16f,.48f,.07f,.12f,.07f,Black);Rod(p,new Vector3(x,1.37f,.26f),new Vector3(x,1.37f,.12f),.045f,Gold);}}
        }
    }
}
