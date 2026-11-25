"""Remove zero-area triangles from OBJ files in place and prune vertices nothing references (Tier 3 integration slice C,
user November 24, 2026). It applies the SET16 OakCleanup operation to the current files, so later edits to them are kept.
Only 'v' lines and 'f' lines change; comments, object, smoothing, 'vt' and 'vn' lines keep their order and text.
Usage: python Tools/Art/clean_degenerate_obj.py FILE.obj [...]   (prints removed triangles/vertices per file)
"""
import sys

AREA2 = 1e-20  # squared cross-product length below which a triangle has zero area


def clean(path):
    lines = open(path, encoding='utf-8').read().split('\n')
    verts = [tuple(map(float, l.split()[1:4])) for l in lines if l.startswith('v ')]
    keep, removed = [], 0
    for l in lines:
        if not l.startswith('f '):
            keep.append(l); continue
        refs = l.split()[1:]
        if len(refs) != 3: raise SystemExit(f'{path}: only triangles are supported ({l})')
        a, b, c = (verts[int(r.split('/')[0]) - 1] for r in refs)
        u = [b[i] - a[i] for i in range(3)]; w = [c[i] - a[i] for i in range(3)]
        cross = (u[1]*w[2] - u[2]*w[1], u[2]*w[0] - u[0]*w[2], u[0]*w[1] - u[1]*w[0])
        if sum(x*x for x in cross) < AREA2: removed += 1; continue
        keep.append(l)
    used = sorted({int(r.split('/')[0]) for l in keep if l.startswith('f ') for r in l.split()[1:]})
    remap = {old: new for new, old in enumerate(used, 1)}
    out, index = [], 0
    for l in keep:
        if l.startswith('v '):
            index += 1
            if index in remap: out.append(l)
        elif l.startswith('f '):
            parts = []
            for r in l.split()[1:]:
                bits = r.split('/'); bits[0] = str(remap[int(bits[0])]); parts.append('/'.join(bits))
            out.append('f ' + ' '.join(parts))
        else:
            out.append(l)
    open(path, 'w', encoding='utf-8', newline='\n').write('\n'.join(out))
    print(f'{path}: removed {removed} zero-area triangles, {len(verts) - len(used)} unused vertices')


if __name__ == '__main__':
    for p in sys.argv[1:]: clean(p)
