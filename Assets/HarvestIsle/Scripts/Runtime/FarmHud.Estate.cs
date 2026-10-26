using System;
using UnityEngine;
using UnityEngine.UI;

namespace HarvestIsle
{
    public sealed partial class FarmHud
    {
        private readonly Image[] estateTabs=new Image[5];
        public void SetEstateZone(int zone)
        {for(int i=0;i<estateTabs.Length;i++)if(estateTabs[i]!=null)estateTabs[i].color=i==zone?gold:paper;}
        public GameObject AddEstateNavigation(Action<int> show)
        {
            var panel=Panel("An Nhien navigation",safeArea,new Vector2(.5f,1),new Vector2(-360,-90),new Vector2(720,52),paper);
            Label("AN NHIÊN",panel,new Vector2(14,-8),new Vector2(140,36),18,ink,FontStyle.Bold);
            string[] names={"Toàn trại","Ruộng","Chuồng","Xưởng","Vườn"};
            for(int i=0;i<names.Length;i++)
            {
                int key=i;
                estateTabs[i]=Button(names[i],panel,new Vector2(0,1),new Vector2(154+i*112,-6),new Vector2(106,40),i==0?gold:paper,ink,()=>show(key),out _).GetComponent<Image>();
            }
            return panel.gameObject;
        }
    }
}
