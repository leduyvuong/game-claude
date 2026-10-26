"""Propagate MakeHuman skin weights through the study's subdivision and export.
Binary HM03 contains the adapted skeleton and smooth, weighted surface meshes.
"""
import runpy, json, sys, struct
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.tools/python'))
import numpy as np
FEMALE=globals().get('FEMALE',False)
d=runpy.run_path(str(ROOT/'Tools/build_reference_male.py'),init_globals={'FEMALE':FEMALE})
asset=d['ASSET']
data=d['DATA']; out=d['OUT']; body=d['body']; groups=d['groups']
rig=json.loads((data/'rigs/default.mhskel').read_text())
original=json.loads((data/'rigs/default_weights.mhw').read_text())['weights']
bones=[]; aliases={}
def add(name,parent,source):
    spec=rig['bones'][source]
    head=body[rig['joints'][spec['head']]].mean(axis=0)
    tail=body[rig['joints'][spec['tail']]].mean(axis=0)
    bones.append(dict(name=name,parent=parent,head=head,tail=tail))
    aliases[source]=name
add('Hips',None,'root')
add('Spine','Hips','spine05');add('Chest','Spine','spine02')
add('Neck','Chest','neck01');add('Head','Neck','head')
for suffix,side in [('L','Left'),('R','Right')]:
    add(side+'Clavicle','Chest','clavicle.'+suffix)
    add(side+'UpperArm',side+'Clavicle','upperarm01.'+suffix)
    add(side+'Forearm',side+'UpperArm','lowerarm01.'+suffix)
    add(side+'Hand',side+'Forearm','wrist.'+suffix)
    add(side+'Thigh','Hips','upperleg01.'+suffix)
    add(side+'Shin',side+'Thigh','lowerleg01.'+suffix)
    add(side+'Foot',side+'Shin','foot.'+suffix)
    for finger in range(1,6):
        parent=side+'Hand'
        for joint in range(1,4):
            name=f'{side}Finger{finger}_{joint}'
            add(name,parent,f'finger{finger}-{joint}.{suffix}');parent=name
    aliases.update({f'upperarm02.{suffix}':side+'UpperArm',f'lowerarm02.{suffix}':side+'Forearm',
        f'upperleg02.{suffix}':side+'Thigh',f'lowerleg02.{suffix}':side+'Shin',
        f'shoulder01.{suffix}':side+'Clavicle',f'pelvis.{suffix}':'Hips',f'breast.{suffix}':'Chest'})
    for name in original:
        if name.endswith('.'+suffix) and name.startswith('toe'):aliases[name]=side+'Foot'
        if name.endswith('.'+suffix) and name.startswith('metacarpal'):aliases[name]=side+'Hand'
aliases.update({'spine04':'Spine','spine03':'Spine','spine01':'Chest','neck02':'Neck','neck03':'Neck'})
indices={b['name']:i for i,b in enumerate(bones)}
weights=np.zeros((len(body),len(bones)),np.float64)
for name,pairs in original.items():
    target=indices[aliases.get(name,'Head')]
    for index,weight in pairs:weights[index,target]+=weight
ids=np.unique([i for f,_ in groups['body'] for i in f])
compact_weights,faces=d['compact'](weights,[f for f,_ in groups['body']])
skin,_=d['subdivide'](compact_weights,faces)
skin=np.maximum(skin,0)
sys.path.insert(0,str(ROOT/'Tools'))
from reference_male_clothes import build_clothes
body_material,body_vs,body_fs,body_uv,body_uvfs=d['parts'][0]
visible_body,clothes,clothes_report=build_clothes(body_vs,body_fs,skin,bones,female=FEMALE)
source_parts=[(body_material,body_vs,visible_body,body_uv,body_uvfs,skin)]
for material,vs,fs,uv,uvfaces in d['parts'][1:]:
    partweights=np.zeros((len(vs),len(bones)));partweights[:,indices['Head']]=1
    source_parts.append((material,vs,fs,uv,uvfaces,partweights))
source_parts.extend(clothes)
parts=[]
for material,vs,fs,uv,uvfaces,partweights in source_parts:
    top=np.argsort(partweights,axis=1)[:,-4:][:,::-1]
    values=np.take_along_axis(partweights,top,axis=1)
    assert np.all(values.sum(axis=1)>0)
    values/=values.sum(axis=1)[:,None]
    normals=np.zeros_like(vs)
    for face in fs:
        n=np.cross(vs[face[1]]-vs[face[0]],vs[face[2]]-vs[face[0]])
        for index in face:normals[index]+=n
    normals/=np.maximum(np.linalg.norm(normals,axis=1)[:,None],1e-12)
    # Duplicate only UV seams, preserving the source weight assignment.
    lookup={}; vertices=[]; tri=[]
    for fi,face in enumerate(fs):
        mapped=[]
        for j,index in enumerate(face):
            tex=uvfaces[fi][j] if uv is not None else -1; key=(index,tex)
            if key not in lookup:
                lookup[key]=len(vertices)
                vertices.append((vs[index],normals[index],uv[tex] if tex>=0 else np.zeros(2),top[index],values[index]))
            mapped.append(lookup[key])
        for j in range(1,len(mapped)-1):tri.extend([mapped[0],mapped[j],mapped[j+1]])
    parts.append((material,vertices,tri))
def string(f,s):
    raw=s.encode('utf8');f.write(struct.pack('<i',len(raw)));f.write(raw)
with (out/(asset+'Skin.bytes')).open('wb') as f:
    f.write(b'HM03');f.write(struct.pack('<i',len(bones)))
    for b in bones:
        string(f,b['name']);f.write(struct.pack('<i',indices.get(b['parent'],-1)))
        f.write(np.asarray(b['head'],dtype='<f4').tobytes());f.write(np.asarray(b['tail'],dtype='<f4').tobytes())
    f.write(struct.pack('<i',len(parts)))
    for material,vertices,tri in parts:
        string(f,material);f.write(struct.pack('<i',len(vertices)))
        for p,n,uv,bi,bw in vertices:
            f.write(np.asarray(np.r_[p,n,uv],dtype='<f4').tobytes())
            f.write(np.asarray(bi,dtype='<i4').tobytes());f.write(np.asarray(bw,dtype='<f4').tobytes())
        f.write(struct.pack('<i',len(tri)));f.write(np.asarray(tri,dtype='<i4').tobytes())
report={'bones':len(bones),'maxInfluences':4,'meshes':len(parts),'vertices':sum(len(p[1]) for p in parts),
    'triangles':sum(len(p[2])//3 for p in parts),'weightsNormalized':True,
    'source':'MakeHuman default rig/weights CC0, merged to body and finger chains'}
(out/'rig-report.json').write_text(json.dumps(report,indent=2),encoding='utf8')
(out/'clothes-report.json').write_text(json.dumps(clothes_report,indent=2),encoding='utf8')
print(json.dumps(report))
print(json.dumps(clothes_report))
