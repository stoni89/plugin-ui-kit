using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace PluginUiKit;

/// <summary>
/// Grundbausteine - 1:1 aus TheExplorersCodex.CodexTheme portiert, nur <c>CodexTheme.X</c> durch
/// <c>UiTheme.Active.X</c> und feste px-Werte durch <see cref="UiMetrics"/> ersetzt. Jeder Aufruf
/// erwartet "scale" explizit (ImGuiHelpers.GlobalScale) statt ihn selbst zu lesen - identisch zum
/// bisherigen Verhalten, nur ohne stillen globalen Zugriff.
/// </summary>
public static class UiWidgets
{
    private static UiTheme T => UiTheme.Active;

    /// <summary>Text mit dunklem Schatten (für Text auf halbtransparentem Untergrund, z.B. Overlay-Titel).</summary>
    public static void TextShadowed(string text, Vector4 color, bool shadow)
    {
        if (!shadow)
        {
            ImGui.TextColored(color, text);
            return;
        }

        var pos = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        var shadowCol = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.8f));
        foreach (var offset in stackalloc Vector2[] { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) })
            drawList.AddText(pos + offset, shadowCol, text);
        drawList.AddText(pos, ImGui.GetColorU32(color), text);
        ImGui.Dummy(ImGui.CalcTextSize(text));
    }

    /// <summary>Wie <see cref="TextShadowed"/>, aber für Text, der bereits direkt per drawList.AddText an einer manuell berechneten Position gezeichnet wird.</summary>
    public static void DrawTextShadowed(ImDrawListPtr drawList, Vector2 pos, Vector4 color, string text, bool shadow)
    {
        if (shadow)
        {
            var shadowCol = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.8f));
            foreach (var offset in stackalloc Vector2[] { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) })
                drawList.AddText(pos + offset, shadowCol, text);
        }

        drawList.AddText(pos, ImGui.GetColorU32(color), text);
    }

    /// <summary>Text mit zusätzlichem Zeichenabstand (Letter-Spacing) - zeichnet am aktuellen Cursor, erwartet die gewünschte Schrift bereits gepusht.</summary>
    public static void DrawSpacedText(string text, Vector4 color, float spacing)
    {
        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        var height = ImGui.GetTextLineHeight();
        var x = pos.X;
        foreach (var ch in text)
        {
            var s = ch.ToString();
            drawList.AddText(new Vector2(x, pos.Y), ImGui.GetColorU32(color), s);
            x += ImGui.CalcTextSize(s).X + spacing;
        }

        ImGui.Dummy(new Vector2(MathF.Max(0f, x - pos.X - spacing), height));
    }

    /// <summary>Abschnitts-Label - Versalien, leicht angehoben per TextTertiary.</summary>
    public static void SectionLabel(string text, bool shadow = false)
    {
        using (UiFonts.SectionLabel.Push())
            TextShadowed(text.ToUpperInvariant(), T.TextTertiary, shadow);
    }

    /// <summary>Schalter, 42x22px (UiMetrics.ToggleWidth/Height) - selbst gezeichnet, per InvisibleButton klickbar.
    /// Gibt true zurück, wenn gerade umgeschaltet wurde; der Aufrufer liest den neuen Wert aus "value" (ref).</summary>
    public static bool Toggle(string id, ref bool value, float scale)
    {
        var size = new Vector2(UiMetrics.ToggleWidth * scale, UiMetrics.ToggleHeight * scale);
        var cursor = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton(id, size);
        var hovered = ImGui.IsItemHovered();

        var drawList = ImGui.GetWindowDrawList();
        var rounding = size.Y / 2f;
        var bg = value ? T.Accent : T.LineRow;
        drawList.AddRectFilled(cursor, cursor + size, ImGui.GetColorU32(bg), rounding);
        if (!value)
            drawList.AddRect(cursor, cursor + size, ImGui.GetColorU32(T.LineControl), rounding);

        if (hovered)
            drawList.AddRectFilled(cursor, cursor + size, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.08f)), rounding);

        var knobRadius = size.Y / 2f - 3f * scale;
        var knobX = value ? cursor.X + size.X - size.Y / 2f : cursor.X + size.Y / 2f;
        var knobColor = value ? T.TextOnAccent : T.TextMuted;
        drawList.AddCircleFilled(new Vector2(knobX, cursor.Y + size.Y / 2f), knobRadius, ImGui.GetColorU32(knobColor));

        if (clicked)
            value = !value;
        return clicked;
    }

    /// <summary>Einstellungszeile: Label links, Schalter rechtsbündig, optionaler, immer sichtbarer Erklärtext darunter.</summary>
    public static bool ToggleRow(string id, string label, ref bool value, float scale, string? caption = null)
    {
        var toggleHeight = UiMetrics.ToggleHeight * scale;
        var toggleWidth = UiMetrics.ToggleWidth * scale;

        var rowStartY = ImGui.GetCursorPosY();
        using (UiFonts.BodyMedium.Push())
            ImGui.TextColored(T.TextPrimary, label);

        if (!string.IsNullOrEmpty(caption))
        {
            var wrapX = ImGui.GetWindowContentRegionMax().X - toggleWidth - UiMetrics.DropdownRightMargin * scale - 5f * scale;
            using (UiFonts.Body.Push())
            {
                ImGui.PushTextWrapPos(wrapX);
                ImGui.TextColored(T.TextTertiary, caption);
                ImGui.PopTextWrapPos();
            }
        }

        var textBottomY = ImGui.GetCursorPosY();
        var textHeight = textBottomY - rowStartY;

        var toggleY = rowStartY + (textHeight - toggleHeight) / 2f;
        var rightX = ImGui.GetWindowContentRegionMax().X - toggleWidth - UiMetrics.DropdownRightMargin * scale;

        ImGui.SetCursorPos(new Vector2(rightX, toggleY));
        var changed = Toggle(id, ref value, scale);

        ImGui.SetCursorPosY(MathF.Max(textBottomY, toggleY + toggleHeight));
        return changed;
    }

    // ---- Card.Begin/End ----
    // Nicht gestapelt (statische Felder statt eines Stacks) - wie im Vorbild bewusst so (siehe
    // TheExplorersCodex.CodexTheme.BeginCard-Kommentar): sicher bei sequenzieller Verwendung,
    // NICHT bei verschachtelten BeginCard-Aufrufen.
    private static Vector2 cardOrigin;
    private static float cardWidth;
    private static float cardPaddingX;
    private static float cardPaddingY;
    private static bool cardAccentBar;

    /// <summary>Karte mit inhaltsabhängiger Höhe - BgCard-Hintergrund, LineCard-Rahmen, Innenabstand (UiMetrics.CardPadX/Y). Mit EndCard() abschließen.
    /// "width" überschreibt die Breite explizit (z.B. für zwei Karten nebeneinander ohne ImGui.BeginTable - ImGui.GetContentRegionAvail() kennt eine
    /// solche manuell aufgeteilte Spaltenbreite sonst nicht von selbst, siehe TheExplorersCodex-Vorfall mit Tabellen-in-Karte-Bugs).</summary>
    public static void BeginCard(float scale, bool accentBar = false, float rightMargin = 0f, float? width = null)
    {
        cardPaddingX = UiMetrics.CardPadX * scale;
        cardPaddingY = UiMetrics.CardPadY * scale;
        cardOrigin = ImGui.GetCursorScreenPos();
        cardWidth = width ?? (ImGui.GetContentRegionAvail().X - rightMargin);
        cardAccentBar = accentBar;

        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);

        ImGui.Indent(cardPaddingX);
        ImGui.Dummy(new Vector2(0f, cardPaddingY));
    }

    public static void EndCard()
    {
        ImGui.Dummy(new Vector2(0f, cardPaddingY));
        ImGui.Unindent(cardPaddingX);

        var min = cardOrigin;
        var max = new Vector2(cardOrigin.X + cardWidth, ImGui.GetCursorScreenPos().Y);

        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSetCurrent(0);
        drawList.AddRectFilled(min, max, ImGui.GetColorU32(T.BgCard), UiMetrics.CardRadius);
        drawList.AddRect(min, max, ImGui.GetColorU32(T.LineCard), UiMetrics.CardRadius);
        if (cardAccentBar)
            drawList.AddRectFilled(min, new Vector2(min.X + 2f, max.Y), ImGui.GetColorU32(T.Accent));
        drawList.ChannelsMerge();
    }

    /// <summary>Innerer horizontaler Innenabstand der aktuell offenen Karte (siehe BeginCard).</summary>
    public static float CardPaddingX => cardPaddingX;

    /// <summary>Innerer vertikaler Innenabstand der aktuell offenen Karte (siehe BeginCard).</summary>
    public static float CardPaddingY => cardPaddingY;

    /// <summary>Kartentitel - Versalien, darunter eine Trennlinie. "lineWidth" begrenzt die Linie optional (sonst volle Kartenbreite).</summary>
    public static void CardGroupLabel(string text, float scale, float? lineWidth = null)
    {
        using (UiFonts.CardTitle.Push())
            TextShadowed(text.ToUpperInvariant(), T.TextTertiary, false);
        ImGui.Dummy(new Vector2(0f, 8f * scale));

        var cursor = ImGui.GetCursorScreenPos();
        var width = lineWidth ?? ImGui.GetContentRegionAvail().X;
        ImGui.GetWindowDrawList().AddLine(cursor, cursor + new Vector2(width, 0f), ImGui.GetColorU32(T.LineSubtle), 1f);

        ImGui.Dummy(new Vector2(0f, 10f * scale));
    }

    /// <summary>Für CardGroupLabel(lineWidth:) - damit die Trennlinie exakt dort endet, wo ToggleRow/BeginDropdownRow ihr Steuerelement enden lassen.</summary>
    public static float CardFieldLineWidth(float scale, float rightMargin = UiMetrics.DropdownRightMargin) =>
        ImGui.GetContentRegionAvail().X - rightMargin * scale;

    /// <summary>Feine Trennlinie innerhalb einer Karte, mit etwas vertikalem Abstand.</summary>
    public static void CardDivider(float scale)
    {
        ImGui.Dummy(new Vector2(0f, 8f * scale));
        ImGui.PushStyleColor(ImGuiCol.Separator, T.LineSubtle);
        ImGui.Separator();
        ImGui.PopStyleColor();
        ImGui.Dummy(new Vector2(0f, 8f * scale));
    }

    private static IDisposable? dropdownValueFontPop;
    private static float dropdownRowTextBottomY;
    private static float dropdownRowComboY;
    private static float dropdownRowComboHeight;

    /// <summary>Zeilenlayout für eine Menü-Combo: Label links, Combo rechtsbündig mit festem Randabstand. Bei geöffnetem Dropdown gibt es etwas mehr
    /// Innenabstand für die Einträge (siehe EndDropdownRow). Muss IMMER mit EndDropdownRow() abgeschlossen werden, unabhängig vom Rückgabewert.</summary>
    public static bool BeginDropdownRow(string label, string currentValueLabel, string comboId, float scale,
        string? tooltip = null, string? caption = null, bool disabled = false, float width = 205f, float rightMargin = UiMetrics.DropdownRightMargin)
    {
        var comboWidth = width * scale;

        var rowStartY = ImGui.GetCursorPosY();
        using (UiFonts.BodyMedium.Push())
            ImGui.TextColored(disabled ? T.TextDisabled : T.TextPrimary, label);
        if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered(disabled ? ImGuiHoveredFlags.AllowWhenDisabled : ImGuiHoveredFlags.None))
            ImGui.SetTooltip(tooltip);

        if (!string.IsNullOrEmpty(caption))
        {
            var wrapX = ImGui.GetWindowContentRegionMax().X - comboWidth - rightMargin * scale - 5f * scale;
            using (UiFonts.Body.Push())
            {
                ImGui.PushTextWrapPos(wrapX);
                ImGui.TextColored(T.TextTertiary, caption);
                ImGui.PopTextWrapPos();
            }
        }

        var textBottomY = ImGui.GetCursorPosY();
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10f * scale, 9f * scale));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f * scale);
        ImGui.PushStyleColor(ImGuiCol.Border, T.LineCard);
        dropdownValueFontPop = UiFonts.BodyMedium.Push();

        var comboHeight = ImGui.GetFrameHeight();
        var comboY = rowStartY + (textBottomY - rowStartY - comboHeight) / 2f;
        dropdownRowTextBottomY = textBottomY;
        dropdownRowComboY = comboY;
        dropdownRowComboHeight = comboHeight;

        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - comboWidth - rightMargin * scale, comboY));
        ImGui.SetNextItemWidth(comboWidth);

        if (disabled)
            ImGui.BeginDisabled();

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0f, 0f, 0f, 0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0f, 0f, 0f, 0f));
        var open = ImGui.BeginCombo(comboId, currentValueLabel);
        ImGui.PopStyleColor(2);

        if (open)
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5f * scale, 7f * scale));
        return open;
    }

    /// <summary>Gegenstück zu BeginDropdownRow - IMMER aufrufen, auch wenn das Dropdown gerade nicht offen ist.</summary>
    public static void EndDropdownRow(bool wasOpen, bool disabled)
    {
        if (wasOpen)
        {
            ImGui.PopStyleVar();
            ImGui.EndCombo();
        }

        if (disabled)
            ImGui.EndDisabled();

        dropdownValueFontPop?.Dispose();
        dropdownValueFontPop = null;
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);
        ImGui.SetCursorPosY(MathF.Max(dropdownRowTextBottomY, dropdownRowComboY + dropdownRowComboHeight));
    }

    // ---- Verzierungen (orchestrieren IUiOrnaments) ----

    /// <summary>Zeichnet alle vier Eck-Verzierungen über das aktuelle Fensterrechteck - ruft <paramref name="ornaments"/>.DrawCorner einmal je Ecke auf.</summary>
    public static void DrawWindowCorners(IUiOrnaments ornaments, float length, float inset)
    {
        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();

        ornaments.DrawCorner(min + new Vector2(inset, inset), 1, 1);
        ornaments.DrawCorner(new Vector2(max.X - inset, min.Y + inset), -1, 1);
        ornaments.DrawCorner(new Vector2(min.X + inset, max.Y - inset), 1, -1);
        ornaments.DrawCorner(max - new Vector2(inset, inset), -1, -1);
    }

    /// <summary>Horizontales Trenn-Ornament, mittig unter dem aktuellen Cursor über <see cref="UiMetrics.DividerOrnamentWidth"/> gezeichnet (Standard-Layout-Vorgabe) - ruft <paramref name="ornaments"/>.DrawDivider auf.</summary>
    public static void DrawDividerOrnament(IUiOrnaments ornaments, float scale)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var width = UiMetrics.DividerOrnamentWidth * scale;
        ornaments.DrawDivider(new Vector2(cursor.X, cursor.Y + 6f * scale), width);
        ImGui.Dummy(new Vector2(width, 12f * scale));
    }
}
