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

    /// <summary>Hintergrund kleiner Statuskarten (siehe UiNav.SidebarStatusCard) - eigener Ton statt BgCard,
    /// da manche Paletten (z.B. Ocean) hier bewusst einen anderen Wert als normale Karten wollen.</summary>
    public Vector4 BgStatus { get; set; }

    /// <summary>Hintergrund eines ausgeschalteten Toggle (siehe UiWidgets.Toggle) - eigener Ton statt LineRow,
    /// da manche Paletten (z.B. Ocean) hier bewusst einen anderen Wert als normale Zeilenlinien wollen.</summary>
    public Vector4 BgToggleOff { get; set; }

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

    /// <summary>Beschreibungstext unter einem Einstellungstitel (siehe UiWidgets.SettingRow) - eigener Ton statt
    /// TextTertiary, da manche Paletten (z.B. Ocean) Beschreibung und "gedämpft" bewusst unterschiedlich färben.</summary>
    public Vector4 TextDesc { get; set; }

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

    /// <summary>Parst eine "#RRGGBB"-Hex-Farbe (führendes "#" optional) - öffentlich, damit Seiten-Code
    /// seiten-/zustandsspezifische Einzelfarben (die bewusst nicht Teil der generischen Palette sind,
    /// siehe z.B. OceanMainWindow.FishDataActiveGreen) ebenfalls aus dem Hex-Wert der Vorgabe bauen kann,
    /// statt ihn von Hand in r/g/b-Floats umzurechnen.</summary>
    public static Vector4 Hex(string hex, float alpha = 1f)
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
        BgStatus = Hex("#221C14"),
        BgToggleOff = Hex("#2A2219"),

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
        TextDesc = Hex("#B8A88A"),
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

    /// <summary>Zweite mitgelieferte Palette - ein dunkles Blaugrün mit türkisem Akzent ("Ocean"), z.B. für
    /// BigFishHelper. Werte wie vom jeweiligen Plugin vorgegeben; Token, die dort nicht explizit benannt
    /// wurden (z.B. die Status-Semantikfarben OkFg/WarnFg/ErrFg/InfoFg), sind sinnvoll aus der Akzentfarbe
    /// abgeleitete Platzhalter, da die General-Seite von BigFishHelper sie nicht verwendet.</summary>
    public static readonly UiTheme Ocean = new()
    {
        BgWindow = Hex("#0D1820"),
        BgSidebar = Hex("#09121A"),
        BgCard = Hex("#12212C"),
        BgInput = Hex("#0A141C"),
        BgPopup = Hex("#12212C"),
        BgSelected = Hex("#16303D"),
        BgSelectedStrong = Hex("#1C3B4A"),
        BgStatus = Hex("#0F1D27"),
        BgToggleOff = Hex("#10202A"),

        LineFrame = Hex("#2E4A58"),
        LineControl = Hex("#2E4A58"),
        LineCard = Hex("#22394A"),
        LineSubtle = Hex("#1D3240"),
        LineRow = Hex("#1A2E3B"),
        LineDisabled = Hex("#1D3240"),

        TextHeading = Hex("#E6F2F4"),
        TextPrimary = Hex("#DCEBEE"),
        TextCardTitle = Hex("#BFE0E6"),
        TextValue = Hex("#DCEBEE"),
        TextSecondary = Hex("#9FC0C8"),
        TextTertiary = Hex("#9FC0C8"),
        TextDesc = Hex("#8FB0B8"),
        TextMuted = Hex("#7E9EA8"),
        TextDim = Hex("#7E9EA8"),
        TextDisabled = Hex("#5C7A82"),
        TextOnAccent = Hex("#04191C"),

        Accent = Hex("#46C2B8"),

        OkFg = Hex("#46C2B8"),
        OkBg = Hex("#12212C"),
        OkLine = Hex("#2E4A58"),
        WarnFg = Hex("#E0B35C"),
        WarnBg = Hex("#2A2416"),
        WarnLine = Hex("#4A3F26"),
        ErrFg = Hex("#E08A7A"),
        ErrBg = Hex("#2A1816"),
        ErrLine = Hex("#4A2826"),
        InfoFg = Hex("#9FC0C8"),
        InfoBg = Hex("#12212C"),
        InfoLine = Hex("#2E4A58"),
    };

    /// <summary>Die aktuell aktive Palette - von genau einem Plugin einmal beim Start gesetzt
    /// (z.B. <c>UiTheme.Active = UiTheme.Codex;</c>), danach von allen PluginUiKit-Widgets gelesen.
    /// Bewusst NACH <see cref="Codex"/> deklariert: statische Feld-Initialisierer laufen in
    /// Deklarationsreihenfolge, ein Verweis auf "Codex" vor dessen eigener Deklaration würde zur
    /// Initialisierungszeit noch null liefern.</summary>
    public static UiTheme Active { get; set; } = Codex;
}
