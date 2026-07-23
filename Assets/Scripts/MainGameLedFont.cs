using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public sealed class MainGameLedFont
{
    public struct HighlightSpan
    {
        public int Start;
        public int Length;
        public Color Color;
        public bool Underline;

        public HighlightSpan(int start, int length, Color color, bool underline)
        {
            Start = start;
            Length = length;
            Color = color;
            Underline = underline;
        }
    }

    public enum TextStyle
    {
        Display,
        Compact,
        Micro
    }

    public enum TextRole
    {
        FocusAmber,
        FocusTeal,
        PrimaryAmber,
        SecondaryAmber,
        SecondaryWarm,
        Ambient,
        Disabled,
        InkOnLight,
        Warning,
        Success
    }

    private sealed class Geometry
    {
        public TextStyle Style;
        public int Dot;
        public int Pitch;
        public int CharacterGap;
        public int LineGap;

        public int CellHeight
        {
            get { return 16 * Pitch - (Pitch - Dot); }
        }

        public int LineHeight
        {
            get { return CellHeight + LineGap; }
        }
    }

    private sealed class Glyph
    {
        public readonly ushort[] Rows = new ushort[16];
        public int ActiveWidth;
    }

    private sealed class RoleSpec
    {
        public TextStyle Preferred;
        public readonly List<TextStyle> Fallbacks = new List<TextStyle>();
        public Color CoreColor;
    }

    private struct StyledGlyph
    {
        public char Character;
        public bool OverrideColor;
        public Color Color;
        public bool Underline;
    }

    private const string AtlasResourcePath = "Art/MainGame/Flow/main_game_led_font_atlas";
    private const string MapResourcePath = "Art/MainGame/Flow/main_game_led_font_map";
    private const string StylesResourcePath = "Art/MainGame/Flow/main_game_led_font_styles";
    private const string RolesResourcePath = "Art/MainGame/Flow/main_game_led_text_roles";
    private const string BrightnessGroupsResourcePath = "Art/MainGame/Flow/main_game_led_text_brightness_groups";
    private const int TextureCacheLimit = 256;

    private readonly Dictionary<int, Glyph> glyphs = new Dictionary<int, Glyph>();
    private readonly Dictionary<TextRole, RoleSpec> roles = new Dictionary<TextRole, RoleSpec>();
    private readonly Dictionary<string, Texture2D> lineTextureCache = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
    private readonly Queue<string> lineTextureOrder = new Queue<string>();
    private Texture2D atlas;
    private Geometry displayGeometry;
    private Geometry compactGeometry;
    private Geometry microGeometry;
    private bool brightnessContractValid;

    public bool IsReady
    {
        get
        {
            return atlas != null
                && glyphs.Count > 0
                && displayGeometry != null
                && compactGeometry != null
                && microGeometry != null
                && roles.Count == Enum.GetValues(typeof(TextRole)).Length
                && brightnessContractValid;
        }
    }

    public static MainGameLedFont LoadFromResources()
    {
        Texture2D texture = Resources.Load<Texture2D>(AtlasResourcePath);
        TextAsset map = Resources.Load<TextAsset>(MapResourcePath);
        TextAsset styles = Resources.Load<TextAsset>(StylesResourcePath);
        TextAsset roleContract = Resources.Load<TextAsset>(RolesResourcePath);
        TextAsset brightnessGroups = Resources.Load<TextAsset>(BrightnessGroupsResourcePath);
        if (texture == null || map == null || styles == null || roleContract == null || brightnessGroups == null)
        {
            return null;
        }

        MainGameLedFont font = new MainGameLedFont();
        font.atlas = texture;
        font.atlas.filterMode = FilterMode.Point;
        font.atlas.wrapMode = TextureWrapMode.Clamp;
        font.ParseStyles(styles.text);
        font.ParseMap(map.text);
        font.ParseRoles(roleContract.text);
        font.ParseBrightnessGroups(brightnessGroups.text);
        return font.IsReady ? font : null;
    }

    public void Draw(Rect rect, string text, float requestedSize, Color color, TextAnchor alignment, float ignoredGlowIntensity)
    {
        DrawInternal(rect, text, requestedSize, color, alignment, false);
    }

    public void DrawWrapped(Rect rect, string text, float requestedSize, Color color, TextAnchor alignment, float ignoredGlowIntensity)
    {
        DrawInternal(rect, text, requestedSize, color, alignment, true);
    }

    public void DrawRole(Rect rect, string text, TextRole role, TextAnchor alignment, float opacity)
    {
        DrawRoleInternal(rect, text, role, alignment, false, opacity);
    }

    public void DrawRoleWrapped(Rect rect, string text, TextRole role, TextAnchor alignment, float opacity)
    {
        DrawRoleInternal(rect, text, role, alignment, true, opacity);
    }

    public void DrawRoleWrappedHighlights(
        Rect rect,
        string text,
        TextRole role,
        IList<HighlightSpan> highlights,
        TextAnchor alignment,
        float opacity)
    {
        if (!IsReady
            || string.IsNullOrEmpty(text)
            || Event.current == null
            || Event.current.type != EventType.Repaint)
        {
            return;
        }

        RoleSpec spec;
        if (!roles.TryGetValue(role, out spec))
        {
            return;
        }

        Geometry geometry = ChooseRoleGeometry(rect, text, spec, true);
        DrawHighlightedWithGeometry(rect, text, spec.CoreColor, highlights, alignment, geometry, opacity);
    }

    private void DrawInternal(Rect rect, string text, float requestedSize, Color color, TextAnchor alignment, bool wrapped)
    {
        if (!IsReady || string.IsNullOrEmpty(text) || Event.current == null || Event.current.type != EventType.Repaint)
        {
            return;
        }

        Geometry geometry = ChooseGeometry(rect, text, requestedSize, wrapped);
        DrawWithGeometry(rect, text, color, alignment, wrapped, geometry);
    }

    private void DrawRoleInternal(Rect rect, string text, TextRole role, TextAnchor alignment, bool wrapped, float opacity)
    {
        if (!IsReady || string.IsNullOrEmpty(text) || Event.current == null || Event.current.type != EventType.Repaint)
        {
            return;
        }

        RoleSpec spec;
        if (!roles.TryGetValue(role, out spec))
        {
            return;
        }

        Geometry geometry = ChooseRoleGeometry(rect, text, spec, wrapped);
        Color color = spec.CoreColor;
        color.a *= Mathf.Clamp01(opacity);
        DrawWithGeometry(rect, text, color, alignment, wrapped, geometry);
    }

    private void DrawWithGeometry(Rect rect, string text, Color color, TextAnchor alignment, bool wrapped, Geometry geometry)
    {
        List<string> lines = wrapped
            ? WrapLines(text, geometry, Mathf.Max(1f, rect.width))
            : SplitLines(text);
        if (lines.Count == 0)
        {
            return;
        }

        float blockHeight = geometry.CellHeight + Mathf.Max(0, lines.Count - 1) * geometry.LineHeight;
        float lineTop = AlignedBlockY(rect, blockHeight, alignment);
        Matrix4x4 drawingMatrix = GUI.matrix;
        float scaleX = AxisScale(drawingMatrix, Vector3.right);
        float scaleY = AxisScale(drawingMatrix, Vector3.up);
        if (scaleX <= 0.0001f || scaleY <= 0.0001f)
        {
            return;
        }

        Rect screenClip = PixelClipRect(TransformRect(drawingMatrix, rect));
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        GUI.matrix = Matrix4x4.identity;
        GUI.color = Multiply(previousColor, color);
        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];
            if (line.Length == 0)
            {
                continue;
            }

            float lineWidth = Measure(line, geometry);
            float lineX = AlignedLineX(rect, lineWidth, alignment);
            float lineY = lineTop + lineIndex * geometry.LineHeight;
            Vector3 screenOrigin = drawingMatrix.MultiplyPoint3x4(new Vector3(lineX, lineY, 0f));
            Texture2D texture = GetLineTexture(line, geometry, scaleX, scaleY);
            if (texture == null)
            {
                continue;
            }

            Rect destination = new Rect(
                Mathf.Round(screenOrigin.x),
                Mathf.Round(screenOrigin.y),
                texture.width,
                texture.height);
            DrawClippedTexture(destination, screenClip, texture);
        }

        GUI.color = previousColor;
        GUI.matrix = previousMatrix;
    }

    private void DrawHighlightedWithGeometry(
        Rect rect,
        string text,
        Color baseColor,
        IList<HighlightSpan> highlights,
        TextAnchor alignment,
        Geometry geometry,
        float opacity)
    {
        StyledGlyph[] glyphsByIndex = BuildStyledGlyphs(text, highlights);
        List<List<StyledGlyph>> lines = WrapStyledLines(glyphsByIndex, geometry, Mathf.Max(1f, rect.width));
        if (lines.Count == 0)
        {
            return;
        }

        float blockHeight = geometry.CellHeight + Mathf.Max(0, lines.Count - 1) * geometry.LineHeight;
        float lineTop = AlignedBlockY(rect, blockHeight, alignment);
        Matrix4x4 drawingMatrix = GUI.matrix;
        float scaleX = AxisScale(drawingMatrix, Vector3.right);
        float scaleY = AxisScale(drawingMatrix, Vector3.up);
        if (scaleX <= 0.0001f || scaleY <= 0.0001f)
        {
            return;
        }

        Rect screenClip = PixelClipRect(TransformRect(drawingMatrix, rect));
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        GUI.matrix = Matrix4x4.identity;
        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            List<StyledGlyph> line = lines[lineIndex];
            if (line.Count == 0)
            {
                continue;
            }

            float lineWidth = MeasureStyledLine(line, geometry);
            float lineX = AlignedLineX(rect, lineWidth, alignment);
            float lineY = lineTop + lineIndex * geometry.LineHeight;
            int runStart = 0;
            float runOffset = 0f;
            while (runStart < line.Count)
            {
                StyledGlyph first = line[runStart];
                int runEnd = runStart + 1;
                while (runEnd < line.Count && SameStyle(first, line[runEnd]))
                {
                    runEnd++;
                }

                StringBuilder runText = new StringBuilder(runEnd - runStart);
                for (int glyphIndex = runStart; glyphIndex < runEnd; glyphIndex++)
                {
                    runText.Append(line[glyphIndex].Character);
                }

                string run = runText.ToString();
                Vector3 screenOrigin = drawingMatrix.MultiplyPoint3x4(new Vector3(lineX + runOffset, lineY, 0f));
                Texture2D texture = GetLineTexture(run, geometry, scaleX, scaleY);
                if (texture != null)
                {
                    Color runColor = first.OverrideColor ? first.Color : baseColor;
                    runColor.a *= Mathf.Clamp01(opacity);
                    GUI.color = Multiply(previousColor, runColor);
                    Rect destination = new Rect(
                        Mathf.Round(screenOrigin.x),
                        Mathf.Round(screenOrigin.y),
                        texture.width,
                        texture.height);
                    DrawClippedTexture(destination, screenClip, texture);
                    if (first.Underline)
                    {
                        DrawClippedSolidRect(
                            new Rect(destination.x, destination.yMax + 1f, destination.width, 1f),
                            screenClip);
                    }
                }

                for (int glyphIndex = runStart; glyphIndex < runEnd; glyphIndex++)
                {
                    StyledGlyph glyph = line[glyphIndex];
                    runOffset += Advance(glyph.Character, GlyphFor(glyph.Character), geometry);
                }
                runStart = runEnd;
            }
        }

        GUI.color = previousColor;
        GUI.matrix = previousMatrix;
    }

    private static StyledGlyph[] BuildStyledGlyphs(string text, IList<HighlightSpan> highlights)
    {
        StyledGlyph[] result = new StyledGlyph[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            result[i] = new StyledGlyph
            {
                Character = text[i],
                OverrideColor = false,
                Color = Color.white,
                Underline = false
            };
        }

        if (highlights == null)
        {
            return result;
        }

        for (int spanIndex = 0; spanIndex < highlights.Count; spanIndex++)
        {
            HighlightSpan span = highlights[spanIndex];
            int start = Mathf.Clamp(span.Start, 0, text.Length);
            int end = Mathf.Clamp(span.Start + Mathf.Max(0, span.Length), start, text.Length);
            for (int characterIndex = start; characterIndex < end; characterIndex++)
            {
                StyledGlyph glyph = result[characterIndex];
                glyph.OverrideColor = true;
                glyph.Color = span.Color;
                glyph.Underline = span.Underline;
                result[characterIndex] = glyph;
            }
        }

        return result;
    }

    private List<List<StyledGlyph>> WrapStyledLines(StyledGlyph[] glyphsByIndex, Geometry geometry, float maxWidth)
    {
        List<List<StyledGlyph>> lines = new List<List<StyledGlyph>>();
        List<StyledGlyph> current = new List<StyledGlyph>();
        for (int i = 0; i < glyphsByIndex.Length; i++)
        {
            StyledGlyph glyph = glyphsByIndex[i];
            if (glyph.Character == '\r')
            {
                continue;
            }
            if (glyph.Character == '\n')
            {
                TrimStyledLineEnd(current);
                lines.Add(current);
                current = new List<StyledGlyph>();
                continue;
            }

            current.Add(glyph);
            if (current.Count > 1 && MeasureStyledLine(current, geometry) > maxWidth)
            {
                StyledGlyph overflow = current[current.Count - 1];
                current.RemoveAt(current.Count - 1);
                TrimStyledLineEnd(current);
                lines.Add(current);
                current = new List<StyledGlyph> { overflow };
            }
        }

        TrimStyledLineEnd(current);
        lines.Add(current);
        return lines;
    }

    private float MeasureStyledLine(List<StyledGlyph> line, Geometry geometry)
    {
        int width = 0;
        for (int i = 0; i < line.Count; i++)
        {
            char character = line[i].Character;
            width += Advance(character, GlyphFor(character), geometry);
        }

        return Mathf.Max(0, width - geometry.CharacterGap);
    }

    private static void TrimStyledLineEnd(List<StyledGlyph> line)
    {
        while (line.Count > 0 && char.IsWhiteSpace(line[line.Count - 1].Character))
        {
            line.RemoveAt(line.Count - 1);
        }
    }

    private static bool SameStyle(StyledGlyph left, StyledGlyph right)
    {
        return left.OverrideColor == right.OverrideColor
            && left.Underline == right.Underline
            && (!left.OverrideColor || left.Color == right.Color);
    }

    private void ParseRoles(string text)
    {
        roles.Clear();
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 6)
            {
                continue;
            }

            TextRole role;
            TextStyle preferred;
            Color coreColor;
            float alpha;
            if (!TryParseRole(columns[0], out role)
                || !TryParseStyle(columns[2], out preferred)
                || !TryParseHexColor(columns[4], out coreColor)
                || !float.TryParse(columns[5], NumberStyles.Float, CultureInfo.InvariantCulture, out alpha))
            {
                continue;
            }

            RoleSpec spec = new RoleSpec
            {
                Preferred = preferred,
                CoreColor = new Color(coreColor.r, coreColor.g, coreColor.b, Mathf.Clamp01(alpha))
            };
            string[] fallbackNames = columns.Length > 3
                ? columns[3].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                : new string[0];
            for (int fallbackIndex = 0; fallbackIndex < fallbackNames.Length; fallbackIndex++)
            {
                TextStyle fallback;
                if (TryParseStyle(fallbackNames[fallbackIndex], out fallback) && !spec.Fallbacks.Contains(fallback))
                {
                    spec.Fallbacks.Add(fallback);
                }
            }

            roles[role] = spec;
        }
    }

    private void ParseBrightnessGroups(string text)
    {
        brightnessContractValid = false;
        if (roles.Count != Enum.GetValues(typeof(TextRole)).Length)
        {
            return;
        }

        bool valid = true;
        HashSet<TextRole> seenRoles = new HashSet<TextRole>();
        Dictionary<string, double> gradeTargets = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            TextRole role;
            double target;
            double tolerance;
            RoleSpec spec;
            if (columns.Length < 4
                || !TryParseRole(columns[0], out role)
                || !double.TryParse(columns[2], NumberStyles.Float, CultureInfo.InvariantCulture, out target)
                || !double.TryParse(columns[3], NumberStyles.Float, CultureInfo.InvariantCulture, out tolerance)
                || tolerance < 0d
                || !roles.TryGetValue(role, out spec)
                || !seenRoles.Add(role))
            {
                valid = false;
                continue;
            }

            string grade = columns[1].Trim();
            if (grade.Length == 0 || Math.Abs(OklabLightness(spec.CoreColor) - target) > tolerance + 0.000001d)
            {
                valid = false;
            }

            double existingTarget;
            if (gradeTargets.TryGetValue(grade, out existingTarget))
            {
                if (Math.Abs(existingTarget - target) > Math.Max(tolerance, 0.000001d))
                {
                    valid = false;
                }
            }
            else
            {
                gradeTargets[grade] = target;
            }
        }

        brightnessContractValid = valid && seenRoles.Count == roles.Count;
    }

    private static double OklabLightness(Color color)
    {
        double red = SrgbToLinear(color.r);
        double green = SrgbToLinear(color.g);
        double blue = SrgbToLinear(color.b);
        double l = 0.4122214708d * red + 0.5363325363d * green + 0.0514459929d * blue;
        double m = 0.2119034982d * red + 0.6806995451d * green + 0.1073969566d * blue;
        double s = 0.0883024619d * red + 0.2817188376d * green + 0.6299787005d * blue;
        double lRoot = Math.Pow(Math.Max(0d, l), 1d / 3d);
        double mRoot = Math.Pow(Math.Max(0d, m), 1d / 3d);
        double sRoot = Math.Pow(Math.Max(0d, s), 1d / 3d);
        return 0.2104542553d * lRoot + 0.793617785d * mRoot - 0.0040720468d * sRoot;
    }

    private static double SrgbToLinear(float channel)
    {
        double value = Mathf.Clamp01(channel);
        return value <= 0.04045d
            ? value / 12.92d
            : Math.Pow((value + 0.055d) / 1.055d, 2.4d);
    }

    private void ParseMap(string text)
    {
        glyphs.Clear();
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 10)
            {
                continue;
            }

            int codepoint;
            int activeWidth;
            if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out codepoint)
                || !int.TryParse(columns[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out activeWidth))
            {
                continue;
            }

            string[] rowValues = columns[9].Split(';');
            if (rowValues.Length != 16)
            {
                continue;
            }

            Glyph glyph = new Glyph
            {
                ActiveWidth = Mathf.Clamp(activeWidth, 1, 16)
            };
            bool valid = true;
            for (int row = 0; row < glyph.Rows.Length; row++)
            {
                ushort bits;
                if (!ushort.TryParse(rowValues[row], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bits))
                {
                    valid = false;
                    break;
                }

                glyph.Rows[row] = bits;
            }

            if (valid)
            {
                glyphs[codepoint] = glyph;
            }
        }
    }

    private void ParseStyles(string text)
    {
        displayGeometry = null;
        compactGeometry = null;
        microGeometry = null;
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 5)
            {
                continue;
            }

            TextStyle style;
            if (string.Equals(columns[0], "display", StringComparison.OrdinalIgnoreCase))
            {
                style = TextStyle.Display;
            }
            else if (string.Equals(columns[0], "compact", StringComparison.OrdinalIgnoreCase))
            {
                style = TextStyle.Compact;
            }
            else if (string.Equals(columns[0], "micro", StringComparison.OrdinalIgnoreCase))
            {
                style = TextStyle.Micro;
            }
            else
            {
                continue;
            }

            int dot;
            int pitch;
            int characterGap;
            int lineGap;
            if (!int.TryParse(columns[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out dot)
                || !int.TryParse(columns[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out pitch)
                || !int.TryParse(columns[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out characterGap)
                || !int.TryParse(columns[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out lineGap)
                || dot <= 0
                || pitch < dot
                || characterGap < 0
                || lineGap < 0)
            {
                continue;
            }

            Geometry geometry = new Geometry
            {
                Style = style,
                Dot = dot,
                Pitch = pitch,
                CharacterGap = characterGap,
                LineGap = lineGap
            };
            if (style == TextStyle.Display)
            {
                displayGeometry = geometry;
            }
            else if (style == TextStyle.Compact)
            {
                compactGeometry = geometry;
            }
            else
            {
                microGeometry = geometry;
            }
        }
    }

    private Geometry ChooseGeometry(Rect rect, string text, float requestedSize, bool wrapped)
    {
        if (requestedSize >= 18f)
        {
            if (Fits(rect, text, displayGeometry, wrapped, 3f))
            {
                return displayGeometry;
            }

            if (Fits(rect, text, compactGeometry, wrapped, 2f))
            {
                return compactGeometry;
            }

            return microGeometry;
        }

        if (requestedSize >= 12f && Fits(rect, text, compactGeometry, wrapped, 2f))
        {
            return compactGeometry;
        }

        return microGeometry;
    }

    private Geometry ChooseRoleGeometry(Rect rect, string text, RoleSpec spec, bool wrapped)
    {
        Geometry preferred = GeometryFor(spec.Preferred);
        if (Fits(rect, text, preferred, wrapped, HeightTolerance(preferred)))
        {
            return preferred;
        }

        Geometry last = preferred;
        for (int i = 0; i < spec.Fallbacks.Count; i++)
        {
            Geometry fallback = GeometryFor(spec.Fallbacks[i]);
            last = fallback;
            if (Fits(rect, text, fallback, wrapped, HeightTolerance(fallback)))
            {
                return fallback;
            }
        }

        // Semantic roles intentionally stop at their declared floor. A primary label
        // that does not fit Compact must be reflowed or given more room, never silently
        // degraded to Micro.
        return last;
    }

    private Geometry GeometryFor(TextStyle style)
    {
        if (style == TextStyle.Display)
        {
            return displayGeometry;
        }

        if (style == TextStyle.Compact)
        {
            return compactGeometry;
        }

        return microGeometry;
    }

    private static float HeightTolerance(Geometry geometry)
    {
        return geometry.Style == TextStyle.Display ? 3f : geometry.Style == TextStyle.Compact ? 2f : 1f;
    }

    private static bool TryParseStyle(string value, out TextStyle style)
    {
        if (string.Equals(value, "display", StringComparison.OrdinalIgnoreCase))
        {
            style = TextStyle.Display;
            return true;
        }

        if (string.Equals(value, "compact", StringComparison.OrdinalIgnoreCase))
        {
            style = TextStyle.Compact;
            return true;
        }

        if (string.Equals(value, "micro", StringComparison.OrdinalIgnoreCase))
        {
            style = TextStyle.Micro;
            return true;
        }

        style = TextStyle.Micro;
        return false;
    }

    private static bool TryParseRole(string value, out TextRole role)
    {
        string normalized = (value ?? string.Empty).Trim().Replace("_", string.Empty);
        TextRole[] values = (TextRole[])Enum.GetValues(typeof(TextRole));
        for (int i = 0; i < values.Length; i++)
        {
            string candidate = values[i].ToString().Replace("_", string.Empty);
            if (string.Equals(candidate, normalized, StringComparison.OrdinalIgnoreCase))
            {
                role = values[i];
                return true;
            }
        }

        role = TextRole.Ambient;
        return false;
    }

    private static bool TryParseHexColor(string value, out Color color)
    {
        string hex = (value ?? string.Empty).Trim();
        if (hex.StartsWith("#", StringComparison.Ordinal))
        {
            hex = hex.Substring(1);
        }

        int rgb;
        if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rgb))
        {
            color = Color.white;
            return false;
        }

        color = new Color(
            ((rgb >> 16) & 0xff) / 255f,
            ((rgb >> 8) & 0xff) / 255f,
            (rgb & 0xff) / 255f,
            1f);
        return true;
    }

    private bool Fits(Rect rect, string text, Geometry geometry, bool wrapped, float heightTolerance)
    {
        List<string> lines = wrapped
            ? WrapLines(text, geometry, Mathf.Max(1f, rect.width))
            : SplitLines(text);
        float maximumWidth = 0f;
        for (int i = 0; i < lines.Count; i++)
        {
            maximumWidth = Mathf.Max(maximumWidth, Measure(lines[i], geometry));
        }

        float height = geometry.CellHeight + Mathf.Max(0, lines.Count - 1) * geometry.LineHeight;
        return maximumWidth <= rect.width + 0.01f && height <= rect.height + heightTolerance;
    }

    private Texture2D GetLineTexture(string text, Geometry geometry, float scaleX, float scaleY)
    {
        int scaleXKey = Mathf.Max(1, Mathf.RoundToInt(scaleX * 1000f));
        int scaleYKey = Mathf.Max(1, Mathf.RoundToInt(scaleY * 1000f));
        string key = ((int)geometry.Style).ToString(CultureInfo.InvariantCulture)
            + "|" + scaleXKey.ToString(CultureInfo.InvariantCulture)
            + "|" + scaleYKey.ToString(CultureInfo.InvariantCulture)
            + "|" + text;
        Texture2D cached;
        if (lineTextureCache.TryGetValue(key, out cached) && cached != null)
        {
            return cached;
        }

        Texture2D texture = BuildLineTexture(text, geometry, scaleXKey / 1000f, scaleYKey / 1000f);
        lineTextureCache[key] = texture;
        lineTextureOrder.Enqueue(key);
        while (lineTextureOrder.Count > TextureCacheLimit)
        {
            string oldest = lineTextureOrder.Dequeue();
            Texture2D evicted;
            if (!lineTextureCache.TryGetValue(oldest, out evicted))
            {
                continue;
            }

            lineTextureCache.Remove(oldest);
            if (evicted != null)
            {
                UnityEngine.Object.Destroy(evicted);
            }
        }

        return texture;
    }

    private Texture2D BuildLineTexture(string text, Geometry geometry, float scaleX, float scaleY)
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(Measure(text, geometry) * scaleX));
        int height = Mathf.Max(1, Mathf.RoundToInt(geometry.CellHeight * scaleY));
        Color32[] pixels = new Color32[width * height];
        Color32 lit = new Color32(255, 255, 255, 255);
        int cursor = 0;
        for (int characterIndex = 0; characterIndex < text.Length; characterIndex++)
        {
            char character = text[characterIndex];
            Glyph glyph = GlyphFor(character);
            if (glyph != null && !char.IsWhiteSpace(character))
            {
                for (int row = 0; row < glyph.Rows.Length; row++)
                {
                    ushort bits = glyph.Rows[row];
                    if (bits == 0)
                    {
                        continue;
                    }

                    for (int column = 0; column < 16; column++)
                    {
                        if ((bits & 1 << column) == 0)
                        {
                            continue;
                        }

                        int x0 = Mathf.RoundToInt((cursor + column * geometry.Pitch) * scaleX);
                        int x1 = Mathf.RoundToInt((cursor + column * geometry.Pitch + geometry.Dot) * scaleX);
                        int y0 = Mathf.RoundToInt(row * geometry.Pitch * scaleY);
                        int y1 = Mathf.RoundToInt((row * geometry.Pitch + geometry.Dot) * scaleY);
                        x0 = Mathf.Clamp(x0, 0, width);
                        x1 = Mathf.Clamp(Mathf.Max(x0 + 1, x1), 0, width);
                        y0 = Mathf.Clamp(y0, 0, height);
                        y1 = Mathf.Clamp(Mathf.Max(y0 + 1, y1), 0, height);
                        for (int topY = y0; topY < y1; topY++)
                        {
                            int textureY = height - 1 - topY;
                            int rowOffset = textureY * width;
                            for (int x = x0; x < x1; x++)
                            {
                                pixels[rowOffset + x] = lit;
                            }
                        }
                    }
                }
            }

            cursor += Advance(character, glyph, geometry);
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
        texture.name = "WabishLedText_" + geometry.Style + "_" + text;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private float Measure(string text, Geometry geometry)
    {
        int width = 0;
        for (int i = 0; i < text.Length; i++)
        {
            Glyph glyph = GlyphFor(text[i]);
            width += Advance(text[i], glyph, geometry);
        }

        return Mathf.Max(0, width - geometry.CharacterGap);
    }

    private Glyph GlyphFor(char character)
    {
        Glyph glyph;
        if (glyphs.TryGetValue(character, out glyph))
        {
            return glyph;
        }

        glyphs.TryGetValue('?', out glyph);
        return glyph;
    }

    private static int Advance(char character, Glyph glyph, Geometry geometry)
    {
        if (character == '\t')
        {
            return geometry.Pitch * 6;
        }

        if (char.IsWhiteSpace(character))
        {
            return geometry.Pitch * 3;
        }

        int activeWidth = glyph != null ? glyph.ActiveWidth : 8;
        return activeWidth * geometry.Pitch - (geometry.Pitch - geometry.Dot) + geometry.CharacterGap;
    }

    private List<string> WrapLines(string text, Geometry geometry, float maxWidth)
    {
        List<string> result = new List<string>();
        string[] sourceLines = text.Replace("\r", string.Empty).Split('\n');
        for (int sourceIndex = 0; sourceIndex < sourceLines.Length; sourceIndex++)
        {
            string source = sourceLines[sourceIndex];
            if (source.Length == 0)
            {
                result.Add(string.Empty);
                continue;
            }

            string current = string.Empty;
            for (int i = 0; i < source.Length; i++)
            {
                string candidate = current + source[i];
                if (current.Length > 0 && Measure(candidate, geometry) > maxWidth)
                {
                    result.Add(current.TrimEnd());
                    current = source[i].ToString();
                }
                else
                {
                    current = candidate;
                }
            }

            result.Add(current.TrimEnd());
        }

        return result;
    }

    private static List<string> SplitLines(string text)
    {
        return new List<string>(text.Replace("\r", string.Empty).Split('\n'));
    }

    private static void DrawClippedTexture(Rect destination, Rect clip, Texture2D texture)
    {
        float xMin = Mathf.Max(destination.xMin, clip.xMin);
        float yMin = Mathf.Max(destination.yMin, clip.yMin);
        float xMax = Mathf.Min(destination.xMax, clip.xMax);
        float yMax = Mathf.Min(destination.yMax, clip.yMax);
        if (xMax <= xMin || yMax <= yMin || destination.width <= 0f || destination.height <= 0f)
        {
            return;
        }

        Rect visible = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        float uMin = (visible.xMin - destination.xMin) / destination.width;
        float uMax = (visible.xMax - destination.xMin) / destination.width;
        float vMin = 1f - (visible.yMax - destination.yMin) / destination.height;
        float vMax = 1f - (visible.yMin - destination.yMin) / destination.height;
        GUI.DrawTextureWithTexCoords(visible, texture, Rect.MinMaxRect(uMin, vMin, uMax, vMax), true);
    }

    private static void DrawClippedSolidRect(Rect destination, Rect clip)
    {
        float xMin = Mathf.Max(destination.xMin, clip.xMin);
        float yMin = Mathf.Max(destination.yMin, clip.yMin);
        float xMax = Mathf.Min(destination.xMax, clip.xMax);
        float yMax = Mathf.Min(destination.yMax, clip.yMax);
        if (xMax <= xMin || yMax <= yMin)
        {
            return;
        }

        GUI.DrawTexture(Rect.MinMaxRect(xMin, yMin, xMax, yMax), Texture2D.whiteTexture);
    }

    private static Rect TransformRect(Matrix4x4 matrix, Rect rect)
    {
        Vector3 topLeft = matrix.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMin, 0f));
        Vector3 topRight = matrix.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMin, 0f));
        Vector3 bottomLeft = matrix.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMax, 0f));
        Vector3 bottomRight = matrix.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMax, 0f));
        float xMin = Mathf.Min(topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
        float yMin = Mathf.Min(topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
        float xMax = Mathf.Max(topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
        float yMax = Mathf.Max(topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Rect PixelClipRect(Rect rect)
    {
        return Rect.MinMaxRect(
            Mathf.Floor(rect.xMin),
            Mathf.Floor(rect.yMin),
            Mathf.Ceil(rect.xMax),
            Mathf.Ceil(rect.yMax));
    }

    private static float AxisScale(Matrix4x4 matrix, Vector3 axis)
    {
        Vector3 transformed = matrix.MultiplyVector(axis);
        return Mathf.Sqrt(transformed.x * transformed.x + transformed.y * transformed.y);
    }

    private static Color Multiply(Color left, Color right)
    {
        return new Color(left.r * right.r, left.g * right.g, left.b * right.b, left.a * right.a);
    }

    private static float AlignedLineX(Rect rect, float width, TextAnchor alignment)
    {
        if (alignment == TextAnchor.UpperCenter || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.LowerCenter)
        {
            return rect.x + (rect.width - width) * 0.5f;
        }

        if (alignment == TextAnchor.UpperRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.LowerRight)
        {
            return rect.xMax - width;
        }

        return rect.x;
    }

    private static float AlignedBlockY(Rect rect, float height, TextAnchor alignment)
    {
        if (alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.MiddleRight)
        {
            return rect.y + (rect.height - height) * 0.5f;
        }

        if (alignment == TextAnchor.LowerLeft || alignment == TextAnchor.LowerCenter || alignment == TextAnchor.LowerRight)
        {
            return rect.yMax - height;
        }

        return rect.y;
    }
}
