using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    internal sealed class EnvironmentMesh
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<int>[] triangles;
        internal EnvironmentMesh(int materials)
        {
            triangles = new List<int>[materials];
            for (int i = 0; i < materials; i++) triangles[i] = new List<int>();
        }
        internal void Tri(Vector3 a, Vector3 b, Vector3 c, int material, Color ca, Color cb, Color cc)
        {
            if (Vector3.Cross(b - a, c - a).sqrMagnitude < 0.0000000001f) return;
            int start = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); colors.Add(ca); colors.Add(cb); colors.Add(cc);
            triangles[material].Add(start); triangles[material].Add(start + 1); triangles[material].Add(start + 2);
        }
        internal void Tri(Vector3 a, Vector3 b, Vector3 c, int material)
        {
            Tri(a,b,c,material,Color.white,Color.white,Color.white);
        }
        internal void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int material)
        {
            Tri(a,b,c,material); Tri(a,c,d,material);
        }
        internal void Cube(Vector3 center, Vector3 size, int top, int side, bool includeTop=true)
        {
            Vector3 h = size * 0.5f;
            Vector3 a=center+new Vector3(-h.x,h.y,-h.z), b=center+new Vector3(-h.x,h.y,h.z),
                c=center+new Vector3(h.x,h.y,h.z), d=center+new Vector3(h.x,h.y,-h.z);
            if(includeTop)Quad(a,b,c,d,top);
            Vector3 drop=Vector3.down*size.y;
            Quad(b,a,a+drop,b+drop,side); Quad(c,b,b+drop,c+drop,side);
            Quad(d,c,c+drop,d+drop,side); Quad(a,d,d+drop,a+drop,side);
            Quad(a+drop,d+drop,c+drop,b+drop,side);
        }
        internal void Tube(Vector3[] path, float radius, float tipRadius, int material, int sides=6)
        {
            var rings=new Vector3[path.Length,sides];
            for(int i=0;i<path.Length;i++)
            {
                Vector3 tangent=(path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(0,i-1)]).normalized;
                Vector3 across=Vector3.Cross(tangent,Mathf.Abs(tangent.y)<0.9f?Vector3.up:Vector3.right).normalized;
                Vector3 up=Vector3.Cross(tangent,across);
                float radiusHere=Mathf.Lerp(radius,tipRadius,(float)i/(path.Length-1));
                for(int j=0;j<sides;j++)
                {
                    float angle=j*Mathf.PI*2/sides;
                    rings[i,j]=path[i]+radiusHere*(across*Mathf.Cos(angle)+up*Mathf.Sin(angle));
                }
            }
            for(int i=0;i<path.Length-1;i++) for(int j=0;j<sides;j++)
                Quad(rings[i,j],rings[i,(j+1)%sides],rings[i+1,(j+1)%sides],rings[i+1,j],material);
            for(int j=0;j<sides;j++)
            {
                Tri(path[0],rings[0,(j+1)%sides],rings[0,j],material);
                int last=path.Length-1; Tri(path[last],rings[last,j],rings[last,(j+1)%sides],material);
            }
        }
        internal void Disc(Vector3 center, float rx, float rz, int material, int sides=20)
        {
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                Tri(center,center+new Vector3(Mathf.Cos(b)*rx,0,Mathf.Sin(b)*rz),
                    center+new Vector3(Mathf.Cos(a)*rx,0,Mathf.Sin(a)*rz),material);
            }
        }
        internal void Leaf(Vector3 start,Vector3 direction,float length,float width,int material)
        {
            direction.y=0; direction.Normalize();
            Vector3 side=Vector3.Cross(Vector3.up,direction);
            Vector3 end=start+direction*length+Vector3.up*length*0.30f;
            Vector3 mid=Vector3.Lerp(start,end,0.5f)+Vector3.up*length*0.16f;
            Vector3 left=mid-side*width,right=mid+side*width,ridge=mid+Vector3.up*width*0.2f;
            Tri(start,left,ridge,material); Tri(left,end,ridge,material);
            Tri(start,ridge,right,material); Tri(ridge,end,right,material);
            Tri(ridge,left,start,material); Tri(ridge,end,left,material);
            Tri(right,ridge,start,material); Tri(right,end,ridge,material);
        }
        internal void Ring(Vector3[] inside,Vector3[] outside,int material,bool faceUp=true)
        {
            for(int i=0;i<inside.Length;i++)
            {
                int next=(i+1)%inside.Length;
                if(faceUp) Quad(inside[i],inside[next],outside[next],outside[i],material);
                else Quad(inside[i],outside[i],outside[next],inside[next],material);
            }
        }
        internal void Grid(float width,float depth,int divisions,Func<float,float,float> height,
            Func<float,float,int> material)
        {
            for(int x=0;x<divisions;x++) for(int z=0;z<divisions;z++)
            {
                float x0=-width*0.5f+width*x/divisions,x1=-width*0.5f+width*(x+1)/divisions;
                float z0=-depth*0.5f+depth*z/divisions,z1=-depth*0.5f+depth*(z+1)/divisions;
                Quad(new Vector3(x0,height(x0,z0),z0),new Vector3(x0,height(x0,z1),z1),
                    new Vector3(x1,height(x1,z1),z1),new Vector3(x1,height(x1,z0),z0),material((x0+x1)*0.5f,(z0+z1)*0.5f));
            }
        }
        private static int SmoothGroup(Vector3 normal)
        {
            if(normal.y>.1f)return 1;
            if(normal.y<-.1f)return -1;
            return normal.x>0||(normal.x==0&&normal.z>0)?2:-2;
        }
        internal Mesh Finish(string name,bool smooth=false)
        {
            foreach(Vector3 v in vertices)
                if(float.IsNaN(v.x)||float.IsInfinity(v.x)||float.IsNaN(v.y)||float.IsInfinity(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.z))
                    throw new InvalidOperationException("Invalid environment vertex: "+name);
            var mesh=new Mesh {name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.subMeshCount=triangles.Length;
            for(int i=0;i<triangles.Length;i++)mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            if(smooth)
            {
                var sums=new Dictionary<(Vector3,int),Vector3>();var normals=mesh.normals;
                for(int i=0;i<vertices.Count;i++)
                {var key=(vertices[i],SmoothGroup(normals[i]));if(!sums.ContainsKey(key))sums[key]=Vector3.zero;sums[key]+=normals[i];}
                for(int i=0;i<vertices.Count;i++)normals[i]=sums[(vertices[i],SmoothGroup(normals[i]))].normalized;
                mesh.normals=normals;
            }
            if(mesh.vertexCount==0)throw new InvalidOperationException("Empty environment mesh: "+name);
            return mesh;
        }
    }
}
