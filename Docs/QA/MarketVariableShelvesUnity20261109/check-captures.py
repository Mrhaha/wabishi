from __future__ import annotations

import csv
import hashlib
import json
from pathlib import Path

from PIL import Image


QA = Path(__file__).resolve().parent
ROOT = QA.parents[2]
capture = json.loads((QA / "capture.json").read_text(encoding="utf-8-sig"))
assert capture["Passed"] and capture["PlayMode"], "Unity 图形验证尚未通过"
assert len(capture["Captures"]) == 24, "没有取得完整二十四张原生截图"

native = []
for name in capture["Captures"]:
    expected = (1280, 720) if name.endswith("1280x720.png") else (1920, 1080)
    with Image.open(QA / name) as image:
        assert image.size == expected, (name, image.size, expected)
        native.append({"file": name, "width": image.width, "height": image.height})

font_contract = json.loads((QA / "font-contract-metrics.json").read_text(encoding="utf-8-sig"))
assert all(font_contract["checks"].values()), "字库同源合同有未通过项"


def glyphs(path: Path) -> dict[str, dict[str, str]]:
    with path.open(encoding="utf-8-sig", newline="") as stream:
        return {row["codepoint"]: row for row in csv.DictReader(stream)}


before = glyphs(ROOT / "Temp/MarketUiFontBefore20261109/main_game_led_font_map.csv")
after = glyphs(ROOT / "Assets/Resources/Art/MainGame/Flow/main_game_led_font_map.csv")
changed = [code for code, old in before.items() if code in after and (old["bitmap_hex"] != after[code]["bitmap_hex"] or old["active_width"] != after[code]["active_width"])]
removed = [chr(int(code)) for code in before if code not in after]
assert not changed, "已有批准字形发生变化"

pressure = []
for stem in ("4dice_4spells", "spell_hover_longest", "owned_spell_cards"):
    with Image.open(QA / f"{stem}_1920x1080.png") as image:
        name = f"{stem}_pressure_1164x654.png"
        image.resize((1164, 654), Image.Resampling.BILINEAR).save(QA / name)
        pressure.append(name)

crops = []
with Image.open(QA / "4dice_4spells_1280x720.png") as image:
    for name, bounds in (("dice_price_key_1to1.png", (236, 242, 476, 332)), ("spell_card_1to1.png", (236, 390, 476, 614))):
        image.crop(bounds).save(QA / name)
        crops.append(name)

core_counts = []
for width, height, scale in ((1280, 720, 1.0), (1920, 1080, 1.5)):
    with Image.open(QA / f"4dice_4spells_{width}x{height}.png").convert("RGB") as image:
        for kind, y in (("dice", 249), ("spell", 530)):
            for slot in range(4):
                x = 248 + slot * 256
                bounds = (round(x * scale), round(y * scale), round((x + 216) * scale), round((y + 31) * scale))
                count = sum(1 for pixel in image.crop(bounds).get_flattened_data() if pixel == (255, 192, 74))
                assert count > 10, (width, kind, slot, "价格普通亮度核心缺失")
                core_counts.append({"resolution": width, "kind": kind, "slot": slot, "normal_core_pixels": count})

result = {
    "date": "2026-11-09",
    "native": native,
    "pressure": pressure,
    "native_crops": crops,
    "font": {"before_glyphs": len(before), "after_glyphs": len(after), "old_shapes_changed": changed, "added": [chr(int(code)) for code in after if code not in before], "removed_unreferenced_by_current_sources": removed, "contract_all_passed": True},
    "price_normal_core": core_counts,
    "runtime_script_sha256": hashlib.sha256((ROOT / "Assets/Scripts/DiceKingDemo.cs").read_bytes()).hexdigest(),
    "scope": "原生尺寸、已批准字形守恒及价格核心色检查；非整数图仅作识别压力，不替代人工字体与连续交互验收。",
}
(QA / "capture-metrics.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"native_captures": len(native), "old_shapes_changed": len(changed), "glyph_count": len(after), "price_core_checks": len(core_counts)}, ensure_ascii=False))
