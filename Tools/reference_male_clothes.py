"""Fitted, skinned cotton T-shirt and trousers for the isolated male study.

Cut garment panels from the anatomical surface, add ease, relax the panels,
and finish every opening with a separate folded trim. Weights follow the same
surface interpolation as the garment; covered body triangles are omitted.
"""
from collections import defaultdict
import numpy as np


def build_clothes(vertices, faces, weights, bones, female=False):
    normals = np.zeros_like(vertices)
    for face in faces:
        n = np.cross(vertices[face[1]]-vertices[face[0]], vertices[face[2]]-vertices[face[0]])
        for i in face: normals[i] += n
    normals /= np.maximum(np.linalg.norm(normals, axis=1)[:, None], 1e-12)
    shoulder = next(b['head'] for b in bones if b['name'] == 'LeftUpperArm')
    elbow = next(b['head'] for b in bones if b['name'] == 'LeftForearm')
    axis = (elbow-shoulder)/np.linalg.norm(elbow-shoulder)

    def shirt_dist(p):
        mirrored = p.copy(); mirrored[0] = abs(mirrored[0])
        sleeve = max(.205-abs(p[0]), .155-np.dot(mirrored-shoulder, axis))
        neck = 1.522-.022*np.clip((p[2]-.02)/.10, 0, 1)
        return np.array([p[1]-1.005, neck-p[1], sleeve])

    def pants_dist(p): return np.array([p[1]-.145, 1.045-p[1], .285-abs(p[0])])

    garments = []
    for name, trim, distance, ease, relax in [('Shirt', 'ShirtTrim', shirt_dist, .028, 10),
                                               ('Trousers', 'TrouserTrim', pants_dist, .018, 14)]:
        out_vertices = []; out_weights = []; out_normals = []; out_faces = []; lookup = {}
        distances = np.array([distance(p) for p in vertices])
        for face in faces:
            if np.any(np.max(distances[face], axis=0) < 0): continue
            polygon = [(vertices[i], weights[i], normals[i], distances[i]) for i in face]
            for plane in range(distances.shape[1]):
                clipped = []
                for j, a in enumerate(polygon):
                    b = polygon[(j+1) % len(polygon)]; da, db = a[3][plane], b[3][plane]
                    if da >= 0: clipped.append(a)
                    if (da >= 0) != (db >= 0):
                        t = da/(da-db)
                        clipped.append(tuple(x+(y-x)*t for x, y in zip(a, b)))
                polygon = clipped
                if not polygon: break
            if len(polygon) < 3: continue
            indices = []
            for point, w, normal, _ in polygon:
                key = tuple(np.round(point, 7))
                if key not in lookup:
                    lookup[key] = len(out_vertices)
                    offset = normal * ease; offset[1] *= .25
                    out_vertices.append(point+offset); out_weights.append(w); out_normals.append(normal)
                indices.append(lookup[key])
            if len(set(indices)) >= 3: out_faces.append(indices)
        v = np.array(out_vertices); w = np.array(out_weights)
        neighbors = defaultdict(set); edges = defaultdict(int); edge_direction = {}
        for face in out_faces:
            for i, a in enumerate(face):
                b = face[(i+1) % len(face)]
                if a == b: continue
                edges[tuple(sorted((a, b)))] += 1
                edge_direction[tuple(sorted((a, b)))] = (a, b)
                neighbors[a].add(b); neighbors[b].add(a)
        border = {i for edge, count in edges.items() if count == 1 for i in edge}
        # Relax small anatomical creases, while preserving the cut openings.
        for _ in range(relax):
            before = v.copy()
            for i, adjacent in neighbors.items():
                if i not in border:
                    delta = before[list(adjacent)].mean(axis=0)-before[i]
                    v[i] += delta * np.array([.36, .06, .36])
        if female and name == 'Shirt':
            # Smooth only small chest-surface details. Keep shoulders, sleeves,
            # waist and all openings at their fitted size.
            from scipy.sparse import csr_matrix
            rows=[];cols=[];values=[]
            for i, adjacent in neighbors.items():
                if i in border: rows.append(i);cols.append(i);values.append(1)
                else:
                    for j in adjacent: rows.append(i);cols.append(j);values.append(1/len(adjacent))
            average=csr_matrix((values,(rows,cols)),shape=(len(v),len(v)))
            strength=np.exp(-((abs(v[:,0])-.085)/.075)**2-((v[:,1]-1.32)/.085)**2)
            strength*=np.clip((v[:,2]-.06)/.05,0,1)
            for i in border: strength[i]=0
            for _ in range(60): v += .35*strength[:,None]*(average@v-v)
        garments.append((name, v, out_faces, None, None, w))
        # Folded hem/collar/cuff: outer lip, inset rim and an inner return.
        tv = []; tw = []; tf = []; trim_ids = {}; border_neighbors = defaultdict(list)
        for (a, b), count in edges.items():
            if count == 1: border_neighbors[a].append(b); border_neighbors[b].append(a)
        for index in sorted(border):
            point = v[index]; normal = out_normals[index]
            normal = normal / max(np.linalg.norm(normal), 1e-10)
            interior = [i for i in neighbors[index] if i not in border]
            inward = v[interior or list(neighbors[index])].mean(axis=0)-point
            inward -= normal*np.dot(inward,normal)
            adjacent = border_neighbors[index]
            if len(adjacent) == 2:
                tangent = v[adjacent[0]]-v[adjacent[1]]
                tangent /= max(np.linalg.norm(tangent),1e-10)
                inward -= tangent*np.dot(inward,tangent)
            inward /= max(np.linalg.norm(inward),1e-10)
            trim_ids[index] = len(tv)
            tv.extend([point+normal*.0015, point+inward*.012+normal*.002, point-normal*.003])
            tw.extend([w[index]]*3)
        for (a, b), count in edges.items():
            if count != 1: continue
            a,b = edge_direction[(a,b)]
            n,m=trim_ids[a],trim_ids[b]
            tf.extend([[n, m, m+1, n+1], [n+2, m+2, m, n]])
        garments.append((trim, np.array(tv), tf, None, None, np.array(tw)))
    # Keep the original boundary faces under the openings to avoid cracks.
    body_faces = [face for face in faces if not (
        all(np.min(shirt_dist(vertices[i])) > .004 for i in face) or
        all(np.min(pants_dist(vertices[i])) > .004 for i in face))]
    report = {'style': 'short-sleeve cotton T-shirt and tapered full-length trousers',
              'garmentMeshes': len(garments), 'garmentTriangles': sum(sum(len(f)-2 for f in g[2]) for g in garments),
              'hiddenBodyTriangles': 2*(len(faces)-len(body_faces)), 'skinWeights': 'interpolated from body; same 49-bone rig'}
    return body_faces, garments, report
