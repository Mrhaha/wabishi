from __future__ import annotations

import hashlib
import json
import math
from dataclasses import dataclass
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[6]
HERE = Path(__file__).resolve().parent
QA = ROOT / "Docs/QA"
FONT = ROOT / "Assets/Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold.ttf"
FONT_MANIFEST = HERE / "wabish_physical_key_sans_sc_semibold_manifest.json"
OUTPUT = QA / "20260717_market_physical_key_runtime_metrics_v1.json"

ENABLED_INK = (24, 14, 8)
DISABLED_INK = (48, 38, 29)


@dataclass(frozen=True)
class KeySpec:
    key_id: str
    rect: tuple[int, int, int, int]
    minimum_core_height_720: int


KEYS = (
    KeySpec("sell", (71, 510, 148, 44), 18),
    KeySpec("buy_1", (335, 482, 191, 37), 18),
    KeySpec("buy_2", (629, 482, 191, 37), 18),
    KeySpec("buy_3", (922, 482, 191, 37), 18),
    KeySpec("leave", (928, 615, 269, 54), 21),
)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def scale_rect(rect: tuple[int, int, int, int], scale: float) -> tuple[int, int, int, int]:
    return tuple(round(value * scale) for value in rect)


def exact_ink_metrics(crop: Image.Image, ink: tuple[int, int, int]) -> dict[str, object]:
    width, height = crop.size
    pixels = crop.load()
    points = [(x, y) for y in range(height) for x in range(width) if pixels[x, y] == ink]
    if not points:
        return {"exact_core_pixels": 0, "bbox": None}

    xs = [point[0] for point in points]
    ys = [point[1] for point in points]
    left, top, right, bottom = min(xs), min(ys), max(xs) + 1, max(ys) + 1
    luminance = []
    for y in range(height):
        for x in range(width):
            pixel = pixels[x, y]
            if pixel == ink:
                continue
            luminance.append(0.2126 * pixel[0] + 0.7152 * pixel[1] + 0.0722 * pixel[2])
    mean = sum(luminance) / max(1, len(luminance))
    variance = sum((value - mean) ** 2 for value in luminance) / max(1, len(luminance))
    return {
        "exact_core_pixels": len(points),
        "bbox": [left, top, right, bottom],
        "core_width": right - left,
        "core_height": bottom - top,
        "core_center_y": (top + bottom) * 0.5,
        "safe_rect_center_y": height * 0.5,
        "center_offset_y": (top + bottom - height) * 0.5,
        "padding": {
            "left": left,
            "right": width - right,
            "top": top,
            "bottom": height - bottom,
        },
        "non_ink_luminance_stddev": math.sqrt(variance),
    }


def validate() -> dict[str, object]:
    failures: list[str] = []
    captures: dict[str, object] = {}
    font_manifest = json.loads(FONT_MANIFEST.read_text(encoding="utf-8"))
    actual_font_hash = sha256(FONT)
    if actual_font_hash != font_manifest["output_sha256"]:
        failures.append("Runtime font hash does not match the frozen manifest.")

    for width, height in ((1280, 720), (1920, 1080)):
        scale = width / 1280
        resolution_key = f"{width}x{height}"
        resolution_records: dict[str, object] = {}
        images: dict[str, Image.Image] = {}
        for state in ("normal", "purchase", "leave"):
            path = QA / f"20260717_market_runtime_{state}_{resolution_key}.png"
            if not path.is_file():
                failures.append(f"Missing capture: {path.name}")
                continue
            image = Image.open(path).convert("RGB")
            if image.size != (width, height):
                failures.append(f"Unexpected capture size for {path.name}: {image.size}")
            images[state] = image

        if "normal" not in images or "leave" not in images:
            continue

        normal_records: dict[str, object] = {}
        disabled_records: dict[str, object] = {}
        buy_center_lines: list[float] = []
        for spec in KEYS:
            x, y, rect_width, rect_height = scale_rect(spec.rect, scale)
            box = (x, y, x + rect_width, y + rect_height)
            normal_metrics = exact_ink_metrics(images["normal"].crop(box), ENABLED_INK)
            disabled_metrics = exact_ink_metrics(images["leave"].crop(box), DISABLED_INK)
            normal_records[spec.key_id] = normal_metrics
            disabled_records[spec.key_id] = disabled_metrics

            minimum_height = round(spec.minimum_core_height_720 * scale)
            if normal_metrics["exact_core_pixels"] <= 0:
                failures.append(f"{resolution_key} {spec.key_id}: enabled solid ink is missing.")
            elif normal_metrics["core_height"] < minimum_height:
                failures.append(
                    f"{resolution_key} {spec.key_id}: core height {normal_metrics['core_height']} < {minimum_height}."
                )
            if abs(float(normal_metrics.get("center_offset_y", 999))) > 2.5 * scale:
                failures.append(f"{resolution_key} {spec.key_id}: label is not vertically centered.")
            if float(normal_metrics.get("non_ink_luminance_stddev", 0)) < 10:
                failures.append(f"{resolution_key} {spec.key_id}: original key texture appears flattened.")
            if disabled_metrics["exact_core_pixels"] <= 0:
                failures.append(f"{resolution_key} {spec.key_id}: disabled solid ink is missing.")
            if float(disabled_metrics.get("non_ink_luminance_stddev", 0)) < 10:
                failures.append(f"{resolution_key} {spec.key_id}: disabled state lost the key texture.")
            if spec.key_id.startswith("buy_") and normal_metrics.get("core_center_y") is not None:
                buy_center_lines.append(float(normal_metrics["core_center_y"]))

        buy_baseline_spread = max(buy_center_lines) - min(buy_center_lines) if buy_center_lines else 999
        if buy_baseline_spread > 0.5 * scale:
            failures.append(f"{resolution_key}: buy labels do not share one horizontal line ({buy_baseline_spread}px spread).")

        resolution_records["normal_enabled_ink"] = normal_records
        resolution_records["leave_disabled_ink"] = disabled_records
        resolution_records["buy_label_center_line_spread_px"] = buy_baseline_spread
        resolution_records["native_capture_states"] = sorted(images.keys())
        captures[resolution_key] = resolution_records

    report = {
        "schema": "wabish-market-physical-key-runtime-validation-v1",
        "passed": not failures,
        "enabled_ink_rgb": list(ENABLED_INK),
        "disabled_ink_rgb": list(DISABLED_INK),
        "font_resource": str(FONT.relative_to(ROOT)).replace("\\", "/"),
        "font_sha256": actual_font_hash,
        "font_manifest_sha256": font_manifest["output_sha256"],
        "captures": captures,
        "failures": failures,
    }
    OUTPUT.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return report


if __name__ == "__main__":
    result = validate()
    print(json.dumps(result, ensure_ascii=False, indent=2))
    raise SystemExit(0 if result["passed"] else 1)
