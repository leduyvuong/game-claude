using System.Collections.Generic;
using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static class FittedHumanHair
    {
        private struct Point
        {
            public Vector3 p,n;
            public Point(Vector3 p,Vector3 n){this.p=p;this.n=n;}
        }
        public static void Build(ReferenceMaleMotion motion,bool female,List<Mesh> meshes,List<Material> materials)
        {
            var body=motion.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            var head=System.Array.Find(motion.bones,t=>t.name=="Head");
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Fitted hair"};
            material.color=FarmWorld.Hex(female?"352820":"443027");
            material.SetFloat("_Smoothness",.25f);materials.Add(material);
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();
            var source=body.vertices;var sourceNormals=body.normals;var indices=body.triangles;
            for(int i=0;i<indices.Length;i+=3)
            {
                if(source[indices[i]].y<1.61f&&source[indices[i+1]].y<1.61f&&source[indices[i+2]].y<1.61f)continue;
                var polygon=new List<Point>();
                for(int j=0;j<3;j++)
                {
                    var a=new Point(source[indices[i+j]],sourceNormals[indices[i+j]]);
                    var b=new Point(source[indices[i+(j+1)%3]],sourceNormals[indices[i+(j+1)%3]]);
                    float da=Distance(a.p,female),db=Distance(b.p,female);
                    if(da>=0)polygon.Add(a);
                    if((da>=0)!=(db>=0))
                    {float t=da/(da-db);polygon.Add(new Point(Vector3.Lerp(a.p,b.p,t),Vector3.Lerp(a.n,b.n,t).normalized));}
                }
                int start=vertices.Count;
                foreach(var point in polygon)
                {
                    float edge=Mathf.SmoothStep(0,1,Mathf.Clamp01(Distance(point.p,female)/.022f));
                    float volume=(female?.007f:.009f)*edge;
                    float wave=.0013f*Mathf.Sin(point.p.x*100+point.p.z*35)*edge;
                    Vector3 p=point.p+point.n*(.002f+volume+wave);
                    vertices.Add(head.InverseTransformPoint(motion.transform.TransformPoint(p)));
                    var gradient=new Vector3(.13f,0,.0455f)*Mathf.Cos(point.p.x*100+point.p.z*35)*edge;
                    Vector3 hairNormal=(point.n-Vector3.ProjectOnPlane(gradient,point.n)).normalized;
                    normals.Add(head.InverseTransformDirection(motion.transform.TransformDirection(hairNormal)));
                }
                for(int j=1;j<polygon.Count-1;j++){triangles.Add(start);triangles.Add(start+j);triangles.Add(start+j+1);}
            }
            Add("Fitted scalp hair",head,vertices,normals,triangles,material,meshes);
            if(female) Bun(motion,head,material,meshes);
        }
        private static float Distance(Vector3 p,bool female)
        {
            float angle=Mathf.Atan2(p.x,p.z-.055f),front=Mathf.Cos(angle);
            float line=front>=0?Mathf.Lerp(1.710f,female?1.777f:1.765f,Mathf.Pow(front,1.35f)):Mathf.Lerp(1.710f,female?1.650f:1.640f,-front);
            if(front>0)line+=(female?.004f:.009f)*Mathf.Sin(angle*3)*front;
            // Tapered sideburn in front of the ear; the ear itself stays uncovered.
            float temple=(Mathf.Abs(angle)-1.10f)/.13f;
            line-=.018f*Mathf.Exp(-temple*temple);
            // The source includes the ears: exclude the upper helix rather than
            // turning it into detached dark lumps at the sides of the scalp.
            float ear=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.077f,.091f,Mathf.Abs(p.x)));
            line=Mathf.Lerp(line,Mathf.Max(line,1.742f),ear);
            return p.y-line;
        }
        private static void Bun(ReferenceMaleMotion motion,Transform head,Material material,List<Mesh> meshes)
        {
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();
            const int rows=24,columns=48;
            for(int row=0;row<=rows;row++)for(int column=0;column<=columns;column++)
            {
                float latitude=-Mathf.PI/2+row*Mathf.PI/rows,angle=column*Mathf.PI*2/columns;
                var n=new Vector3(Mathf.Cos(latitude)*Mathf.Cos(angle),Mathf.Sin(latitude),Mathf.Cos(latitude)*Mathf.Sin(angle));
                float fold=1+.035f*Mathf.Cos(angle*7+latitude*3)*Mathf.Cos(latitude);
                Vector3 p=new Vector3(0,1.661f,-.072f)+Vector3.Scale(n,new Vector3(.045f,.038f,.040f))*fold;
                vertices.Add(head.InverseTransformPoint(motion.transform.TransformPoint(p)));
                normals.Add(head.InverseTransformDirection(motion.transform.TransformDirection(n)));
                if(row<rows&&column<columns)
                {
                    int a=row*(columns+1)+column,b=a+columns+1;
                    triangles.Add(a);triangles.Add(b);triangles.Add(a+1);triangles.Add(a+1);triangles.Add(b);triangles.Add(b+1);
                }
            }
            Add("Low tied bun",head,vertices,normals,triangles,material,meshes);
        }
        private static void Add(string name,Transform head,List<Vector3> vertices,List<Vector3> normals,List<int> triangles,Material material,List<Mesh> meshes)
        {
            var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();meshes.Add(mesh);
            var item=new GameObject(name);item.transform.SetParent(head,false);
            item.AddComponent<MeshFilter>().sharedMesh=mesh;item.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
}
