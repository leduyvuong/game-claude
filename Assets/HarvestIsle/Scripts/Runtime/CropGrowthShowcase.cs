using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HarvestIsle
{
    public sealed class CropGrowthShowcase : MonoBehaviour
    {
        [Serializable] public sealed class Row
        {public string id,label;public Transform root;public Transform[] stages;}
        [SerializeField] private Row[] rows;
        [SerializeField] private Camera view;
        private int selected=-1;
        private Vector2 listScroll;
        private Vector3 focus;
        private Vector2 previousMouse;
        private bool dragging;
        private int screenWidth,screenHeight;
        public Row[] Rows=>rows;
        public void Configure(Row[] data,Camera camera){rows=data;view=camera;}
        private void Start(){Viewport();Overview();}
        private void Viewport()
        {screenWidth=Screen.width;screenHeight=Screen.height;view.pixelRect=new Rect(225,0,Mathf.Max(1,Screen.width-225),Mathf.Max(1,Screen.height-85));view.ResetAspect();}
        public void Overview()
        {
            foreach(var row in rows)row.root.gameObject.SetActive(true);
            selected=-1;focus=new Vector3(0,0,0);view.transform.position=new Vector3(0,29,-28);view.transform.LookAt(focus);
            view.orthographicSize=Mathf.Max(13.8f,21f/view.aspect);
        }
        public bool Select(int index)
        {
            if(index<0||index>=rows.Length)return false;
            for(int i=0;i<rows.Length;i++)rows[i].root.gameObject.SetActive(i==index);
            selected=index;focus=rows[index].root.position+new Vector3(4.2f,.45f,0);
            view.transform.position=focus+new Vector3(0,7,-9);view.transform.LookAt(focus);view.orthographicSize=Mathf.Max(2.5f,5.9f/view.aspect);return true;
        }
        private void Update()
        {
            if(screenWidth!=Screen.width||screenHeight!=Screen.height){Viewport();if(selected<0)Overview();else Select(selected);}
            var mouse=Mouse.current;if(mouse==null)return;Vector2 p=mouse.position.ReadValue();
            bool over=p.x<225||p.y>Screen.height-80;
            if(mouse.leftButton.wasPressedThisFrame&&!over){dragging=true;previousMouse=p;}
            if(mouse.leftButton.wasReleasedThisFrame)dragging=false;
            if(dragging){Vector2 delta=p-previousMouse;previousMouse=p;float scale=view.orthographicSize*2/Screen.height;Vector3 shift=(-view.transform.right*delta.x-view.transform.up*delta.y)*scale;view.transform.position+=shift;}
            if(!over)view.orthographicSize=Mathf.Clamp(view.orthographicSize-mouse.scroll.ReadValue().y*.006f,1.4f,24);
        }
        private void OnGUI()
        {
            var title=new GUIStyle(GUI.skin.label){fontSize=20,fontStyle=FontStyle.Bold};
            GUI.Box(new Rect(10,10,205,Screen.height-20),"");
            GUI.Label(new Rect(23,20,185,30),"VƯỜN 30 LOẠI CÂY",new GUIStyle(title){fontSize=16});
            if(GUI.Button(new Rect(23,56,179,34),"Xem toàn bộ 150 cây"))Overview();
            GUI.Label(new Rect(23,96,184,43),"Chọn loại cây để xem gần\nKéo để nhìn · Cuộn để zoom");
            listScroll=GUI.BeginScrollView(new Rect(20,145,189,Screen.height-172),listScroll,new Rect(0,0,165,rows.Length*35));
            for(int i=0;i<rows.Length;i++){GUI.backgroundColor=selected==i?new Color(.63f,.86f,.56f):Color.white;if(GUI.Button(new Rect(0,i*35,165,31),(i+1)+". "+rows[i].label))Select(i);}
            GUI.backgroundColor=Color.white;GUI.EndScrollView();
            GUI.Box(new Rect(235,12,Mathf.Max(400,Screen.width-250),65),"");
            GUI.Label(new Rect(250,18,Screen.width-270,28),selected<0?"30 loại · 5 giai đoạn · Từ trái sang phải":rows[selected].label+" · Từ nhỏ đến trưởng thành",title);
            GUI.Label(new Rect(250,48,Screen.width-270,24),"1. Mầm non   →   2. Cây non   →   3. Thân lá   →   4. Hoa / quả non   →   5. Thu hoạch");
        }
    }
}
