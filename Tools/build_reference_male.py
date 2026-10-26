"""Reference study v2 from MakeHuman CC0 anatomical quad topology.

Pinned data and license: Design/References/MakeHuman. Requires numpy.
Only the isolated experiment is rebuilt, never the game character catalog.
"""
from pathlib import Path
from collections import defaultdict
import sys, json, hashlib, shutil
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / '.tools/python'))
import numpy as np
SOURCE = ROOT / 'Design/References/MakeHuman'
DATA = SOURCE / 'makehuman/data'
FEMALE = globals().get('FEMALE', False)
ASSET = 'ReferenceFemale' if FEMALE else 'ReferenceMale'
OUT = ROOT / 'Assets/HarvestIsle/Experiments' / ASSET
OUT.mkdir(parents=True, exist_ok=True)


def read_obj(path):
    vertices, uvs, groups = [], [], defaultdict(list)
    group = 'default'
    for line in path.read_text(encoding='utf8').splitlines():
        values = line.split()
        if not values: continue
        if values[0] == 'v': vertices.append(list(map(float, values[1:4])))
        elif values[0] == 'vt': uvs.append(list(map(float, values[1:3])))
        elif values[0] == 'g': group = values[1]
        elif values[0] == 'f':
            ids = [v.split('/') for v in values[1:]]
            groups[group].append(([int(v[0])-1 for v in ids],
                [int(v[1])-1 if len(v)>1 and v[1] else 0 for v in ids]))
    return np.array(vertices), np.array(uvs), groups


base, uv, groups = read_obj(DATA / '3dobjs/base.obj')
body = base.copy()
targets = {
    'macrodetails/asian-male-young': .6,
    'macrodetails/caucasian-male-young': .4,
    'macrodetails/universal-male-young-averagemuscle-averageweight': .7,
    'macrodetails/universal-male-young-maxmuscle-averageweight': .3,
    'head/head-age-decr': .12,
    'nose/nose-scale-depth-decr': .12,
    'chin/chin-width-incr': .12,
    'mouth/mouth-angles-up': .3,
    'mouth/mouth-upperlip-middle-down': .2,
    'mouth/mouth-lowerlip-middle-up': .2,
    'mouth/mouth-scale-depth-decr': .12,
}
if FEMALE:
    targets = {name.replace('-male-', '-female-'): weight for name,weight in targets.items()}
    targets['macrodetails/universal-female-young-averagemuscle-averageweight'] = .85
    targets['macrodetails/universal-female-young-maxmuscle-averageweight'] = .15
    targets['chin/chin-width-incr'] = 0
for name, weight in targets.items():
    for line in (DATA / ('targets/' + name + '.target')).read_text().splitlines():
        if not line.strip() or line.startswith('#'): continue
        i, x, y, z = line.split()
        body[int(i)] += np.array([float(x), float(y), float(z)]) * weight


def fit_proxy(path):
    fitted=[]; scales=np.ones(3); active=False
    for line in path.read_text().splitlines():
        vals=line.split()
        if not vals or vals[0].startswith('#'): continue
        if vals[0] in ['x_scale','y_scale','z_scale']:
            axis='xyz'.index(vals[0][0]); a,b=int(vals[1]),int(vals[2])
            scales[axis]=abs(body[a,axis]-body[b,axis])/float(vals[3])
        elif vals[0]=='verts': active=True
        elif active and vals[0][0].isdigit():
            if len(vals)==1: fitted.append(body[int(vals[0])])
            elif len(vals)==9:
                ids=np.array(vals[:3],int); weights=np.array(vals[3:6],float)
                fitted.append((body[ids]*weights[:,None]).sum(axis=0)+np.array(vals[6:9],float)*scales)
        elif active: break
    return np.array(fitted)


eye_orig, eye_uv, eye_groups = read_obj(DATA/'eyes/high-poly/high-poly.obj')
eyes = fit_proxy(DATA/'eyes/high-poly/high-poly.mhclo')
assert len(eyes)==len(eye_orig), (len(eyes),len(eye_orig))
body_ids=np.unique([v for f,_ in groups['body'] for v in f])
ground=body[body_ids,1].min(); top=body[body_ids,1].max()
scale=1.82/(top-ground)


def proportions(vertices):
    v=vertices.copy(); v[:,1]-=ground; v*=scale
    blend=np.clip((v[:,1]-1.51)/.07,0,1)
    blend=blend*blend*(3-2*blend)
    v[:,0]*=1+.075*blend; v[:,2]*=1+.06*blend
    v[:,1]+=np.maximum(0,v[:,1]-1.55)*.075
    return v


body=proportions(body); eyes=proportions(eyes)

# Relax the A-pose at the shoulders to follow the supplied turnaround.
for sign in [-1,1]:
    ids=np.where((sign*body[:,0]>.195)&(body[:,1]>.92)&(body[:,1]<1.54))[0]
    w=np.clip((sign*body[ids,0]-.195)/.10,0,1)
    w=w*w*(3-2*w)
    angle=-sign*np.deg2rad(13)*w
    dx=body[ids,0]-sign*.205; dy=body[ids,1]-1.46
    body[ids,0]=sign*.205+dx*np.cos(angle)-dy*np.sin(angle)
    body[ids,1]=1.46+dx*np.sin(angle)+dy*np.cos(angle)


def compact(vertices, faces):
    ids=np.unique(np.concatenate(faces)); mapping=np.full(len(vertices),-1,int)
    mapping[ids]=np.arange(len(ids))
    return vertices[ids], [mapping[np.array(f,int)].tolist() for f in faces]


def subdivide(vertices, faces):
    """Catmull-Clark with the open eyelid/lip boundaries preserved."""
    facepoints=np.array([vertices[f].mean(axis=0) for f in faces])
    edges={}; vedges=defaultdict(list); vfaces=defaultdict(list)
    for fi,f in enumerate(faces):
        for i,a in enumerate(f):
            b=f[(i+1)%len(f)]; edge=tuple(sorted((a,b)))
            if edge not in edges: edges[edge]=[]
            edges[edge].append(fi); vfaces[a].append(fi)
    keys=list(edges); edge_id={k:len(vertices)+i for i,k in enumerate(keys)}
    edgepoints=[]
    for a,b in keys:
        adjacent=edges[(a,b)]; ep=(vertices[a]+vertices[b])*.5
        if len(adjacent)==2: ep=(vertices[a]+vertices[b]+facepoints[adjacent].sum(axis=0))*.25
        edgepoints.append(ep); vedges[a].append((a,b)); vedges[b].append((a,b))
    moved=vertices.copy()
    for i in range(len(vertices)):
        boundary=[b if a==i else a for a,b in vedges[i] if len(edges[(a,b)])==1]
        if len(boundary)==2: moved[i]=vertices[i]*.75+vertices[boundary].sum(axis=0)*.125
        elif not boundary and vedges[i]:
            n=len(vedges[i]); f=facepoints[vfaces[i]].mean(axis=0)
            r=np.array([(vertices[a]+vertices[b])*.5 for a,b in vedges[i]]).mean(axis=0)
            moved[i]=(f+2*r+(n-3)*vertices[i])/n
    result=np.concatenate([moved,np.array(edgepoints),facepoints]); outfaces=[]
    start=len(vertices)+len(keys)
    for fi,f in enumerate(faces):
        for i,a in enumerate(f):
            outfaces.append([a,edge_id[tuple(sorted((a,f[(i+1)%len(f)])))],start+fi,
                edge_id[tuple(sorted((f[i-1],a)))]])
    return result,outfaces


body_vertices, body_faces=compact(body,[f for f,_ in groups['body']])
body_vertices, body_faces=subdivide(body_vertices,body_faces)
# Soften the tear-trough crease and bring the neutral lips together, retaining
# the eye rim and the anatomical mouth loops rather than adding surface tubes.
neighbors=defaultdict(set)
for face in body_faces:
    for i,a in enumerate(face):
        neighbors[a].add(face[i-1]); neighbors[a].add(face[(i+1)%len(face)])
v=body_vertices
weights=.5*np.exp(-((abs(v[:,0])-.025)/.019)**2-((v[:,1]-1.693)/.010)**2)
weights*=np.clip((v[:,2]-.13)/.03,0,1)
selected=np.where(weights>.01)[0]
for _ in range(5):
    before=v.copy()
    for i in selected: v[i]+=weights[i]*(before[list(neighbors[i])].mean(axis=0)-before[i])
lip_weight=np.exp(-((v[:,1]-1.630)/.012)**2)*np.clip(1-(abs(v[:,0])/.028)**4,0,1)*np.clip((v[:,2]-.15)/.02,0,1)
v[:,1]-=.0004*np.tanh((v[:,1]-1.630)/.0007)*lip_weight
triangles=[]
for f in body_faces: triangles.extend([(f[0],f[1],f[2]),(f[0],f[2],f[3])])
face_triangles=body_vertices[np.array(triangles)]
face_triangles=face_triangles[np.all(face_triangles[:,:,1]>1.55,axis=1)]


def front(x,y):
    """Intersect the real face surface to seat a thin eyebrow on the skin."""
    a=face_triangles[:,0]; b=face_triangles[:,1]; c=face_triangles[:,2]
    den=(b[:,1]-c[:,1])*(a[:,0]-c[:,0])+(c[:,0]-b[:,0])*(a[:,1]-c[:,1])
    den=np.where(abs(den)>1e-12,den,np.nan)
    u=((b[:,1]-c[:,1])*(x-c[:,0])+(c[:,0]-b[:,0])*(y-c[:,1]))/den
    w=((c[:,1]-a[:,1])*(x-c[:,0])+(a[:,0]-c[:,0])*(y-c[:,1]))/den
    v=1-u-w; valid=(u>=0)&(w>=0)&(v>=0)
    z=u*a[:,2]+w*b[:,2]+v*c[:,2]
    return float(np.max(z[valid]))


parts=[('Clay',body_vertices,body_faces,None,None)]
all_eye_faces=[f for group in eye_groups.values() for f,_ in group]
all_eye_uvfaces=[f for group in eye_groups.values() for _,f in group]
parts.append(('Eye',eyes,all_eye_faces,eye_uv,all_eye_uvfaces))
eye_centers=[]
for side,name in [(1,'joint-l-eye'),(-1,'joint-r-eye')]:
    ids=np.unique([i for f,_ in groups[name] for i in f]); center=body[ids].mean(axis=0)
    eye_centers.append(center.tolist()); vs=[]; fs=[]
    for i,t in enumerate(np.linspace(0,1,40)):
        x=side*(abs(center[0])-.019+.048*t)
        y=center[1]+.023+.007*np.sin(np.pi*t)-.004*t
        width=(.0025 if FEMALE else .0031)*np.sin(np.pi*(.12+.83*t))**.7
        for dy in [-width,width]: vs.append([x,y+dy,front(x,y+dy)+.00045])
        if i: fs.append([i*2-2,i*2,i*2+1,i*2-1])
    if side<0: fs=[f[::-1] for f in fs]
    parts.append(('Brow',np.array(vs),fs,None,None))

with (OUT/(ASSET+'.obj')).open('w',encoding='utf8') as f:
    f.write('# Derived from MakeHuman CC0; see SOURCE.txt\nmtllib '+ASSET+'.mtl\n')
    offset=1; uvoffset=1
    for i,(name,vs,fs,uvs,uvfs) in enumerate(parts):
        normals=np.zeros_like(vs)
        for face in fs:
            normal=np.cross(vs[face[1]]-vs[face[0]],vs[face[2]]-vs[face[0]])
            for index in face: normals[index]+=normal
        normals/=np.maximum(np.linalg.norm(normals,axis=1)[:,None],1e-12)
        f.write(f'o {name}_{i}\nusemtl {name}\ns 1\n')
        for v in vs:f.write('v %.7f %.7f %.7f\n'%tuple(v))
        for n in normals:f.write('vn %.7f %.7f %.7f\n'%tuple(n))
        if uvs is not None:
            for uvpoint in uvs:f.write('vt %.7f %.7f\n'%tuple(uvpoint))
        for fi,face in enumerate(fs):
            refs=[]
            for j,index in enumerate(face):
                tex=str(uvfs[fi][j]+uvoffset) if uvs is not None else ''
                refs.append(f'{index+offset}/{tex}/{index+offset}')
            f.write('f '+' '.join(refs)+'\n')
        offset+=len(vs)
        if uvs is not None:uvoffset+=len(uvs)
(OUT/(ASSET+'.mtl')).write_text('newmtl Clay\nKd .65 .62 .57\n\nnewmtl Eye\nKd 1 1 1\nmap_Kd Eyes.png\n\nnewmtl Brow\nKd .22 .20 .18\n',encoding='utf8')
shutil.copyfile(DATA/'eyes/materials/brown_eye.png',OUT/'Eyes.png')
report={'revision':2,'profile':'adult female' if FEMALE else 'adult male','height':float(body_vertices[:,1].max()-body_vertices[:,1].min()),
    'triangles':sum(sum(len(f)-2 for f in p[2]) for p in parts),
    'vertices':sum(len(p[1]) for p in parts),'parts':len(parts),'eyeCenters':eye_centers,
    'targets':targets,'source':'MakeHuman CC0 anatomical base, adapted to reference',
    'subdivision':'one Catmull-Clark level','rigged':False}
(OUT/'mesh-report.json').write_text(json.dumps(report,indent=2),encoding='utf8')
manifest={'repository':'https://github.com/makehumancommunity/makehuman',
    'revision':json.loads((SOURCE/'tree.json').read_text())['sha'],'license':'CC0-1.0',
    'files':{str(p.relative_to(SOURCE)).replace('\\','/'):hashlib.sha256(p.read_bytes()).hexdigest()
        for p in SOURCE.rglob('*') if p.is_file() and p.name not in ['tree.json','source-manifest.json']}}
(SOURCE/'source-manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')
print(json.dumps(report,indent=2))
