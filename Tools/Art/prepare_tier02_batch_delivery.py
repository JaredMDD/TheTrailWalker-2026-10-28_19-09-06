"""Prepare reusable later-batch builder/validator from accepted batch 08 plumbing.

Run once when setting up the sequential run; never changes existing art.
"""
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]


def main():
    editor=ROOT/'Assets/_Game/Editor'
    code=(editor/'Tier02Batch08Assets.cs').read_text(encoding='utf-8-sig')
    code=code.replace('Tier02Batch08Assets','Tier02BatchArtAssets')
    old='''        const string ManifestPath = "Assets/_Game/Art/Crafting/Tier02/Tier02Batch08Manifest.json";
        const string MaterialPath = "Assets/_Game/Resources/Tier02/Batch08Materials/";
        const string Docs = "Docs/Art/Tier02/Batch08/";'''
    new='''        static string ManifestPath, MaterialPath, Docs;
        static void SelectBatch(int batch)
        {
            if (batch < 9 || batch > 45) throw new ArgumentOutOfRangeException(nameof(batch));
            string code = batch.ToString("D2");
            ManifestPath = "Assets/_Game/Art/Crafting/Tier02/Tier02Batch" + code + "Manifest.json";
            MaterialPath = "Assets/_Game/Resources/Tier02/Batch" + code + "Materials/";
            Docs = "Docs/Art/Tier02/Batch" + code + "/";
        }'''
    assert old in code
    code=code.replace(old,new)
    code=code.replace('        [MenuItem("The Trail Walker/Art/Build Tier 2 Batch 08")]\n','')
    code=code.replace('        [MenuItem("The Trail Walker/Art/Capture Tier 2 Batch 08")]\n','')
    for method in ['Build','Capture']:
        code=code.replace(f'public static void {method}()\n        {{',f'public static void {method}(int batch)\n        {{\n            SelectBatch(batch);')
    code=code.replace('five passive prefabs/materials','passive batch prefabs/materials')
    code=code.replace('ten actual imported-prefab images','actual imported-prefab front/study images')
    code=code.replace('PrefabPath, ItemId;', 'PrefabPath, ItemId, Representation;')
    code=code.replace('Render(prefab, row.Model + "-unity-front.png", new Vector3(0,0,-4));',
                      'float forward = row.Representation == "worn" ? 1f : -1f;\n                Render(prefab, row.Model + "-unity-front.png", new Vector3(0,0,4*forward));')
    code=code.replace('new Vector3(2.3f,1.3f,-4)', 'new Vector3(2.3f,1.3f,4*forward)')
    (editor/'Tier02BatchArtAssets.cs').write_text(code,encoding='utf-8')
    art=ROOT/'Tools/Art'
    code=(art/'validate_tier02_batch08.py').read_text()
    code=code.replace("DOC=ROOT/'Docs/Art/Tier02/Batch08'","BATCH=int(sys.argv[sys.argv.index('--batch')+1])\nDOC=ROOT/f'Docs/Art/Tier02/Batch{BATCH:02}'")
    code=code.replace("DOC/'tier02-batch08.blend'","DOC/f'tier02-batch{BATCH:02}.blend'")
    code=code.replace("ROOT/'Assets/_Game/Art/Crafting/Tier02/Tier02Batch08Manifest.json'","ROOT/f'Assets/_Game/Art/Crafting/Tier02/Tier02Batch{BATCH:02}Manifest.json'")
    code=code.replace("ROOT/'Assets/_Game/Resources/Tier02/Batch08Materials'","ROOT/f'Assets/_Game/Resources/Tier02/Batch{BATCH:02}Materials'")
    code=code.replace("ROOT/'Temp/Tier02Batch08/prior-obj-hashes.json'","ROOT/f'Temp/Tier02Batch{BATCH:02}/prior-obj-hashes.json'")
    code=code.replace("ROOT/'Temp/Tier02Batch08/prior-atlas-hash.json'","ROOT/f'Temp/Tier02Batch{BATCH:02}/prior-atlas-hash.json'")
    code=code.replace('Five passive Material assets','Passive batch art assets')
    code=code.replace("DOC/'batch08-validation.json'","DOC/f'batch{BATCH:02}-validation.json'")
    code=code.replace("'BATCH08_VALIDATED'","f'BATCH{BATCH:02}_VALIDATED'")
    (art/'validate_tier02_batch.py').write_text(code,encoding='utf-8')
    print('Prepared generic passive Unity builder and later-batch validator.')


if __name__=='__main__':main()
