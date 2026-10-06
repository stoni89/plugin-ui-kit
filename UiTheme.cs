using System.Numerics;

namespace PluginUiKit;

/// <summary>
/// Eine vollständige Farbpalette für PluginUiKit-Widgets. Jedes Plugin erstellt eine eigene
/// Instanz (eigene Hex-Werte) und setzt sie einmal beim Start als <see cref="Active"/> - alle
/// Widgets in diesem Paket lesen ausschließlich über <see cref="Active"/>, nie über eine fest
/// verdrahtete Palette, damit dasselbe Paket in mehreren Plugins mit jeweils eigenem Farblook
/// laufen kann, ohne dass Widget-Code angepasst werden muss.
///
/// Diese Werte sind 1:1 aus TheExplorersCodex.CodexTheme (dem bisherigen, plugin-eigenen Theme)
/// übernommen - siehe <see cref="Codex"/>. Seitenspezifische/einmalige Codex-Farben (z.B. Log-
/// Level-Farben, Sammelobjekt-Typ-Etiketten) sind bewusst NICHT Teil dieser generischen Palette;
/// die bleiben Sache des jeweiligen Plugins (dort als eigene Konstanten neben der UiTheme-Instanz).
/// </summary>
public sealed class UiTheme
{
    // ---- Flächen ----
    public Vector4 BgWindow { get; set; }
    public Vector4 BgSidebar { get; set; }
    public Vector4 BgCard { get; set; }
    public Vector4 BgInput { get; set; }
    public Vector4 BgPopup { get; set; }
    public Vector4 BgSelected { get; set; }
    public Vector4 BgSelectedStrong { get; set; }

    // ---- Linien ----
    public Vector4 LineFrame { get; set; }
    public Vector4 LineControl { get; set; }
    public Vector4 LineCard { get; set; }
    public Vector4 LineSubtle { get; set; }
    public Vector4 LineRow { get; set; }
    public Vector4 LineDisabled { get; set; }

    // ---- Text ----
    public Vector4 TextHeading { get; set; }
    public Vector4 TextPrimary { get; set; }
    public Vector4 TextCardTitle { get; set; }
    public Vector4 TextValue { get; set; }
    public Vector4 TextSecondary { get; set; }
    public Vector4 TextTertiary { get; set; }
    public Vector4 TextMuted { get; set; }
    public Vector4 TextDim { get; set; }
    public Vector4 TextDisabled { get; set; }
    public Vector4 TextOnAccent { get; set; }

    // ---- Akzent ----
    public Vector4 Accent { get; set; }

    // ---- Status/Semantik ----
    public Vector4 OkFg { get; set; }
    public Vector4 OkBg { get; set; }
    public Vector4 OkLine { get; set; }
    public Vector4 WarnFg { get; set; }
    public Vector4 WarnBg { get; set; }
    public Vector4 WarnLine { get; set; }
    public Vector4 ErrFg { get; set; }
    public Vector4 ErrBg { get; set; }
    public Vector4 ErrLine { get; set; }
    public Vector4 InfoFg { get; set; }
    public Vector4 InfoBg { get; set; }
    public Vector4 InfoLine { get; set; }

    private static Vector4 Hex(string hex, float alpha = 1f)
    {
        hex = hex.TrimStart('#');
        var r = System.Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
        var g = System.Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
        var b = System.Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
        return new Vector4(r, g, b, alpha);
    }

    /// <summary>Die ursprüngliche "dunkles Tinten-Braun mit Gold-Akzent"-Palette aus TheExplorersCodex.CodexTheme - erste Instanz dieses Pakets.</summary>
    public static readonly UiTheme Codex = new()
    {
        BgWindow = Hex("#1A150F"),
        BgSidebar = Hex("#130F0A"),
        BgCard = Hex("#221C14"),
        BgInput = Hex("#17120C"),
        BgPopup = Hex("#1D1812"),
        BgSelected = Hex("#2E2516"),
        BgSelectedStrong = Hex("#3A2F1E"),

        LineFrame = Hex("#6A5638"),
        LineControl = Hex("#5A4A33"),
        LineCard = Hex("#43372A"),
        LineSubtle = Hex("#362C1F"),
        LineRow = Hex("#2A2219"),
        LineDisabled = Hex("#3E3324"),

        TextHeading = Hex("#F6E9C8"),
        TextPrimary = Hex("#F1E6CF"),
        TextCardTitle = Hex("#E9D7AE"),
        TextValue = Hex("#E2D3B2"),
        TextSecondary = Hex("#C2B396"),
        TextTertiary = Hex("#B8A88A"),
        TextMuted = Hex("#A8987A"),
        TextDim = Hex("#8A7B62"),
        TextDisabled = Hex("#6E604A"),
        TextOnAccent = Hex("#1A1208"),

        Accent = Hex("#D4A94F"),

        OkFg = Hex("#9BD3A2"),
        OkBg = Hex("#1C2A1D"),
        OkLine = Hex("#35513A"),
        WarnFg = Hex("#E9C46A"),
        WarnBg = Hex("#30271A"),
        WarnLine = Hex("#5C4A26"),
        ErrFg = Hex("#EE9A86"),
        ErrBg = Hex("#331C17"),
        ErrLine = Hex("#5A2E26"),
        InfoFg = Hex("#9CC4E4"),
        InfoBg = Hex("#18242E"),
        InfoLine = Hex("#2C4458"),
    };

    /// <summary>Die aktuell aktive Palette - von genau einem Plugin einmal beim Start gesetzt
    /// (z.B. <c>UiTheme.Active = UiTheme.Codex;</c>), danach von allen PluginUiKit-Widgets gelesen.
    /// Bewusst NACH <see cref="Codex"/> deklariert: statische Feld-Initialisierer laufen in
    /// Deklarationsreihenfolge, ein Verweis auf "Codex" vor dessen eigener Deklaration würde zur
    /// Initialisierungszeit noch null liefern.</summary>
    public static UiTheme Active { get; set; } = Codex;
}
