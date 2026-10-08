using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace PluginUiKit;

/// <summary>
/// Von Hand gezeichnete Linien-Icons (16x16-Logikraster) als Ersatz für FontAwesomeIcon-Glyphen -
/// für Plugins, deren Theme (z.B. BigFishHelper/Ocean) bewusst keine Icon-Schriftart im Look
/// verwenden will. Jedes Icon zeichnet nur Linien/Kreise/Dreiecke in der übergebenen Farbe - keine
/// eigene Farbe/kein eigenes Theme fest verdrahtet, rein geometrisch und beliebig einsetzbar.
/// </summary>
public enum UiIcon
{
    Play,
    Sliders,
    Bug,
    Fish,
    Console,
    Plug,
    Info,
    Hook,
    Travel,
    Overlay,
    Clock,
    Check,
    Cross,
    Copy,
    Sparkle,
    Lightbulb,
    Heart,
    Mug,
    Search,
    Pin,
    Trash,
    Stop,
    SkipForward,
    Warning,
    ChevronUp,
    ChevronDown,
}

public static class UiIcons
{
    /// <summary>Zeichnet "icon" in ein size x size Quadrat mit linker oberer Ecke bei "pos" (Bildschirmkoordinaten), Linienstärke proportional zu "size".</summary>
    public static void Draw(UiIcon icon, Vector2 pos, float size, uint color)
    {
        var dl = ImGui.GetWindowDrawList();
        var t = MathF.Max(1f, size / 11f);
        Vector2 P(float x, float y) => pos + new Vector2(x, y) * size;

        switch (icon)
        {
            case UiIcon.Play:
                dl.AddTriangleFilled(P(0.28f, 0.18f), P(0.28f, 0.82f), P(0.82f, 0.5f), color);
                break;

            case UiIcon.Sliders:
                dl.AddLine(P(0.12f, 0.28f), P(0.88f, 0.28f), color, t);
                dl.AddLine(P(0.12f, 0.5f), P(0.88f, 0.5f), color, t);
                dl.AddLine(P(0.12f, 0.72f), P(0.88f, 0.72f), color, t);
                dl.AddCircleFilled(P(0.62f, 0.28f), size * 0.08f, color);
                dl.AddCircleFilled(P(0.34f, 0.5f), size * 0.08f, color);
                dl.AddCircleFilled(P(0.56f, 0.72f), size * 0.08f, color);
                break;

            case UiIcon.Bug:
                dl.AddCircle(P(0.5f, 0.52f), size * 0.24f, color, 24, t);
                dl.AddLine(P(0.5f, 0.24f), P(0.5f, 0.14f), color, t);
                dl.AddLine(P(0.3f, 0.34f), P(0.14f, 0.24f), color, t);
                dl.AddLine(P(0.7f, 0.34f), P(0.86f, 0.24f), color, t);
                dl.AddLine(P(0.26f, 0.52f), P(0.08f, 0.52f), color, t);
                dl.AddLine(P(0.74f, 0.52f), P(0.92f, 0.52f), color, t);
                dl.AddLine(P(0.3f, 0.72f), P(0.14f, 0.82f), color, t);
                dl.AddLine(P(0.7f, 0.72f), P(0.86f, 0.82f), color, t);
                break;

            case UiIcon.Fish:
                dl.AddBezierCubic(P(0.1f, 0.5f), P(0.25f, 0.2f), P(0.6f, 0.22f), P(0.72f, 0.42f), color, t);
                dl.AddBezierCubic(P(0.72f, 0.42f), P(0.6f, 0.78f), P(0.25f, 0.8f), P(0.1f, 0.5f), color, t);
                dl.AddTriangle(P(0.72f, 0.42f), P(0.94f, 0.3f), P(0.72f, 0.58f), color, t);
                dl.AddCircleFilled(P(0.26f, 0.47f), size * 0.035f, color);
                break;

            case UiIcon.Console:
                dl.AddLine(P(0.16f, 0.3f), P(0.42f, 0.5f), color, t);
                dl.AddLine(P(0.42f, 0.5f), P(0.16f, 0.7f), color, t);
                dl.AddLine(P(0.5f, 0.74f), P(0.86f, 0.74f), color, t);
                break;

            case UiIcon.Plug:
                dl.AddRect(P(0.3f, 0.34f), P(0.7f, 0.64f), color, size * 0.08f, ImDrawFlags.None, t);
                dl.AddLine(P(0.42f, 0.34f), P(0.42f, 0.18f), color, t);
                dl.AddLine(P(0.58f, 0.34f), P(0.58f, 0.18f), color, t);
                dl.AddLine(P(0.5f, 0.64f), P(0.5f, 0.78f), color, t);
                dl.AddBezierCubic(P(0.5f, 0.78f), P(0.5f, 0.9f), P(0.78f, 0.86f), P(0.82f, 0.94f), color, t);
                break;

            case UiIcon.Info:
                dl.AddCircle(P(0.5f, 0.5f), size * 0.38f, color, 32, t);
                dl.AddCircleFilled(P(0.5f, 0.3f), size * 0.045f, color);
                dl.AddLine(P(0.5f, 0.44f), P(0.5f, 0.72f), color, t);
                break;

            case UiIcon.Hook:
                dl.AddLine(P(0.4f, 0.1f), P(0.4f, 0.5f), color, t);
                dl.AddBezierCubic(P(0.4f, 0.5f), P(0.4f, 0.86f), P(0.78f, 0.86f), P(0.78f, 0.62f), color, t);
                dl.AddBezierCubic(P(0.78f, 0.62f), P(0.78f, 0.46f), P(0.58f, 0.46f), P(0.56f, 0.6f), color, t);
                break;

            case UiIcon.Travel:
                dl.AddTriangleFilled(P(0.1f, 0.78f), P(0.9f, 0.46f), P(0.42f, 0.56f), color);
                dl.AddTriangleFilled(P(0.42f, 0.56f), P(0.9f, 0.46f), P(0.5f, 0.9f), color);
                break;

            case UiIcon.Overlay:
                dl.AddRect(P(0.12f, 0.2f), P(0.88f, 0.8f), color, size * 0.06f, ImDrawFlags.None, t);
                dl.AddLine(P(0.12f, 0.36f), P(0.88f, 0.36f), color, t);
                dl.AddCircleFilled(P(0.22f, 0.28f), size * 0.03f, color);
                break;

            case UiIcon.Clock:
                dl.AddCircle(P(0.5f, 0.5f), size * 0.38f, color, 32, t);
                dl.AddLine(P(0.5f, 0.5f), P(0.5f, 0.28f), color, t);
                dl.AddLine(P(0.5f, 0.5f), P(0.68f, 0.58f), color, t);
                break;

            case UiIcon.Check:
                dl.AddLine(P(0.14f, 0.52f), P(0.4f, 0.78f), color, t);
                dl.AddLine(P(0.4f, 0.78f), P(0.86f, 0.26f), color, t);
                break;

            case UiIcon.Cross:
                dl.AddLine(P(0.22f, 0.22f), P(0.78f, 0.78f), color, t);
                dl.AddLine(P(0.78f, 0.22f), P(0.22f, 0.78f), color, t);
                break;

            case UiIcon.Copy:
                dl.AddRect(P(0.34f, 0.12f), P(0.86f, 0.64f), color, size * 0.05f, ImDrawFlags.None, t);
                dl.AddLine(P(0.14f, 0.34f), P(0.14f, 0.86f), color, t);
                dl.AddLine(P(0.14f, 0.86f), P(0.64f, 0.86f), color, t);
                dl.AddLine(P(0.64f, 0.86f), P(0.64f, 0.64f), color, t);
                break;

            case UiIcon.Sparkle:
            {
                // Großer 4-zackiger Funke (senkrechte + waagerechte Raute übereinander) plus ein
                // kleinerer Funke oben rechts daneben - Debug-Menüpunkt-Icon nach Nutzervorgabe.
                Span<Vector2> big = stackalloc Vector2[]
                {
                    P(0.5f, 0.04f), P(0.6f, 0.4f), P(0.96f, 0.5f), P(0.6f, 0.6f),
                    P(0.5f, 0.96f), P(0.4f, 0.6f), P(0.04f, 0.5f), P(0.4f, 0.4f),
                };
                dl.AddConvexPolyFilled(ref big[0], big.Length, color);

                Span<Vector2> small = stackalloc Vector2[]
                {
                    P(0.78f, 0.06f), P(0.82f, 0.16f), P(0.92f, 0.2f), P(0.82f, 0.24f),
                    P(0.78f, 0.34f), P(0.74f, 0.24f), P(0.64f, 0.2f), P(0.74f, 0.16f),
                };
                dl.AddConvexPolyFilled(ref small[0], small.Length, color);
                break;
            }

            case UiIcon.Lightbulb:
                dl.AddCircle(P(0.5f, 0.4f), size * 0.26f, color, 24, t);
                dl.AddLine(P(0.38f, 0.64f), P(0.62f, 0.64f), color, t);
                dl.AddLine(P(0.4f, 0.74f), P(0.6f, 0.74f), color, t);
                dl.AddLine(P(0.44f, 0.84f), P(0.56f, 0.84f), color, t);
                dl.AddLine(P(0.5f, 0.1f), P(0.5f, 0.18f), color, t);
                dl.AddLine(P(0.22f, 0.4f), P(0.3f, 0.4f), color, t);
                dl.AddLine(P(0.7f, 0.4f), P(0.78f, 0.4f), color, t);
                dl.AddLine(P(0.28f, 0.18f), P(0.34f, 0.24f), color, t);
                dl.AddLine(P(0.72f, 0.18f), P(0.66f, 0.24f), color, t);
                break;

            case UiIcon.Heart:
            {
                Span<Vector2> pts = stackalloc Vector2[16];
                for (var i = 0; i < pts.Length; i++)
                {
                    var t2 = i / (float)(pts.Length - 1) * MathF.PI * 2f;
                    var hx = 16f * MathF.Pow(MathF.Sin(t2), 3f);
                    var hy = -(13f * MathF.Cos(t2) - 5f * MathF.Cos(2f * t2) - 2f * MathF.Cos(3f * t2) - MathF.Cos(4f * t2));
                    pts[i] = P(0.5f + hx / 42f, 0.5f + hy / 36f);
                }
                dl.AddConvexPolyFilled(ref pts[0], pts.Length, color);
                break;
            }

            case UiIcon.Mug:
                dl.AddRect(P(0.2f, 0.3f), P(0.68f, 0.82f), color, size * 0.05f, ImDrawFlags.None, t);
                dl.AddBezierCubic(P(0.68f, 0.38f), P(0.9f, 0.38f), P(0.9f, 0.66f), P(0.68f, 0.66f), color, t);
                dl.AddLine(P(0.3f, 0.1f), P(0.34f, 0.2f), color, t);
                dl.AddLine(P(0.44f, 0.1f), P(0.48f, 0.2f), color, t);
                break;

            case UiIcon.Search:
                dl.AddCircle(P(0.42f, 0.42f), size * 0.26f, color, 24, t);
                dl.AddLine(P(0.6f, 0.6f), P(0.84f, 0.84f), color, t * 1.3f);
                break;

            case UiIcon.Pin:
                dl.AddCircle(P(0.5f, 0.38f), size * 0.2f, color, 20, t);
                dl.AddLine(P(0.5f, 0.38f), P(0.5f, 0.86f), color, t);
                break;

            case UiIcon.Trash:
                dl.AddLine(P(0.22f, 0.3f), P(0.78f, 0.3f), color, t);
                dl.AddLine(P(0.4f, 0.3f), P(0.4f, 0.18f), color, t);
                dl.AddLine(P(0.6f, 0.3f), P(0.6f, 0.18f), color, t);
                dl.AddLine(P(0.4f, 0.18f), P(0.6f, 0.18f), color, t);
                dl.AddLine(P(0.28f, 0.3f), P(0.32f, 0.86f), color, t);
                dl.AddLine(P(0.72f, 0.3f), P(0.68f, 0.86f), color, t);
                dl.AddLine(P(0.32f, 0.86f), P(0.68f, 0.86f), color, t);
                dl.AddLine(P(0.42f, 0.4f), P(0.44f, 0.76f), color, t);
                dl.AddLine(P(0.58f, 0.4f), P(0.56f, 0.76f), color, t);
                break;

            case UiIcon.Stop:
                dl.AddRectFilled(P(0.24f, 0.24f), P(0.76f, 0.76f), color, size * 0.06f);
                break;

            case UiIcon.SkipForward:
                dl.AddTriangleFilled(P(0.16f, 0.2f), P(0.16f, 0.8f), P(0.56f, 0.5f), color);
                dl.AddTriangleFilled(P(0.5f, 0.2f), P(0.5f, 0.8f), P(0.9f, 0.5f), color);
                dl.AddLine(P(0.9f, 0.2f), P(0.9f, 0.8f), color, t * 1.4f);
                break;

            case UiIcon.Warning:
            {
                Span<Vector2> tri = stackalloc Vector2[] { P(0.5f, 0.1f), P(0.92f, 0.84f), P(0.08f, 0.84f) };
                dl.AddConvexPolyFilled(ref tri[0], tri.Length, color);
                break;
            }

            case UiIcon.ChevronUp:
                dl.AddLine(P(0.2f, 0.62f), P(0.5f, 0.34f), color, t * 1.3f);
                dl.AddLine(P(0.5f, 0.34f), P(0.8f, 0.62f), color, t * 1.3f);
                break;

            case UiIcon.ChevronDown:
                dl.AddLine(P(0.2f, 0.38f), P(0.5f, 0.66f), color, t * 1.3f);
                dl.AddLine(P(0.5f, 0.66f), P(0.8f, 0.38f), color, t * 1.3f);
                break;
        }
    }
}
