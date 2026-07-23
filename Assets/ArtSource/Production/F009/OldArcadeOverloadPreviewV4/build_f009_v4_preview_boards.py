from __future__ import annotations

import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
FRAME_SIZE = (960, 540)
CANVAS_SIZE = (2048, 1280)
BACKGROUND = (7, 13, 20)
CARD_BACKGROUND = (11, 20, 29)
ACCENT = (222, 157, 64)
TEXT = (247, 226, 188)
MUTED = (123, 139, 146)


def load_font(size: int) -> ImageFont.FreeTypeFont:
    candidates = (
        Path(r"C:\Windows\Fonts\msyh.ttc"),
        Path(r"C:\Windows\Fonts\simhei.ttf"),
        Path(r"C:\Windows\Fonts\arial.ttf"),
    )
    for path in candidates:
        if path.exists():
            return ImageFont.truetype(str(path), size=size)
    return ImageFont.load_default()


TITLE_FONT = load_font(42)
LABEL_FONT = load_font(34)
NOTE_FONT = load_font(22)


def load_frame(filename: str) -> tuple[Image.Image, tuple[int, int]]:
    image = Image.open(ROOT / filename).convert("RGB")
    original_size = image.size
    image = image.resize(FRAME_SIZE, Image.Resampling.LANCZOS)
    return image, original_size


def draw_card(
    canvas: Image.Image,
    frame: Image.Image,
    label: str,
    x: int,
    y: int,
) -> None:
    draw = ImageDraw.Draw(canvas)
    label_height = 46
    card_rect = (x - 3, y - 3, x + FRAME_SIZE[0] + 3, y + label_height + FRAME_SIZE[1] + 3)
    draw.rounded_rectangle(card_rect, radius=10, fill=CARD_BACKGROUND, outline=ACCENT, width=3)
    label_box = draw.textbbox((0, 0), label, font=LABEL_FONT)
    label_width = label_box[2] - label_box[0]
    draw.text((x + (FRAME_SIZE[0] - label_width) / 2, y + 3), label, font=LABEL_FONT, fill=TEXT)
    canvas.paste(frame, (x, y + label_height))


def draw_title(canvas: Image.Image, title: str, note: str) -> None:
    draw = ImageDraw.Draw(canvas)
    title_box = draw.textbbox((0, 0), title, font=TITLE_FONT)
    title_width = title_box[2] - title_box[0]
    draw.text(((CANVAS_SIZE[0] - title_width) / 2, 9), title, font=TITLE_FONT, fill=TEXT)
    note_box = draw.textbbox((0, 0), note, font=NOTE_FONT)
    note_width = note_box[2] - note_box[0]
    draw.text(((CANVAS_SIZE[0] - note_width) / 2, CANVAS_SIZE[1] - 25), note, font=NOTE_FONT, fill=MUTED)


def save_board(filename: str, title: str, cards: list[tuple[str, str, int, int]]) -> dict:
    canvas = Image.new("RGB", CANVAS_SIZE, BACKGROUND)
    sizes: dict[str, tuple[int, int]] = {}
    for source, label, x, y in cards:
        frame, original_size = load_frame(source)
        sizes[source] = original_size
        draw_card(canvas, frame, label, x, y)
    draw_title(canvas, title, "评审用源图｜保持 F021 母版，不是 Unity 运行资源")
    output = ROOT / filename
    canvas.save(output, format="PNG", optimize=True)
    return {
        "file": output.name,
        "size": list(canvas.size),
        "sha256": hashlib.sha256(output.read_bytes()).hexdigest().upper(),
        "source_sizes": {key: list(value) for key, value in sizes.items()},
    }


def main() -> None:
    style_board = save_board(
        "f009_v4_style_fit_review_board_20260721.png",
        "V4 旧街机过载｜强度分级贴合板",
        [
            ("f009_v4_pass_preview_20260721.png", "过关", 44, 63),
            ("f009_v4_far_exceed_preview_20260721.png", "远超过", 1044, 63),
            ("f009_v4_critical_impact_preview_20260721.png", "暴击", 544, 661),
        ],
    )
    critical_board = save_board(
        "f009_v4_critical_four_frame_review_board_20260721.png",
        "V4 暴击｜预压—主击—余震—恢复",
        [
            ("f009_v4_critical_precompress_preview_20260721.png", "预压 50–70ms", 44, 63),
            ("f009_v4_critical_impact_preview_20260721.png", "主击 80–110ms", 1044, 63),
            ("f009_v4_critical_aftershock_preview_20260721.png", "余震 180–260ms", 44, 661),
            ("f009_v4_far_exceed_preview_20260721.png", "恢复 350–500ms", 1044, 661),
        ],
    )
    report = {
        "status": "样张待验收",
        "runtime_asset": False,
        "boards": [style_board, critical_board],
    }
    (ROOT / "f009_v4_preview_metrics_20260721.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
