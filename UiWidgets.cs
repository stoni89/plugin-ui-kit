using System;
using System.Numerics;
using Dalamud.Interface.ManagedFontAtlas;
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
        var bg = value ? T.Accent : T.BgToggleOff;
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

    /// <summary>Einstellungszeile: Label links, Schalter rechtsbündig, optionaler, immer sichtbarer Erklärtext darunter.
    /// "labelFont"/"captionFont" überschreiben optional die sonst genutzten UiFonts.BodyMedium/Body - für Plugins (wie TheExplorersCodex), die eigene,
    /// abweichende Schriftgrößen für diese Rolle pflegen, statt des generischen Kit-Defaults.</summary>
    public static bool ToggleRow(string id, string label, ref bool value, float scale, string? caption = null, IFontHandle? labelFont = null, IFontHandle? captionFont = null)
    {
        var toggleHeight = UiMetrics.ToggleHeight * scale;
        var toggleWidth = UiMetrics.ToggleWidth * scale;

        var rowStartY = ImGui.GetCursorPosY();
        using (var _ = (labelFont ?? UiFonts.BodyMedium).Push())
            ImGui.TextColored(T.TextPrimary, label);

        if (!string.IsNullOrEmpty(caption))
        {
            var wrapX = ImGui.GetWindowContentRegionMax().X - toggleWidth - UiMetrics.DropdownRightMargin * scale - 5f * scale;
            using (var _ = (captionFont ?? UiFonts.Body).Push())
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

    /// <summary>Kartentitel - Versalien, darunter eine Trennlinie. "lineWidth" begrenzt die Linie optional (sonst volle Kartenbreite).
    /// "font" überschreibt optional UiFonts.CardTitle (siehe ToggleRow-Kommentar).</summary>
    public static void CardGroupLabel(string text, float scale, float? lineWidth = null, IFontHandle? font = null)
    {
        using (var _ = (font ?? UiFonts.CardTitle).Push())
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
    /// Innenabstand für die Einträge (siehe EndDropdownRow). Muss IMMER mit EndDropdownRow() abgeschlossen werden, unabhängig vom Rückgabewert.
    /// "labelFont"/"captionFont"/"valueFont" überschreiben optional die sonst genutzten UiFonts.BodyMedium/Body/BodyMedium (siehe ToggleRow-Kommentar).</summary>
    public static bool BeginDropdownRow(string label, string currentValueLabel, string comboId, float scale,
        string? tooltip = null, string? caption = null, bool disabled = false, float width = 205f, float rightMargin = UiMetrics.DropdownRightMargin,
        IFontHandle? labelFont = null, IFontHandle? captionFont = null, IFontHandle? valueFont = null)
    {
        var comboWidth = width * scale;

        var rowStartY = ImGui.GetCursorPosY();
        using (var _ = (labelFont ?? UiFonts.BodyMedium).Push())
            ImGui.TextColored(disabled ? T.TextDisabled : T.TextPrimary, label);
        if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered(disabled ? ImGuiHoveredFlags.AllowWhenDisabled : ImGuiHoveredFlags.None))
            ImGui.SetTooltip(tooltip);

        if (!string.IsNullOrEmpty(caption))
        {
            var wrapX = ImGui.GetWindowContentRegionMax().X - comboWidth - rightMargin * scale - 5f * scale;
            using (var _ = (captionFont ?? UiFonts.Body).Push())
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
        dropdownValueFontPop = (valueFont ?? UiFonts.BodyMedium).Push();

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

    /// <summary>Von BeginSettingRow zurückgegeben - "ControlPos" (per ImGui.SetCursorPos zu setzen) und "ControlWidth"/"ControlHeight"
    /// sind exakt die beim Aufruf übergebenen Werte, zur bequemen Weiterverwendung beim Zeichnen des Steuerelements.</summary>
    public readonly struct SettingRowHandle
    {
        public Vector2 ControlPos { get; init; }
        public float ControlWidth { get; init; }
        public float ControlHeight { get; init; }
        internal float TextBottomY { get; init; }
        internal float RowBottomY { get; init; }
        internal bool TrailingPadding { get; init; }
    }

    /// <summary>Generische Einstellungszeile: optionale obere Trennlinie (LineRow), Titel + optionale, umbrechende Beschreibung (TextDesc) links,
    /// Platz für EIN rechtsbündiges Steuerelement beliebiger Art (Toggle/Combo/Slider/.../Badge) - der Aufrufer zeichnet das Steuerelement selbst
    /// an der zurückgegebenen Position (ImGui.SetCursorPos(handle.ControlPos)) und zeigt bei Bedarf die Beschreibung als Tooltip darauf.
    /// Mindesthöhe über "minHeight" (Basis-px, *scale). "drawDivider"=false lässt die eigene obere Trennlinie weg - für Listen, die stattdessen
    /// EINE Trennlinie ZWISCHEN Zeilen über UiWidgets.CardDivider ziehen (siehe dessen Aufrufer) - sonst verdoppelt sich der Zeilenabstand.
    /// Immer mit EndSettingRow() abschließen.</summary>
    public static SettingRowHandle BeginSettingRow(string title, string? description, float scale, float controlWidth, float controlHeight,
        float minHeight = 52f, float rightMargin = UiMetrics.DropdownRightMargin, IFontHandle? titleFont = null, IFontHandle? descFont = null, bool drawDivider = true)
    {
        if (drawDivider)
        {
            ImGui.Dummy(new Vector2(0f, 8f * scale));
            var lineCursor = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(lineCursor, lineCursor + new Vector2(ImGui.GetContentRegionAvail().X, 0f), ImGui.GetColorU32(T.LineRow), 1f * scale);
            ImGui.Dummy(new Vector2(0f, 8f * scale));
        }

        var rowStartY = ImGui.GetCursorPosY();
        using (var _ = (titleFont ?? UiFonts.BodyMedium).Push())
            ImGui.TextColored(T.TextPrimary, title);

        if (!string.IsNullOrEmpty(description))
        {
            var wrapX = ImGui.GetWindowContentRegionMax().X - controlWidth - rightMargin * scale - 16f * scale;
            using (var _ = (descFont ?? UiFonts.Body).Push())
            {
                ImGui.PushTextWrapPos(wrapX);
                ImGui.TextColored(T.TextDesc, description);
                ImGui.PopTextWrapPos();
            }
        }

        var textBottomY = ImGui.GetCursorPosY();
        var textHeight = textBottomY - rowStartY;
        var rowHeight = MathF.Max(minHeight * scale, textHeight);

        var controlY = rowStartY + (rowHeight - controlHeight) / 2f;
        var rightX = ImGui.GetWindowContentRegionMax().X - controlWidth - rightMargin * scale;

        return new SettingRowHandle
        {
            ControlPos = new Vector2(rightX, controlY),
            ControlWidth = controlWidth,
            ControlHeight = controlHeight,
            TextBottomY = textBottomY,
            RowBottomY = rowStartY + rowHeight,
            TrailingPadding = drawDivider,
        };
    }

    /// <summary>Gegenstück zu BeginSettingRow - immer aufrufen, nachdem das Steuerelement gezeichnet wurde.</summary>
    public static void EndSettingRow(SettingRowHandle handle, float scale)
    {
        ImGui.SetCursorPosY(MathF.Max(handle.TextBottomY, handle.RowBottomY));
        if (handle.TrailingPadding)
            ImGui.Dummy(new Vector2(0f, 8f * scale));
    }

    /// <summary>Pusht einen einheitlichen Eingabefeld-Look (BgInput-Füllung, LineFrame-Rand, UiMetrics.ControlRadius) für ein direkt folgendes
    /// ImGui-Steuerelement (Combo/SliderInt/...), dessen Darstellung dieses Paket nicht selbst übernimmt. Immer mit PopInputStyle() abschließen.</summary>
    public static void PushInputStyle(float scale)
    {
        ImGui.PushStyleColor(ImGuiCol.FrameBg, T.BgInput);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, T.BgInput);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, T.BgInput);
        ImGui.PushStyleColor(ImGuiCol.Border, T.LineFrame);
        ImGui.PushStyleColor(ImGuiCol.Text, T.TextPrimary);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, UiMetrics.ControlRadius * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f * scale);
    }

    /// <summary>Gegenstück zu PushInputStyle().</summary>
    public static void PopInputStyle()
    {
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(5);
    }

    // ---- Verzierungen (orchestrieren IUiOrnaments) ----

    /// <summary>Zeichnet alle vier Eck-Verzierungen über das aktuelle Fensterrechteck - ruft <paramref name="ornaments"/>.DrawCorner einmal je Ecke auf
    /// (die Strichlänge entscheidet die jeweilige IUiOrnaments-Implementierung selbst, siehe deren Konstruktor/Felder - nicht Teil dieser Signatur,
    /// da das generische IUiOrnaments.DrawCorner(corner, sx, sy) keinen Längenparameter hat).</summary>
    public static void DrawWindowCorners(IUiOrnaments ornaments, float inset)
    {
        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();

        // Eigener Clip-Rect übers volle Fenster (nicht den evtl. engeren Content-Clip, der nach
        // verschachtelten BeginChild/EndChild-Aufrufen theoretisch noch aktiv sein könnte) - sonst
        // würden Ecken-Verzierungen, die bis an den Fensterrand reichen, abgeschnitten.
        var drawList = ImGui.GetWindowDrawList();
        drawList.PushClipRect(min, max, false);

        ornaments.DrawCorner(min + new Vector2(inset, inset), 1, 1);
        ornaments.DrawCorner(new Vector2(max.X - inset, min.Y + inset), -1, 1);
        ornaments.DrawCorner(new Vector2(min.X + inset, max.Y - inset), 1, -1);
        ornaments.DrawCorner(max - new Vector2(inset, inset), -1, -1);

        drawList.PopClipRect();
    }

    /// <summary>Horizontales Trenn-Ornament, mittig unter dem aktuellen Cursor über <see cref="UiMetrics.DividerOrnamentWidth"/> gezeichnet (Standard-Layout-Vorgabe) - ruft <paramref name="ornaments"/>.DrawDivider auf.</summary>
    public static void DrawDividerOrnament(IUiOrnaments ornaments, float scale)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var width = UiMetrics.DividerOrnamentWidth * scale;
        ornaments.DrawDivider(new Vector2(cursor.X, cursor.Y + 6f * scale), width);
        ImGui.Dummy(new Vector2(width, 12f * scale));
    }

    /// <summary>Gestrichelte Linie (kurze Segmente mit Lücken) - für Trennlinien, die sich optisch von einer durchgezogenen CardDivider/Zeilenlinie
    /// abheben sollen (z.B. eine "nur für Entwickler sichtbare Zusatzspalte"-Markierung).</summary>
    public static void DrawDashedLine(Vector2 from, Vector2 to, uint color, float thickness, float dashLength, float gapLength)
    {
        var dl = ImGui.GetWindowDrawList();
        var delta = to - from;
        var length = delta.Length();
        if (length <= 0f)
            return;
        var dir = delta / length;
        var step = dashLength + gapLength;

        for (var pos = 0f; pos < length; pos += step)
        {
            var segStart = from + dir * pos;
            var segEnd = from + dir * MathF.Min(pos + dashLength, length);
            dl.AddLine(segStart, segEnd, color, thickness);
        }
    }

    /// <summary>Volle Kopfzeile einer Datentabelle (Versalien, Spacing, TextSecondary, Trennlinie LineCard darunter) - pro Spalte einmal INNERHALB
    /// einer ImGui.TableNextRow(Headers, headerHeight) aufrufen (nach ImGui.TableSetColumnIndex(column)), die Trennlinie zieht der Aufrufer selbst
    /// (siehe DrawDataTableHeaderDivider) - getrennt, da die Kopfzeile selbst je nach Tabelle in einer Karte oder einem Kindfenster sitzen kann.</summary>
    public static void DrawDataTableHeaderCell(string label, float headerHeight, float leftPad, IFontHandle font, bool rightAlign = false, float rightPad = 8f)
    {
        using (font.Push())
        {
            var text = label.ToUpperInvariant();
            var textHeight = ImGui.GetTextLineHeight();
            if (rightAlign)
            {
                var textWidth = ImGui.CalcTextSize(text).X;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - textWidth - rightPad);
            }
            else
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + leftPad);
            }
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (headerHeight - textHeight) / 2f);
            DrawSpacedText(text, T.TextSecondary, 1f);
        }
    }

    /// <summary>Trennlinie unter einer Datentabellen-Kopfzeile, über die volle angegebene Breite - direkt nach der Kopfzeile (vor der ersten Datenzeile) aufrufen.</summary>
    public static void DrawDataTableHeaderDivider(float width)
    {
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddLine(cursor, cursor + new Vector2(width, 0f), ImGui.GetColorU32(T.LineCard));
    }

    /// <summary>Hervorgehobene Tabellenzeile (z.B. "nächster"/"aktiver" Eintrag einer DataTable) - "bg" als
    /// Hintergrund über die gesamte Zeile plus ein "barWidth" breiter Akzent-Balken ganz links, beide exakt
    /// bündig mit der ECHTEN Zeilenhöhe/dem inneren linken Kartenrand - NICHT per GetCursorScreenPos()/
    /// GetItemRectMin() innerhalb einer Zelle berechnet (das wäre um ImGui.GetStyle().CellPadding verschoben,
    /// siehe Aufrufer-Kommentar) und NICHT vom Clip-Rect der ersten Spalte abgeschnitten (eigenes
    /// PushClipRect/PopClipRect, das diese Spalten-Grenze bewusst überschreibt). "rowTopScreen" ist der
    /// Bildschirm-Y direkt nach ImGui.TableNextRow (VOR dem ersten TableSetColumnIndex!), "rowHeight"
    /// dieselbe min_row_height, die auch an TableNextRow übergeben wurde, "tableInnerLeft"/"tableInnerRight"
    /// die feste Breite der Tabelle/Karte (einmal vor BeginTable gemerkt, nicht pro Zeile neu ermittelt).</summary>
    public static void DrawHighlightedRow(float rowTopScreen, float rowHeight, float tableInnerLeft, float tableInnerRight, Vector4 bg, Vector4 barColor, float barWidth)
    {
        var dl = ImGui.GetWindowDrawList();
        var min = new Vector2(tableInnerLeft, rowTopScreen);
        var max = new Vector2(tableInnerRight, rowTopScreen + rowHeight);
        dl.PushClipRect(min, max, false);
        dl.AddRectFilled(min, max, ImGui.GetColorU32(bg));
        dl.AddRectFilled(min, new Vector2(min.X + barWidth, max.Y), ImGui.GetColorU32(barColor));
        dl.PopClipRect();
    }

    /// <summary>Klickbarer Text ohne Knopf-Hintergrund - Hand-Cursor + Unterstrich bei Hover (z.B. "Fish Data" in einem Hinweistext, "Undo" in einem
    /// Toast). Zeichnet an der aktuellen Cursorposition, erwartet die gewünschte Schrift bereits gepusht. Gibt true zurück, wenn gerade geklickt wurde.</summary>
    public static bool LinkText(string text, Vector4 color)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(text);
        var clicked = ImGui.InvisibleButton("##UiLinkText_" + text, size);
        var hovered = ImGui.IsItemHovered();

        var dl = ImGui.GetWindowDrawList();
        dl.AddText(cursor, ImGui.GetColorU32(color), text);
        if (hovered)
        {
            dl.AddLine(new Vector2(cursor.X, cursor.Y + size.Y), new Vector2(cursor.X + size.X, cursor.Y + size.Y), ImGui.GetColorU32(color));
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return clicked;
    }
}
