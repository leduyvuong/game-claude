using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    /// <summary>Smooth normals for sculpted bodies, flat normals for bills, ears and hooves.</summary>
    internal sealed class AnimalMesh
    {
        private readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
        private readonly List<int>[] triangles;
        internal AnimalMesh(int count){triangles=new List<int>[count];for(int i=0;i<count;i++)triangles[i]=new List<int>();}
        private void Tri(Vector3 a,Vector3 b,Vector3 c,Vector3 na,Vector3 nb,Vector3 nc,int material)
        {
            if(Vector3.Cross(b-a,c-a).sqrMagnitude<0.00000000001f)return;
            int v=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);normals.Add(na);normals.Add(nb);normals.Add(nc);
            triangles[material].Add(v);triangles[material].Add(v+1);triangles[material].Add(v+2);
        }
        internal void Tri(Vector3 a,Vector3 b,Vector3 c,int material,bool doubleSided=false)
        {
            Vector3 n=Vector3.Cross(b-a,c-a).normalized;Tri(a,b,c,n,n,n,material);
            if(doubleSided)Tri(c,b,a,-n,-n,-n,material);
        }
        internal void Ellipsoid(Vector3 center,Vector3 radius,int material,Quaternion rotation=default(Quaternion),
            int sides=16,int rows=10,int patchColor=-1,int patchMode=0,bool flatBottom=false)
        {
            if(Quaternion.Dot(rotation,rotation)<0.000001f)rotation=Quaternion.identity;
            var points=new Vector3[rows+1,sides];var ns=new Vector3[rows+1,sides];var directions=new Vector3[rows+1,sides];
            for(int row=0;row<=rows;row++)for(int i=0;i<sides;i++)
            {
                float y=-Mathf.PI*0.5f+row*Mathf.PI/rows,a=i*Mathf.PI*2/sides;
                Vector3 unit=new Vector3(Mathf.Cos(y)*Mathf.Cos(a),Mathf.Sin(y),Mathf.Cos(y)*Mathf.Sin(a));
                Vector3 point=Vector3.Scale(radius,unit);
                if(flatBottom)point.y=Mathf.Max(-radius.y*0.5f,point.y);
                points[row,i]=center+rotation*point;
                ns[row,i]=rotation*new Vector3(unit.x/radius.x,unit.y/radius.y,unit.z/radius.z).normalized;
                directions[row,i]=unit;
            }
            for(int row=0;row<rows;row++)for(int i=0;i<sides;i++)
            {
                int next=(i+1)%sides;
                if(patchColor>=0&&patchMode>0)
                {
                    PaintTriangle(directions[row,i],directions[row+1,i],directions[row+1,next],center,radius,rotation,material,patchColor,patchMode,3);
                    PaintTriangle(directions[row,i],directions[row+1,next],directions[row,next],center,radius,rotation,material,patchColor,patchMode,3);
                    continue;
                }
                Tri(points[row,i],points[row+1,i],points[row+1,next],ns[row,i],ns[row+1,i],ns[row+1,next],material);
                Tri(points[row,i],points[row+1,next],points[row,next],ns[row,i],ns[row+1,next],ns[row,next],material);
            }
        }
        private static bool Patch(Vector3 p,int mode)
        {
            if(mode==7)
            {
                float ripple=.055f*Mathf.Sin(p.z*14+p.y*8)+.035f*Mathf.Cos(p.x*17-p.z*9);
                return Vector3.Dot(p,new Vector3(1,.35f,-.45f).normalized)>.78f+ripple
                    ||Vector3.Dot(p,new Vector3(-1,.3f,.15f).normalized)>.80f+ripple
                    ||Vector3.Dot(p,new Vector3(.1f,.9f,-.6f).normalized)>.94f+ripple*.4f;
            }
            if(mode==1)return Vector3.Dot(p,new Vector3(1,0.55f,-0.4f).normalized)>0.68f
                ||Vector3.Dot(p,new Vector3(-1,0.28f,0.37f).normalized)>0.74f
                ||Vector3.Dot(p,new Vector3(-0.30f,0.82f,-0.58f).normalized)>0.91f;
            if(mode==2)return p.y>0&&Mathf.Abs(Mathf.Sin(p.z*8+0.7f))<0.28f;
            if(mode==3&&p.y>0.38f&&p.y<0.87f&&p.z>0.20f)
                return Mathf.Abs(p.x)<0.035f||Mathf.Abs(Mathf.Abs(p.x)-(0.24f+0.13f*(0.75f-p.y)))<0.035f;
            if(mode==4)return p.y>-0.05f&&(Vector3.Dot(p,new Vector3(0.25f,0.85f,0.43f).normalized)>0.90f
                ||Vector3.Dot(p,new Vector3(-0.12f,0.88f,-0.42f).normalized)>0.88f);
            if(mode==5)return p.y<-0.22f||(p.z>0.30f&&p.y<0.2f);
            if(mode==6)return p.y<0.10f&&p.z>0.15f||(Mathf.Abs(p.x)>0.50f&&p.y<0.30f&&p.z>0.12f);
            return false;
        }
        private void PaintTriangle(Vector3 a,Vector3 b,Vector3 c,Vector3 center,Vector3 radius,Quaternion rotation,
            int baseColor,int patchColor,int mode,int depth)
        {
            bool pa=Patch(a.normalized,mode),pb=Patch(b.normalized,mode),pc=Patch(c.normalized,mode),middle=Patch((a+b+c).normalized,mode);
            if(depth>0&&(pa!=pb||pb!=pc||pa!=middle))
            {
                // Keep a common face plane at shared edges to avoid cracks along coat markings.
                Vector3 ab=(a+b)*0.5f,bc=(b+c)*0.5f,ca=(c+a)*0.5f;
                PaintTriangle(a,ab,ca,center,radius,rotation,baseColor,patchColor,mode,depth-1);
                PaintTriangle(ab,b,bc,center,radius,rotation,baseColor,patchColor,mode,depth-1);
                PaintTriangle(ca,bc,c,center,radius,rotation,baseColor,patchColor,mode,depth-1);
                PaintTriangle(ab,bc,ca,center,radius,rotation,baseColor,patchColor,mode,depth-1);return;
            }
            Vector3 na=rotation*new Vector3(a.x/radius.x,a.y/radius.y,a.z/radius.z).normalized;
            Vector3 nb=rotation*new Vector3(b.x/radius.x,b.y/radius.y,b.z/radius.z).normalized;
            Vector3 nc=rotation*new Vector3(c.x/radius.x,c.y/radius.y,c.z/radius.z).normalized;
            Tri(center+rotation*Vector3.Scale(radius,a),center+rotation*Vector3.Scale(radius,b),center+rotation*Vector3.Scale(radius,c),
                na,nb,nc,middle?patchColor:baseColor);
        }
        internal void Tube(Vector3[] path,float radius,float tipRadius,int material,int sides=8)
        {
            var points=new Vector3[path.Length,sides];var ns=new Vector3[path.Length,sides];
            Vector3 previousAcross=Vector3.zero;
            for(int i=0;i<path.Length;i++)
            {
                Vector3 tangent=(path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(0,i-1)]).normalized;
                Vector3 a=i==0?Vector3.zero:Vector3.ProjectOnPlane(previousAcross,tangent);
                if(a.sqrMagnitude<0.00001f)a=Vector3.Cross(tangent,Mathf.Abs(tangent.y)<0.9f?Vector3.up:Vector3.right);
                a.Normalize();previousAcross=a;
                Vector3 b=Vector3.Cross(tangent,a);float r=Mathf.Lerp(radius,tipRadius,(float)i/(path.Length-1));
                for(int j=0;j<sides;j++){float angle=j*Mathf.PI*2/sides;ns[i,j]=a*Mathf.Cos(angle)+b*Mathf.Sin(angle);points[i,j]=path[i]+r*ns[i,j];}
            }
            for(int i=0;i<path.Length-1;i++)for(int j=0;j<sides;j++)
            {
                int n=(j+1)%sides;
                Tri(points[i,j],points[i,n],points[i+1,n],ns[i,j],ns[i,n],ns[i+1,n],material);
                Tri(points[i,j],points[i+1,n],points[i+1,j],ns[i,j],ns[i+1,n],ns[i+1,j],material);
            }
            for(int j=0;j<sides;j++){int n=(j+1)%sides;Tri(path[0],points[0,n],points[0,j],material);int last=path.Length-1;Tri(path[last],points[last,j],points[last,n],material);}
        }
        internal void Shoe(Vector3 center,Vector3 size,int material)
        {
            const int sides=12;var bottom=new Vector3[sides];var top=new Vector3[sides];
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides;
                bottom[i]=center+new Vector3(Mathf.Cos(a)*size.x,-size.y,Mathf.Sin(a)*size.z);
                top[i]=center+new Vector3(Mathf.Cos(a)*size.x*0.87f,size.y*0.70f,Mathf.Sin(a)*size.z*0.87f);
            }
            for(int i=0;i<sides;i++)
            {
                int n=(i+1)%sides;Tri(bottom[i],top[i],top[n],material);Tri(bottom[i],top[n],bottom[n],material);
                Tri(center+Vector3.up*size.y,top[n],top[i],material);Tri(center-Vector3.up*size.y,bottom[i],bottom[n],material);
            }
        }
        internal Mesh Finish(string name)
        {
            foreach(Vector3 v in vertices)if(float.IsNaN(v.x)||float.IsInfinity(v.x)||float.IsNaN(v.y)||float.IsInfinity(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.z))throw new InvalidOperationException("Invalid animal vertex: "+name);
            var mesh=new Mesh{name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.subMeshCount=triangles.Length;
            for(int i=0;i<triangles.Length;i++)mesh.SetTriangles(triangles[i],i);mesh.RecalculateBounds();return mesh;
        }
    }
}
