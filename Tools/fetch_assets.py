"""Fetch a small, CC0-only selection from the author's public download folders.

Run: python -m pip install gdown
     python Tools/fetch_assets.py
The Unity project also has procedural fallback models and runs without downloads.
"""
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
import hashlib
import json
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / '.tools' / 'python'))
import gdown

PACKS = [
    ('UltimateCrops', '1uhbi-NWp7pwqOGtraBZurbphyxAvoABZ',
     'https://quaternius.com/packs/ultimatecrops.html',
     {f'{crop}_{stage}.fbx' for crop in ('Carrot', 'Corn', 'Pumpkin') for stage in range(1, 5)}),
    ('FarmBuildings', '1gdZ39vcLML_ULU5sHirkKQ-gEkk458Ak',
     'https://quaternius.com/packs/farmbuildings.html',
     {'SmallBarn.fbx', 'Well.fbx', 'Windmill.fbx'}),
]


def fetch_file(task):
    file_id, destination = task
    destination.parent.mkdir(parents=True, exist_ok=True)
    if not destination.exists():
        result = gdown.download(id=file_id, output=str(destination), quiet=True, use_cookies=False)
        if not result:
            raise RuntimeError(f'Download failed: {destination.name}')
    data = destination.read_bytes()
    if destination.suffix.lower() == '.fbx' and not (data.startswith(b'Kaydara FBX Binary') or b'FBX' in data[:100]):
        raise RuntimeError(f'Unexpected file content: {destination.name}')
    print(f'OK {destination.relative_to(ROOT)} ({len(data):,} bytes)')
    return {'file': destination.relative_to(ROOT).as_posix(), 'sha256': hashlib.sha256(data).hexdigest()}


def main():
    records = []
    for pack, folder_id, source, wanted in PACKS:
        files = gdown.download_folder(id=folder_id, skip_download=True, quiet=True,
                                     output=str(ROOT / '.tools' / 'asset-index' / pack))
        if not files:
            raise RuntimeError(f'Cannot list {pack}')
        tasks = []
        found = set()
        for item in files:
            normalized = item.path.replace('\\', '/')
            name = normalized.rsplit('/', 1)[-1]
            if (normalized.lower().startswith('fbx/') and name in wanted) or name.lower() == 'license.txt':
                folder = 'Models' if name.lower().endswith('.fbx') else ''
                destination = ROOT / 'Assets' / 'HarvestIsle' / 'Resources' / 'Art' / pack / folder / name
                tasks.append((item.id, destination))
                found.add(name)
        missing = wanted - found
        if missing:
            raise RuntimeError(f'{pack}: missing models {sorted(missing)}')
        with ThreadPoolExecutor(max_workers=3) as pool:
            downloads = list(pool.map(fetch_file, tasks))
        records.append({'pack': pack, 'author': 'Quaternius', 'license': 'CC0-1.0',
                        'source': source, 'files': downloads})
    (ROOT / 'Tools' / 'asset-manifest.json').write_text(json.dumps(records, indent=2), encoding='utf-8')


if __name__ == '__main__':
    main()
