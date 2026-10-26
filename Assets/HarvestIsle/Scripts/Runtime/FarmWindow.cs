using UnityEngine;

namespace HarvestIsle
{
    // Shared screen-space frame for farm dialogs (IMGUI coordinates, top-left).
    public static class FarmWindow
    {
        public static Rect Frame(float width,float height)
        {return Frame(Screen.safeArea,Screen.height,width,height);}
        public static Rect Frame(Rect safe,float screenHeight,float width,float height)
        {
            float scale=Mathf.Min(1.2f,Mathf.Min(safe.width*.82f/width,safe.height*.82f/height));
            return new Rect(safe.x+(safe.width-width*scale)/2,screenHeight-safe.yMax+(safe.height-height*scale)/2,width*scale,height*scale);
        }
        public static Matrix4x4 Matrix(Rect frame,float width)
        {return Matrix4x4.TRS(new Vector3(frame.x,frame.y,0),Quaternion.identity,Vector3.one*(frame.width/width));}
        public static Rect Viewport(float width,float height)
        {
            Rect frame=Frame(width,height);
            return new Rect(frame.x/Screen.width,1-frame.yMax/Screen.height,frame.width/Screen.width,frame.height/Screen.height);
        }
        public static bool DismissRequested(Rect frame)
        {
            var e=Event.current;
            if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape||e.type==EventType.MouseDown&&e.button==0&&!frame.Contains(e.mousePosition))
            {e.Use();return true;}
            return false;
        }
        public static void Draw(Rect frame,bool fill=true)
        {
            Color previous=GUI.color;
            GUI.color=new Color(.13f,.17f,.10f,.18f);
            // Only dim the outside: the restaurant camera remains clear inside.
            GUI.DrawTexture(new Rect(0,0,Screen.width,frame.y),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0,frame.yMax,Screen.width,Screen.height-frame.yMax),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0,frame.y,frame.x,frame.height),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(frame.xMax,frame.y,Screen.width-frame.xMax,frame.height),Texture2D.whiteTexture);
            GUI.color=new Color(.21f,.16f,.10f,.65f);
            GUI.DrawTexture(new Rect(frame.x-3,frame.y-3,frame.width+6,3),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(frame.x-3,frame.yMax,frame.width+6,5),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(frame.x-3,frame.y,3,frame.height),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(frame.xMax,frame.y,3,frame.height),Texture2D.whiteTexture);
            if(fill){GUI.color=FarmWorld.Hex("F5EBD4");GUI.DrawTexture(frame,Texture2D.whiteTexture);}
            GUI.color=previous;
        }
    }
}
