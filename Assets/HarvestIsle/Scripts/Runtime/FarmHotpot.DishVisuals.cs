using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private void SyncSpecialtyVisual(Table table)
        {
            if(table.pot==null)return;
            if(!table.served){if(table.specialtyVisual!=null)table.specialtyVisual.SetActive(false);return;}
            var recipe=game.Model.State.hotpot.recipes.Find(r=>r.id==table.recipeId);
            int dish=recipe==null?-1:HotpotRules.Specialty(recipe);
            var order=Service==null?null:Service.Find(table.floor*26+table.slot);
            if(order!=null) dish=HotpotRules.Specialty(order.recipe);
            // A shared pot for a party is shown using the baked divided pot; flavors are still scored per guest.
            bool party=dish<0&&order!=null&&order.party.Count>1;
            int visualId=party?12:dish;
            if(visualId<0){if(table.specialtyVisual!=null)table.specialtyVisual.SetActive(false);return;}
            if(table.specialtyVisual==null||table.specialtyKind!=visualId)
            {
                if(table.specialtyVisual!=null){table.specialtyVisual.SetActive(false);Destroy(table.specialtyVisual);}
                var renderers=table.pot.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0)return;
                Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
                int index=party?3:HotpotKitchenCatalog.DishStart+dish;
                var prefab=Resources.Load<GameObject>(HotpotKitchenCatalog.Items[index].Resource);if(prefab==null)return;
                var root=Instantiate(prefab,table.pot.parent,false);root.name=party?"Shared divided hotpot":"Discovered regional hotpot";
                foreach(var collider in root.GetComponentsInChildren<Collider>(true))collider.enabled=false;
                root.transform.position=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                var images=root.GetComponentsInChildren<Renderer>();
                if(images.Length>0)
                {
                    Bounds actual=images[0].bounds;foreach(var renderer in images)actual.Encapsulate(renderer.bounds);
                    root.transform.localScale*=.85f/Mathf.Max(.1f,Mathf.Max(actual.size.x,actual.size.z));
                }
                table.specialtyVisual=root;table.specialtyKind=visualId;
            }
            table.pot.gameObject.SetActive(false);table.specialtyVisual.SetActive(true);
        }
    }
}
