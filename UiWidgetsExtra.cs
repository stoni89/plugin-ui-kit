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

    /// <summary>Hinweis-Karte mit Tastenkürzel (z.B. "Ctrl" + "Shift" + "Click" ... Text). "maxWidth" begrenzt die Kartenbreite optional (sonst volle verfügbare Breite).</summary>
    public static void ShortcutHint(IReadOnlyList<string> keys, string text, float? maxWidth = null)
    {
        var s = ImGuiHelpers.GlobalScale;
        var dl = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var width = MathF.Min(ImGui.GetContentRegionAvail().X, maxWidth ?? float.MaxValue);

        float padX = 16f * s, padY = 12f * s;
        float keyPadX = 7f * s, keyPadY = 2f * s;
        float plusGap = 5f * s, textGap = 14f * s;

        float keyTextH, bodyH;
        using (UiFonts.Body.Push())
            keyTextH = ImGui.GetTextLineHeight();
        using (UiFonts.Body.Push())
            bodyH = ImGui.GetTextLineHeight();
        var keyH = keyTextH + keyPadY * 2f + 1f * s;
        var height = MathF.Max(keyH, bodyH) + padY * 2f;
        var midY = origin.Y + height / 2f;

        var cardMax = origin + new Vector2(width, height);
        dl.AddRectFilled(origin, cardMax, ImGui.GetColorU32(T.BgPopup), UiMetrics.CardRadius * s);
        dl.AddRect(origin, cardMax, ImGui.GetColorU32(T.LineCard), UiMetrics.CardRadius * s, ImDrawFlags.None, 1f * s);

        var x = origin.X + padX;

        using (UiFonts.Body.Push())
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
        using (UiFonts.Body.Push())
            dl.AddText(new Vector2(x, midY - bodyH / 2f), ImGui.GetColorU32(T.TextSecondary), text);

        ImGui.Dummy(new Vector2(width, height));
    }

    /// <summary>Gefüllter Fortschrittsbalken - abgerundete Enden, Füllung bei fraction &gt; 0 mindestens 4px breit (sonst bei sehr kleinen Anteilen praktisch unsichtbar).
    /// "width" überschreibt optional die sonst über ImGui.GetContentRegionAvail() ermittelte Breite.</summary>
    public static void ProgressBar(float fraction, float height, string? tooltip = null, float? width = null)
    {
        var s = ImGuiHelpers.GlobalScale;
        height *= s;
        var dl = ImGui.GetWindowDrawList();
        var p = ImGui.GetCursorScreenPos();
        var w = width ?? ImGui.GetContentRegionAvail().X;
        var r = height / 2f;

        dl.AddRectFilled(p, p + new Vector2(w, height), ImGui.GetColorU32(T.BgSelected), r);
        if (fraction > 0f)
        {
            var fw = MathF.Max(w * Math.Clamp(fraction, 0f, 1f), 4f * s);
            dl.AddRectFilled(p, p + new Vector2(fw, height), ImGui.GetColorU32(T.Accent), r);
        }

        ImGui.Dummy(new Vector2(w, height));
        if (tooltip != null && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }

    /// <summary>Fließende Knopfreihe mit Umbruch - ImGui bricht SameLine-Ketten nicht von selbst um, daher hier manuell: ein Knopf, der nicht mehr in die
    /// aktuelle Zeile passt, beginnt eine neue. "isConfirmed" markiert einen Knopf optional als "gerade bestätigt" (z.B. 1,5s nach einem Klick) - zeigt dann
    /// ein Häkchen in OkFg statt des "icon"-Symbols.</summary>
    public static void FlowButtons(IReadOnlyList<(string Label, Action OnClick)> items, float gap, Func<string, bool>? isConfirmed = null, FontAwesomeIcon icon = FontAwesomeIcon.Terminal)
    {
        var s = ImGuiHelpers.GlobalScale;
        gap *= s;
        var rightEdge = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(gap, gap));
        for (var i = 0; i < items.Count; i++)
        {
            var (label, onClick) = items[i];
            var confirmed = isConfirmed?.Invoke(label) ?? false;
            var size = MeasureFlowButton(label, s, icon);

            if (i > 0)
            {
                ImGui.SameLine();
                if (ImGui.GetCursorScreenPos().X + size.X > rightEdge)
                    ImGui.NewLine();
            }

            if (DrawFlowButton(label, confirmed, size, s, icon))
                onClick();
        }
        ImGui.PopStyleVar();
    }

    private static Vector2 MeasureFlowButton(string label, float scale, FontAwesomeIcon icon)
    {
        var padding = new Vector2(11f * scale, 5f * scale);
        var iconGap = 6f * scale;

        float iconWidth;
        using (UiFonts.PluginInterfaceIconFont.Push())
            iconWidth = ImGui.CalcTextSize(icon.ToIconString()).X;
        float textWidth, textHeight;
        using (UiFonts.BodyMedium.Push())
        {
            var size = ImGui.CalcTextSize(label);
            textWidth = size.X;
            textHeight = size.Y;
        }

        return new Vector2(iconWidth + iconGap + textWidth + padding.X * 2f, textHeight + padding.Y * 2f);
    }

    private static bool DrawFlowButton(string label, bool confirmed, Vector2 buttonSize, float scale, FontAwesomeIcon icon)
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
        using (UiFonts.BodyMedium.Push())
            textHeight = ImGui.CalcTextSize(label).Y;

        var iconCursor = cursor + new Vector2(padding.X, (buttonSize.Y - iconHeight) / 2f);
        using (UiFonts.PluginInterfaceIconFont.Push())
            dl.AddText(iconCursor, ImGui.GetColorU32(iconColor), drawIcon.ToIconString());
        var textCursor = new Vector2(cursor.X + padding.X + iconWidth + iconGap, cursor.Y + (buttonSize.Y - textHeight) / 2f);
        using (UiFonts.BodyMedium.Push())
            dl.AddText(textCursor, ImGui.GetColorU32(T.TextHeading), label);

        if (hovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        return clicked;
    }

    /// <summary>Kleines Badge/Typ-Etikett - gefüllter, umrandeter, abgerundeter Hintergrund mit zentriertem Text. Generischer Baustein für
    /// Status-/Typ-/Tag-Chips (NEW/IMPROVED/..., Sammelobjekt-Typ, Plugin-Status) - Aufrufer übergibt Text/Farben, zeichnet an der aktuellen Cursorposition.</summary>
    public static Vector2 Badge(string text, Vector4 fg, Vector4 bg, Vector4 line, float scale, IFontHandle? font = null)
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
        dl.AddRectFilled(cursor, cursor + size2, ImGui.GetColorU32(bg), UiMetrics.ControlRadius * scale);
        dl.AddRect(cursor, cursor + size2, ImGui.GetColorU32(line), UiMetrics.ControlRadius * scale);
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
}
