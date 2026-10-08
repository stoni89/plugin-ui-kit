using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace PluginUiKit;

/// <summary>
/// Navigations-/Fensterrahmen-Bausteine: Titelleisten-Icon-Knopf, voll ausgefüllter Akzent-Knopf
/// (z.B. "Schließen"), ein Seitenleisten-Menüpunkt (mit optionalem "NEW"-Badge) und eine kleine
/// Statuskarte für den unteren Seitenleistenrand. Seiten-/Menüpunkt-Verwaltung selbst (welche
/// Seiten es gibt, welche gerade aktiv ist) bleibt Sache des jeweiligen Plugins - das hier sind
/// nur die wiederverwendbaren Zeichen-Bausteine dafür, 1:1 aus TheExplorersCodex portiert
/// (Windows.CodexOverlayWindow.DrawHeaderIconButton/ShowTooltip, Windows.CodexMenuWindow.
/// DrawNavItem/DrawSidebarCloseButton).
/// </summary>
public static class UiNav
{
    private static UiTheme T => UiTheme.Active;

    /// <summary>Tooltip mit schmalerem Innenabstand als ImGui-Standard.</summary>
    public static void ShowTooltip(string text)
    {
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(2f * scale, 2f * scale));
        ImGui.SetTooltip(text);
        ImGui.PopStyleVar();
    }

    /// <summary>Kleiner, randloser Icon-Knopf für eine Titelleiste (Einklappen/Schließen/Sperren) - Hover-Hintergrund etwas größer als der Knopf selbst.</summary>
    public static bool IconButton(FontAwesomeIcon icon, string id, Vector2 size, IFontHandle iconFont, string? tooltip = null, Vector4? iconColor = null)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton(id, size);
        var hovered = ImGui.IsItemHovered();

        var drawList = ImGui.GetWindowDrawList();
        if (hovered)
        {
            var hoverPadding = new Vector2(3f, 3f);
            drawList.AddRectFilled(cursor - hoverPadding, cursor + size + hoverPadding, ImGui.GetColorU32(T.BgSelected), UiMetrics.ControlRadius);
            if (!string.IsNullOrEmpty(tooltip))
                ShowTooltip(tooltip);
        }

        using (iconFont.Push())
        {
            var glyph = icon.ToIconString();
            var glyphWidth = ImGui.CalcTextSize(glyph).X;
            var lineHeight = ImGui.GetFontSize();
            drawList.AddText(
                cursor + new Vector2((size.X - glyphWidth) / 2f, (size.Y - lineHeight) / 2f),
                ImGui.GetColorU32(iconColor ?? T.TextSecondary),
                glyph);
        }

        if (hovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        return clicked;
    }

    /// <summary>Wie <see cref="IconButton(FontAwesomeIcon,string,Vector2,IFontHandle,string?,Vector4?)"/>, aber mit einem von Hand gezeichneten
    /// <see cref="UiIcon"/> statt eines FontAwesomeIcon-Glyphen (für Themes ohne Icon-Schriftart im Look, z.B. Ocean) - "hoverBg"/"hoverIconColor"
    /// überschreiben optional Hintergrund/Icon-Farbe bei Hover (z.B. Koralle für einen "Löschen"-Knopf statt der generischen BgSelected-Hervorhebung).</summary>
    public static bool IconButton(UiIcon icon, string id, Vector2 size, string? tooltip = null, Vector4? iconColor = null, Vector4? hoverBg = null, Vector4? hoverIconColor = null)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton(id, size);
        var hovered = ImGui.IsItemHovered();

        var drawList = ImGui.GetWindowDrawList();
        if (hovered)
        {
            var hoverPadding = new Vector2(3f, 3f);
            drawList.AddRectFilled(cursor - hoverPadding, cursor + size + hoverPadding, ImGui.GetColorU32(hoverBg ?? T.BgSelected), UiMetrics.ControlRadius);
            if (!string.IsNullOrEmpty(tooltip))
                ShowTooltip(tooltip);
        }

        var iconPx = MathF.Min(size.X, size.Y) * 0.62f;
        var fg = hovered ? (hoverIconColor ?? iconColor ?? T.TextSecondary) : (iconColor ?? T.TextSecondary);
        UiIcons.Draw(icon, cursor + (size - new Vector2(iconPx)) / 2f, iconPx, ImGui.GetColorU32(fg));

        if (hovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        return clicked;
    }

    /// <summary>Voll ausgefüllter Akzent-Knopf über die angegebene Breite/Höhe, mittig beschrifteter Text in TextOnAccent (z.B. "Schließen"-Knopf am unteren Seitenleistenrand).</summary>
    public static bool AccentButton(string label, Vector2 size, float scale, IFontHandle font)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton("##UiAccentButton_" + label, size);
        var hovered = ImGui.IsItemHovered();

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(cursor, cursor + size, ImGui.GetColorU32(T.Accent with { W = hovered ? 0.85f : 1f }), UiMetrics.ControlRadius * scale);

        using (font.Push())
        {
            var textSize = ImGui.CalcTextSize(label);
            drawList.AddText(cursor + (size - textSize) / 2f, ImGui.GetColorU32(T.TextOnAccent), label);
        }

        if (hovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        return clicked;
    }

    /// <summary>Ein Menüpunkt in der Seitenleiste: Icon + Label, BgSelected-Hervorhebung + 2px Accent-Balken links wenn aktiv, dezentere Hervorhebung bei Hover.
    /// "newBadgeDraw" (optional) zeichnet rechtsbündig ein zusätzliches Badge (z.B. "NEW") - als Callback, damit dieses Paket keine Meinung dazu hat, WIE das
    /// Badge aussieht (siehe UiWidgetsExtra.Badge für einen fertigen Baustein dafür).</summary>
    public static bool NavItem(string id, FontAwesomeIcon icon, string label, bool selected, float scale, IFontHandle iconFont, IFontHandle labelFont,
        float rightInset = 18f, System.Action<Vector2, float, float>? newBadgeDraw = null)
    {
        float iconWidth;
        using (iconFont.Push())
            iconWidth = ImGui.CalcTextSize(icon.ToIconString()).X;

        var height = 30f * scale;
        var width = ImGui.GetContentRegionAvail().X - rightInset * scale;
        var cursor = ImGui.GetCursorScreenPos();

        var clicked = ImGui.InvisibleButton(id, new Vector2(width, height));
        var hovered = ImGui.IsItemHovered();

        var drawList = ImGui.GetWindowDrawList();
        if (selected)
        {
            drawList.AddRectFilled(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.BgSelected), UiMetrics.ControlRadius);
            drawList.AddRectFilled(cursor, cursor + new Vector2(2f * scale, height), ImGui.GetColorU32(T.Accent));
        }
        else if (hovered)
        {
            drawList.AddRectFilled(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.BgSelected with { W = 0.5f }), UiMetrics.ControlRadius);
        }

        var fg = selected ? T.TextHeading : T.TextSecondary;
        var contentX = cursor.X + 10f * scale;

        using (iconFont.Push())
            drawList.AddText(new Vector2(contentX, cursor.Y + (height - ImGui.GetFontSize()) / 2f), ImGui.GetColorU32(selected ? T.Accent : fg), icon.ToIconString());

        using (labelFont.Push())
        {
            var labelSize = ImGui.CalcTextSize(label);
            drawList.AddText(new Vector2(contentX + iconWidth + 10f * scale, cursor.Y + (height - labelSize.Y) / 2f), ImGui.GetColorU32(fg), label);
        }

        newBadgeDraw?.Invoke(cursor, width, height);

        return clicked;
    }

    /// <summary>Wie <see cref="NavItem(string,FontAwesomeIcon,string,bool,float,IFontHandle,IFontHandle,float,System.Action{Vector2,float,float}?)"/>,
    /// aber mit einem von Hand gezeichneten <see cref="UiIcon"/> statt eines FontAwesomeIcon-Glyphen (für Themes ohne Icon-Schriftart im Look, z.B. Ocean).</summary>
    public static bool NavItem(string id, UiIcon icon, string label, bool selected, float scale, IFontHandle labelFont,
        float rightInset = 18f, System.Action<Vector2, float, float>? newBadgeDraw = null)
    {
        const float iconBoxSize = 16f;
        var height = 30f * scale;
        var width = ImGui.GetContentRegionAvail().X - rightInset * scale;
        var cursor = ImGui.GetCursorScreenPos();

        var clicked = ImGui.InvisibleButton(id, new Vector2(width, height));
        var hovered = ImGui.IsItemHovered();

        var drawList = ImGui.GetWindowDrawList();
        if (selected)
        {
            drawList.AddRectFilled(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.BgSelected), UiMetrics.ControlRadius);
            drawList.AddRectFilled(cursor, cursor + new Vector2(2f * scale, height), ImGui.GetColorU32(T.Accent));
        }
        else if (hovered)
        {
            drawList.AddRectFilled(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.BgSelected with { W = 0.5f }), UiMetrics.ControlRadius);
        }

        var fg = selected ? T.TextHeading : T.TextSecondary;
        var contentX = cursor.X + 10f * scale;
        var iconPx = iconBoxSize * scale;

        UiIcons.Draw(icon, new Vector2(contentX, cursor.Y + (height - iconPx) / 2f), iconPx, ImGui.GetColorU32(selected ? T.Accent : fg));

        using (labelFont.Push())
        {
            var labelSize = ImGui.CalcTextSize(label);
            drawList.AddText(new Vector2(contentX + iconPx + 10f * scale, cursor.Y + (height - labelSize.Y) / 2f), ImGui.GetColorU32(fg), label);
        }

        newBadgeDraw?.Invoke(cursor, width, height);
        return clicked;
    }

    /// <summary>Kleine Statuskarte am unteren Seitenleistenrand (Icon/Raute + fette Hauptzeile + gedämpfte Nebenzeile), z.B. für einen Sync-/Versionsstatus.</summary>
    public static void SidebarStatusCard(string headline, string subline, float scale, IFontHandle headlineFont, IFontHandle sublineFont, Vector4? accentColor = null)
    {
        var padding = new Vector2(12f * scale, 10f * scale);
        var width = ImGui.GetContentRegionAvail().X;

        float headlineHeight, sublineHeight;
        using (headlineFont.Push())
            headlineHeight = ImGui.GetTextLineHeight();
        using (sublineFont.Push())
            sublineHeight = ImGui.GetTextLineHeight();

        var height = padding.Y * 2f + headlineHeight + 4f * scale + sublineHeight;
        var cursor = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();

        drawList.AddRectFilled(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.BgStatus), UiMetrics.CardRadius * scale);
        drawList.AddRect(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.LineCard), UiMetrics.CardRadius * scale);

        var diamondColor = accentColor ?? T.OkFg;
        var diamondCenter = cursor + new Vector2(padding.X + 4f * scale, padding.Y + headlineHeight / 2f);
        UiWidgetsExtra.Diamond(diamondCenter, 4f * scale, ImGui.GetColorU32(diamondColor), ImGui.GetColorU32(diamondColor));

        using (headlineFont.Push())
            drawList.AddText(cursor + new Vector2(padding.X + 12f * scale, padding.Y), ImGui.GetColorU32(T.TextHeading), headline);
        using (sublineFont.Push())
            drawList.AddText(cursor + new Vector2(padding.X, padding.Y + headlineHeight + 4f * scale), ImGui.GetColorU32(T.TextMuted), subline);

        ImGui.Dummy(new Vector2(width, height));
    }

    /// <summary>Ein Eintrag in einer horizontalen Tab-Leiste (z.B. Erweiterungs-Tabs) - Label + optionaler, gedämpfter Zählertext dahinter,
    /// aktiv: TextHeading fett + Accent-Zähler + 2px Accent-Unterstrich; inaktiv: TextSecondary/TextMuted, heller bei Hover. Reiht Tabs per
    /// SameLine(0, gap) selbst aneinander, also IMMER innerhalb derselben Zeile aufrufen (erster Tab ohne vorheriges SameLine).</summary>
    public static bool TabItem(string id, string label, string? countText, bool selected, float scale, IFontHandle labelFont, IFontHandle countFont, bool isFirst, float gap)
    {
        if (!isFirst)
            ImGui.SameLine(0f, gap);

        float labelWidth, countWidth, height;
        using (labelFont.Push())
        {
            var size = ImGui.CalcTextSize(label);
            labelWidth = size.X;
            height = size.Y;
        }
        countWidth = 0f;
        if (!string.IsNullOrEmpty(countText))
        {
            using (countFont.Push())
                countWidth = ImGui.CalcTextSize(countText).X;
        }
        var countGap = string.IsNullOrEmpty(countText) ? 0f : 6f * scale;
        var underlineGap = 8f * scale;
        var width = labelWidth + countGap + countWidth;
        var totalHeight = height + underlineGap + 2f * scale;

        var cursor = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton(id, new Vector2(width, totalHeight));
        var hovered = ImGui.IsItemHovered();

        var dl = ImGui.GetWindowDrawList();
        var labelColor = selected ? T.TextHeading : hovered ? T.TextPrimary : T.TextSecondary;
        var countColor = selected ? T.Accent : T.TextMuted;

        using (labelFont.Push())
            dl.AddText(cursor, ImGui.GetColorU32(labelColor), label);
        if (!string.IsNullOrEmpty(countText))
        {
            using (countFont.Push())
                dl.AddText(cursor + new Vector2(labelWidth + countGap, (height - ImGui.CalcTextSize(countText).Y) / 2f), ImGui.GetColorU32(countColor), countText);
        }

        if (selected)
            dl.AddRectFilled(cursor + new Vector2(0f, height + underlineGap), cursor + new Vector2(width, height + underlineGap + 2f * scale), ImGui.GetColorU32(T.Accent));

        if (hovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        return clicked;
    }
}
