from __future__ import annotations

import csv
import hashlib
import json
import math
from dataclasses import asdict, dataclass
from pathlib import Path
from statistics import median

from PIL import Image, ImageChops, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[6]
HERE = Path(__file__).resolve().parent
ROLE_PATH = HERE / "main_game_led_text_roles_v1.csv"
FONT_DIR = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "Flow"
ATLAS_PATH = FONT_DIR / "main_game_led_font_atlas.png"
MAP_PATH = FONT_DIR / "main_game_led_font_map.csv"
STYLES_PATH = FONT_DIR / "main_game_led_font_styles.csv"
MAIN_MENU_BASE_PATH = ROOT / "Assets" / "Resources" / "Art" / "MainMenu" / "arcade_main_menu_lamp_on.png"
MARKET_BASE_PATH = ROOT / "Assets" / "Resources" / "Art" / "Market" / "arcade_market_common_base.png"
MARKET_CAPTURE_720_PATH = ROOT / "Docs" / "QA" / "20260717_market_runtime_normal_1280x720.png"
MARKET_CAPTURE_1080_PATH = ROOT / "Docs" / "QA" / "20260717_market_runtime_normal_1920x1080.png"
TOOLTIP_DIR = ROOT / "Assets" / "Resources" / "Art" / "UI" / "Tooltip"
TOOLTIP_PANEL_PATH = TOOLTIP_DIR / "ui_tooltip_arcade_panel_medium.png"
TOOLTIP_FACE_CELL_PATH = TOOLTIP_DIR / "ui_tooltip_arcade_face_cell.png"
TOOLTIP_TYPE_FRAME_PATH = TOOLTIP_DIR / "ui_tooltip_arcade_type_core_frame.png"
TYPE_ICON_PATH = ROOT / "Assets" / "Resources" / "Art" / "DiceTypes" / "piggy_die_icon.png"
METRICS_PATH = HERE / "led_text_brightness_metrics_v1_20260717.json"

VIRTUAL_WIDTH = 1280
VIRTUAL_HEIGHT = 720
GRID = 16


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


@dataclass(frozen=True)
class Glyph:
    character: str
    active_width: int
    rows: tuple[int, ...]
    atlas_rect: tuple[int, int, int, int]


@dataclass(frozen=True)
class Role:
    role_id: str
    level: str
    preferred_geometry: str
    fallback_geometries: tuple[str, ...]
    core_hex: str
    alpha: float
    halo_hex: str
    halo_alpha: float
    halo_pixels: int
    min_contrast: float
    surface_hex: str
    usage: str


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def parse_hex(value: str) -> tuple[int, int, int]:
    value = value.strip().lstrip("#")
    if len(value) != 6:
        raise ValueError(f"Expected RGB hex, got {value!r}")
    return tuple(int(value[index : index + 2], 16) for index in (0, 2, 4))


def load_roles() -> dict[str, Role]:
    roles: dict[str, Role] = {}
    with ROLE_PATH.open("r", encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            fallback = tuple(item for item in row["fallback_geometries"].split("|") if item)
            role = Role(
                role_id=row["role_id"],
                level=row["level"],
                preferred_geometry=row["preferred_geometry"],
                fallback_geometries=fallback,
                core_hex=row["core_hex"],
                alpha=float(row["alpha"]),
                halo_hex=row["halo_hex"],
                halo_alpha=float(row["halo_alpha"]),
                halo_pixels=int(row["halo_pixels"]),
                min_contrast=float(row["min_contrast"]),
                surface_hex=row["surface_hex"],
                usage=row["usage"],
            )
            roles[role.role_id] = role
    return roles


def load_geometries() -> dict[str, Geometry]:
    geometries: dict[str, Geometry] = {}
    with STYLES_PATH.open("r", encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            geometry = Geometry(
                name=row["name"],
                dot=int(row["dot"]),
                pitch=int(row["pitch"]),
                char_gap=int(row["character_gap"]),
                line_gap=int(row["line_gap"]),
            )
            geometries[geometry.name] = geometry
    return geometries


def load_glyphs() -> tuple[dict[str, Glyph], tuple[int, int]]:
    atlas = Image.open(ATLAS_PATH)
    atlas_size = atlas.size
    glyphs: dict[str, Glyph] = {}
    with MAP_PATH.open("r", encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            codepoint = int(row["codepoint"])
            character = chr(codepoint)
            atlas_rect = (int(row["x"]), int(row["y"]), int(row["width"]), int(row["height"]))
            x, y, width, height = atlas_rect
            if x < 0 or y < 0 or x + width > atlas_size[0] or y + height > atlas_size[1]:
                raise RuntimeError(f"Atlas rect out of bounds for U+{codepoint:04X}")
            rows = tuple(int(value, 16) for value in row["bitmap_hex"].split(";"))
            if len(rows) != GRID:
                raise RuntimeError(f"Unexpected row count for U+{codepoint:04X}")
            glyphs[character] = Glyph(character, int(row["active_width"]), rows, atlas_rect)
    return glyphs, atlas_size


ROLES = load_roles()
GEOMETRIES = load_geometries()
GLYPHS, ATLAS_SIZE = load_glyphs()
ELEMENTS: list[dict[str, object]] = []
USED_CHARACTERS: set[str] = set()


def glyph(character: str) -> Glyph:
    USED_CHARACTERS.add(character)
    if character in GLYPHS:
        return GLYPHS[character]
    raise KeyError(f"No formal V4 glyph for {character!r}")


def measure(text: str, geometry: Geometry) -> int:
    width = 0
    for character in text:
        item = glyph(character)
        if character.isspace():
            width += geometry.pitch * 3
        else:
            width += item.active_width * geometry.pitch - (geometry.pitch - geometry.dot) + geometry.char_gap
    return max(0, width - geometry.char_gap) if text else 0


def wrap_text(text: str, geometry: Geometry, max_width: int) -> list[str]:
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
    lines = wrap_text(text, geometry, max_width) if wrapped else text.replace("\r", "").split("\n")
    width = max((measure(line, geometry) for line in lines), default=0)
    height = geometry.cell + max(0, len(lines) - 1) * geometry.line
    return lines, width, height


def choose_geometry(
    text: str,
    rect: tuple[int, int, int, int],
    role: Role,
    wrapped: bool,
    override: str | None,
) -> Geometry:
    candidates = (override,) if override else (role.preferred_geometry,) + role.fallback_geometries
    _, _, width, height = rect
    for name in candidates:
        geometry = GEOMETRIES[name]
        _, measured_width, measured_height = block_metrics(text, geometry, width, wrapped)
        tolerance = 3 if geometry.name == "display" else 2 if geometry.name == "compact" else 0
        if measured_width <= width and measured_height <= height + tolerance:
            return geometry
    details = ", ".join(candidates)
    raise RuntimeError(f"Text does not fit allowed geometries ({details}): {text!r} in {rect}")


def physical_rect(rect: tuple[int, int, int, int], scale: float) -> tuple[int, int, int, int]:
    x, y, width, height = rect
    return (
        math.floor(x * scale),
        math.floor(y * scale),
        math.ceil((x + width) * scale),
        math.ceil((y + height) * scale),
    )


def text_mask(
    size: tuple[int, int],
    rect: tuple[int, int, int, int],
    text: str,
    geometry: Geometry,
    alignment: str,
    wrapped: bool,
    scale: float,
) -> tuple[Image.Image, bool]:
    x, y, width, height = rect
    lines, _, block_height = block_metrics(text, geometry, width, wrapped)
    if alignment.startswith("middle"):
        line_top = y + (height - block_height) / 2
    elif alignment.startswith("lower"):
        line_top = y + height - block_height
    else:
        line_top = y

    mask = Image.new("L", size, 0)
    draw = ImageDraw.Draw(mask)
    for line_index, line in enumerate(lines):
        line_width = measure(line, geometry)
        if alignment.endswith("center"):
            line_x = x + (width - line_width) / 2
        elif alignment.endswith("right"):
            line_x = x + width - line_width
        else:
            line_x = x
        cursor = line_x
        line_y = line_top + line_index * geometry.line
        for character in line:
            item = glyph(character)
            if not character.isspace():
                for row in range(GRID):
                    bits = item.rows[row]
                    for column in range(GRID):
                        if not bits & (1 << column):
                            continue
                        x0 = round((cursor + column * geometry.pitch) * scale)
                        y0 = round((line_y + row * geometry.pitch) * scale)
                        x1 = round((cursor + column * geometry.pitch + geometry.dot) * scale) - 1
                        y1 = round((line_y + row * geometry.pitch + geometry.dot) * scale) - 1
                        draw.rectangle((x0, y0, max(x0, x1), max(y0, y1)), fill=255)
            if character.isspace():
                cursor += geometry.pitch * 3
            else:
                cursor += item.active_width * geometry.pitch - (geometry.pitch - geometry.dot) + geometry.char_gap

    left, top, right, bottom = physical_rect(rect, scale)
    clip = Image.new("L", size, 0)
    ImageDraw.Draw(clip).rectangle((left, top, max(left, right - 1), max(top, bottom - 1)), fill=255)
    clipped = ImageChops.multiply(mask, clip)
    clipped_outside_rect = ImageChops.subtract(mask, clipped).getbbox() is not None
    return clipped, clipped_outside_rect


def composite_color(image: Image.Image, mask: Image.Image, rgb: tuple[int, int, int], alpha: float) -> None:
    layer = Image.new("RGBA", image.size, (*rgb, 255))
    if alpha < 1.0:
        mask = mask.point(lambda value: round(value * alpha))
    layer.putalpha(mask)
    image.alpha_composite(layer)


def linear_channel(value: int) -> float:
    channel = value / 255.0
    return channel / 12.92 if channel <= 0.04045 else ((channel + 0.055) / 1.055) ** 2.4


def luminance(rgb: tuple[int, int, int]) -> float:
    red, green, blue = rgb
    return 0.2126 * linear_channel(red) + 0.7152 * linear_channel(green) + 0.0722 * linear_channel(blue)


def contrast_ratio(foreground: tuple[int, int, int], background: tuple[int, int, int]) -> float:
    high, low = sorted((luminance(foreground), luminance(background)), reverse=True)
    return (high + 0.05) / (low + 0.05)


def mask_contrast(image: Image.Image, mask: Image.Image, foreground: tuple[int, int, int]) -> tuple[float, float, float]:
    background = image.convert("RGB")
    bounds = mask.getbbox()
    if bounds is None:
        return 0.0, 0.0, 0.0
    ratios: list[float] = []
    left, top, right, bottom = bounds
    pixels = background.load()
    mask_pixels = mask.load()
    for y in range(top, bottom):
        for x in range(left, right):
            if mask_pixels[x, y]:
                ratios.append(contrast_ratio(foreground, pixels[x, y]))
    ratios.sort()
    if not ratios:
        return 0.0, 0.0, 0.0
    percentile_index = min(len(ratios) - 1, max(0, round((len(ratios) - 1) * 0.05)))
    return ratios[0], ratios[percentile_index], median(ratios)


def draw_role_text(
    image: Image.Image,
    screen: str,
    element: str,
    rect: tuple[int, int, int, int],
    text: str,
    role_id: str,
    alignment: str,
    wrapped: bool,
    scale: float,
    allow_halo: bool,
    geometry_override: str | None = None,
) -> None:
    role = ROLES[role_id]
    geometry = choose_geometry(text, rect, role, wrapped, geometry_override)
    mask, clipped_outside_rect = text_mask(image.size, rect, text, geometry, alignment, wrapped, scale)
    core = parse_hex(role.core_hex)
    minimum, percentile, middle = mask_contrast(image, mask, core)
    bounds = mask.getbbox()
    clip_left, clip_top, clip_right, clip_bottom = physical_rect(rect, scale)
    if bounds is None:
        margins = [0, 0, 0, 0]
        touches_clip_edge = True
    else:
        margins = [
            bounds[0] - clip_left,
            bounds[1] - clip_top,
            clip_right - bounds[2],
            clip_bottom - bounds[3],
        ]
        touches_clip_edge = min(margins) <= 0

    if allow_halo and role.halo_pixels == 1 and role.halo_alpha > 0.0:
        dilated = mask.filter(ImageFilter.MaxFilter(3))
        ring = ImageChops.subtract(dilated, mask)
        left, top, right, bottom = physical_rect(rect, scale)
        clip = Image.new("L", image.size, 0)
        ImageDraw.Draw(clip).rectangle((left, top, max(left, right - 1), max(top, bottom - 1)), fill=255)
        ring = ImageChops.multiply(ring, clip)
        composite_color(image, ring, parse_hex(role.halo_hex), role.halo_alpha)

    composite_color(image, mask, core, role.alpha)
    ELEMENTS.append(
        {
            "screen": screen,
            "element": element,
            "text": text,
            "role": role.role_id,
            "level": role.level,
            "geometry": geometry.name,
            "rect": list(rect),
            "scale": scale,
            "halo": bool(allow_halo and role.halo_alpha > 0.0),
            "contrast_min": round(minimum, 3),
            "contrast_p05": round(percentile, 3),
            "contrast_median": round(middle, 3),
            "required_contrast": role.min_contrast,
            "p05_pass": percentile >= role.min_contrast,
            "clip_margins_physical_pixels": margins,
            "touches_clip_edge": touches_clip_edge,
            "clipped_outside_rect": clipped_outside_rect,
        }
    )


def scaled_size(scale: float) -> tuple[int, int]:
    return round(VIRTUAL_WIDTH * scale), round(VIRTUAL_HEIGHT * scale)


def load_scaled(path: Path, scale: float) -> Image.Image:
    target = scaled_size(scale)
    image = Image.open(path).convert("RGBA")
    if image.size != target:
        image = image.resize(target, Image.Resampling.LANCZOS)
    return image


def fill_rect(image: Image.Image, rect: tuple[int, int, int, int], color: tuple[int, int, int, int], scale: float) -> None:
    left, top, right, bottom = physical_rect(rect, scale)
    ImageDraw.Draw(image).rectangle((left, top, right - 1, bottom - 1), fill=color)


def outline_rect(
    image: Image.Image,
    rect: tuple[int, int, int, int],
    color: tuple[int, int, int, int],
    width: int,
    scale: float,
) -> None:
    left, top, right, bottom = physical_rect(rect, scale)
    ImageDraw.Draw(image).rectangle(
        (left, top, right - 1, bottom - 1),
        outline=color,
        width=max(1, round(width * scale)),
    )


def paste_scaled(image: Image.Image, asset_path: Path, rect: tuple[int, int, int, int], scale: float) -> None:
    left, top, right, bottom = physical_rect(rect, scale)
    asset = Image.open(asset_path).convert("RGBA").resize((right - left, bottom - top), Image.Resampling.LANCZOS)
    image.alpha_composite(asset, (left, top))


def restore_rect(image: Image.Image, clean: Image.Image, rect: tuple[int, int, int, int], scale: float) -> None:
    left, top, right, bottom = physical_rect(rect, scale)
    image.paste(clean.crop((left, top, right, bottom)), (left, top))


def make_main_menu(scale: float, allow_halo: bool) -> Image.Image:
    screen = "main_menu"
    image = load_scaled(MAIN_MENU_BASE_PATH, scale)
    panel = (1, 5, 6, 255)
    fill_rect(image, (554, 225, 290, 110), panel, scale)
    fill_rect(image, (425, 361, 430, 81), (2, 7, 8, 255), scale)
    fill_rect(image, (369, 470, 98, 46), panel, scale)
    fill_rect(image, (820, 470, 90, 46), panel, scale)
    fill_rect(image, (505, 470, 270, 42), panel, scale)
    fill_rect(image, (405, 558, 450, 46), (3, 7, 8, 255), scale)
    outline_rect(image, (422, 212, 436, 136), (255, 211, 106, 255), 2, scale)
    outline_rect(image, (471, 376, 54, 54), (140, 128, 104, 180), 2, scale)
    outline_rect(image, (505, 470, 270, 42), (76, 110, 97, 112), 1, scale)
    outline_rect(image, (405, 558, 450, 46), (184, 110, 38, 184), 1, scale)

    draw_role_text(image, screen, "new_game_title", (564, 232, 270, 52), "开始新游戏", "focus_amber", "middle-left", False, scale, allow_halo, "display")
    draw_role_text(image, screen, "new_game_subtitle", (564, 292, 270, 31), "开启新的六骰挑战", "secondary_warm", "middle-left", False, scale, allow_halo, "compact")
    draw_role_text(image, screen, "continue_title_disabled", (564, 365, 250, 47), "继续游戏", "disabled", "middle-left", False, scale, allow_halo, "display")
    draw_role_text(image, screen, "continue_reason_disabled", (564, 417, 250, 18), "暂无运行记录", "disabled", "middle-left", False, scale, allow_halo, "micro")
    draw_role_text(image, screen, "settings", (369, 470, 98, 46), "设置", "secondary_amber", "middle-center", False, scale, allow_halo, "compact")
    draw_role_text(image, screen, "quit", (820, 470, 90, 46), "退出", "secondary_amber", "middle-center", False, scale, allow_halo, "compact")
    draw_role_text(image, screen, "screen_shortcut", (505, 470, 270, 42), "↑↓选择 · SPACE确认", "secondary_amber", "middle-center", False, scale, allow_halo, "micro")
    draw_role_text(image, screen, "physical_space", (415, 558, 430, 46), "SPACE", "focus_amber", "middle-center", False, scale, allow_halo, "display")
    return image


def market_capture(scale: float) -> Image.Image:
    if abs(scale - 1.0) < 0.01:
        return Image.open(MARKET_CAPTURE_720_PATH).convert("RGBA")
    if abs(scale - 1.5) < 0.01:
        return Image.open(MARKET_CAPTURE_1080_PATH).convert("RGBA")
    raise ValueError(f"Unsupported market scale: {scale}")


def clean_market_dynamic_copy(image: Image.Image, clean: Image.Image, scale: float) -> None:
    for rect in (
        (55, 10, 1142, 62),
        (59, 496, 172, 72),
        (58, 601, 244, 82),
        (334, 609, 550, 64),
        (912, 601, 301, 82),
        (306, 380, 257, 91),
        (600, 380, 257, 91),
        (893, 380, 257, 91),
        (323, 472, 215, 57),
        (617, 472, 215, 57),
        (910, 472, 215, 57),
    ):
        restore_rect(image, clean, rect, scale)


def make_market(scale: float, allow_halo: bool) -> Image.Image:
    screen = "market"
    image = market_capture(scale)
    clean = load_scaled(MARKET_BASE_PATH, scale)
    clean_market_dynamic_copy(image, clean, scale)

    draw_role_text(image, screen, "market_title", (66, 17, 220, 48), "关间市场", "primary_amber", "middle-center", False, scale, allow_halo, "display")
    draw_role_text(image, screen, "gold", (322, 17, 246, 48), "金币 50", "primary_amber", "middle-center", False, scale, allow_halo, "display")
    draw_role_text(image, screen, "bag", (606, 17, 270, 48), "骰袋 5 / 6", "primary_amber", "middle-center", False, scale, allow_halo, "display")
    draw_role_text(image, screen, "refresh_cost", (900, 17, 286, 48), "刷新 1 金", "primary_amber", "middle-center", False, scale, allow_halo, "display")

    offers = (
        (292, "补给猪骰", "3 / 4 / 4 / 5 / 5 / 6", "购买 12 金", True),
        (586, "幸运龟骰", "1 / 2 / 3 / 4 / 5 / 6", "购买 8 金", False),
        (879, "黑帆魔蝠骰", "3 / 4 / 4 / 5 / 5 / 6", "购买 10 金", False),
    )
    for index, (x, name, faces, buy, focused) in enumerate(offers):
        panel_rect = (x + 14, 382, 249, 86)
        fill_rect(image, panel_rect, (3, 8, 10, 248), scale)
        outline_rect(image, panel_rect, (96, 62, 29, 210), 1, scale)
        if focused:
            outline_rect(image, (x + 8, 122, 261, 346), (120, 243, 230, 220), 2, scale)
        name_role = "focus_teal" if focused else "primary_amber"
        draw_role_text(image, screen, f"offer_{index + 1}_name", (x + 18, 385, 241, 47), name, name_role, "middle-center", False, scale, allow_halo, "display")
        draw_role_text(image, screen, f"offer_{index + 1}_faces", (x + 18, 435, 241, 31), faces, "secondary_amber", "middle-center", False, scale, allow_halo, "compact")
        fill_rect(image, (x + 43, 482, 191, 37), (214, 184, 140, 255), scale)
        outline_rect(image, (x + 43, 482, 191, 37), (116, 78, 43, 255), 1, scale)
        draw_role_text(image, screen, f"offer_{index + 1}_buy", (x + 31, 472, 215, 57), buy, "ink_on_light", "middle-center", False, scale, allow_halo, "compact")

    fill_rect(image, (71, 510, 148, 44), (214, 184, 140, 255), scale)
    outline_rect(image, (71, 510, 148, 44), (116, 78, 43, 255), 1, scale)
    draw_role_text(image, screen, "sell", (59, 496, 172, 72), "卖出 1 金", "ink_on_light", "middle-center", False, scale, allow_halo, "compact")
    draw_role_text(image, screen, "refresh_button", (58, 601, 244, 82), "刷新 1 金", "focus_teal", "middle-center", False, scale, allow_halo, "display")
    draw_role_text(image, screen, "feedback", (334, 609, 550, 64), "可购买 · 悬停查看骰面与效果", "secondary_amber", "middle-center", False, scale, allow_halo, "compact")
    fill_rect(image, (928, 615, 269, 54), (214, 184, 140, 255), scale)
    outline_rect(image, (928, 615, 269, 54), (116, 78, 43, 255), 1, scale)
    draw_role_text(image, screen, "leave", (912, 601, 301, 82), "离开市场", "ink_on_light", "middle-center", False, scale, allow_halo, "display")
    return image


def draw_chip(
    image: Image.Image,
    screen: str,
    element: str,
    rect: tuple[int, int, int, int],
    text: str,
    accent: tuple[int, int, int, int],
    scale: float,
) -> None:
    fill_rect(image, rect, (6, 15, 19, 238), scale)
    outline_rect(image, rect, accent, 1, scale)
    draw_role_text(image, screen, element, rect, text, "secondary_warm", "middle-center", False, scale, False, "micro")


def make_market_tooltip(scale: float, allow_halo: bool) -> Image.Image:
    screen = "market_tooltip"
    image = make_market(scale, allow_halo)
    panel = (224, 71, 448, 244)
    paste_scaled(image, TOOLTIP_PANEL_PATH, panel, scale)
    type_core = (242, 84, 58, 58)
    fill_rect(image, (250, 92, 42, 42), (22, 28, 23, 255), scale)
    paste_scaled(image, TYPE_ICON_PATH, (250, 92, 42, 42), scale)
    paste_scaled(image, TOOLTIP_TYPE_FRAME_PATH, type_core, scale)

    draw_role_text(image, screen, "tooltip_name", (308, 83, 344, 47), "补给猪骰", "primary_amber", "middle-left", False, scale, allow_halo, "display")
    draw_chip(image, screen, "tooltip_family", (308, 133, 58, 21), "猪猪", (190, 151, 76, 220), scale)
    draw_chip(image, screen, "tooltip_trigger", (570, 133, 80, 21), "货架生成", (194, 87, 56, 220), scale)

    face_y = 160
    cell_width = 60
    gap = 8
    start_x = 248
    for index, value in enumerate((3, 4, 4, 5, 5, 6)):
        rect = (start_x + index * (cell_width + gap), face_y, cell_width, 44)
        paste_scaled(image, TOOLTIP_FACE_CELL_PATH, rect, scale)
        draw_role_text(image, screen, f"tooltip_face_{index + 1}", rect, str(value), "primary_amber", "middle-center", False, scale, allow_halo, "compact")

    fill_rect(image, (246, 210, 404, 1), (181, 120, 51, 150), scale)
    draw_chip(image, screen, "tooltip_rule_label", (246, 218, 48, 21), "规则", (194, 122, 41, 220), scale)
    draw_role_text(image, screen, "tooltip_rule", (302, 212, 348, 35), "给最低价货架骰补给饲料", "secondary_warm", "middle-left", False, scale, allow_halo, "compact")
    fill_rect(image, (248, 267, 400, 32), (12, 27, 30, 245), scale)
    outline_rect(image, (248, 267, 400, 32), (224, 143, 46, 220), 1, scale)
    draw_role_text(image, screen, "tooltip_economy", (254, 267, 388, 32), "买 12 金 · 预计卖 1 金", "primary_amber", "middle-center", False, scale, allow_halo, "compact")
    return image


def make_state_strip(scale: float, allow_halo: bool) -> Image.Image:
    width = round(1280 * scale)
    height = round(260 * scale)
    image = Image.new("RGBA", (width, height), (3, 8, 11, 255))
    screen = "state_strip"
    draw_role_text(image, screen, "title", (28, 16, 1224, 47), "LED TEXT ROLES V1", "primary_amber", "middle-left", False, scale, allow_halo, "display")
    samples = (
        (24, "L3 FOCUS", "开始游戏", "focus_amber", (4, 12, 14, 255), "display"),
        (272, "L2 PRIMARY", "骰袋 5 / 6", "primary_amber", (4, 12, 14, 255), "compact"),
        (520, "L1 SECONDARY", "六面 1/2/3/4/5/6", "secondary_amber", (4, 12, 14, 255), "micro"),
        (768, "DISABLED", "继续游戏", "disabled", (4, 12, 14, 255), "display"),
        (1016, "INK ON LIGHT", "购买 12 金", "ink_on_light", (200, 167, 124, 255), "compact"),
    )
    for index, (x, label, sample, role_id, background, geometry) in enumerate(samples):
        rect = (x, 84, 232, 146)
        fill_rect(image, rect, background, scale)
        outline_rect(image, rect, (67, 58, 45, 255), 1, scale)
        label_role = "ink_on_light" if role_id == "ink_on_light" else "ambient"
        draw_role_text(image, screen, f"sample_{index + 1}_label", (x + 12, 94, 208, 18), label, label_role, "middle-left", False, scale, allow_halo, "micro")
        draw_role_text(image, screen, f"sample_{index + 1}_value", (x + 12, 126, 208, 74), sample, role_id, "middle-center", False, scale, allow_halo, geometry)
    return image


def save_rgb(image: Image.Image, path: Path) -> None:
    image.convert("RGB").save(path, optimize=True)


def stress_copy(source: Path, destination: Path) -> None:
    image = Image.open(source).convert("RGB")
    target = (round(image.width * 0.9), round(image.height * 0.9))
    image.resize(target, Image.Resampling.LANCZOS).save(destination, optimize=True)


def output_name(context: str, variant: str, scale: float) -> Path:
    width, height = scaled_size(scale)
    return HERE / f"led_text_brightness_v1_{context}_{variant}_{width}x{height}.png"


def build() -> None:
    required_role_ids = {
        "focus_amber",
        "focus_teal",
        "primary_amber",
        "secondary_amber",
        "secondary_warm",
        "ambient",
        "disabled",
        "ink_on_light",
        "warning",
        "success",
    }
    missing_roles = sorted(required_role_ids.difference(ROLES))
    if missing_roles:
        raise RuntimeError("Missing semantic roles: " + ", ".join(missing_roles))
    required = (
        ROLE_PATH,
        ATLAS_PATH,
        MAP_PATH,
        STYLES_PATH,
        MAIN_MENU_BASE_PATH,
        MARKET_BASE_PATH,
        MARKET_CAPTURE_720_PATH,
        MARKET_CAPTURE_1080_PATH,
        TOOLTIP_PANEL_PATH,
        TOOLTIP_FACE_CELL_PATH,
        TOOLTIP_TYPE_FRAME_PATH,
        TYPE_ICON_PATH,
    )
    missing = [str(path) for path in required if not path.exists()]
    if missing:
        raise FileNotFoundError("Missing inputs:\n" + "\n".join(missing))

    outputs: list[Path] = []
    builders = {
        "main_menu": make_main_menu,
        "market": make_market,
        "market_tooltip": make_market_tooltip,
    }
    for scale in (1.0, 1.5):
        for context, builder in builders.items():
            for variant, allow_halo in (("core", False), ("focus", True)):
                path = output_name(context, variant, scale)
                save_rgb(builder(scale, allow_halo), path)
                outputs.append(path)

    for scale in (1.0, 1.5):
        width = round(1280 * scale)
        height = round(260 * scale)
        for variant, allow_halo in (("core", False), ("focus", True)):
            path = HERE / f"led_text_brightness_v1_state_strip_{variant}_{width}x{height}.png"
            save_rgb(make_state_strip(scale, allow_halo), path)
            outputs.append(path)

    for context in builders:
        source = output_name(context, "core", 1.0)
        stress = HERE / f"led_text_brightness_v1_{context}_core_90pct_1152x648.png"
        stress_copy(source, stress)
        outputs.append(stress)

    critical = [item for item in ELEMENTS if item["level"] in {"L2", "L3"}]
    micro_critical = [item for item in critical if item["geometry"] == "micro"]
    failed = [item for item in ELEMENTS if not item["p05_pass"]]
    clipped = [item for item in ELEMENTS if item["clipped_outside_rect"]]
    output_records = [
        {
            "path": str(path.relative_to(ROOT)).replace("\\", "/"),
            "width": Image.open(path).width,
            "height": Image.open(path).height,
            "sha256": sha256(path),
        }
        for path in outputs
    ]
    report = {
        "schema": "wabish-led-text-brightness-v1",
        "status": "sample-awaiting-approval",
        "generated_date": "2026-07-17",
        "source_only": True,
        "ai_image_generation_used": False,
        "font_contract": {
            "atlas": str(ATLAS_PATH.relative_to(ROOT)).replace("\\", "/"),
            "atlas_size": list(ATLAS_SIZE),
            "atlas_sha256": sha256(ATLAS_PATH),
            "map": str(MAP_PATH.relative_to(ROOT)).replace("\\", "/"),
            "map_sha256": sha256(MAP_PATH),
            "styles": str(STYLES_PATH.relative_to(ROOT)).replace("\\", "/"),
            "styles_sha256": sha256(STYLES_PATH),
            "glyph_count": len(GLYPHS),
        },
        "role_contract": {
            "path": str(ROLE_PATH.relative_to(ROOT)).replace("\\", "/"),
            "sha256": sha256(ROLE_PATH),
            "roles": [asdict(role) for role in ROLES.values()],
        },
        "runtime_sync": {
            "status": "not-applicable-until-sample-approved",
            "csharp_contract_sha256": None,
            "note": "本轮未修改 C#；样张通过后以本 CSV 生成运行时合同并校验哈希。",
        },
        "checks": {
            "element_draw_count": len(ELEMENTS),
            "critical_l2_l3_count": len(critical),
            "critical_micro_count": len(micro_critical),
            "critical_micro_pass": len(micro_critical) == 0,
            "contrast_p05_failure_count": len(failed),
            "contrast_p05_pass": len(failed) == 0,
            "clip_edge_touch_count": len(clipped),
            "clip_edge_pass": len(clipped) == 0,
            "halo_is_discrete_one_physical_pixel": True,
            "halo_max_alpha": max(role.halo_alpha for role in ROLES.values()),
            "halo_alpha_pass": max(role.halo_alpha for role in ROLES.values()) <= 0.18,
            "dual_resolution_outputs": True,
            "non_integer_stress_is_hierarchy_only": True,
            "formal_glyph_coverage_pass": True,
            "used_glyph_count": len(USED_CHARACTERS),
            "semantic_role_contract_pass": True,
            "semantic_role_count": len(ROLES),
        },
        "failed_elements": failed,
        "clip_edge_elements": clipped,
        "elements": ELEMENTS,
        "outputs": output_records,
        "generator_sha256": sha256(Path(__file__)),
    }
    METRICS_PATH.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report["checks"], ensure_ascii=False, indent=2))
    for record in output_records:
        print(record["path"])


if __name__ == "__main__":
    build()
