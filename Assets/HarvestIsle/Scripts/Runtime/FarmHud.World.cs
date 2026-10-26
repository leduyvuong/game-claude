using System;
using System.Collections.Generic;
using HarvestIsle.Core;
using UnityEngine;
using UnityEngine.UI;
namespace HarvestIsle
{
    public sealed partial class FarmHud
    {
        private sealed class WorldBadge{public RectTransform rect;public Text label;public RawImage icon;public Vector3 position;public int kind;public bool visible;}
        private readonly List<WorldBadge> worldBadges=new List<WorldBadge>();
        private static readonly Vector3[] PenPoints={new Vector3(-10.5f,1.7f,-1.5f),new Vector3(9.5f,1.7f,-.7f),new Vector3(-10.5f,1.7f,1.6f),new Vector3(9.5f,1.7f,-3.4f),new Vector3(-10.5f,1.7f,-4.4f),new Vector3(9.5f,1.7f,2.1f)};
        private void BuildWorldBadges()
        {
            for(int i=0;i<DevelopmentCatalog.Buildings.Length+7;i++)
            {
                int key=i;var button=Button("",safeArea,new Vector2(.5f,.5f),Vector2.zero,new Vector2(128,72),paper,ink,()=>ActivateBadge(key),out var label);
                var badge=new WorldBadge{rect=button.GetComponent<RectTransform>(),label=label,kind=i};badge.rect.name="World status "+i;
                badge.icon=Picture(badge.rect,"Basket",new Vector2(48,-3),new Vector2(32,32));MoveRect(label,3,-37,122,31);label.fontSize=13;
                if(i<9)badge.position=new Vector3((i%5-2)*2.8f,2,-6.6f-i/5*3);
                else if(i<15)badge.position=PenPoints[i-9];else badge.position=new Vector3(3.8f,1.5f,2.4f);
                if(game.IsEstate)badge.position=i<9?FarmEstate.BuildingPosition(i)+Vector3.up*4:i<15?FarmEstate.PenPosition(i-9)+Vector3.up*2.8f:new Vector3(8,2,8);
                badge.rect.SetAsFirstSibling();worldBadges.Add(badge);
            }
        }
        private void ActivateBadge(int key)
        {
            ClosePanels();
            if(key<9)game.Development.OpenBuilding(DevelopmentCatalog.Buildings[key].id);
            else if(key<15)game.Ranch.OpenPen(key-9);
            else OpenOrders();
        }
        private void RefreshWorldBadges()
        {
            foreach(var b in worldBadges)
            {
                int i=b.kind;b.visible=true;
                if(i<9)
                {
                    var spec=DevelopmentCatalog.Buildings[i];b.visible=game.Model.BuildingLevel(spec.id)>0;
                    var job=game.Model.State.development.jobs.Find(j=>DevelopmentCatalog.Recipe(j.recipe).building==spec.id);
                    b.icon.texture=PlayImage(job==null?"Basket":ProductPicture(job.recipe));
                    b.label.text=job==null?spec.name:job.readyAt<=game.Now?"Thu thành phẩm":"Còn "+Math.Ceiling(job.readyAt-game.Now)+"s";
                }
                else if(i<15)
                {
                    var spec=LivestockCatalog.All[i-9];var state=game.Model.Animal(spec.id);b.visible=state.count>0;
                    double left=game.Model.AnimalRemaining(spec.id,game.Now);b.icon.texture=PlayImage(state.producing&&left<=0?ProductPicture(spec.id=="cow"?"milk":spec.id=="bee"?"honey":spec.id=="fish"?"fish":"eggs"):"FeedBag");
                    b.label.text=!state.producing?"Cho ăn · "+spec.name:left<=0?"Thu "+spec.product:Math.Ceiling(left)+"s";
                }
                else {b.icon.texture=PlayImage(ProductPicture(game.Model.OrderItem));b.label.text="Đơn hàng";}
            }
        }
        private void PositionWorldBadges()
        {
            foreach(var b in worldBadges)
            {
                var point=game.World.GameCamera.WorldToScreenPoint(b.position);
                Rect bounds=Screen.safeArea;bounds.xMin+=55;bounds.xMax-=55;bounds.yMin+=100;bounds.yMax-=105;
                bool show=b.visible&&!IsReading&&!detailPanel.activeSelf&&game.World.GameCamera.orthographicSize<(game.IsEstate?8:12)&&point.z>0&&bounds.Contains(point);
                b.rect.gameObject.SetActive(show);if(!show)continue;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(safeArea,point,null,out var local);b.rect.anchoredPosition=local+new Vector2(-safeArea.rect.width/2-64,safeArea.rect.height/2);
            }
        }
    }
}
