from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parent
FRAME_DIR = ROOT / "_frames"
OUTPUT = ROOT / "f009_v6b_r1_old_arcade_tower_core_motion_fullframe_60fps_20260721.webp"
FRAME_COUNT = 90


def main() -> None:
    frame_paths = [FRAME_DIR / f"frame_{index:04d}.png" for index in range(FRAME_COUNT)]
    missing = [str(path) for path in frame_paths if not path.is_file()]
    if missing:
        raise FileNotFoundError(f"缺少渲染帧：{missing[0]}")

    frames = []
    for path in frame_paths:
        with Image.open(path) as image:
            frames.append(image.convert("RGB"))

    # 17 / 17 / 16 毫秒循环恰好组成 60fps 的 50 毫秒三帧周期，
    # 90 帧总时长严格保持为 1.50 秒，不丢弃任何源帧。
    durations = [value for _ in range(FRAME_COUNT // 3) for value in (17, 17, 16)]
    frames[0].save(
        OUTPUT,
        format="WEBP",
        save_all=True,
        append_images=frames[1:],
        duration=durations,
        loop=0,
        quality=88,
        method=6,
        minimize_size=True,
        allow_mixed=True,
    )

    with Image.open(OUTPUT) as result:
        actual_frames = getattr(result, "n_frames", 1)
        if actual_frames != FRAME_COUNT:
            raise RuntimeError(f"WebP 帧数错误：{actual_frames}，预期 {FRAME_COUNT}")
        if result.size != (1280, 720):
            raise RuntimeError(f"WebP 尺寸错误：{result.size}")

    print(f"WEBP {actual_frames} 帧 1280x720 1.50s {OUTPUT}")


if __name__ == "__main__":
    main()
