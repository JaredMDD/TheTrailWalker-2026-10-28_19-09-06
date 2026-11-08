"""Run in a fresh Blender process; pass --inspector path/to/inspect_asset.py.

Uses the Blender asset validation skill inspector, retaining per-import metrics.
This validates passive art and prefab references, never gameplay.
"""
import sys, json, hashlib, re, importlib.util
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Docs/Art/Tier02/Batch08'


def main():
    args=sys.argv[sys.argv.index('--')+1:]
    inspector=Path(args[args.index('--inspector')+1])
    spec=importlib.util.spec_from_file_location('asset_inspector',inspector)
    inspect=importlib.util.module_from_spec(spec);spec.loader.exec_module(inspect)
    def report(path,out):
        sys.argv=['inspect','--','--input',str(path),'--output',str(out)]
        inspect.main()
        result=json.loads(out.read_text())
        assert result['hard_gate_pass'],result['issues']
        return result
    native=report(DOC/'tier02-batch08.blend',DOC/'native-inspection.json')
    manifest=json.loads((ROOT/'Assets/_Game/Art/Crafting/Tier02/Tier02Batch08Manifest.json').read_text())
    items=[]
    zero_fields=['degenerate_faces','zero_length_edges','invalid_vertices','isolated_vertices','boundary_edges','non_manifold_edges']
    for row in manifest['Items']:
        name=row['Model'];fresh=report(ROOT/row['SourcePath'],DOC/(name+'-fresh-inspection.json'))
        assert fresh['totals']['triangles']==row['Triangles']
        for field in zero_fields:assert fresh['totals'][field]==0,(name,field)
        bounds=fresh['scene']['bounds']
        lo,hi=row['BoundsMin'],row['BoundsMax']
        expected=([lo[0],-hi[2],lo[1]],[hi[0],-lo[2],hi[1]])
        # Inspector bounds are an explicit {min,max,size,center} record.
        for key,want in zip(['min','max'],expected):
            assert all(abs(a-b)<2e-5 for a,b in zip(bounds[key],want)),(name,bounds,expected)
        native_mesh=next(m for m in native['meshes'] if m['name']==name)
        assert native_mesh['triangles']==row['Triangles']
        for field in zero_fields:assert native_mesh[field]==0
        def guid(path):return re.search(r'^guid: (\w+)',Path(str(path)+'.meta').read_text(),re.M).group(1)
        prefab=ROOT/'Assets/_Game/Resources'/(row['PrefabPath']+'.prefab')
        yaml=prefab.read_text()
        material=ROOT/'Assets/_Game/Resources/Tier02/Batch08Materials'/(name+'.mat')
        assert guid(ROOT/row['SourcePath']) in yaml
        assert guid(material) in yaml
        assert guid(ROOT/row['PalettePath']) in material.read_text()
        items.append({'Model':name,'Triangles':row['Triangles'],'FreshImportCountsAndBoundsMatch':True,
                      'ClosedComponents':True,'PrefabSourceMaterialPaletteReferencesVerified':True,'BareObjWarnings':fresh['issues']})
    prior=json.loads((ROOT/'Temp/Tier02Batch08/prior-obj-hashes.json').read_text(encoding='utf-8-sig'))
    for row in prior:assert hashlib.sha256(Path(row['Path']).read_bytes()).hexdigest().upper()==row['Hash']
    atlas=json.loads((ROOT/'Temp/Tier02Batch08/prior-atlas-hash.json').read_text(encoding='utf-8-sig'))
    assert hashlib.sha256(Path(atlas['Path']).read_bytes()).hexdigest().upper()==atlas['Hash']
    for name in ['unity-build-result.txt','unity-capture-result.txt']:assert (DOC/name).read_text().startswith('SUCCEEDED')
    result={'Scope':'Five passive Material assets; no runtime binding or gameplay/device verification',
            'Blender':native['blender'],'Items':items,'EarlierMeshHashesPreserved':len(prior),'SharedAtlasUnchanged':True,
            'NativeViews':sum(len(list(DOC.glob(i['Model']+'-blender-*.png'))) for i in items),
            'UnityViews':sum(len(list(DOC.glob(i['Model']+'-unity-*.png'))) for i in items),
            'VisualReview':'Review final_report.md for six-view and actual Unity findings.'}
    (DOC/'batch08-validation.json').write_text(json.dumps(result,indent=2)+'\n')
    print('BATCH08_VALIDATED',[(i['Model'],i['Triangles']) for i in items])


if __name__=='__main__':main()
