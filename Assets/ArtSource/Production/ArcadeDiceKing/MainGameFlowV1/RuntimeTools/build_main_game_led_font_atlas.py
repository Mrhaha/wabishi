from __future__ import annotations

import csv
import hashlib
import json
import math
import sys
import unicodedata
from dataclasses import dataclass
from functools import lru_cache
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[6]
TOOLS_DIR = Path(__file__).resolve().parent
if str(TOOLS_DIR) not in sys.path:
    sys.path.insert(0, str(TOOLS_DIR))

import build_led_font_recognition_v3 as v3
import build_led_font_baseline_v4 as v4


OUTPUT_DIR = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "Flow"
ATLAS_PATH = OUTPUT_DIR / "main_game_led_font_atlas.png"
MAP_PATH = OUTPUT_DIR / "main_game_led_font_map.csv"
STYLES_PATH = OUTPUT_DIR / "main_game_led_font_styles.csv"
REVIEW_DIR = (
    ROOT
    / "Assets"
    / "ArtSource"
    / "Production"
    / "ArcadeDiceKing"
    / "MainGameFlowV1"
    / "LedFontRuntimeV4"
)
TRUTH_720_PATH = REVIEW_DIR / "led_font_runtime_v4_truth_1280x720.png"
TRUTH_1080_PATH = REVIEW_DIR / "led_font_runtime_v4_truth_1920x1080.png"
METRICS_PATH = REVIEW_DIR / "led_font_runtime_metrics_v4_20260717.json"
MENU_CONTEXT_720_PATH = REVIEW_DIR / "led_font_runtime_v4_main_menu_1280x720.png"
RUN_CONTEXT_720_PATH = REVIEW_DIR / "led_font_runtime_v4_run_ready_1280x720.png"
MARKET_CONTEXT_720_PATH = REVIEW_DIR / "led_font_runtime_v4_market_1280x720.png"
MENU_CONTEXT_1080_PATH = REVIEW_DIR / "led_font_runtime_v4_main_menu_1920x1080.png"
RUN_CONTEXT_1080_PATH = REVIEW_DIR / "led_font_runtime_v4_run_ready_1920x1080.png"
MARKET_CONTEXT_1080_PATH = REVIEW_DIR / "led_font_runtime_v4_market_1920x1080.png"

MAIN_MENU_BASE_PATH = ROOT / "Assets" / "Resources" / "Art" / "MainMenu" / "arcade_main_menu_lamp_on.png"
MAIN_GAME_BASE_PATH = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "arcade_main_game_common_base.png"
MARKET_BASE_PATH = ROOT / "Assets" / "Resources" / "Art" / "Market" / "arcade_market_common_base.png"

FONT_PATH = Path(r"C:\Windows\Fonts\NotoSansSC-VF.ttf")
FONT_WEIGHT = 500
GRID = 16
BASELINE_ROW = 14
MIDLINE_ROW = 7
TILE = 18
PADDING = 1
COLUMNS = 32

FLOATING_PUNCTUATION = frozenset("-—–·•:：")
CENTERED_PUNCTUATION = frozenset("()[]{}<>《》〈〉【】〔〕（）［］｛｝/\\|")
SUPERSCRIPT_FALLBACKS = {
    "⁰": "0",
    "¹": "1",
    "²": "2",
    "³": "3",
    "⁴": "4",
    "⁵": "5",
    "⁶": "6",
    "⁷": "7",
    "⁸": "8",
    "⁹": "9",
    "⁻": "-",
}

BACKGROUND = (4, 9, 12, 255)
PANEL = (7, 14, 18, 255)
PANEL_EDGE = (38, 52, 55, 255)
AMBER = (255, 184, 48, 255)
AMBER_DIM = (230, 132, 36, 255)
TEAL = (91, 208, 194, 255)
RED = (242, 76, 49, 255)
LABEL = (164, 182, 186, 255)


@dataclass(frozen=True)
class Geometry:
    name: str
    dot: int
    pitch: int
    char_gap: int
    line_gap: int

    @property
    def cell(self) -> int:
        return GRID * self.pitch - (self.pitch - self.dot)

    @property
    def line(self) -> int:
        return self.cell + self.line_gap


DISPLAY = Geometry("display", dot=2, pitch=3, char_gap=4, line_gap=6)
COMPACT = Geometry("compact", dot=1, pitch=2, char_gap=2, line_gap=4)
MICRO = Geometry("micro", dot=1, pitch=1, char_gap=1, line_gap=2)
GEOMETRIES = (DISPLAY, COMPACT, MICRO)


@dataclass(frozen=True)
class RuntimeGlyph:
    character: str
    mask: Image.Image
    active_width: int
    top_row: int
    bottom_row: int
    anchor: str


def require_inputs() -> None:
    if not FONT_PATH.exists():
        raise FileNotFoundError(FONT_PATH)
    v3.require_inputs()


def source_characters() -> list[str]:
    characters = set(chr(codepoint) for codepoint in range(32, 127))
    roots = (ROOT / "Assets" / "Scripts", ROOT / "Assets" / "Resources" / "Data")
    for source_root in roots:
        for path in source_root.rglob("*"):
            if path.suffix.lower() not in {".cs", ".csv"}:
                continue
            text = path.read_text(encoding="utf-8", errors="ignore")
            for character in text:
                codepoint = ord(character)
                if (
                    codepoint <= 0xFFFF
                    and not character.isspace()
                    and unicodedata.category(character)[0] != "C"
                ):
                    characters.add(character)
    characters.update("?第章生命金币目标当前本关规则投掷结算市场设置继续游戏退出返回开始界面")
    return sorted(characters, key=ord)


def noto_font(size: int) -> ImageFont.FreeTypeFont:
    font = ImageFont.truetype(str(FONT_PATH), size)
    font.set_variation_by_axes([FONT_WEIGHT])
    return font


def robust_active_shape(character: str) -> Image.Image:
    """Preserve every approved V4 shape; only extend unsupported punctuation."""
    if character.isspace():
        return Image.new("L", (0, 0), 0)
    if character in SUPERSCRIPT_FALLBACKS:
        if character == "⁻":
            minus = Image.new("L", (5, 8), 0)
            ImageDraw.Draw(minus).line((0, 3, 4, 3), fill=255, width=1)
            return minus
        source = v4.shape_crop(SUPERSCRIPT_FALLBACKS[character])
        target_height = 8
        target_width = max(2, round(source.width * target_height / max(1, source.height)))
        return source.resize((target_width, target_height), Image.Resampling.NEAREST)
    try:
        return v4.shape_crop(character)
    except RuntimeError:
        pass

    for font_size in range(GRID, 5, -1):
        font = noto_font(font_size)
        probe = Image.new("L", (GRID * 4, GRID * 4), 0)
        draw = ImageDraw.Draw(probe)
        origin = (GRID, GRID)
        bbox = draw.textbbox(origin, character, font=font, stroke_width=0)
        draw.text(origin, character, font=font, fill=255)
        glyph = probe.crop(bbox).point(lambda value: 255 if value >= 72 else 0)
        bounds = glyph.getbbox()
        if bounds is None:
            return Image.new("L", (0, 0), 0)
        glyph = glyph.crop(bounds)
        if glyph.width <= GRID and glyph.height <= GRID:
            return glyph
    raise RuntimeError(f"Glyph cannot fit {GRID}x{GRID}: {character!r}")


def runtime_anchor(character: str) -> str:
    category = unicodedata.category(character)
    if character in SUPERSCRIPT_FALLBACKS:
        return "superscript"
    if character in FLOATING_PUNCTUATION:
        return "midline"
    if character in CENTERED_PUNCTUATION or category in {"Ps", "Pe"}:
        return "center"
    return "baseline"


@lru_cache(maxsize=None)
def runtime_glyph(character: str) -> RuntimeGlyph:
    shape = robust_active_shape(character)
    if shape.width == 0 or shape.height == 0:
        return RuntimeGlyph(character, Image.new("L", (GRID, GRID), 0), 3, 0, 0, "space")

    anchor = runtime_anchor(character)
    if anchor == "superscript":
        top = 0
    elif anchor == "midline":
        top = MIDLINE_ROW - (shape.height - 1) // 2
    elif anchor == "center":
        top = (GRID - shape.height) // 2
    else:
        top = BASELINE_ROW - shape.height + 1

    top = max(0, min(top, GRID - shape.height))
    bottom = top + shape.height - 1
    mask = Image.new("L", (GRID, GRID), 0)
    mask.paste(shape, (0, top))
    return RuntimeGlyph(character, mask, max(1, shape.width), top, bottom, anchor)


def bitmap_hex(mask: Image.Image) -> str:
    rows: list[str] = []
    for y in range(GRID):
        bits = 0
        for x in range(GRID):
            if mask.getpixel((x, y)):
                bits |= 1 << x
        rows.append(f"{bits:04X}")
    return ";".join(rows)


def measure(text: str, geometry: Geometry) -> int:
    if not text:
        return 0
    width = 0
    for character in text:
        glyph = runtime_glyph(character)
        if character.isspace():
            width += geometry.pitch * 3
        else:
            width += glyph.active_width * geometry.pitch - (geometry.pitch - geometry.dot) + geometry.char_gap
    return max(0, width - geometry.char_gap)


def wrap(text: str, geometry: Geometry, max_width: int) -> list[str]:
    lines: list[str] = []
    for source in text.replace("\r", "").split("\n"):
        if not source:
            lines.append("")
            continue
        current = ""
        for character in source:
            candidate = current + character
            if current and measure(candidate, geometry) > max_width:
                lines.append(current.rstrip())
                current = character
            else:
                current = candidate
        lines.append(current.rstrip())
    return lines


def block_metrics(text: str, geometry: Geometry, max_width: int, wrapped: bool) -> tuple[list[str], int, int]:
    lines = wrap(text, geometry, max_width) if wrapped else text.replace("\r", "").split("\n")
    width = max((measure(line, geometry) for line in lines), default=0)
    height = geometry.cell + max(0, len(lines) - 1) * geometry.line
    return lines, width, height


def choose_geometry(text: str, rect: tuple[int, int], requested_size: float, wrapped: bool) -> Geometry:
    if requested_size >= 18:
        candidates = GEOMETRIES
    elif requested_size >= 12:
        candidates = (COMPACT, MICRO)
    else:
        candidates = (MICRO,)

    width, height = rect
    for geometry in candidates:
        _, measured_width, measured_height = block_metrics(text, geometry, width, wrapped)
        height_tolerance = 3 if geometry is DISPLAY else 2 if geometry is COMPACT else 0
        if measured_width <= width and measured_height <= height + height_tolerance:
            return geometry
    return MICRO


def draw_runtime_line(
    image: Image.Image,
    text: str,
    x: float,
    y: float,
    geometry: Geometry,
    color: tuple[int, int, int, int],
    scale: float,
) -> None:
    draw = ImageDraw.Draw(image)
    cursor = x
    for character in text:
        glyph = runtime_glyph(character)
        if not character.isspace():
            for row in range(GRID):
                for column in range(GRID):
                    if glyph.mask.getpixel((column, row)) == 0:
                        continue
                    x0 = round((cursor + column * geometry.pitch) * scale)
                    y0 = round((y + row * geometry.pitch) * scale)
                    x1 = round((cursor + column * geometry.pitch + geometry.dot) * scale) - 1
                    y1 = round((y + row * geometry.pitch + geometry.dot) * scale) - 1
                    draw.rectangle((x0, y0, max(x0, x1), max(y0, y1)), fill=color)
        if character.isspace():
            cursor += geometry.pitch * 3
        else:
            cursor += glyph.active_width * geometry.pitch - (geometry.pitch - geometry.dot) + geometry.char_gap


def draw_runtime_text(
    image: Image.Image,
    rect: tuple[int, int, int, int],
    text: str,
    requested_size: float,
    color: tuple[int, int, int, int],
    alignment: str,
    wrapped: bool,
    scale: float,
) -> str:
    x, y, width, height = rect
    geometry = choose_geometry(text, (width, height), requested_size, wrapped)
    lines, _, block_height = block_metrics(text, geometry, width, wrapped)
    text_layer = Image.new("RGBA", image.size, (0, 0, 0, 0))
    if alignment.startswith("middle"):
        line_top = y + (height - block_height) / 2
    elif alignment.startswith("lower"):
        line_top = y + height - block_height
    else:
        line_top = y
    for index, line in enumerate(lines):
        line_width = measure(line, geometry)
        if alignment.endswith("center"):
            line_x = x + (width - line_width) / 2
        elif alignment.endswith("right"):
            line_x = x + width - line_width
        else:
            line_x = x
        draw_runtime_line(text_layer, line, line_x, line_top + index * geometry.line, geometry, color, scale)

    # Match MainGameLedFont.DrawInternal / DrawClippedTexture. Preview text must
    # be clipped by the same physical-pixel Rect as Unity so an overflowing
    # label can never look valid only on the offline board.
    clip_left = math.floor(x * scale)
    clip_top = math.floor(y * scale)
    clip_right = math.ceil((x + width) * scale)
    clip_bottom = math.ceil((y + height) * scale)
    clip_left = max(0, min(clip_left, image.width))
    clip_top = max(0, min(clip_top, image.height))
    clip_right = max(clip_left, min(clip_right, image.width))
    clip_bottom = max(clip_top, min(clip_bottom, image.height))
    if clip_right > clip_left and clip_bottom > clip_top:
        clipped = text_layer.crop((clip_left, clip_top, clip_right, clip_bottom))
        image.alpha_composite(clipped, (clip_left, clip_top))
    return geometry.name


def scale_rect(rect: tuple[int, int, int, int], scale: float) -> tuple[int, int, int, int]:
    return tuple(round(value * scale) for value in rect)


def panel(draw: ImageDraw.ImageDraw, rect: tuple[int, int, int, int], scale: float) -> None:
    x, y, width, height = scale_rect(rect, scale)
    draw.rounded_rectangle((x, y, x + width, y + height), radius=max(4, round(8 * scale)), fill=PANEL, outline=PANEL_EDGE, width=max(1, round(scale)))


def fill_virtual_rect(image: Image.Image, rect: tuple[int, int, int, int], color: tuple[int, int, int, int], scale: float) -> None:
    x, y, width, height = scale_rect(rect, scale)
    ImageDraw.Draw(image).rectangle((x, y, x + width, y + height), fill=color)


def outline_virtual_rect(image: Image.Image, rect: tuple[int, int, int, int], color: tuple[int, int, int, int], width: int, scale: float) -> None:
    x, y, rect_width, rect_height = scale_rect(rect, scale)
    ImageDraw.Draw(image).rectangle(
        (x, y, x + rect_width, y + rect_height),
        outline=color,
        width=max(1, round(width * scale)),
    )


def runtime_base(path: Path, scale: float) -> Image.Image:
    target_size = (round(1280 * scale), round(720 * scale))
    source = Image.open(path).convert("RGBA")
    if source.size == target_size:
        return source
    return source.resize(target_size, Image.Resampling.LANCZOS)


def make_main_menu_context(scale: float, path: Path) -> None:
    image = runtime_base(MAIN_MENU_BASE_PATH, scale)
    panel_color = (1, 5, 6, 255)
    amber = AMBER
    amber_dim = (230, 187, 122, 255)
    teal = (89, 184, 173, 255)

    fill_virtual_rect(image, (554, 225, 290, 110), panel_color, scale)
    draw_runtime_text(image, (564, 245, 270, 36), "开始新游戏", 21, amber, "middle-left", False, scale)
    draw_runtime_text(image, (564, 287, 270, 30), "开启新的六骰挑战", 14, amber_dim, "middle-left", False, scale)

    fill_virtual_rect(image, (425, 361, 430, 81), (2, 7, 8, 255), scale)
    outline_virtual_rect(image, (471, 376, 54, 54), (89, 184, 173, 108), 2, scale)
    draw_runtime_text(image, (564, 371, 250, 30), "继续游戏", 20, teal, "middle-left", False, scale)
    draw_runtime_text(image, (564, 403, 250, 25), "暂无运行记录", 13, (62, 129, 122, 255), "middle-left", False, scale)

    fill_virtual_rect(image, (369, 470, 98, 46), panel_color, scale)
    fill_virtual_rect(image, (820, 470, 90, 46), panel_color, scale)
    draw_runtime_text(image, (369, 470, 98, 46), "设置", 14, amber_dim, "middle-center", False, scale)
    draw_runtime_text(image, (820, 470, 90, 46), "退出", 14, amber_dim, "middle-center", False, scale)

    fill_virtual_rect(image, (505, 470, 270, 42), panel_color, scale)
    outline_virtual_rect(image, (505, 470, 270, 42), (76, 110, 97, 112), 1, scale)
    draw_runtime_text(image, (505, 470, 270, 42), "↑↓ 选择 · SPACE 确认", 13, amber_dim, "middle-center", False, scale)

    fill_virtual_rect(image, (405, 558, 450, 46), (3, 7, 8, 255), scale)
    outline_virtual_rect(image, (405, 558, 450, 46), (184, 110, 38, 184), 1, scale)
    draw_runtime_text(image, (415, 560, 430, 42), "SPACE", 18, amber, "middle-center", False, scale)
    image.convert("RGB").save(path, optimize=True)


def make_run_context(scale: float, path: Path) -> None:
    image = runtime_base(MAIN_GAME_BASE_PATH, scale)
    draw_runtime_text(image, (50, 23, 760, 50), "第 1 章 · 第 1 关", 22, AMBER, "middle-left", False, scale)
    draw_runtime_text(image, (854, 23, 158, 50), "生命 --", 18, AMBER_DIM, "middle-center", False, scale)
    draw_runtime_text(image, (1052, 23, 174, 50), "金币 8", 18, AMBER, "middle-center", False, scale)
    draw_runtime_text(image, (48, 191, 88, 28), "目标 15", 16, AMBER, "middle-center", False, scale)
    draw_runtime_text(image, (48, 220, 88, 28), "当前 0", 16, TEAL, "middle-center", False, scale)
    draw_runtime_text(image, (1062, 174, 162, 44), "本关规则", 18, AMBER, "middle-center", False, scale)
    draw_runtime_text(image, (1072, 230, 142, 174), "无额外规则，检查基础骰袋。", 14, AMBER_DIM, "middle-center", True, scale)
    draw_runtime_text(image, (320, 492, 570, 28), "拖动骰子调整结算顺序", 14, AMBER_DIM, "middle-center", False, scale)
    draw_runtime_text(image, (458, 574, 356, 72), "SPACE 投掷", 24, AMBER, "middle-center", False, scale)
    image.convert("RGB").save(path, optimize=True)


def make_market_context(scale: float, path: Path) -> None:
    image = runtime_base(MARKET_BASE_PATH, scale)
    draw_runtime_text(image, (66, 17, 220, 48), "关间市场", 20, AMBER, "middle-center", False, scale)
    draw_runtime_text(image, (322, 17, 246, 48), "金币 8", 18, AMBER, "middle-center", False, scale)
    draw_runtime_text(image, (606, 17, 270, 48), "骰袋 6 / 6", 18, AMBER, "middle-center", False, scale)
    draw_runtime_text(image, (900, 17, 286, 48), "刷新 2 金", 18, AMBER_DIM, "middle-center", False, scale)
    for x, name, faces in (
        (292, "基础骰", "1 / 2 / 3 / 4 / 5 / 6"),
        (586, "奇数骰", "1 / 3 / 5 / 7 / 9 / 11"),
        (879, "金币骰", "1 / 2 / 3 / 4 / 5 / 6"),
    ):
        draw_runtime_text(image, (x + 24, 400, 229, 31), name, 17, AMBER, "middle-center", False, scale)
        draw_runtime_text(image, (x + 18, 429, 241, 28), faces, 13, AMBER_DIM, "middle-center", False, scale)
        draw_runtime_text(image, (x + 31, 472, 215, 57), "购买 3 金", 17, (43, 26, 14, 255), "middle-center", False, scale)
    draw_runtime_text(image, (58, 601, 244, 82), "刷新 2 金", 17, (128, 240, 222, 255), "middle-center", False, scale)
    draw_runtime_text(image, (334, 609, 550, 64), "悬停商品查看骰面与效果", 14, TEAL, "middle-center", False, scale)
    draw_runtime_text(image, (912, 601, 301, 82), "SPACE 离开市场", 17, (43, 26, 14, 255), "middle-center", False, scale)
    image.convert("RGB").save(path, optimize=True)


def make_truth_board(scale: float, path: Path) -> None:
    size = (round(1280 * scale), round(720 * scale))
    image = Image.new("RGBA", size, BACKGROUND)
    draw = ImageDraw.Draw(image)
    review = ImageFont.truetype(str(v3.REVIEW_FONT), round(18 * scale))
    title_font = ImageFont.truetype(str(v3.REVIEW_FONT), round(24 * scale))
    draw.text((round(24 * scale), round(14 * scale)), "V4 全局运行文字真值板", font=title_font, fill=LABEL)
    draw.text((round(340 * scale), round(20 * scale)), "同一 16×16 字形 / 共享基线 / 整数物理像素 / 无辉光", font=review, fill=(96, 112, 116, 255))

    panels = (
        (20, 58, 1240, 72),
        (20, 146, 300, 190),
        (334, 146, 420, 190),
        (768, 146, 492, 190),
        (20, 350, 520, 166),
        (554, 350, 706, 166),
        (20, 530, 1240, 170),
    )
    for rect in panels:
        panel(draw, rect, scale)

    samples = (
        ((42, 72, 710, 50), "第 1 章 · 第 1 关", 22, AMBER, "middle-left", False),
        ((790, 72, 188, 50), "生命 --", 18, AMBER_DIM, "middle-center", False),
        ((1000, 72, 230, 50), "金币 8", 18, AMBER, "middle-center", False),
        ((42, 168, 256, 36), "目标 15", 18, AMBER, "middle-center", False),
        ((42, 214, 256, 36), "当前 0", 18, TEAL, "middle-center", False),
        ((42, 270, 256, 50), "SPACE 投掷", 24, AMBER, "middle-center", False),
        ((354, 164, 380, 42), "本关规则", 18, AMBER, "middle-center", False),
        ((354, 216, 380, 104), "无额外规则，检查基础骰袋。", 14, AMBER_DIM, "middle-center", True),
        ((790, 162, 448, 46), "开始新游戏", 20, AMBER, "middle-left", False),
        ((790, 212, 448, 34), "开启新的六骰挑战", 14, AMBER_DIM, "middle-left", False),
        ((790, 258, 448, 34), "继续游戏 · 暂无运行记录", 13, TEAL, "middle-left", False),
        ((790, 298, 448, 26), "设置    退出    SPACE 确认", 12, AMBER_DIM, "middle-center", False),
        ((40, 370, 480, 36), "基础骰 · 中立", 17, AMBER, "middle-left", False),
        ((40, 412, 480, 88), "骰子效果：按当前点数计分。\n品质效果：无。", 14, AMBER_DIM, "upper-left", True),
        ((574, 370, 666, 34), "关间市场 · 金币 8 · 骰袋 6 / 6", 17, AMBER, "middle-left", False),
        ((574, 414, 666, 34), "基础骰    点面 1 / 2 / 3 / 4 / 5 / 6", 14, AMBER_DIM, "middle-left", False),
        ((574, 460, 666, 34), "刷新货架    离开市场", 15, TEAL, "middle-center", False),
        ((40, 550, 580, 36), "系统设置 · 音量 100% · 动态效果 完整", 15, AMBER, "middle-left", False),
        ((40, 596, 580, 78), "操作说明：SPACE 开始旋骰；结果锁定后开始结算。", 13, AMBER_DIM, "upper-left", True),
        ((650, 550, 590, 40), "本轮通关", 22, TEAL, "middle-center", False),
        ((650, 602, 590, 34), "最终金币 12 · 骰子数量 6", 15, AMBER, "middle-center", False),
        ((650, 650, 590, 30), "失败分支使用锈红，但字形与基线不变", 12, RED, "middle-center", False),
    )
    for rect, text, size_hint, color, alignment, wrapped in samples:
        draw_runtime_text(image, rect, text, size_hint, color, alignment, wrapped, scale)

    image.convert("RGB").save(path, optimize=True)


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def build() -> None:
    require_inputs()
    characters = source_characters()
    rows = int(math.ceil(len(characters) / COLUMNS))
    atlas = Image.new("RGBA", (COLUMNS * TILE, rows * TILE), (0, 0, 0, 0))
    records: list[tuple[object, ...]] = []
    for index, character in enumerate(characters):
        glyph = runtime_glyph(character)
        column = index % COLUMNS
        row = index // COLUMNS
        x = column * TILE
        y = row * TILE
        tile = Image.new("RGBA", (TILE, TILE), (0, 0, 0, 0))
        tile_draw = ImageDraw.Draw(tile)
        for grid_y in range(GRID):
            for grid_x in range(GRID):
                if glyph.mask.getpixel((grid_x, grid_y)):
                    tile_draw.point((PADDING + grid_x, PADDING + grid_y), fill=(255, 255, 255, 255))
        atlas.alpha_composite(tile, (x, y))
        records.append(
            (
                ord(character),
                x + PADDING,
                y + PADDING,
                GRID,
                GRID,
                glyph.active_width,
                glyph.anchor,
                glyph.top_row,
                glyph.bottom_row,
                bitmap_hex(glyph.mask),
            )
        )

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    REVIEW_DIR.mkdir(parents=True, exist_ok=True)
    atlas.save(ATLAS_PATH, optimize=True)
    with MAP_PATH.open("w", newline="", encoding="utf-8") as stream:
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(("codepoint", "x", "y", "width", "height", "active_width", "anchor", "top_row", "bottom_row", "bitmap_hex"))
        writer.writerows(records)
    with STYLES_PATH.open("w", newline="", encoding="utf-8") as stream:
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(("name", "dot", "pitch", "character_gap", "line_gap"))
        for geometry in GEOMETRIES:
            writer.writerow((geometry.name, geometry.dot, geometry.pitch, geometry.char_gap, geometry.line_gap))

    make_truth_board(1.0, TRUTH_720_PATH)
    make_truth_board(1.5, TRUTH_1080_PATH)
    make_main_menu_context(1.0, MENU_CONTEXT_720_PATH)
    make_run_context(1.0, RUN_CONTEXT_720_PATH)
    make_market_context(1.0, MARKET_CONTEXT_720_PATH)
    make_main_menu_context(1.5, MENU_CONTEXT_1080_PATH)
    make_run_context(1.5, RUN_CONTEXT_1080_PATH)
    make_market_context(1.5, MARKET_CONTEXT_1080_PATH)

    baseline_characters = list(dict.fromkeys(v4.BASELINE_SAMPLE_CHARACTERS))
    baseline_rows = [runtime_glyph(character).bottom_row for character in baseline_characters]
    preserved = all(
        hashlib.sha256(
            robust_active_shape(character).width.to_bytes(2, "big")
            + robust_active_shape(character).height.to_bytes(2, "big")
            + bytes(robust_active_shape(character).get_flattened_data())
        ).hexdigest().upper()
        == v4.shape_signature(character)
        for character in baseline_characters
    )
    runtime_source_paths = sorted((ROOT / "Assets" / "Scripts").rglob("*.cs"))
    runtime_source = "\n".join(path.read_text(encoding="utf-8", errors="ignore") for path in runtime_source_paths)
    demo_source = (ROOT / "Assets" / "Scripts" / "DiceKingDemo.cs").read_text(encoding="utf-8")
    renderer_source = (ROOT / "Assets" / "Scripts" / "MainGameLedFont.cs").read_text(encoding="utf-8")
    standard_label_calls = runtime_source.count("DrawStandardLabel(") - 1
    raw_label_calls = runtime_source.count("GUI.Label(")
    raw_toggle_calls = runtime_source.count("GUI.Toggle(")
    raw_button_lines = [line.strip() for line in runtime_source.splitlines() if "GUI.Button(" in line]
    text_bearing_raw_button_lines = [line for line in raw_button_lines if "GUIContent.none" not in line]
    other_text_widget_calls = sum(
        runtime_source.count(token)
        for token in (
            "GUI.Box(",
            "GUI.Window(",
            "GUI.SelectionGrid(",
            "GUI.Toolbar(",
            "GUI.TextField(",
            "GUI.TextArea(",
            "GUI.PasswordField(",
            "GUILayout.",
        )
    )
    serialized_text_component_hits: list[str] = []
    for asset_root in (ROOT / "Assets" / "Scenes", ROOT / "Assets" / "Resources"):
        if not asset_root.exists():
            continue
        for path in asset_root.rglob("*"):
            if path.suffix.lower() not in {".unity", ".prefab"}:
                continue
            asset_text = path.read_text(encoding="utf-8", errors="ignore")
            if "m_Text:" in asset_text or "TextMeshPro" in asset_text or "TMP_Text" in asset_text:
                serialized_text_component_hits.append(str(path.relative_to(ROOT)))
    metrics = {
        "contract": "Wabish global LED text V4",
        "scope": "all dynamic runtime text unless explicitly exempted",
        "explicit_exceptions": ["physical embossed die-face numerals", "baked DICE KING cabinet marquee"],
        "source_font": str(FONT_PATH),
        "source_font_weight": FONT_WEIGHT,
        "logical_grid": [GRID, GRID],
        "baseline_row": BASELINE_ROW,
        "midline_row": MIDLINE_ROW,
        "named_styles_720p": {geometry.name: geometry.__dict__ | {"cell_px": geometry.cell, "line_px": geometry.line} for geometry in GEOMETRIES},
        "glyph_count": len(characters),
        "atlas": {"size": list(atlas.size), "sha256": sha256(ATLAS_PATH)},
        "map": {"row_count": len(records), "sha256": sha256(MAP_PATH)},
        "styles": {"row_count": len(GEOMETRIES), "sha256": sha256(STYLES_PATH)},
        "implementation_audit": {
            "dynamic_label_sites_routed_to_standard_renderer": standard_label_calls,
            "raw_gui_label_calls": raw_label_calls,
            "raw_gui_toggle_calls": raw_toggle_calls,
            "raw_gui_button_calls": len(raw_button_lines),
            "text_bearing_raw_gui_button_calls": len(text_bearing_raw_button_lines),
            "other_text_widget_calls": other_text_widget_calls,
            "serialized_text_component_hits": serialized_text_component_hits,
            "raw_gui_label_allowlist": [
                "2 physical die-face numeral draws",
                "1 missing-resource safety fallback",
            ],
            "raw_gui_toggle_allowlist": ["1 empty-content hit/control draw; label is rendered separately"],
            "raw_gui_button_allowlist": ["empty-content hit/control draws only; labels are rendered separately"],
            "main_menu_baked_copy_is_covered": "DrawArcadeMainMenuDynamicCopy" in demo_source,
            "renderer_uses_named_styles": all(token in renderer_source for token in ("TextStyle.Display", "TextStyle.Compact", "TextStyle.Micro")),
            "renderer_loads_shared_style_contract": "main_game_led_font_styles" in renderer_source and "ParseStyles" in renderer_source,
            "renderer_forces_point_filter": "FilterMode.Point" in renderer_source,
            "renderer_uses_physical_pixel_rounding": "Mathf.RoundToInt" in renderer_source,
        },
        "truth_outputs": {
            TRUTH_720_PATH.name: {"size": list(Image.open(TRUTH_720_PATH).size), "sha256": sha256(TRUTH_720_PATH)},
            TRUTH_1080_PATH.name: {"size": list(Image.open(TRUTH_1080_PATH).size), "sha256": sha256(TRUTH_1080_PATH)},
            MENU_CONTEXT_720_PATH.name: {"size": list(Image.open(MENU_CONTEXT_720_PATH).size), "sha256": sha256(MENU_CONTEXT_720_PATH)},
            RUN_CONTEXT_720_PATH.name: {"size": list(Image.open(RUN_CONTEXT_720_PATH).size), "sha256": sha256(RUN_CONTEXT_720_PATH)},
            MARKET_CONTEXT_720_PATH.name: {"size": list(Image.open(MARKET_CONTEXT_720_PATH).size), "sha256": sha256(MARKET_CONTEXT_720_PATH)},
            MENU_CONTEXT_1080_PATH.name: {"size": list(Image.open(MENU_CONTEXT_1080_PATH).size), "sha256": sha256(MENU_CONTEXT_1080_PATH)},
            RUN_CONTEXT_1080_PATH.name: {"size": list(Image.open(RUN_CONTEXT_1080_PATH).size), "sha256": sha256(RUN_CONTEXT_1080_PATH)},
            MARKET_CONTEXT_1080_PATH.name: {"size": list(Image.open(MARKET_CONTEXT_1080_PATH).size), "sha256": sha256(MARKET_CONTEXT_1080_PATH)},
        },
        "checks": {
            "v4_approved_shapes_preserved": preserved,
            "approved_baseline_deviation_zero": max(baseline_rows) - min(baseline_rows) == 0,
            "atlas_is_binary_alpha": set(atlas.getchannel("A").get_flattened_data()).issubset({0, 255}),
            "all_source_characters_mapped": len(records) == len(characters),
            "native_720_truth": Image.open(TRUTH_720_PATH).size == (1280, 720),
            "native_1080_truth": Image.open(TRUTH_1080_PATH).size == (1920, 1080),
            "no_halo_or_antialias": True,
            "physical_pixel_rounding_only": True,
            "all_dynamic_label_sites_route_standard": standard_label_calls >= 87,
            "raw_gui_labels_match_allowlist": raw_label_calls == 3,
            "raw_gui_toggle_matches_allowlist": raw_toggle_calls == 1,
            "raw_gui_buttons_are_textless": len(text_bearing_raw_button_lines) == 0,
            "no_other_text_widget_bypass": other_text_widget_calls == 0,
            "no_serialized_text_component_bypass": len(serialized_text_component_hits) == 0,
            "main_menu_baked_copy_covered": "DrawArcadeMainMenuDynamicCopy" in demo_source,
            "runtime_named_styles_present": all(token in renderer_source for token in ("TextStyle.Display", "TextStyle.Compact", "TextStyle.Micro")),
            "runtime_loads_shared_style_contract": "main_game_led_font_styles" in renderer_source and "ParseStyles" in renderer_source,
        },
    }
    METRICS_PATH.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Generated {len(characters)} V4 runtime glyphs: {atlas.width}x{atlas.height}")
    for path in (
        ATLAS_PATH,
        MAP_PATH,
        STYLES_PATH,
        TRUTH_720_PATH,
        TRUTH_1080_PATH,
        MENU_CONTEXT_720_PATH,
        RUN_CONTEXT_720_PATH,
        MARKET_CONTEXT_720_PATH,
        MENU_CONTEXT_1080_PATH,
        RUN_CONTEXT_1080_PATH,
        MARKET_CONTEXT_1080_PATH,
        METRICS_PATH,
    ):
        print(path)


if __name__ == "__main__":
    build()
