#!/usr/bin/env python3
"""Build a deterministic 0.606x LED-text survivability review from raw Unity captures.

The Unity editor screenshots that exposed this issue used a 1920x1080 Game View at
Scale 0.606.  Raw captures remain the sharpness authority; this derivative checks
whether the semantic hierarchy survives the same non-integer resampling pressure.
"""

from __future__ import annotations

import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont


SCALE = 0.606
TARGET_SIZE = (round(1920 * SCALE), round(1080 * SCALE))

SCRIPT_PATH = Path(__file__).resolve()
PROJECT_ROOT = SCRIPT_PATH.parents[6]
QA_DIR = PROJECT_ROOT / "Docs" / "QA"

AFTER_INPUTS = {
    "main_menu": QA_DIR / "20260717_led_brightness_runtime_main_menu_1920x1080.png",
    "run_ready": QA_DIR / "20260716_main_game_runtime_ready_1920x1080.png",
    "market": QA_DIR / "20260717_market_runtime_normal_1920x1080.png",
}

BEFORE_INPUTS = {
    "main_menu": QA_DIR / "20260717_led_brightness_feedback_main_menu_editor_0606.png",
    "run_ready": QA_DIR / "20260717_led_brightness_before_run_ready_1920x1080.png",
    "market": QA_DIR / "20260717_led_brightness_before_market_1920x1080.png",
}

OUTPUTS = {
    key: QA_DIR / f"20260717_led_brightness_runtime_{key}_0606.png"
    for key in AFTER_INPUTS
}

# Normalized boxes deliberately cover complete UI modules rather than hand-picked
# glyph pixels.  The report therefore remains stable when the text content changes.
REGIONS = {
    "main_menu": {
        "primary_cards": (0.29, 0.26, 0.71, 0.69),
        "confirm_key": (0.30, 0.74, 0.70, 0.91),
    },
    "run_ready": {
        "top_hud": (0.00, 0.00, 1.00, 0.13),
        "target_panel": (0.00, 0.12, 0.17, 0.76),
        "terminal": (0.77, 0.15, 1.00, 0.63),
        "action_key": (0.28, 0.75, 0.72, 0.98),
    },
    "market": {
        "top_hud": (0.00, 0.00, 1.00, 0.14),
        "product_names": (0.18, 0.47, 0.97, 0.71),
        "refresh_key": (0.00, 0.79, 0.27, 1.00),
        "feedback": (0.20, 0.80, 0.79, 1.00),
    },
}

ROLE_CORE_COLORS = {
    "focus_amber": (255, 211, 106),
    "focus_teal": (120, 241, 228),
    "primary_amber": (255, 192, 74),
    "secondary_amber": (217, 154, 74),
    "secondary_warm": (189, 165, 122),
}


def normalized_crop(image: Image.Image, box: tuple[float, float, float, float]) -> Image.Image:
    width, height = image.size
    left, top, right, bottom = box
    return image.crop(
        (
            round(left * width),
            round(top * height),
            round(right * width),
            round(bottom * height),
        )
    )


def region_metrics(image: Image.Image, box: tuple[float, float, float, float]) -> dict[str, object]:
    pixels = np.asarray(normalized_crop(image, box).convert("RGB"), dtype=np.float32) / 255.0
    luminance = 0.2126 * pixels[..., 0] + 0.7152 * pixels[..., 1] + 0.0722 * pixels[..., 2]
    saturation = pixels.max(axis=2) - pixels.min(axis=2)
    bright_chroma = (luminance >= 0.42) & (saturation >= 0.10)
    return {
        "p95_luminance": round(float(np.percentile(luminance, 95)), 6),
        "p99_luminance": round(float(np.percentile(luminance, 99)), 6),
        "bright_chroma_pixels": int(bright_chroma.sum()),
        "bright_chroma_ratio": round(float(bright_chroma.mean()), 6),
    }


def exact_role_counts(image: Image.Image, box: tuple[float, float, float, float]) -> dict[str, int]:
    pixels = np.asarray(normalized_crop(image, box).convert("RGB"), dtype=np.uint8)
    return {
        role: int(np.all(pixels == np.asarray(rgb, dtype=np.uint8), axis=2).sum())
        for role, rgb in ROLE_CORE_COLORS.items()
    }


def load_before(key: str) -> Image.Image:
    image = Image.open(BEFORE_INPUTS[key]).convert("RGB")
    if key == "main_menu":
        # Strip Unity chrome and letterbox from the supplied editor screenshot.
        # The visible Game View occupies y=115..769 at the reported 0.606 scale.
        image = image.crop((0, 115, image.width, 770))
    return image.resize(TARGET_SIZE, Image.Resampling.BILINEAR)


def load_font(size: int) -> ImageFont.ImageFont:
    for candidate in (Path("C:/Windows/Fonts/arialbd.ttf"), Path("C:/Windows/Fonts/arial.ttf")):
        if candidate.exists():
            return ImageFont.truetype(str(candidate), size=size)
    return ImageFont.load_default()


def build_contact_sheet(before: dict[str, Image.Image], after: dict[str, Image.Image]) -> Path:
    thumb_size = (560, 315)
    margin = 22
    header_height = 58
    row_gap = 44
    width = margin * 3 + thumb_size[0] * 2
    height = header_height + (thumb_size[1] + row_gap) * 3 + margin
    sheet = Image.new("RGB", (width, height), (13, 15, 17))
    draw = ImageDraw.Draw(sheet)
    title_font = load_font(22)
    row_font = load_font(18)
    draw.text((margin, 16), "BEFORE - USER REPORTED", fill=(184, 168, 138), font=title_font)
    draw.text((margin * 2 + thumb_size[0], 16), "AFTER - SEMANTIC RUNTIME ROLES", fill=(255, 211, 106), font=title_font)

    for row, key in enumerate(("main_menu", "run_ready", "market")):
        y = header_height + row * (thumb_size[1] + row_gap)
        left = before[key].resize(thumb_size, Image.Resampling.BILINEAR)
        right = after[key].resize(thumb_size, Image.Resampling.BILINEAR)
        sheet.paste(left, (margin, y))
        sheet.paste(right, (margin * 2 + thumb_size[0], y))
        draw.rectangle((margin - 1, y - 1, margin + thumb_size[0], y + thumb_size[1]), outline=(74, 69, 60))
        draw.rectangle(
            (margin * 2 + thumb_size[0] - 1, y - 1, margin * 2 + thumb_size[0] * 2, y + thumb_size[1]),
            outline=(105, 89, 54),
        )
        draw.text((margin, y + thumb_size[1] + 8), key.upper(), fill=(205, 205, 205), font=row_font)

    output = QA_DIR / "20260717_led_brightness_runtime_0606_before_after.png"
    sheet.save(output, optimize=True)
    return output


def main() -> None:
    QA_DIR.mkdir(parents=True, exist_ok=True)
    after_stress: dict[str, Image.Image] = {}
    before_stress: dict[str, Image.Image] = {}
    report: dict[str, object] = {
        "scale": SCALE,
        "source_size": [1920, 1080],
        "stress_size": list(TARGET_SIZE),
        "resampler": "Pillow Image.Resampling.BILINEAR",
        "screens": {},
    }

    for key, source in AFTER_INPUTS.items():
        if not source.exists():
            raise FileNotFoundError(source)
        raw = Image.open(source).convert("RGB")
        if raw.size != (1920, 1080):
            raise ValueError(f"{source.name}: expected 1920x1080, got {raw.size}")
        stress = raw.resize(TARGET_SIZE, Image.Resampling.BILINEAR)
        stress.save(OUTPUTS[key], optimize=True)
        after_stress[key] = stress
        before_stress[key] = load_before(key)

        screen_metrics: dict[str, object] = {}
        for region_name, box in REGIONS[key].items():
            screen_metrics[region_name] = {
                "normalized_box": list(box),
                "raw_exact_role_core_pixels": exact_role_counts(raw, box),
                "stress": region_metrics(stress, box),
                "before_stress": region_metrics(before_stress[key], box),
            }
        report["screens"][key] = screen_metrics

    contact_sheet = build_contact_sheet(before_stress, after_stress)

    # Critical modules must retain a meaningful bright/chromatic population after
    # non-integer scaling.  Raw exact-role counts additionally prove the runtime is
    # using the authoritative semantic palette, not an arbitrary legacy tint.
    failures: list[str] = []
    for screen_name, screen in report["screens"].items():
        for region_name, metrics in screen.items():
            exact_total = sum(metrics["raw_exact_role_core_pixels"].values())
            stress_bright = metrics["stress"]["bright_chroma_pixels"]
            if exact_total < 120:
                failures.append(f"{screen_name}/{region_name}: only {exact_total} exact role-core pixels")
            if stress_bright < 60:
                failures.append(f"{screen_name}/{region_name}: only {stress_bright} bright chromatic stress pixels")

    report["contact_sheet"] = contact_sheet.name
    report["outputs"] = {key: path.name for key, path in OUTPUTS.items()}
    report["status"] = "PASS" if not failures else "FAIL"
    report["failures"] = failures
    report_path = QA_DIR / "20260717_led_brightness_runtime_0606_metrics.json"
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    if failures:
        raise RuntimeError("; ".join(failures))
    print(f"PASS: 0.606x stress outputs built at {TARGET_SIZE[0]}x{TARGET_SIZE[1]}")
    print(f"PASS: {len(REGIONS)} screens / {sum(len(value) for value in REGIONS.values())} critical regions")
    print(f"Report: {report_path}")


if __name__ == "__main__":
    main()
