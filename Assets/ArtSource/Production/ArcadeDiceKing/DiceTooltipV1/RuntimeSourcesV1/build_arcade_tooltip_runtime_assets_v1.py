from __future__ import annotations

import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw


SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = Path(__file__).resolve().parents[6]
SOURCE_PATH = SCRIPT_DIR / "arcade_tooltip_runtime_parts_chromakey_source_v1_20260717.png"
RUNTIME_DIR = PROJECT_ROOT / "Assets" / "Resources" / "Art" / "UI" / "Tooltip"
PREVIEW_PATH = SCRIPT_DIR / "arcade_tooltip_runtime_assets_preview_v1_20260717.png"
METRICS_PATH = SCRIPT_DIR / "arcade_tooltip_runtime_assets_metrics_v1_20260717.json"

REGIONS = {
    "short": (0, 0, 1200, 260),
    "medium": (0, 260, 1200, 540),
    "tall": (0, 540, 1200, 941),
    "face_cell": (1200, 250, 1672, 470),
    "type_core": (1200, 470, 1672, 800),
}

OUTPUTS = {
    "short": ("ui_tooltip_arcade_panel_short.png", (896, 392)),
    "medium": ("ui_tooltip_arcade_panel_medium.png", (896, 488)),
    "tall": ("ui_tooltip_arcade_panel_tall.png", (896, 576)),
    "face_cell": ("ui_tooltip_arcade_face_cell.png", (120, 88)),
    "type_core": ("ui_tooltip_arcade_type_core_frame.png", (128, 128)),
}


def chroma_to_rgba(rgb_image: Image.Image, region: tuple[int, int, int, int]) -> tuple[Image.Image, tuple[int, int, int, int]]:
    crop = np.asarray(rgb_image.crop(region).convert("RGB"), dtype=np.float32)
    red = crop[:, :, 0]
    green = crop[:, :, 1]
    blue = crop[:, :, 2]

    dominance = green - np.maximum(red, blue)
    alpha = np.clip((140.0 - dominance) / (140.0 - 35.0), 0.0, 1.0)

    background = np.array([17.0, 241.0, 12.0], dtype=np.float32)
    safe_alpha = np.maximum(alpha[:, :, None], 1.0 / 255.0)
    recovered = (crop - (1.0 - alpha[:, :, None]) * background) / safe_alpha
    recovered = np.clip(recovered, 0.0, 255.0)

    rgba = np.zeros((crop.shape[0], crop.shape[1], 4), dtype=np.uint8)
    rgba[:, :, :3] = np.rint(recovered).astype(np.uint8)
    rgba[:, :, 3] = np.rint(alpha * 255.0).astype(np.uint8)
    rgba[rgba[:, :, 3] == 0, :3] = 0

    visible = rgba[:, :, 3] > 4
    ys, xs = np.where(visible)
    if xs.size == 0:
        raise RuntimeError(f"No visible pixels found in region {region}.")

    pad = 2
    left = max(0, int(xs.min()) - pad)
    top = max(0, int(ys.min()) - pad)
    right = min(rgba.shape[1], int(xs.max()) + 1 + pad)
    bottom = min(rgba.shape[0], int(ys.max()) + 1 + pad)
    extracted = Image.fromarray(rgba, "RGBA").crop((left, top, right, bottom))
    bbox = (region[0] + left, region[1] + top, region[0] + right, region[1] + bottom)
    return extracted, bbox


def resize_piece(piece: Image.Image, size: tuple[int, int]) -> Image.Image:
    if piece.size == size:
        return piece
    return piece.resize(size, Image.Resampling.LANCZOS)


def nine_slice(
    source: Image.Image,
    output_size: tuple[int, int],
    source_margins: tuple[int, int, int, int],
    output_margins: tuple[int, int, int, int],
) -> Image.Image:
    source_width, source_height = source.size
    output_width, output_height = output_size
    source_left, source_top, source_right, source_bottom = source_margins
    output_left, output_top, output_right, output_bottom = output_margins

    if source_left + source_right >= source_width or source_top + source_bottom >= source_height:
        raise ValueError(f"Source margins do not fit {source.size}.")
    if output_left + output_right >= output_width or output_top + output_bottom >= output_height:
        raise ValueError(f"Output margins do not fit {output_size}.")

    source_x = (0, source_left, source_width - source_right, source_width)
    source_y = (0, source_top, source_height - source_bottom, source_height)
    output_x = (0, output_left, output_width - output_right, output_width)
    output_y = (0, output_top, output_height - output_bottom, output_height)

    result = Image.new("RGBA", output_size, (0, 0, 0, 0))
    for row in range(3):
        for column in range(3):
            source_box = (
                source_x[column],
                source_y[row],
                source_x[column + 1],
                source_y[row + 1],
            )
            output_box = (
                output_x[column],
                output_y[row],
                output_x[column + 1],
                output_y[row + 1],
            )
            target_size = (output_box[2] - output_box[0], output_box[3] - output_box[1])
            piece = resize_piece(source.crop(source_box), target_size)
            result.alpha_composite(piece, (output_box[0], output_box[1]))

    return result


def checkerboard(size: tuple[int, int], cell: int = 24) -> Image.Image:
    image = Image.new("RGBA", size, (28, 33, 39, 255))
    draw = ImageDraw.Draw(image)
    colors = ((42, 49, 57, 255), (56, 64, 73, 255))
    for y in range(0, size[1], cell):
        for x in range(0, size[0], cell):
            draw.rectangle(
                (x, y, min(size[0], x + cell), min(size[1], y + cell)),
                fill=colors[(x // cell + y // cell) % 2],
            )
    return image


def alpha_composite_scaled(canvas: Image.Image, source: Image.Image, box: tuple[int, int, int, int]) -> None:
    width = box[2] - box[0]
    height = box[3] - box[1]
    scaled = source.resize((width, height), Image.Resampling.LANCZOS)
    canvas.alpha_composite(scaled, (box[0], box[1]))


def asset_metrics(image: Image.Image) -> dict[str, object]:
    rgba = np.asarray(image.convert("RGBA"))
    alpha = rgba[:, :, 3]
    visible = alpha > 4
    green_fringe = (
        visible
        & (rgba[:, :, 1] > 100)
        & (rgba[:, :, 1] > rgba[:, :, 0] * 1.35)
        & (rgba[:, :, 1] > rgba[:, :, 2] * 1.25)
    )
    ys, xs = np.where(visible)
    visible_bounds = None
    if xs.size:
        visible_bounds = [int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1]
    return {
        "width": image.width,
        "height": image.height,
        "visible_bounds": visible_bounds,
        "transparent_pixel_count": int((alpha == 0).sum()),
        "partial_alpha_pixel_count": int(((alpha > 0) & (alpha < 255)).sum()),
        "green_fringe_pixel_count": int(green_fringe.sum()),
    }


def main() -> None:
    if not SOURCE_PATH.exists():
        raise FileNotFoundError(SOURCE_PATH)

    RUNTIME_DIR.mkdir(parents=True, exist_ok=True)
    source = Image.open(SOURCE_PATH).convert("RGB")

    extracted: dict[str, Image.Image] = {}
    source_boxes: dict[str, list[int]] = {}
    for name, region in REGIONS.items():
        image, bbox = chroma_to_rgba(source, region)
        extracted[name] = image
        source_boxes[name] = list(bbox)

    outputs: dict[str, Image.Image] = {}
    for name in ("short", "medium", "tall"):
        filename, size = OUTPUTS[name]
        outputs[name] = nine_slice(
            extracted[name],
            size,
            source_margins=(60, 42, 60, 60),
            output_margins=(52, 42, 52, 60),
        )
        outputs[name].save(RUNTIME_DIR / filename, optimize=True)

    cell_filename, cell_size = OUTPUTS["face_cell"]
    outputs["face_cell"] = nine_slice(
        extracted["face_cell"],
        cell_size,
        source_margins=(34, 28, 34, 28),
        output_margins=(18, 18, 18, 18),
    )
    outputs["face_cell"].save(RUNTIME_DIR / cell_filename, optimize=True)

    core_filename, core_size = OUTPUTS["type_core"]
    outputs["type_core"] = extracted["type_core"].resize(core_size, Image.Resampling.LANCZOS)
    outputs["type_core"].save(RUNTIME_DIR / core_filename, optimize=True)

    preview = checkerboard((1600, 1000))
    alpha_composite_scaled(preview, outputs["short"], (48, 44, 720, 338))
    alpha_composite_scaled(preview, outputs["medium"], (48, 372, 720, 738))
    alpha_composite_scaled(preview, outputs["tall"], (850, 44, 1522, 476))
    alpha_composite_scaled(preview, outputs["face_cell"], (850, 560, 1090, 736))
    alpha_composite_scaled(preview, outputs["type_core"], (1190, 520, 1446, 776))
    preview.save(PREVIEW_PATH, optimize=True)

    report = {
        "source": str(SOURCE_PATH.relative_to(PROJECT_ROOT)).replace("\\", "/"),
        "source_dimensions": [source.width, source.height],
        "source_boxes": source_boxes,
        "outputs": {
            name: {
                "path": str((RUNTIME_DIR / OUTPUTS[name][0]).relative_to(PROJECT_ROOT)).replace("\\", "/"),
                "expected_dimensions": list(OUTPUTS[name][1]),
                "metrics": asset_metrics(image),
            }
            for name, image in outputs.items()
        },
        "preview": str(PREVIEW_PATH.relative_to(PROJECT_ROOT)).replace("\\", "/"),
    }
    METRICS_PATH.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
