using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HarvestIsle
{
    public sealed partial class FarmHud
    {
        private GameObject windowBackdrop;
        private void PositionReadingWindow()
        {
            GameObject active=inventoryPanel!=null&&inventoryPanel.activeSelf?inventoryPanel:orderPanel!=null&&orderPanel.activeSelf?orderPanel:null;
            if(active==null){if(windowBackdrop!=null)windowBackdrop.SetActive(false);return;}
            if(windowBackdrop==null)
            {
                windowBackdrop=new GameObject("Window backdrop",typeof(RectTransform),typeof(Image),typeof(Button));
                var rect=windowBackdrop.GetComponent<RectTransform>();rect.SetParent(safeArea,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                var shade=windowBackdrop.GetComponent<Image>();shade.color=new Color(.13f,.17f,.10f,.18f);
                var dismiss=windowBackdrop.GetComponent<Button>();dismiss.targetGraphic=shade;dismiss.transition=Selectable.Transition.None;dismiss.onClick.AddListener(()=>{ClosePanels();Refresh();});
            }
            windowBackdrop.SetActive(true);windowBackdrop.transform.SetAsLastSibling();active.transform.SetAsLastSibling();
            var panel=active.GetComponent<RectTransform>();Vector2 design=active==inventoryPanel?new Vector2(740,530):new Vector2(860,460);
            float scale=Mathf.Min(1,Mathf.Min(safeArea.rect.width*.82f/design.x,safeArea.rect.height*.82f/design.y));
            panel.anchorMin=panel.anchorMax=new Vector2(.5f,.5f);panel.localScale=Vector3.one*scale;
            panel.anchoredPosition=new Vector2(-design.x*scale/2,design.y*scale/2);
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){ClosePanels();Refresh();}
        }
    }
}
