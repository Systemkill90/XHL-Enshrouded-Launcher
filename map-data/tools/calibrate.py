#!/usr/bin/env python3
"""ShroudNav affine map calibration; stdlib only, no game connection.

Input: a calibration entry with >=3 non-collinear trainingPoints and >=1
independent holdoutPoints; each has pixel:[x,y], world:[x,y].
Coordinates are *declared* map units, never inferred from image dimensions.
"""
import math


class CalibrationError(ValueError):
    pass


def _finite_pair(pair):
    return (isinstance(pair, list) and len(pair) == 2
            and all(isinstance(v, (int, float)) and not isinstance(v, bool)
                    and math.isfinite(v) and abs(v) <= 1e7 for v in pair))


def _solve3(matrix, values):
    a = [list(row) + [float(rhs)] for row, rhs in zip(matrix, values)]
    for col in range(3):
        pivot = max(range(col, 3), key=lambda r: abs(a[r][col]))
        if abs(a[pivot][col]) < 1e-9:
            raise CalibrationError("Control points are collinear or numerically degenerate.")
        a[col], a[pivot] = a[pivot], a[col]
        scale = a[col][col]
        for j in range(col, 4):
            a[col][j] /= scale
        for row in range(3):
            if row == col:
                continue
            factor = a[row][col]
            for j in range(col, 4):
                a[row][j] -= factor * a[col][j]
    return [a[i][3] for i in range(3)]


def _fit(points, axis):
    # Ordinary least squares affine transform from (image x,y,1) to world coordinate.
    rows = [[p["pixel"][0], p["pixel"][1], 1.0] for p in points]
    b = [p["world"][axis] for p in points]
    normal = [[sum(row[i] * row[j] for row in rows) for j in range(3)]
              for i in range(3)]
    target = [sum(row[i] * v for row, v in zip(rows, b)) for i in range(3)]
    return _solve3(normal, target)


def apply(transform, pixel):
    if len(transform) != 6 or not _finite_pair(pixel):
        raise CalibrationError("Invalid affine transform or pixel location.")
    x, y = pixel
    a, b, c, d, e, f = transform
    xx, yy = a*x + b*y + c, d*x + e*y + f
    if not all(math.isfinite(v) and abs(v) <= 1e7 for v in (xx, yy)):
        raise CalibrationError("Affine projection returned non-finite/out-of-range coordinates.")
    return [xx, yy]


def validate(entry):
    if not isinstance(entry, dict) or entry.get("status") != "approved":
        raise CalibrationError("A reviewed, approved calibration is required.")
    if not isinstance(entry.get("sourceImageId"), str) or not entry["sourceImageId"]:
        raise CalibrationError("Calibration must identify its exact source image.")
    if entry.get("targetFrame") != "enshrouded-world-xy-calibrated-v1":
        raise CalibrationError("Calibration target is not the ShroudNav world frame.")
    training, holdouts = entry.get("trainingPoints"), entry.get("holdoutPoints")
    if not isinstance(training, list) or len(training) < 3 or len(training) > 100:
        raise CalibrationError("At least 3 training control points are required.")
    if not isinstance(holdouts, list) or not (1 <= len(holdouts) <= 100):
        raise CalibrationError("A separate held-out landmark is required.")
    for point in training + holdouts:
        if not isinstance(point, dict) or not _finite_pair(point.get("pixel")) \
                or not _finite_pair(point.get("world")):
            raise CalibrationError("Malformed control-point coordinates.")
    threshold = entry.get("maxHoldoutError")
    if not isinstance(threshold, (int, float)) or isinstance(threshold, bool) \
            or not math.isfinite(threshold) or threshold <= 0 or threshold > 1000:
        raise CalibrationError("Explicit finite holdout error threshold is required.")
    transform = _fit(training, 0) + _fit(training, 1)
    if not all(math.isfinite(x) for x in transform):
        raise CalibrationError("Numerically invalid affine calibration.")
    errors = []
    for point in holdouts:
        projected = apply(transform, point["pixel"])
        errors.append(math.dist(projected, point["world"]))
    max_error = max(errors)
    if max_error > threshold:
        raise CalibrationError("Independent landmark residual {:.3f} exceeds {:.3f} map units.".format(max_error, threshold))
    return transform, {"maxHoldoutError": max_error,
                       "rmsHoldoutError": math.sqrt(sum(x*x for x in errors)/len(errors)),
                       "holdoutCount": len(holdouts), "trainingCount": len(training)}
