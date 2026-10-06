using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace PluginUiKit;

/// <summary>
/// Die ursprüngliche Codex-Verzierung aus TheExplorersCodex.CodexTheme: goldene L-förmige
/// Eckstriche, eine Linie-Raute-Linie-Trennung, und das Kompass-Symbol (dünner Ring + Raute/Nadel)
/// als Seitenleisten-Bildmarke.
/// </summary>
public sealed class CodexOrnaments(float cornerLength = UiMetrics.CornerLenMenu, float cornerThickness = UiMetrics.CornerThickMenu) : IUiOrnaments
{
    public void DrawCorner(Vector2 corner, int sx, int sy)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var length = cornerLength * scale;
        var thickness = cornerThickness * scale;
        var col = ImGui.GetColorU32(UiTheme.Active.Accent);
        var drawList = ImGui.GetWindowDrawList();

        drawList.AddLine(corner, corner + new Vector2(length * sx, 0f), col, thickness);
        drawList.AddLine(corner, corner + new Vector2(0f, length * sy), col, thickness);
    }

    public void DrawDivider(Vector2 pos, float width)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var col = ImGui.GetColorU32(UiTheme.Active.Accent with { W = 0.6f });
        var diamond = 4f * scale;
        var gap = 4f * scale;
        var halfLine = (width - diamond - gap * 2f) / 2f;

        var left = pos.X;
        drawList.AddLine(new Vector2(left, pos.Y), new Vector2(left + halfLine, pos.Y), col, 1f * scale);
        var center = new Vector2(left + halfLine + gap + diamond / 2f, pos.Y);
        drawList.AddCircleFilled(center, diamond / 2f, col, 4);
        var right = center.X + diamond / 2f + gap;
        drawList.AddLine(new Vector2(right, pos.Y), new Vector2(right + halfLine, pos.Y), col, 1f * scale);
    }

    public void DrawSidebarLogo(float size)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var center = cursor + new Vector2(size / 2f, size / 2f);
        var drawList = ImGui.GetWindowDrawList();
        var accent = ImGui.GetColorU32(UiTheme.Active.Accent);

        drawList.AddCircle(center, size * 0.39f, accent, 24, 1.5f);

        var r = size * 0.3f;
        var top = center + new Vector2(0f, -r);
        var right = center + new Vector2(r * 0.65f, 0f);
        var bottom = center + new Vector2(0f, r);
        var left = center + new Vector2(-r * 0.65f, 0f);
        drawList.AddQuadFilled(top, right, bottom, left, accent);

        ImGui.Dummy(new Vector2(size, size));
    }
}
