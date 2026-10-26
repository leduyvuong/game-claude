using UnityEngine;

namespace HarvestIsle.Editor
{
    internal static partial class StructureModelBuilder
    {
        internal static Mesh DetailedShopRoof()
        {
            var m=new EnvironmentMesh(Colors.Length);
            Roof(m,2.20f,3.34f,2.09f,.64f,true);
            var mesh=m.Finish("Cafe laid clay tiles");
            var vertices=mesh.vertices;Quaternion turn=Quaternion.Euler(0,90,0);
            for(int i=0;i<vertices.Length;i++)vertices[i]=turn*vertices[i]+Vector3.forward*.12f;
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}
