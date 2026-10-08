using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace PluginUiKit;

/// <summary>
/// Zusammengesetzte Widgets aus mehreren von Hand gezeichneten Teilen - 1:1 aus
/// TheExplorersCodex.CodexWidgets portiert (siehe UiWidgets.cs für die Low-Level-Bausteine).
/// </summary>
public static class UiWidgetsExtra
{
    private static UiTheme T => UiTheme.Active;

    /// <summary>Rückt den Cursor so weit nach rechts, dass ein als Nächstes gezeichnetes Element mit der Breite "itemWidth" horizontal zentriert ist.
    /// Ohne "availableWidth" zentriert sich das Element zum aktuell verfügbaren Bereich, mit explizitem Wert gezielt gegen eine kleinere, lokale Breite.</summary>
    public static void CenterNext(float itemWidth, float? availableWidth = null)
    {
        var avail = availableWidth ?? ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (avail - itemWidth) / 2f));
    }

    /// <summary>Um 45° gedrehtes Quadrat (Raute) - z.B. Zeitleisten-Marker, Hintergrund für ein Symbol.</summary>
    public static void Diamond(Vector2 center, float size, uint fill, uint border)
    {
        var dl = ImGui.GetWindowDrawList();
        var h = size * 0.7071f;
        var top = center + new Vector2(0f, -h);
        var right = center + new Vector2(h, 0f);
        var bottom = center + new Vector2(0f, h);
        var left = center + new Vector2(-h, 0f);
        dl.AddQuadFilled(top, right, bottom, left, fill);
        dl.AddQuad(top, right, bottom, left, border, 1f * ImGuiHelpers.GlobalScale);
    }

    /// <summary>Fließtext mit zentrierten Zeilen - eigener Umbruch samt Zentrierung pro Zeile (ImGui.TextWrapped richtet immer linksbündig aus).
    /// Erwartet die gewünschte Schrift bereits gepusht. "centerLocalX" ist die window-lokale X-Position, auf die der Cursor vor jeder Zeile zurückgesetzt wird.</summary>
    public static void CenteredWrappedText(string text, float maxWidth, uint color, float? centerLocalX = null, float? availableWidth = null)
    {
        var lines = new List<string>();
        var line = string.Empty;
        foreach (var word in text.Split(' '))
        {
            var test = line.Length == 0 ? word : line + " " + word;
            if (ImGui.CalcTextSize(test).X > maxWidth && line.Length > 0)
            {
                lines.Add(line);
                line = word;
            }
            else
            {
                line = test;
            }
        }
        if (line.Length > 0)
            lines.Add(line);

        ImGui.PushStyleColor(ImGuiCol.Text, color);
        foreach (var l in lines)
        {
            if (centerLocalX is { } x)
                ImGui.SetCursorPosX(x);
            CenterNext(ImGui.CalcTextSize(l).X, availableWidth);
            ImGui.TextUnformatted(l);
        }
        ImGui.PopStyleColor();
    }

    /// <summary>Trennlinie mit mittiger Beschriftung. Erwartet die gewünschte Schrift bereits gepusht. Mit "availableWidth" = "width" übergeben
    /// startet die Linie exakt am aktuellen Cursor ohne erneute Selbst-Zentrierung (wichtig, wenn der Aufrufer bereits am gewünschten linken Rand einer schmaleren Box steht).</summary>
    public static void LabeledDivider(string label, float width, float? availableWidth = null)
    {
        var s = ImGuiHelpers.GlobalScale;
        CenterNext(width, availableWidth);
        var p = ImGui.GetCursorScreenPos();
        var ts = ImGui.CalcTextSize(label);
        var gap = 12f * s;
        var y = p.Y + ts.Y / 2f;
        var textX = p.X + (width - ts.X) / 2f;
        var dl = ImGui.GetWindowDrawList();
        var line = ImGui.GetColorU32(T.LineCard);
        dl.AddLine(new Vector2(p.X, y), new Vector2(textX - gap, y), line, 1f * s);
        dl.AddLine(new Vector2(textX + ts.X + gap, y), new Vector2(p.X + width, y), line, 1f * s);
        dl.AddText(new Vector2(textX, p.Y), ImGui.GetColorU32(T.TextTertiary), label);
        ImGui.Dummy(new Vector2(width, ts.Y));
    }

    /// <summary>Hinweis-Karte mit Tastenkürzel (z.B. "Ctrl" + "Shift" + "Click" ... Text). "maxWidth" begrenzt die Kartenbreite optional (sonst volle verfügbare Breite).
    /// "keyFont"/"textFont" überschreiben optional die sonst genutzte UiFonts.Body (siehe UiWidgets.ToggleRow-Kommentar zum selben Muster).</summary>
    public static void ShortcutHint(IReadOnlyList<string> keys, string text, float? maxWidth = null, IFontHandle? keyFont = null, IFontHandle? textFont = null)
    {
        var s = ImGuiHelpers.GlobalScale;
        var kf = keyFont ?? UiFonts.Body;
        var tf = textFont ?? UiFonts.Body;
        var dl = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var width = MathF.Min(ImGui.GetContentRegionAvail().X, maxWidth ?? float.MaxValue);

        float padX = 16f * s, padY = 12f * s;
        float keyPadX = 7f * s, keyPadY = 2f * s;
        float plusGap = 5f * s, textGap = 14f * s;

        float keyTextH, bodyH;
        using (var _ = kf.Push())
            keyTextH = ImGui.GetTextLineHeight();
        using (var _ = tf.Push())
            bodyH = ImGui.GetTextLineHeight();
        var keyH = keyTextH + keyPadY * 2f + 1f * s;
        var height = MathF.Max(keyH, bodyH) + padY * 2f;
        var midY = origin.Y + height / 2f;

        var cardMax = origin + new Vector2(width, height);
        dl.AddRectFilled(origin, cardMax, ImGui.GetColorU32(T.BgPopup), UiMetrics.CardRadius * s);
        dl.AddRect(origin, cardMax, ImGui.GetColorU32(T.LineCard), UiMetrics.CardRadius * s, ImDrawFlags.None, 1f * s);

        var x = origin.X + padX;

        using (var _ = kf.Push())
        {
            for (var i = 0; i < keys.Count; i++)
            {
                var tw = ImGui.CalcTextSize(keys[i]).X;
                var kMin = new Vector2(x, midY - keyH / 2f);
                var kMax = new Vector2(x + tw + keyPadX * 2f, midY + keyH / 2f);

                dl.AddRectFilled(kMin, kMax, ImGui.GetColorU32(T.BgInput), UiMetrics.ControlRadius * s);
                dl.AddRect(kMin, kMax, ImGui.GetColorU32(T.LineFrame), UiMetrics.ControlRadius * s, ImDrawFlags.None, 1f * s);
                dl.AddLine(new Vector2(kMin.X + 3f * s, kMax.Y - 1f * s), new Vector2(kMax.X - 3f * s, kMax.Y - 1f * s), ImGui.GetColorU32(T.LineFrame), 2f * s);
                dl.AddText(new Vector2(kMin.X + keyPadX, kMin.Y + keyPadY), ImGui.GetColorU32(T.TextHeading), keys[i]);

                x = kMax.X;
                if (i < keys.Count - 1)
                {
                    x += plusGap;
                    dl.AddText(new Vector2(x, midY - keyTextH / 2f), ImGui.GetColorU32(T.TextDim), "+");
                    x += ImGui.CalcTextSize("+").X + plusGap;
                }
            }
        }

        x += textGap;
        using (var _ = tf.Push())
            dl.AddText(new Vector2(x, midY - bodyH / 2f), ImGui.GetColorU32(T.TextSecondary), text);

        ImGui.Dummy(new Vector2(width, height));
    }

    /// <summary>Gefüllter Fortschrittsbalken - abgerundete Enden, Füllung bei fraction &gt; 0 mindestens 4px breit (sonst bei sehr kleinen Anteilen praktisch unsichtbar).
    /// "width" überschreibt optional die sonst über ImGui.GetContentRegionAvail() ermittelte Breite.</summary>
    public static void ProgressBar(float fraction, float height, string? tooltip = null, float? width = null, Vector4? fillColor = null, Vector4? trackColor = null)
    {
        var s = ImGuiHelpers.GlobalScale;
        height *= s;
        var dl = ImGui.GetWindowDrawList();
        var p = ImGui.GetCursorScreenPos();
        var w = width ?? ImGui.GetContentRegionAvail().X;
        var r = height / 2f;

        dl.AddRectFilled(p, p + new Vector2(w, height), ImGui.GetColorU32(trackColor ?? T.BgSelected), r);
        if (fraction > 0f)
        {
            var fw = MathF.Max(w * Math.Clamp(fraction, 0f, 1f), 4f * s);
            dl.AddRectFilled(p, p + new Vector2(fw, height), ImGui.GetColorU32(fillColor ?? T.Accent), r);
        }

        ImGui.Dummy(new Vector2(w, height));
        if (tooltip != null && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }

    /// <summary>Fließende Knopfreihe mit Umbruch - ImGui bricht SameLine-Ketten nicht von selbst um, daher hier manuell: ein Knopf, der nicht mehr in die
    /// aktuelle Zeile passt, beginnt eine neue. "isConfirmed" markiert einen Knopf optional als "gerade bestätigt" (z.B. 1,5s nach einem Klick) - zeigt dann
    /// ein Häkchen in OkFg statt des "icon"-Symbols. "labelFont" überschreibt optional UiFonts.BodyMedium (siehe UiWidgets.ToggleRow-Kommentar zum selben Muster).</summary>
    public static void FlowButtons(IReadOnlyList<(string Label, Action OnClick)> items, float gap, Func<string, bool>? isConfirmed = null, FontAwesomeIcon icon = FontAwesomeIcon.Terminal, IFontHandle? labelFont = null)
    {
        var s = ImGuiHelpers.GlobalScale;
        var lf = labelFont ?? UiFonts.BodyMedium;
        gap *= s;
        var rightEdge = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(gap, gap));
        for (var i = 0; i < items.Count; i++)
        {
            var (label, onClick) = items[i];
            var confirmed = isConfirmed?.Invoke(label) ?? false;
            var size = MeasureFlowButton(label, s, icon, lf);

            if (i > 0)
            {
                ImGui.SameLine();
                if (ImGui.GetCursorScreenPos().X + size.X > rightEdge)
                    ImGui.NewLine();
            }

            if (DrawFlowButton(label, confirmed, size, s, icon, lf))
                onClick();
        }
        ImGui.PopStyleVar();
    }

    private static Vector2 MeasureFlowButton(string label, float scale, FontAwesomeIcon icon, IFontHandle labelFont)
    {
        var padding = new Vector2(11f * scale, 5f * scale);
        var iconGap = 6f * scale;

        float iconWidth;
        using (UiFonts.PluginInterfaceIconFont.Push())
            iconWidth = ImGui.CalcTextSize(icon.ToIconString()).X;
        float textWidth, textHeight;
        using (var _ = labelFont.Push())
        {
            var size = ImGui.CalcTextSize(label);
            textWidth = size.X;
            textHeight = size.Y;
        }

        return new Vector2(iconWidth + iconGap + textWidth + padding.X * 2f, textHeight + padding.Y * 2f);
    }

    private static bool DrawFlowButton(string label, bool confirmed, Vector2 buttonSize, float scale, FontAwesomeIcon icon, IFontHandle labelFont)
    {
        var padding = new Vector2(11f * scale, 5f * scale);
        var iconGap = 6f * scale;
        var drawIcon = confirmed ? FontAwesomeIcon.Check : icon;
        var iconColor = confirmed ? T.OkFg : T.TextMuted;

        var cursor = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton($"##UiFlowButton{label}", buttonSize);
        var hovered = ImGui.IsItemHovered();

        var dl = ImGui.GetWindowDrawList();
        var bg = hovered ? T.BgSelected : T.BgPopup;
        var border = hovered ? T.Accent : T.LineControl;
        dl.AddRectFilled(cursor, cursor + buttonSize, ImGui.GetColorU32(bg), UiMetrics.ControlRadius * scale);
        dl.AddRect(cursor, cursor + buttonSize, ImGui.GetColorU32(border), UiMetrics.ControlRadius * scale);

        float iconWidth, iconHeight;
        using (UiFonts.PluginInterfaceIconFont.Push())
        {
            var iconSize = ImGui.CalcTextSize(drawIcon.ToIconString());
            iconWidth = iconSize.X;
            iconHeight = iconSize.Y;
        }
        float textHeight;
        using (var _ = labelFont.Push())
            textHeight = ImGui.CalcTextSize(label).Y;

        var iconCursor = cursor + new Vector2(padding.X, (buttonSize.Y - iconHeight) / 2f);
        using (UiFonts.PluginInterfaceIconFont.Push())
            dl.AddText(iconCursor, ImGui.GetColorU32(iconColor), drawIcon.ToIconString());
        var textCursor = new Vector2(cursor.X + padding.X + iconWidth + iconGap, cursor.Y + (buttonSize.Y - textHeight) / 2f);
        using (var _ = labelFont.Push())
            dl.AddText(textCursor, ImGui.GetColorU32(T.TextHeading), label);

        if (hovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        return clicked;
    }

    /// <summary>Kleines Badge/Typ-Etikett - gefüllter, umrandeter, abgerundeter Hintergrund mit zentriertem Text. Generischer Baustein für
    /// Status-/Typ-/Tag-Chips (NEW/IMPROVED/..., Sammelobjekt-Typ, Plugin-Status) - Aufrufer übergibt Text/Farben, zeichnet an der aktuellen Cursorposition.</summary>
    public static Vector2 Badge(string text, Vector4 fg, Vector4 bg, Vector4 line, float scale, IFontHandle? font = null, float? radius = null)
    {
        var padding = new Vector2(7f * scale, 1f * scale);
        float textWidth, textHeight;
        using (var _ = font?.Push())
        {
            var size = ImGui.CalcTextSize(text);
            textWidth = size.X;
            textHeight = size.Y;
        }
        var size2 = new Vector2(textWidth + padding.X * 2f, textHeight + padding.Y * 2f);
        var cursor = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();
        var rounding = (radius ?? UiMetrics.ControlRadius) * scale;
        dl.AddRectFilled(cursor, cursor + size2, ImGui.GetColorU32(bg), rounding);
        dl.AddRect(cursor, cursor + size2, ImGui.GetColorU32(line), rounding);
        using (var _ = font?.Push())
            dl.AddText(cursor + padding, ImGui.GetColorU32(fg), text);
        ImGui.Dummy(size2);
        return size2;
    }

    /// <summary>Dezente Tabellen-Kopfzeile (Versalien, TextTertiary, feine Trennlinie darunter) - für ein manuell gezeichnetes ImGui.BeginTable statt
    /// ImGui.TableHeadersRow (das den ImGui-Standard-Look bringt). Muss nach ImGui.TableNextRow(Headers, height) aufgerufen werden, EINMAL je Spalte
    /// (innerhalb der Schleife über die Spalten TableSetColumnIndex selbst aufrufen).</summary>
    public static void TableHeaderCell(string label, float headerHeight, float leftPad, IFontHandle font)
    {
        using (font.Push())
        {
            var textHeight = ImGui.GetTextLineHeight();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + leftPad);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (headerHeight - textHeight) / 2f);
            UiWidgets.DrawSpacedText(label.ToUpperInvariant(), T.TextTertiary, 1f);
        }
    }

    /// <summary>Klickbarer Pillenknopf voller Höhe/Breite (z.B. Prep-Timer-Umschalter) - an: akzentfarben (16% Alpha-Füllung, Accent-Rand,
    /// TextHeading-Text), aus: transparent mit LineFrame-Rand und gedämpftem Text. "prefixIcon" (optional) wird links vor dem Text gezeichnet.
    /// Reagiert auf Links- UND Rechtsklick getrennt (z.B. durchschalten vs. Popup öffnen).</summary>
    public static (bool Left, bool Right) PillButton(string id, string label, Vector2 size, bool active, float scale, IFontHandle font, UiIcon? prefixIcon = null)
    {
        var T = UiTheme.Active;
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton(id, size);
        var left = ImGui.IsItemClicked(ImGuiMouseButton.Left);
        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        var hovered = ImGui.IsItemHovered();

        var dl = ImGui.GetWindowDrawList();
        var bg = active ? T.Accent with { W = 0.16f } : new Vector4(0f, 0f, 0f, 0f);
        var border = active ? T.Accent : T.LineFrame;
        var fg = active ? T.TextHeading : T.TextMuted;
        var rounding = size.Y / 2f;
        dl.AddRectFilled(cursor, cursor + size, ImGui.GetColorU32(bg), rounding);
        dl.AddRect(cursor, cursor + size, ImGui.GetColorU32(border), rounding);

        var prefixSize = prefixIcon.HasValue ? size.Y * 0.42f : 0f;
        var prefixGap = prefixIcon.HasValue ? 5f * scale : 0f;
        float textWidth, textHeight;
        using (font.Push())
        {
            var textSize = ImGui.CalcTextSize(label);
            textWidth = textSize.X;
            textHeight = textSize.Y;
        }

        var contentWidth = prefixSize + prefixGap + textWidth;
        var contentX = cursor.X + (size.X - contentWidth) / 2f;
        if (prefixIcon.HasValue)
            UiIcons.Draw(prefixIcon.Value, new Vector2(contentX, cursor.Y + (size.Y - prefixSize) / 2f), prefixSize, ImGui.GetColorU32(fg));

        using (font.Push())
            dl.AddText(new Vector2(contentX + prefixSize + prefixGap, cursor.Y + (size.Y - textHeight) / 2f), ImGui.GetColorU32(fg), label);

        if (hovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        return (left, right);
    }

    /// <summary>Kreisrunder Icon-Rahmen (Füllung + Rand) - der Aufrufer zeichnet den Inhalt (Bild/Icon) selbst mittig hinein, z.B. per
    /// drawList.AddImageRounded mit Radius = size/2 für ein rund beschnittenes Ingame-Icon.</summary>
    public static void CircleIconFrame(Vector2 center, float radius, Vector4 bg, Vector4 border)
    {
        var dl = ImGui.GetWindowDrawList();
        dl.AddCircleFilled(center, radius, ImGui.GetColorU32(bg), 24);
        dl.AddCircle(center, radius, ImGui.GetColorU32(border), 24);
    }

    /// <summary>Wie <see cref="CircleIconFrame"/>, zeichnet zusätzlich ein rund beschnittenes Bild hinein (z.B. ein Ingame-Icon) - "textureHandle" ist
    /// das ImGui-Textur-Handle (z.B. IDalamudTextureWrap.Handle), null lässt das Bild einfach weg (nur der Rahmen bleibt).</summary>
    public static void RoundIcon(Vector2 center, float radius, Vector4 bg, Vector4 border, ImTextureID? textureHandle, float imageAlpha = 1f)
    {
        var dl = ImGui.GetWindowDrawList();
        dl.AddCircleFilled(center, radius, ImGui.GetColorU32(bg), 24);
        if (textureHandle is { } handle)
        {
            dl.AddImageRounded(handle, center - new Vector2(radius), center + new Vector2(radius), Vector2.Zero, Vector2.One,
                ImGui.GetColorU32(new Vector4(1f, 1f, 1f, imageAlpha)), radius);
        }
        dl.AddCircle(center, radius, ImGui.GetColorU32(border), 24);
    }

    /// <summary>Beschriftetes Info-Kästchen (kleines Label darüber, fetter Wert darunter) - z.B. "Prep starts" / "in 2d 16:14:29". BgInput-Füllung,
    /// LineCard-Rahmen, Radius 4, Padding 8/12. "width" ist die volle Kästchenbreite (Aufrufer teilt z.B. mehrere gleich breite Kästchen selbst auf).
    /// "tooltip" zeigt bei Hover den vollen Wert (z.B. wenn der Wert sonst abgeschnitten werden müsste).</summary>
    public static void InfoTile(string label, string value, float scale, IFontHandle labelFont, IFontHandle valueFont, float width, Vector4? valueColor = null, string? tooltip = null)
    {
        var padX = 12f * scale;
        var padY = 8f * scale;

        float labelHeight, valueHeight;
        using (labelFont.Push())
            labelHeight = ImGui.GetTextLineHeight();
        using (valueFont.Push())
            valueHeight = ImGui.GetTextLineHeight();

        var gap = 4f * scale;
        var height = padY * 2f + labelHeight + gap + valueHeight;
        var cursor = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.BgInput), UiMetrics.ControlRadius * scale);
        dl.AddRect(cursor, cursor + new Vector2(width, height), ImGui.GetColorU32(T.LineCard), UiMetrics.ControlRadius * scale);

        using (labelFont.Push())
            dl.AddText(cursor + new Vector2(padX, padY), ImGui.GetColorU32(T.TextMuted), label);

        var maxValueWidth = width - padX * 2f;
        string truncatedValue;
        using (valueFont.Push())
            truncatedValue = TruncateToWidth(value, maxValueWidth);
        using (valueFont.Push())
            dl.AddText(cursor + new Vector2(padX, padY + labelHeight + gap), ImGui.GetColorU32(valueColor ?? T.TextPrimary), truncatedValue);

        ImGui.Dummy(new Vector2(width, height));
        if ((tooltip != null || truncatedValue != value) && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip ?? value);
    }

    /// <summary>Tabellenzelle mit Hauptzeile (fett) + gedämpfter Unterzeile darunter (z.B. Fischname + "Gebiet · Ort") - beide Zeilen vertikal als
    /// Paar innerhalb "rowHeight" zentriert. "subText" null/leer lässt die Unterzeile einfach weg (dann nur die Hauptzeile vertikal zentriert).</summary>
    public static void TwoLineCell(string mainText, IFontHandle mainFont, Vector4 mainColor, string? subText, IFontHandle? subFont, Vector4? subColor, float rowHeight, float leftPad = 0f)
    {
        var cellStart = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();

        float mainHeight;
        using (mainFont.Push())
            mainHeight = ImGui.GetTextLineHeight();

        var hasSub = !string.IsNullOrEmpty(subText) && subFont != null;
        var subHeight = 0f;
        var gap = 0f;
        if (hasSub)
        {
            using (subFont!.Push())
                subHeight = ImGui.GetTextLineHeight();
            gap = 2f * ImGuiHelpers.GlobalScale;
        }

        var totalHeight = mainHeight + gap + subHeight;
        var y = cellStart.Y + (rowHeight - totalHeight) / 2f;

        using (mainFont.Push())
            dl.AddText(new Vector2(cellStart.X + leftPad, y), ImGui.GetColorU32(mainColor), mainText);

        if (hasSub)
        {
            using (subFont!.Push())
                dl.AddText(new Vector2(cellStart.X + leftPad, y + mainHeight + gap), ImGui.GetColorU32(subColor ?? T.TextMuted), subText);
        }

        ImGui.Dummy(new Vector2(0f, rowHeight));
    }

    /// <summary>Kürzt "text" mit Auslassungspunkten, bis er inklusive "…" in "maxWidth" passt - erwartet die gewünschte Schrift bereits gepusht.</summary>
    public static string TruncateToWidth(string text, float maxWidth)
    {
        if (ImGui.CalcTextSize(text).X <= maxWidth)
            return text;

        const string ellipsis = "…";
        var low = 0;
        var high = text.Length;
        while (low < high)
        {
            var mid = (low + high + 1) / 2;
            if (ImGui.CalcTextSize(text[..mid] + ellipsis).X <= maxWidth)
                low = mid;
            else
                high = mid - 1;
        }

        return low <= 0 ? ellipsis : text[..low] + ellipsis;
    }
}
