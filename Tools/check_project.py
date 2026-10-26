"""Read-only checks for project structure, asset provenance, and optional C# syntax."""
from pathlib import Path
import hashlib
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / '.tools' / 'python'))


def main():
    required = ['Packages/manifest.json', 'ProjectSettings/ProjectVersion.txt',
                'ProjectSettings/ProjectSettings.asset', 'Assets/HarvestIsle/Scenes/HarvestIsle.unity']
    for name in required:
        assert (ROOT / name).is_file(), f'Missing {name}'
    manifest = json.loads((ROOT / 'Packages/manifest.json').read_text(encoding='utf-8'))
    assert manifest['dependencies']['com.unity.render-pipelines.universal'] == '17.3.0'
    names = set()
    for file in (ROOT / 'Assets').rglob('*.asmdef'):
        definition = json.loads(file.read_text(encoding='utf-8'))
        assert definition['name'] not in names, 'Duplicate assembly name'
        names.add(definition['name'])
    guids = {}
    for file in (ROOT / 'Assets').rglob('*.meta'):
        match = re.search(r'^guid: ([a-f0-9]{32})$', file.read_text(encoding='utf-8'), re.M)
        assert match, f'Invalid GUID in {file}'
        guid = match.group(1)
        assert guid not in guids, f'Duplicate GUID: {file} and {guids.get(guid)}'
        guids[guid] = file
    scene = (ROOT / 'Assets/HarvestIsle/Scenes/HarvestIsle.unity').read_text(encoding='utf-8')
    script_guid = re.search(r'm_Script: \{fileID: 11500000, guid: ([a-f0-9]{32}), type: 3\}', scene).group(1)
    assert guids[script_guid].name == 'FarmGame.cs.meta', 'Scene must reference FarmGame'
    for file in (ROOT / 'Assets').rglob('*'):
        if file.is_file() and file.suffix != '.meta':
            assert file.with_name(file.name + '.meta').is_file(), f'Missing meta: {file}'
    print('PASS project structure, assembly definitions, scene reference and unique asset GUIDs')
    records = json.loads((ROOT / 'Tools/asset-manifest.json').read_text(encoding='utf-8'))
    total = 0
    for pack in records:
        assert pack['license'] == 'CC0-1.0'
        for item in pack['files']:
            file = (ROOT / item['file']).resolve()
            assert file.is_relative_to(ROOT / 'Assets'), 'Asset path escapes Assets'
            digest = hashlib.sha256(file.read_bytes()).hexdigest()
            assert digest == item['sha256'], f'Asset hash mismatch: {file}'
            total += 1
    print(f'PASS {total} source assets verified against SHA-256 manifest')
    try:
        from tree_sitter import Language, Parser
        import tree_sitter_c_sharp
    except ImportError:
        print('SKIP C# syntax: optional tree-sitter / tree-sitter-c-sharp not installed')
        return
    parser = Parser(Language(tree_sitter_c_sharp.language()))
    sources = list((ROOT / 'Assets').rglob('*.cs')) + list((ROOT / 'Tools/Tests').rglob('*.cs'))
    for source in sources:
        tree = parser.parse(source.read_bytes())
        assert not tree.root_node.has_error, f'C# syntax error: {source}'
    print(f'PASS C# syntax parsed for {len(sources)} source files (not Unity API compilation)')


if __name__ == '__main__':
    main()
