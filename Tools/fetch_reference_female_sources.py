"""Fetch only the four pinned CC0 female morphs; validate Git blob hashes."""
from pathlib import Path
import json, hashlib, urllib.request
ROOT=Path(__file__).resolve().parents[1]
source=ROOT/'Design/References/MakeHuman'
tree=json.loads((source/'tree.json').read_text())
entries={entry['path']:entry['sha'] for entry in tree['tree']}
names=['asian-female-young','caucasian-female-young',
       'universal-female-young-averagemuscle-averageweight',
       'universal-female-young-maxmuscle-averageweight']
for name in names:
    path='makehuman/data/targets/macrodetails/'+name+'.target'
    target=source/path
    if target.exists(): data=target.read_bytes()
    else:
        url='https://raw.githubusercontent.com/makehumancommunity/makehuman/'+tree['sha']+'/'+path
        data=urllib.request.urlopen(url, timeout=60).read()
    assert hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()==entries[path],path
    target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(data)
    print('Verified pinned CC0 morph:',name)
