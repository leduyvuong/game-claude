"""Validate complete native model coverage and rendered review artifacts."""
import collections
import hashlib
import json
from pathlib import Path
import struct

ROOT = Path(__file__).resolve().parents[1]


def png_size(path):
    data = path.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n', f'Invalid PNG: {path}'
    return struct.unpack('>II', data[16:24])


def main():
    folder = ROOT / 'Art/Previews/Library'
    catalog = json.loads((folder / 'models.json').read_text(encoding='utf8'))
    assert catalog == json.loads((ROOT / 'Assets/HarvestIsle/Resources/FullModelCatalog.json').read_text(encoding='utf8'))
    models = catalog['models']
    by_id = {m['id']: m for m in models}
    assert len(by_id) == len(models), 'Duplicate model identifiers'
    crops = [m for m in models if len(m['variants']) == 7]
    levels = [m for m in models if len(m['variants']) == 3]
    assert len(crops) == 60 and len(levels) == 30
    assert len(models) == 503, '353 existing plus 120 expansion designs and 30 harvested products'
    expansion = {f'IX_B{i:02}' for i in range(1, 31)} | {f'IX_C{i:02}' for i in range(1, 31)} | {f'IX_D{i:02}' for i in range(1, 61)} | {f'IX_Product_C{i:02}' for i in range(1, 31)}
    assert {m['id'] for m in models if m['id'].startswith('IX_')} == expansion
    kitchen = by_id['HP_KC_Kitchen']
    assert len(kitchen['variants']) == 8, 'Kitchen must have all eight levels'
    kitchen_models = [m for m in models if m['id'].startswith('HP_KC_')]
    assert len(kitchen_models) == 49 and sum(len(m['variants']) for m in kitchen_models) == 56
    assert len([m for m in crops if m['id'] in {'Mango', 'Coconut', 'Orange', 'Lemon', 'Banana', 'Papaya'}]) == 6
    assert len([m for m in models if m['id'].startswith('Product_')]) == 45
    assert len([m for m in models if m['id'].startswith('IX_Product_')]) == 30
    required = {'ElderTu', 'MerchantHaiAu', 'GoldenFirefly', 'Bee', 'Ladybug', 'Planthopper', 'Snail', 'FruitBorer', 'FieldMouse', 'Locust', 'Myna',
                'DrainStraight', 'DrainCorner', 'DrainJunction', 'IrrigationCanal', 'FloodDike', 'SandbagStack', 'Watchtower', 'RatProofStore', 'VillageWarehouse',
                'RainTank', 'HeatingLamp', 'StrawCover', 'PestNet', 'InsectTrap', 'Campfire', 'VillageCookpot', 'GraftedMango', 'GraftedOrange', 'DualFruitTree',
                'WildRootstock', 'ThornLemonRootstock', 'MangoBranch', 'OrangeBranch', 'Volcano', 'SnowMountain', 'RainCloud', 'StormCloud', 'FrostPatch',
                'RedEarthTile', 'AlluvialTile', 'HumusTile', 'GravelTile', 'MeadowTile', 'FlowerMeadowTile', 'ForestFloorTile', 'DryGrassTile',
                'RainTree', 'BanyanTree', 'PineTree', 'WillowTree', 'FlameTree', 'ArecaPalm', 'YoungForestTree',
                'ConstructionSite', 'BrokenFence', 'DamagedRoof', 'Radio', 'Journal', 'Bailer', 'FishingRod', 'FestivalLantern', 'GiantPumpkin'}
    assert required <= by_id.keys(), f'Missing concept models: {required - by_id.keys()}'
    variants = 0
    for model in models:
        hashes = set()
        for variant in model['variants']:
            prefab = ROOT / variant['prefab']
            assert prefab.is_file() and prefab.with_suffix('.prefab.meta').is_file(), f'Missing prefab/meta: {prefab}'
            expected = variant['prefab'].removeprefix('Assets/HarvestIsle/Resources/').removesuffix('.prefab')
            assert expected == variant['resource'], f'Incorrect Resources path: {variant}'
            assert variant['triangles'] > 0
            preview = folder / variant['preview']
            size = png_size(preview)[0]
            assert size in ({480, 640} if model['id'].startswith('IX_') else {640})
            assert png_size(preview) == (size, size)
            assert png_size(folder / variant['views']) == (size * 3, size)
            hashes.add(hashlib.sha256(preview.read_bytes()).hexdigest())
            variants += 1
        assert len(hashes) == len(model['variants']), f'Visually identical variants: {model["id"]}'
    prefabs = len(list((ROOT / 'Assets/HarvestIsle/Resources').rglob('*.prefab')))
    assert prefabs == catalog['prefabCount']
    assert prefabs == 965 and variants == 930
    counts = collections.Counter(model['group'] for model in models)
    report = {'types': len(models), 'galleryVariants': variants, 'projectPrefabs': prefabs,
              'cropTypes': len(crops), 'growthStages': len(crops) * 5, 'cropHealthStates': len(crops) * 2,
              'levelledModels': len(levels), 'kitchenLevels': 8, 'kitchenComponentPrefabs': 56, 'products': 75, 'groups': dict(counts)}
    (folder / 'coverage.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf8')
    with (folder / 'model-list.csv').open('w', encoding='utf-8-sig', newline='') as file:
        import csv
        writer = csv.writer(file)
        writer.writerow(['ID', 'Model', 'Group', 'Variant', 'Prefab', 'Triangles'])
        for model in models:
            for variant in model['variants']:
                writer.writerow([model['id'], model['label'], model['group'], variant['label'], variant['prefab'], variant['triangles']])
    print(f'PASS {len(models)} types, {variants} unique variants, {prefabs} project prefabs and {variants * 3} rendered views')
    print('PASS 60 crops with 300 growth stages / 120 health states, 30 levelled models, 75 products and named concept coverage')
    print('PASS 56 kitchen components including eight distinct kitchen levels')


if __name__ == '__main__':
    main()
