"""Read-only QA of apparent at-grade road crossings.

Never turns a crossing into a junction. Shared sampled vertices mean a deliberate
connection; crossing lines without a shared vertex must be reviewed. Bridges and
tunnels in different layer IDs are excluded from at-grade crossing checks.
"""


def _orient(a, b, c):
    return (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])


def _proper_cross(a, b, c, d, tol=1e-8):
    ab_c, ab_d = _orient(a, b, c), _orient(a, b, d)
    cd_a, cd_b = _orient(c, d, a), _orient(c, d, b)
    return ((ab_c > tol and ab_d < -tol or ab_c < -tol and ab_d > tol)
            and (cd_a > tol and cd_b < -tol or cd_a < -tol and cd_b > tol))


def audit_unjoined_crossings(segments, max_pairs=6_000_000):
    """Returns (complete, warnings). Each segment is (a,b,layer,road_id).

    Bounds comparison work; incomplete audit MUST NOT be treated as verification.
    """
    warnings = []
    n = len(segments)
    if n*(n-1)//2 > max_pairs:
        return False, ["Too many segment pairs for complete at-grade crossing audit."]
    for i in range(n):
        a, b, layer, road_id = segments[i]
        for j in range(i+1, n):
            c, d, layer2, road_id2 = segments[j]
            if layer != layer2:
                continue
            if a in (c, d) or b in (c, d):
                continue
            # Bounding-box fast reject.
            if max(a[0], b[0]) < min(c[0], d[0]) or max(c[0], d[0]) < min(a[0], b[0]):
                continue
            if max(a[1], b[1]) < min(c[1], d[1]) or max(c[1], d[1]) < min(a[1], b[1]):
                continue
            if _proper_cross(a, b, c, d):
                if len(warnings) < 25:
                    warnings.append("Unjoined same-layer crossing between {} and {}. Verify junction or declare grade separation."
                                    .format(road_id, road_id2))
                # Additional crossings beyond the cap still make the review incomplete.
                if len(warnings) >= 25:
                    return False, warnings
    return True, warnings
