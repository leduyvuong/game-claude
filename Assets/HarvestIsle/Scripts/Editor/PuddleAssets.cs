using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HarvestIsle.Editor
{
    internal static class PuddleAssets
    {
        internal static void Rebuild()
        {
            const string art="Assets/HarvestIsle/Art/Environment/";
            const string folder="Assets/HarvestIsle/Resources/Effects/Puddles/";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var shader=Shader.Find("Harvest Isle/Farm Puddle");
            if(shader==null)throw new InvalidOperationException("Puddle shader missing");
            string materialPath=art+"FarmPuddle.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,materialPath);}
            material.shader=shader;EditorUtility.SetDirty(material);
            for(int variant=0;variant<4;variant++)
            {
                var generated=BuildMesh(variant);
                string meshPath=art+"FarmPuddle"+variant+".asset";
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if(mesh==null){mesh=generated;AssetDatabase.CreateAsset(mesh,meshPath);}
                else{EditorUtility.CopySerialized(generated,mesh);UnityEngine.Object.DestroyImmediate(generated);EditorUtility.SetDirty(mesh);}
                var root=new GameObject("Standing water "+variant);
                try
                {
                    root.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=root.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
                    renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                    renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
                    PrefabUtility.SaveAsPrefabAsset(root,folder+"Puddle"+variant+".prefab");
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
            }
            AssetDatabase.SaveAssets();
        }

        private static Mesh BuildMesh(int variant)
        {
            const int segments=64,rings=4;
            var vertices=new Vector3[1+segments*rings];
            var colors=new Color[vertices.Length];var uv=new Vector2[vertices.Length];
            var triangles=new int[segments*3+(rings-1)*segments*6];
            colors[0]=Color.white;uv[0]=Vector2.one*.5f;
            float phase=variant*1.71f;
            float[] radii={.45f,.76f,.90f,1f};
            for(int ring=0;ring<rings;ring++)for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2/segments;
                float edge=1+.11f*Mathf.Sin(a*3+phase)+.07f*Mathf.Cos(a*5-phase)+.035f*Mathf.Sin(a*9+phase);
                int index=1+ring*segments+i;
                vertices[index]=new Vector3(Mathf.Cos(a)*.405f*edge*radii[ring],0,Mathf.Sin(a)*.365f*edge*radii[ring]);
                uv[index]=new Vector2(vertices[index].x+.5f,vertices[index].z+.5f);
                colors[index]=new Color(1-radii[ring]*.6f,1,1,ring==rings-1?0:1);
            }
            int cursor=0;
            for(int i=0;i<segments;i++)
            {
                int next=(i+1)%segments;
                triangles[cursor++]=0;triangles[cursor++]=1+next;triangles[cursor++]=1+i;
                for(int ring=1;ring<rings;ring++)
                {
                    int inner=1+(ring-1)*segments,outer=inner+segments;
                    triangles[cursor++]=inner+i;triangles[cursor++]=inner+next;triangles[cursor++]=outer+next;
                    triangles[cursor++]=inner+i;triangles[cursor++]=outer+next;triangles[cursor++]=outer+i;
                }
            }
            var mesh=new Mesh{name="Farm puddle "+variant};
            mesh.vertices=vertices;mesh.colors=colors;mesh.uv=uv;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}
