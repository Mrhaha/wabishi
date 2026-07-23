#!/usr/bin/env python3
"""构建 F021 骰子类型芯 V1 的透明运行资源、活动遮罩与 QA 总表。"""

from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont


GROUPS = {
    "基础与中立": [
        "basic", "lightfang", "duet", "trigger", "crown", "relief", "airstrike", "pact", "stitch",
    ],
    "猪猪": [
        "pig_farmer", "meat_pig", "trade_pig", "sow_pig", "three_little_pigs", "greedy_pig", "feed_wholesaler",
    ],
    "恶魔与贡品": ["imp", "devourer", "demon", "demon_bat", "abyss_summon", "tribute"],
    "龟龟": [
        "money_turtle", "tiny_turtle", "double_turtle", "lucky_turtle", "magnet_turtle", "rally_turtle", "leader_turtle",
    ],
    "海盗": [
        "refresh_pirate", "plunder_pirate", "crew_pirate", "pirate_captain", "training_pirate", "treasure_pirate",
        "robbery_pirate", "pirate_king",
    ],
}

SLUGS = [slug for group in GROUPS.values() for slug in group]
FAMILY_EDGE = {
    "基础与中立": (171, 142, 88, 255),
    "猪猪": (198, 103, 87, 255),
    "恶魔与贡品": (143, 62, 48, 255),
    "龟龟": (55, 120, 113, 255),
    "海盗": (51, 79, 104, 255),
}


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = [
        Path("C:/Windows/Fonts/consolab.ttf" if bold else "C:/Windows/Fonts/consola.ttf"),
        Path("C:/Windows/Fonts/arialbd.ttf" if bold else "C:/Windows/Fonts/arial.ttf"),
    ]
    for candidate in candidates:
        if candidate.exists():
            return ImageFont.truetype(str(candidate), size)
    return ImageFont.load_default()


def resize_rgba(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    return image.convert("RGBA").resize(size, Image.Resampling.LANCZOS)


def fit_rgba(image: Image.Image, box: tuple[int, int]) -> Image.Image:
    result = image.convert("RGBA").copy()
    result.thumbnail(box, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", box, (0, 0, 0, 0))
    canvas.alpha_composite(result, ((box[0] - result.width) // 2, (box[1] - result.height) // 2))
    return canvas


def activity_mask(image: Image.Image) -> tuple[Image.Image, int]:
    pixels = np.asarray(image.convert("RGBA"), dtype=np.uint8)
    rgb = pixels[..., :3].astype(np.float32) / 255.0
    alpha = pixels[..., 3].astype(np.float32) / 255.0
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    maximum = rgb.max(axis=2)
    minimum = rgb.min(axis=2)
    saturation = maximum - minimum

    amber = (r > 0.58) & (g > 0.28) & (r >= g * 0.92) & (g > b * 1.30) & ((r + g) * 0.5 > b + 0.16)
    cyan = (g > 0.48) & (b > 0.40) & (g > r * 1.12) & (b > r * 1.06)
    colored_lamp = (maximum > 0.76) & (saturation > 0.34)
    selected = (amber | cyan | colored_lamp) & (alpha > 0.08)

    if int(selected.sum()) < 24:
        subject = alpha > 0.25
        warm_score = (r + g - b * 1.55) + saturation * 0.45 + maximum * 0.2
        candidates = np.where(subject, warm_score, -999.0)
        threshold = np.partition(candidates.reshape(-1), -128)[-128]
        selected = subject & (candidates >= threshold)

    strength = np.clip((maximum - 0.42) / 0.58, 0.34, 1.0)
    out = np.zeros_like(pixels)
    out[..., :3] = pixels[..., :3]
    out[..., 3] = np.where(selected, np.clip(alpha * strength * 255.0, 0, 255), 0).astype(np.uint8)
    return Image.fromarray(out, "RGBA"), int(np.count_nonzero(out[..., 3]))


def save_png(image: Image.Image, target: Path, force: bool) -> None:
    if target.exists() and not force:
        raise FileExistsError(f"拒绝覆盖既有文件：{target}")
    target.parent.mkdir(parents=True, exist_ok=True)
    image.save(target, optimize=True)


def make_review_sheet(images: dict[str, Image.Image], activities: dict[str, Image.Image], output: Path) -> None:
    columns, rows = 7, 6
    cell_w, cell_h = 240, 160
    sheet = Image.new("RGBA", (columns * cell_w, rows * cell_h), (5, 17, 25, 255))
    draw = ImageDraw.Draw(sheet)
    label_font = font(15, True)
    number_font = font(35, True)
    slug_to_group = {slug: group for group, slugs in GROUPS.items() for slug in slugs}

    for index, slug in enumerate(SLUGS):
        col, row = index % columns, index // columns
        x, y = col * cell_w, row * cell_h
        edge = FAMILY_EDGE[slug_to_group[slug]]
        draw.rounded_rectangle((x + 4, y + 4, x + cell_w - 5, y + cell_h - 5), 10, fill=(8, 27, 35, 255), outline=edge, width=2)
        draw.text((x + 12, y + 9), slug, font=label_font, fill=(237, 194, 103, 255))

        ready_face = (x + 12, y + 36, x + 124, y + 146)
        draw.rounded_rectangle(ready_face, 13, fill=(204, 187, 157, 255), outline=(74, 64, 51, 255), width=3)
        ready = fit_rgba(images[slug], (80, 76))
        sheet.alpha_composite(ready, (x + 28, y + 52))

        result_face = (x + 139, y + 36, x + 226, y + 146)
        draw.rounded_rectangle(result_face, 11, fill=(204, 187, 157, 255), outline=(74, 64, 51, 255), width=3)
        number = "8888"
        number_box = draw.textbbox((0, 0), number, font=number_font)
        number_w = number_box[2] - number_box[0]
        draw.text((x + 182 - number_w // 2, y + 48), number, font=number_font, fill=(24, 23, 20, 255))
        mini = fit_rgba(images[slug], (42, 25))
        sheet.alpha_composite(mini, (x + 161, y + 108))

        activity = fit_rgba(activities[slug], (28, 28))
        sheet.alpha_composite(activity, (x + 199, y + 8))

    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.convert("RGB").save(output, quality=95)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--force", action="store_true", help="允许覆盖本脚本先前生成的同名文件")
    args = parser.parse_args()

    here = Path(__file__).resolve().parent
    repo = here.parents[4]
    source_dir = here / "RuntimeSourcesV1" / "transparent"
    runtime_dir = repo / "Assets" / "Resources" / "Art" / "DiceTypes"
    review_dir = here / "RuntimeSourcesV1" / "qa"
    metrics: dict[str, object] = {"expected_count": len(SLUGS), "items": {}}
    images: dict[str, Image.Image] = {}
    activities: dict[str, Image.Image] = {}

    for slug in SLUGS:
        full_path = source_dir / f"arcade_type_core_{slug}_alpha_full_v1.png"
        if not full_path.exists():
            raise FileNotFoundError(full_path)
        image = resize_rgba(Image.open(full_path), (512, 512))
        activity, activity_pixels = activity_mask(image)

        source_static = source_dir / f"arcade_type_core_{slug}_v1.png"
        source_activity = source_dir / f"arcade_type_core_{slug}_activity_v1.png"
        runtime_static = runtime_dir / f"arcade_type_core_{slug}.png"
        runtime_activity = runtime_dir / f"arcade_type_core_{slug}_activity.png"
        for target, payload in (
            (source_static, image), (source_activity, activity), (runtime_static, image), (runtime_activity, activity),
        ):
            if target.exists() and not args.force:
                existing = Image.open(target).convert("RGBA")
                if existing.size != payload.size or existing.tobytes() != payload.tobytes():
                    raise FileExistsError(f"拒绝覆盖内容不同的既有文件：{target}")
            else:
                save_png(payload, target, args.force)

        alpha = np.asarray(image.getchannel("A"), dtype=np.uint8)
        bbox = image.getbbox()
        corners = [int(alpha[0, 0]), int(alpha[0, -1]), int(alpha[-1, 0]), int(alpha[-1, -1])]
        opaque_pixels = int(np.count_nonzero(alpha > 16))
        bbox_coverage = 0.0 if bbox is None else ((bbox[2] - bbox[0]) * (bbox[3] - bbox[1])) / float(512 * 512)
        pass_contract = bool(
            image.size == (512, 512)
            and max(corners) == 0
            and bbox is not None
            and 0.18 <= bbox_coverage <= 0.90
            and activity_pixels >= 24
        )
        metrics["items"][slug] = {
            "size": list(image.size),
            "mode": image.mode,
            "alpha_bbox": list(bbox) if bbox else None,
            "bbox_coverage": round(bbox_coverage, 4),
            "opaque_pixels": opaque_pixels,
            "corner_alpha": corners,
            "activity_pixels": activity_pixels,
            "pass": pass_contract,
        }
        if not pass_contract:
            raise RuntimeError(f"资源合同未通过：{slug}")
        images[slug] = image
        activities[slug] = activity

    metrics["actual_count"] = len(images)
    metrics["all_pass"] = all(item["pass"] for item in metrics["items"].values())
    metrics_path = review_dir / "arcade_type_core_runtime_metrics_v1_20260717.json"
    metrics_path.parent.mkdir(parents=True, exist_ok=True)
    metrics_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2), encoding="utf-8")
    make_review_sheet(images, activities, review_dir / "arcade_type_core_runtime_qa_80x76_v1_20260717.png")
    print(f"已构建 {len(images)} 张静态图、{len(activities)} 张活动遮罩。")
    print(metrics_path)


if __name__ == "__main__":
    main()
