using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace PluginUiKit;

/// <summary>
/// "Kissing Fish"-Verzierung für das Ocean-Theme: zwei gefüllte Fische, die sich in jeder
/// Fensterecke "küssen" (siehe DrawCorner-Kommentar für die genaue Geometrie), ein wellenförmiges
/// Trenn-Ornament, und ein Kreis mit stilisiertem Fisch als Seitenleisten-Bildmarke. Liest nur
/// UiTheme.Active (kein fest verdrahtetes Ocean-Hex) - dieselbe Austauschregel wie CodexOrnaments.
/// </summary>
public sealed class OceanOrnaments : IUiOrnaments
{
    // Referenz-Box 30x30 Einheiten (siehe Geometrie-Kommentare an DrawCorner unten) - anders als
    // CodexOrnaments nimmt dieses Motiv keine Konstruktorparameter für Länge/Stärke entgegen: die
    // Kissing-Fish-Geometrie ist fest proportioniert, nur die Gesamtgröße skaliert mit GlobalScale -
    // EIN einziger Skalierungsfaktor für X und Y, keine getrennte Breiten-/Höhen-Ableitung (Bugfix:
    // sonst wird ein Fisch sichtbar kleiner/dünner als der andere).
    private const float BoxSize = 30f;
    private const int BezierSegments = 12;

    // Fisch A (waagerecht, Kopf zur Ecke) in 30x30-Einheiten - Körper als zwei kubische Béziers
    // (obere Kurve P0 C1 C2 P1, untere Kurve P1 C1 C2 P0, siehe DrawCorner), Schwanz als Dreieck.
    private static readonly (float X, float Y)[] BodyUpper = { (8f, 6f), (11f, 2.4f), (19f, 2.4f), (22f, 6f) };
    private static readonly (float X, float Y)[] BodyLower = { (22f, 6f), (19f, 9.6f), (11f, 9.6f), (8f, 6f) };
    private static readonly (float X, float Y)[] Tail = { (21.5f, 6f), (27f, 2.4f), (27f, 9.6f) };
    private static readonly (float X, float Y) Eye = (11.6f, 5.2f);

    /// <summary>
    /// Zeichnet EIN Eckenpaar "küssender" Fische bei <paramref name="corner"/> (siehe
    /// IUiOrnaments.DrawCorner-Vertrag: bereits um den gewünschten Fensterrand-Abstand verschoben,
    /// sx/sy geben die Blickrichtung zur echten Fensterecke an). Fisch B wird NICHT separat
    /// definiert, sondern aus Fisch A erzeugt (dieselben Punkte, x und y vertauscht - an der
    /// Diagonalen gespiegelt) - garantiert identische Größe beider Fische (Bugfix: zuvor per Hand
    /// abgetippte, separate Fisch-B-Koordinaten konnten dafür nicht garantieren). Körper als gefüllte
    /// "Linse" aus zwei kubischen Bézierkurven (in Punkte aufgelöst, NICHT per AddBezierCubic - das
    /// zeichnet nur eine Linie, keine Füllung), Schwanz als gefülltes Dreieck, Auge als kleiner Kreis
    /// in UiTheme.Active.BgWindow. Die Umlaufrichtung wird umgedreht, wenn eine ungerade Anzahl der
    /// drei Spiegelungen (Achse X, Achse Y, Diagonale/swap für Fisch B) aktiv ist - sonst wirkt die
    /// Füllung durch ImGuis Umlaufrichtung ausgefranst oder dünner.
    /// </summary>
    public void DrawCorner(Vector2 corner, int sx, int sy)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var box = BoxSize * scale;
        var fill = ImGui.GetColorU32(UiTheme.Active.Accent);
        var eye = ImGui.GetColorU32(UiTheme.Active.BgWindow);
        var dl = ImGui.GetWindowDrawList();

        var origin = new Vector2(
            sx > 0 ? corner.X : corner.X - box,
            sy > 0 ? corner.Y : corner.Y - box);
        var flipX = sx < 0;
        var flipY = sy < 0;

        foreach (var swap in new[] { false, true })
        {
            Vector2 T(float x, float y)
            {
                if (swap)
                    (x, y) = (y, x);
                return origin + new Vector2((flipX ? BoxSize - x : x) * scale, (flipY ? BoxSize - y : y) * scale);
            }

            var flip = flipX ^ flipY ^ swap;

            DrawLens(dl, T, flip, fill);
            DrawTri(dl, T, flip, fill);
            dl.AddCircleFilled(T(Eye.X, Eye.Y), 0.95f * scale, eye, 12);
        }
    }

    /// <summary>Gefüllter Fischkörper - zwei kubische Béziers (Ober-/Unterkante) zu einer "Linse" zusammengesetzt, in Punkte aufgelöst und als konvexes Polygon gefüllt.</summary>
    private static void DrawLens(ImDrawListPtr dl, Func<float, float, Vector2> t, bool flip, uint color)
    {
        var points = new List<Vector2>(BezierSegments * 2);
        AddBezierPoints(points, t, BodyUpper[0], BodyUpper[1], BodyUpper[2], BodyUpper[3]);
        AddBezierPoints(points, t, BodyLower[0], BodyLower[1], BodyLower[2], BodyLower[3]);
        if (flip)
            points.Reverse();

        var array = points.ToArray();
        dl.AddConvexPolyFilled(ref array[0], array.Length, color);
    }

    /// <summary>Löst eine kubische Bézierkurve von "a" nach "d" (Kontrollpunkte b/c) in BezierSegments Punkte auf - der Endpunkt wird bewusst ausgelassen, er ist der Startpunkt der jeweils nächsten Kurve.</summary>
    private static void AddBezierPoints(List<Vector2> points, Func<float, float, Vector2> t,
        (float X, float Y) a, (float X, float Y) b, (float X, float Y) c, (float X, float Y) d)
    {
        for (var i = 0; i < BezierSegments; i++)
        {
            var u = i / (float)BezierSegments;
            var v = 1f - u;
            var x = v * v * v * a.X + 3f * v * v * u * b.X + 3f * v * u * u * c.X + u * u * u * d.X;
            var y = v * v * v * a.Y + 3f * v * v * u * b.Y + 3f * v * u * u * c.Y + u * u * u * d.Y;
            points.Add(t(x, y));
        }
    }

    /// <summary>Gefüllter Schwanz - Dreieck, bei umgedrehter Umlaufrichtung in umgekehrter Punktreihenfolge (siehe DrawCorner-Kommentar).</summary>
    private static void DrawTri(ImDrawListPtr dl, Func<float, float, Vector2> t, bool flip, uint color)
    {
        var points = flip
            ? new[] { t(Tail[2].X, Tail[2].Y), t(Tail[1].X, Tail[1].Y), t(Tail[0].X, Tail[0].Y) }
            : new[] { t(Tail[0].X, Tail[0].Y), t(Tail[1].X, Tail[1].Y), t(Tail[2].X, Tail[2].Y) };
        dl.AddConvexPolyFilled(ref points[0], 3, color);
    }

    public void DrawDivider(Vector2 pos, float width)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var dl = ImGui.GetWindowDrawList();
        var col = ImGui.GetColorU32(UiTheme.Active.Accent with { W = 0.7f });
        var amplitude = 3f * scale;
        var step = 6f * scale;

        var prev = pos;
        for (var x = step; x <= width; x += step)
        {
            var y = pos.Y + amplitude * MathF.Sin(x / (width > 0f ? width : 1f) * MathF.PI * 4f);
            var next = new Vector2(pos.X + x, y);
            dl.AddLine(prev, next, col, 1.2f * scale);
            prev = next;
        }
    }

    public void DrawSidebarLogo(float size)
    {
        var cursor = ImGui.GetCursorScreenPos();
        var center = cursor + new Vector2(size / 2f, size / 2f);
        var dl = ImGui.GetWindowDrawList();
        var accent = ImGui.GetColorU32(UiTheme.Active.Accent);

        dl.AddCircle(center, size * 0.42f, accent, 32, 1.5f);
        UiIcons.Draw(UiIcon.Fish, cursor + new Vector2(size * 0.22f, size * 0.3f), size * 0.56f, accent);

        ImGui.Dummy(new Vector2(size, size));
    }
}
