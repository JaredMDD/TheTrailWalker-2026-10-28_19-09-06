"""Record a visually accepted, validated batch and refresh current design status.

Run only after manually reviewing the native and Unity evidence and writing
review-notes.md. This records art completion, never runtime acceptance.
"""
import argparse,json,re,subprocess,sys,os
from pathlib import Path
from datetime import datetime,timezone

ROOT=Path(__file__).resolve().parents[2]


def main():
    parser=argparse.ArgumentParser();parser.add_argument('--batch',type=int,required=True)
    parser.add_argument('--stop-after',action='store_true',help='Record a single-batch delivery and explicit stop checkpoint.')
    args=parser.parse_args();batch=args.batch;code=f'{batch:02}'
    base=ROOT/'Docs/Art/Tier02';doc=base/f'Batch{code}'
    record=json.loads((base/'tier2-design-batch-status.json').read_text(encoding='utf-8'))
    current=next(b for b in record['Batches'] if b['Number']==batch)
    manifest=json.loads((ROOT/f'Assets/_Game/Art/Crafting/Tier02/Tier02Batch{code}Manifest.json').read_text())
    validation=json.loads((doc/f'batch{code}-validation.json').read_text())
    review=(doc/'review-notes.md').read_text(encoding='utf-8')
    assert review.strip() and len(manifest['Items'])==len(validation['Items'])
    assert validation['NativeViews']==len(manifest['Items'])*6
    assert validation['UnityViews']==len(manifest['Items'])*2
    for item in validation['Items']:assert item['FreshImportCountsAndBoundsMatch'] and item['PrefabSourceMaterialPaletteReferencesVerified']
    assert all(b['Status']=='art complete; integration pending' for b in record['Batches'] if b['Number']<batch)
    current.update(Status='art complete; integration pending',Evidence=f'Docs/PROJECT_HANDOFF.md#batch-{code}',
                   Pending='Canonical item/output bindings and current gameplay/save/phone tests remain pending. See batch review and consolidated handoff for exact recipe/prerequisite concerns.')
    (base/'tier2-design-batch-status.json').write_text(json.dumps(record,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    subprocess.run([sys.executable,str(ROOT/'Tools/Art/prepare_tier02_design_inventory.py')],check=True,capture_output=True)
    catalog=json.loads((base/'tier2-item-design-catalog-2026-11-06.json').read_text())
    covered=sum(bool(r['ExistingMeshPaths']) for r in catalog['Recipes'])
    complete=sum(b['Status']=='art complete; integration pending' for b in record['Batches'])
    remaining=45-complete
    following=next((b for b in record['Batches'] if b['Status']=='planned'),None)
    next_text=f"Batch {following['Number']:02}: {', '.join(following['Subjects'])}" if following else 'No planned batch remains'
    run_note=(f'Batch {code} completed for this request under the established one-batch default. Evidence and handoff are updated; batch {batch+1:02} is the next planned packet.'
              if args.stop_after else record.get('CurrentRunAuthorization','Default: one batch per request, then stop.'))
    record['CurrentRunAuthorization']=run_note
    (base/'tier2-design-batch-status.json').write_text(json.dumps(record,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    table=['| Output | Triangles | Documentary recipe | Unlock / region |','| --- | ---: | --- | --- |']
    for row in manifest['Items']:
        unlock=(f"{row['MissionCode']} / {row['MissionRegion']}" if row.get('MissionCode')
                else row.get('UnlockSource','Unresolved; no mission authored'))
        table.append(f"| {row['DisplayName']} | {row['Triangles']} | {row['IngredientBrief']} | {unlock} |")
    links=(f'[contract](modeling-contract.md), [references](reference_notes.md), [native scene](tier02-batch{code}.blend), '
           f'[six-view sheet](batch{code}-six-view-review.png), [Unity overview](batch{code}-unity-overview.png), '
           f'[Unity fronts](batch{code}-unity-front-review.png), [validation](batch{code}-validation.json), '
           '[native inspection](native-inspection.json), individual fresh-import inspections, '
           f'[generator](../../../../Tools/Art/generate_tier02_batch{code}.py), '
           '[shared presentation](../../../../Tools/Art/tier02_batch_workflow.py), '
           '[validator](../../../../Tools/Art/validate_tier02_batch.py), '
           '[Unity builder](../../../../Assets/_Game/Editor/Tier02BatchArtAssets.cs), '
           f'[manifest](../../../../Assets/_Game/Art/Crafting/Tier02/Tier02Batch{code}Manifest.json).')
    checks=(f"Fresh-process native reopen and {len(manifest['Items'])} OBJ imports passed: matching triangle counts and expected source Y-up to Blender Z-up bounds, UVs present, zero degenerate faces, zero-length edges, invalid/isolated vertices, boundary edges and non-manifold edges. Separate components are intentional display parts. Bare OBJ warnings reflect the deliberate no-MTL interchange; Unity source/material/palette GUID references were verified. Selective Unity Build/Capture succeeded. All {validation['EarlierMeshHashesPreserved']} earlier OBJ hashes and the shared atlas were preserved. {validation['NativeViews']} native and {validation['UnityViews']} actual Unity views reviewed. No APK or gameplay/device/performance test was performed.")
    report=(f'# Batch {code} delivery - November 7, 2026\n\nArt complete; runtime binding and gameplay/device validation pending. All seven modeling stages completed.\n\n'+
            '\n'.join(table)+'\n\n'+review+'\n\n'+checks+'\n\nDeliverables: '+links+
            f'\n\nCurrent progress: {complete}/45 art batches, {covered}/82 recipe outputs with source meshes, {82-covered} output designs remaining. Next planned: {next_text}. {run_note}\n')
    (doc/'final_report.md').write_text(report,encoding='utf-8')
    (doc/'work_state.md').write_text(f'# Batch {code} checkpoint\n\nComplete for art. Source, native/fresh import validation and Unity review accepted. See final_report.md and Docs/PROJECT_HANDOFF.md#batch-{code}. Bindings/gameplay/device tests pending. Next planned: {next_text}. {run_note}\n',encoding='utf-8')
    h=ROOT/'Docs/PROJECT_HANDOFF.md';text=h.read_text(encoding='utf-8')
    text=re.sub(r'Updated November \d+, 2026, after tier-two batch \d+\.',f'Updated November 7, 2026, after tier-two batch {code}.',text,count=1)
    start=text.index('## Current checkpoint');end=text.index('## Navigation')
    retained=re.search(r'The current English design now assigns[^\n]+\n\n',text[start:end])
    retained_note=retained.group(0) if retained else ''
    text=text[:start]+f'''## Current checkpoint

- Tier-two batches 01-{code} are complete for passive art and selective Unity import; {remaining} packets remain planned. Latest: {', '.join(current['Subjects'])}. Next: {next_text}.
- Recipe-output source coverage is {covered}/82, with {82-covered} designs remaining. These are source-art counts, not runtime content acceptance.
- Native Blender scenes, OBJ/palettes, passive art prefabs/materials and actual-mesh review views are delivered. Canonical item/output IDs and live bindings remain pending except explicitly documented authored IDs; those do not establish registration. No gameplay, source gates, recipe values, footprints or save behavior were changed in these art batches.
- The other tool owns current logic/gameplay/save/phone verification. Preserve full-stack/debug testing. Outstanding issues include early construction bridges and mission-order migration, barrel pantry permission, pet opportunity capacity, appended T2-G21/T2-H13 order, current T2-G22 optional/nonblocking execution and migration verification, stale exclusive-art food values and Roasted crab premium eligibility. See the retained logic/testing review below.
- **Current run instruction:** {run_note}
- Follow [batch schedule](Art/Tier02/tier2-production-batches-2026-11-06.md), [broad brief](Art/Tier02/remaining-tier2-design-plan-2026-11-06.md) and current English/Spanish plans. Blender is `D:/Program Files/Blender Foundation/Blender 5.2/blender.exe` (5.2.3 LTS); Unity 6000.4.7f1 targeted explicitly at `D:/IdeaProjects/TheTrailWalker`.

'''+retained_note+text[end:]
    text=text.replace('## Navigation\n','## Navigation\n\n'+f'- [Batch {code} - {current["Title"]}](#batch-{code})',1)
    def handoff_link(match):
        link=match.group(1)
        if '://' in link or link.startswith('#'):return match.group(0)
        target,sep,anchor=link.partition('#')
        relative=os.path.relpath((doc/target).resolve(),ROOT/'Docs').replace('\\','/')
        return ']('+relative+(sep+anchor if sep else '')+')'
    handoff_review=re.sub(r'\]\(([^)]+)\)',handoff_link,review)
    section=f'''<a id="batch-{code}"></a>
## Batch {code} - {current['Title']}

Delivered November 7, 2026. **Art complete; live binding and gameplay/device tests pending.** Source coverage {covered}/82, {82-covered} designs remaining; batches 01-{code} complete for art, {remaining} packets planned. Next planned: {next_text}. {run_note}

{chr(10).join(table)}

{handoff_review}

{checks}

Delivery links and source: [full report](Art/Tier02/Batch{code}/final_report.md), [native scene](Art/Tier02/Batch{code}/tier02-batch{code}.blend), [six-view review](Art/Tier02/Batch{code}/batch{code}-six-view-review.png), [actual Unity overview](Art/Tier02/Batch{code}/batch{code}-unity-overview.png), [validation](Art/Tier02/Batch{code}/batch{code}-validation.json), [manifest](../Assets/_Game/Art/Crafting/Tier02/Tier02Batch{code}Manifest.json), [generator](../Tools/Art/generate_tier02_batch{code}.py). Preserve current recipe/input IDs, requests versus crafting ingredients, source exclusivity, timers and yields; other tool to resolve bindings and run current full-stack/debug and phone tests.

'''
    assert f'<a id="batch-{code}"></a>' not in text,'Already recorded; revise deliberately'
    point=text.index('<a id="batch-');text=text[:point]+section+text[point:];h.write_text(text,encoding='utf-8')
    brief=base/'remaining-tier2-design-plan-2026-11-06.md';text=brief.read_text(encoding='utf-8')
    text=re.sub(r'Batches 01-\d+ art have since been delivered',f'Batches 01-{code} art have since been delivered',text)
    text=text.replace('Glass, Glass bottle and Glass panels still need dedicated inventory meshes; windows on a house are not replacements.',
                      'Batch 08 supplies dedicated inventory meshes for Glass, Glass bottle and Glass panels; windows on a house are not replacements.')
    text=re.sub(r'After batch (?:01, eight had meshes;.*?|\d+, \*\*\d+ have source meshes and \d+ still need output designs\*\*\. )These counts describe outputs,',f'After batch {code}, **{covered} have source meshes and {82-covered} still need output designs**. These counts describe outputs,',text)
    marker='**Production schedule and current checkpoint:**';text=text[:text.index(marker)]+f'{marker} [45 restartable batches](tier2-production-batches-2026-11-06.md). Batches 01-{code} complete for passive art with source, selective Unity import and reviewed evidence. Runtime binding remains pending. [Maintained handoff](../../PROJECT_HANDOFF.md#batch-{code}). Coverage {covered}/82 source meshes, {82-covered} designs remaining; {remaining} packets planned. Next planned: {next_text}. {run_note}\n'
    brief.write_text(text,encoding='utf-8')
    for filename,marker,entry in [
        ('PROJECT_RECOMENDATIONS.md','**Tier-two batch',f'**Tier-two batch {code} delivered (November 7, 2026; passive art):** {", ".join(current["Subjects"])} now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. {validation["EarlierMeshHashesPreserved"]} earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-{code}). Progress {complete}/45 art batches; source coverage {covered}/82, {82-covered} designs remaining. Continue sequentially until told to stop.\n\n'),
        ('PROJECT_RECOMENDATIONS_ES.md','**Rango dos, lote',f'**Rango dos, lote {code} entregado (7 de noviembre de 2026; arte pasivo):** {", ".join(current["Subjects"])} tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron {validation["EarlierMeshHashesPreserved"]} mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-{code}). Progreso {complete}/45 lotes de arte; cobertura {covered}/82, faltan {82-covered} diseños. Continuar en orden hasta que el usuario pida parar.\n\n')]:
        entry=entry.replace('Continue sequentially until told to stop.',run_note)
        if args.stop_after:
            entry=entry.replace('Continuar en orden hasta que el usuario pida parar.',f'Trabajo detenido al completar el lote {code}; no iniciar el lote {batch+1:02} hasta que el usuario lo solicite.')
        path=ROOT/filename;text=path.read_text(encoding='utf-8-sig');assert marker in text
        path.write_text(text.replace(marker,entry+marker,1),encoding='utf-8')
    subprocess.run([sys.executable,str(ROOT/'Tools/Art/prepare_tier02_design_batches.py')],check=True,capture_output=True)
    print(f'Batch {code} recorded. {complete}/45 art batches; {covered}/82 recipe sources. Next: {next_text}.')


if __name__=='__main__':main()
