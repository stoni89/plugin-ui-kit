using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Bindings.ImGui;

namespace PluginUiKit;

/// <summary>
/// Einklappen/Schließen-Steuerung für Fenster OHNE eigene ImGui-Titelleiste (NoTitleBar) - ein
/// Baustein, kein fertiges Fenster: jedes Plugin hält selbst den eingeklappten Zustand (meist in
/// der eigenen Configuration, damit er Neustarts übersteht) und zeichnet je nach Zustand entweder
/// die normale Seite PLUS <see cref="DrawExpandedButtons"/>, oder anstelle der normalen Seite
/// <see cref="DrawChrome"/>/<see cref="DrawCollapsedButtons"/>/<see cref="DrawDragArea"/> für die
/// geschrumpfte Mini-Leiste. Marke/Logo bleiben bewusst Sache des jeweiligen Plugins (eigene
/// Schrift/eigenes Icon, siehe IUiOrnaments.DrawSidebarLogo) - dieser Baustein zeichnet nur die
/// Knöpfe/den Rahmen drumherum, damit er unverändert auch von anderen PluginUiKit-Nutzern (z.B.
/// BigFishHelper) mit eigenem Look wiederverwendet werden kann.
/// </summary>
public static class UiWindowControls
{
    public const float IconButtonSize = 30f;
    public const float IconButtonRadius = 4f;
    public const float CollapsedWidth = 560f;
    public const float CollapsedHeight = 64f;

    // Hover-Farben für den Schließen-Knopf (gedämpftes Rot/helles Rosa) - fest statt aus UiTheme,
    // dieselbe Warnfarbe soll unabhängig von der jeweiligen Plugin-Palette gelten (Referenzvorgabe).
    private static readonly Vector4 CloseHoverBg = UiTheme.Hex("#4A231C");
    private static readonly Vector4 CloseHoverFg = UiTheme.Hex("#F6C9BE");

    private static UiTheme T => UiTheme.Active;

    public readonly struct ButtonsResult
    {
        /// <summary>Einklappen (ausgeklappter Zustand) bzw. Ausklappen (eingeklappter Zustand) wurde gerade angeklickt.</summary>
        public bool ToggleCollapse { get; init; }
        public bool Close { get; init; }
    }

    /// <summary>Breite, die <see cref="DrawExpandedButtons"/> inklusive eines evtl. Trenners zu seiten-eigenen
    /// Knöpfen einnimmt - für die Seite, die daneben noch Platz reservieren möchte (Page.HeaderRightReserve).</summary>
    public static float ExpandedButtonsWidth(float scale, bool hasPageButtons)
    {
        var width = IconButtonSize * 2f * scale + 4f * scale;
        if (hasPageButtons)
            width += 6f * scale + 1f * scale + 4f * scale;
        return width;
    }

    /// <summary>
    /// Ausgeklappter Zustand - zwei 30x30-IconButtons (Einklappen/Schließen) ganz rechts in der
    /// Kopfzeile der Seite, optional davor ein senkrechter Trenner zu seiten-eigenen Knöpfen (z.B.
    /// Reset/Save). "rightEdgeX"/"centerY" sind fenster-lokale (ImGui.SetCursorPos-taugliche)
    /// Koordinaten: der rechte Rand, an dem die Knopfreihe endet, und die Y-Mitte, auf die beide
    /// Knöpfe zentriert werden. Muss im Koordinatensystem des FENSTERS aufgerufen werden (nicht
    /// innerhalb eines eigenen BeginChild für die Seite), damit die Knöpfe zentral vom Fenster
    /// gezeichnet werden können, wie gefordert.
    /// </summary>
    public static ButtonsResult DrawExpandedButtons(float scale, float rightEdgeX, float centerY, bool hasPageButtons,
        string? collapseTooltip = null, string? closeTooltip = null)
    {
        var size = IconButtonSize * scale;
        var gap = 4f * scale;

        var closeMin = new Vector2(rightEdgeX - size, centerY - size / 2f);
        var collapseMin = new Vector2(closeMin.X - gap - size, closeMin.Y);

        if (hasPageButtons)
        {
            var dividerX = collapseMin.X - 6f * scale;
            var dividerTop = centerY - 13f * scale;
            ImGui.GetWindowDrawList().AddLine(new Vector2(dividerX, dividerTop), new Vector2(dividerX, dividerTop + 26f * scale),
                ImGui.GetColorU32(T.LineDisabled), 1f * scale);
        }

        var collapseClicked = DrawIconButton("##UiWindowCollapse", collapseMin, size, FontAwesomeIcon.ChevronUp, 13f * scale,
            T.BgSelected, T.TextHeading, alwaysHighlighted: false, collapseTooltip);
        var closeClicked = DrawIconButton("##UiWindowClose", closeMin, size, FontAwesomeIcon.Times, 12f * scale,
            CloseHoverBg, CloseHoverFg, alwaysHighlighted: false, closeTooltip);

        return new ButtonsResult { ToggleCollapse = collapseClicked, Close = closeClicked };
    }

    /// <summary>
    /// Eingeklappter Zustand - Ausklappen (Chevron nach unten, DAUERHAFT hervorgehoben) · Trenner
    /// 1x18 · Schließen, ganz rechts in der Mini-Leiste. Dieselben Koordinatenregeln wie
    /// <see cref="DrawExpandedButtons"/>.
    /// </summary>
    public static ButtonsResult DrawCollapsedButtons(float scale, float rightEdgeX, float centerY,
        string? expandTooltip = null, string? closeTooltip = null)
    {
        var size = IconButtonSize * scale;
        var gap = 4f * scale;

        var closeMin = new Vector2(rightEdgeX - size, centerY - size / 2f);
        var dividerX = closeMin.X - gap - 1f * scale;
        var dividerTop = centerY - 9f * scale;
        ImGui.GetWindowDrawList().AddLine(new Vector2(dividerX, dividerTop), new Vector2(dividerX, dividerTop + 18f * scale),
            ImGui.GetColorU32(T.LineDisabled), 1f * scale);

        var expandMin = new Vector2(dividerX - gap - size, closeMin.Y);

        var expandClicked = DrawIconButton("##UiWindowExpand", expandMin, size, FontAwesomeIcon.ChevronDown, 13f * scale,
            T.BgSelected, T.TextHeading, alwaysHighlighted: true, expandTooltip);
        var closeClicked = DrawIconButton("##UiWindowCollapsedClose", closeMin, size, FontAwesomeIcon.Times, 12f * scale,
            CloseHoverBg, CloseHoverFg, alwaysHighlighted: false, closeTooltip);

        return new ButtonsResult { ToggleCollapse = expandClicked, Close = closeClicked };
    }

    private static bool DrawIconButton(string id, Vector2 min, float size, FontAwesomeIcon icon, float iconPx,
        Vector4 highlightBg, Vector4 highlightFg, bool alwaysHighlighted, string? tooltip)
    {
        ImGui.SetCursorScreenPos(min);
        var clicked = ImGui.InvisibleButton(id, new Vector2(size, size));
        var hovered = ImGui.IsItemHovered();
        var highlighted = alwaysHighlighted || hovered;

        var dl = ImGui.GetWindowDrawList();
        if (highlighted)
            dl.AddRectFilled(min, min + new Vector2(size, size), ImGui.GetColorU32(highlightBg), IconButtonRadius * (size / IconButtonSize));
        var fg = highlighted ? highlightFg : T.TextSecondary;

        using (UiFonts.PluginInterfaceIconFont.Push())
        {
            var nativePx = ImGui.GetFontSize();
            ImGui.SetWindowFontScale(iconPx / nativePx);
            var glyph = icon.ToIconString();
            var glyphSize = ImGui.CalcTextSize(glyph);
            dl.AddText(min + (new Vector2(size, size) - glyphSize) / 2f, ImGui.GetColorU32(fg), glyph);
            ImGui.SetWindowFontScale(1f);
        }

        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (!string.IsNullOrEmpty(tooltip))
                ImGui.SetTooltip(tooltip);
        }

        return clicked;
    }

    /// <summary>Hintergrund (BgSidebar) + Eckverzierungen der eingeklappten Mini-Leiste, über das GESAMTE
    /// aktuelle Fenster (dessen Größe der Aufrufer bereits auf CollapsedWidth/CollapsedHeight gesetzt hat).</summary>
    public static void DrawChrome(float scale, IUiOrnaments ornaments, float cornerInset, float windowRadius)
    {
        var winPos = ImGui.GetWindowPos();
        var winSize = ImGui.GetWindowSize();
        ImGui.GetWindowDrawList().AddRectFilled(winPos, winPos + winSize, ImGui.GetColorU32(T.BgSidebar), windowRadius * scale);
        UiWidgets.DrawWindowCorners(ornaments, cornerInset);
    }

    /// <summary>
    /// Freie Fläche der Mini-Leiste zwischen Logo und den Knöpfen rechts - Ziehen verschiebt das
    /// Fenster (direkt per ImGui.SetWindowPos auf das aktuell aktive Fenster, der Aufrufer muss also
    /// vor diesem Aufruf keine eigene BeginChild() geöffnet haben), ein Doppelklick gibt true zurück
    /// (der Aufrufer wertet das als "ausklappen"). "min"/"max" sind Bildschirmkoordinaten.
    /// </summary>
    public static bool DrawDragArea(string id, Vector2 min, Vector2 max)
    {
        var size = max - min;
        if (size.X <= 0f || size.Y <= 0f)
            return false;

        ImGui.SetCursorScreenPos(min);
        ImGui.InvisibleButton(id, size);
        var hovered = ImGui.IsItemHovered();
        var doubleClicked = hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);

        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            var delta = ImGui.GetIO().MouseDelta;
            if (delta != Vector2.Zero)
                ImGui.SetWindowPos(ImGui.GetWindowPos() + delta);
        }

        return doubleClicked;
    }
}
