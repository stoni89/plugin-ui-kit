using System.Numerics;

namespace PluginUiKit;

/// <summary>
/// Austauschbare Verzierungen (Eck-Verzierung, Trenn-Ornament, Seitenleisten-Logo) - alles andere
/// in PluginUiKit ist zwischen Plugins identisch (nur Farbe via UiTheme), aber diese drei Elemente
/// sind bewusst visuell plugin-spezifisch (Codex: goldene L-Striche/Linie-Raute-Linie/Kompass).
/// Jedes Plugin implementiert dieses Interface einmal für seinen eigenen Look.
/// </summary>
public interface IUiOrnaments
{
    /// <summary>Zeichnet EINE Fensterecken-Verzierung bei <paramref name="corner"/> (Bildschirmkoordinaten,
    /// bereits um den gewünschten Innenabstand verschoben). <paramref name="sx"/>/<paramref name="sy"/>
    /// geben an, in welche Richtung die Verzierung von diesem Punkt aus "zeigt" (+1/-1 je Achse) -
    /// z.B. obere linke Fensterecke: sx=+1, sy=+1. Der Aufrufer (siehe UiWidgets.DrawWindowCorners)
    /// ruft das für alle vier Ecken auf.</summary>
    void DrawCorner(Vector2 corner, int sx, int sy);

    /// <summary>Zeichnet EIN horizontales Trenn-Ornament, linksbündig bei <paramref name="pos"/>
    /// (Bildschirmkoordinaten) beginnend, über die angegebene <paramref name="width"/>.</summary>
    void DrawDivider(Vector2 pos, float width);

    /// <summary>Zeichnet die reine Bildmarke (ohne Markentext) des Seitenleisten-Logos am aktuellen
    /// Cursor, quadratisch in <paramref name="size"/> Pixeln (Codex: der Kompass-Ring+Nadel).</summary>
    void DrawSidebarLogo(float size);
}
