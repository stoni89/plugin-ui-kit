namespace PluginUiKit;

/// <summary>
/// Benannte Maße für PluginUiKit-Widgets - anders als <see cref="UiTheme"/> NICHT pro Plugin
/// austauschbar (nur Farbe/Logo/Verzierungen unterscheiden sich laut Vorgabe zwischen Plugins,
/// Abstände/Größen bleiben einheitlich). Alle Werte sind Basispixel OHNE Skalierung - jeder
/// Aufrufer multipliziert selbst mit ImGuiHelpers.GlobalScale (identisch zum bisherigen
/// "*scale"-Muster in TheExplorersCodex.CodexTheme).
///
/// WICHTIG - vorläufige Defaults: im Schritt-1-Bestandsaufnahme-Bericht gab es für mehrere dieser
/// Rollen mehrere leicht unterschiedliche Werte im bestehenden Code (siehe dortige "Flagged for
/// user decision"-Liste). Die Entscheidung dazu steht noch aus - die Werte unten sind der jeweils
/// im Bestandscode HÄUFIGSTE/dominante Wert als Arbeits-Default, klar als solcher markiert, und
/// lassen sich hier an einer Stelle ändern, sobald die Entscheidung gefallen ist.
/// </summary>
public static class UiMetrics
{
    // ---- Fensterform ----
    public const float SidebarWidth = 236f;
    public const float WindowRadius = 8f;
    public const float CardRadius = 6f;
    public const float OverlayRadius = 6f;

    /// <summary>Vorläufiger Default (siehe Klassenkommentar) - Bestandscode nutzte sowohl 3f (an
    /// den meisten Badge-/Chip-Stellen) als auch 3.5f (nur an der Nav-Item-Auswahl). Hier auf den
    /// häufigeren Wert (3f) vereinheitlicht.</summary>
    public const float ControlRadius = 3f;

    public const float GrabRadius = 999f;
    public const float BorderThickness = 1f;

    // ---- Karte (Card.Begin/End) ----
    public const float CardPadX = 16f;
    public const float CardPadY = 14f;

    // ---- Seiten-Layout ----
    /// <summary>Abstand Seiteninhalt zu Fensterrand (links UND rechts) - 8 von 9 Codex-Menüseiten
    /// nutzten exakt diesen Wert, eine Seite (About) weicht bewusst/asymmetrisch ab und bleibt
    /// Sache des jeweiligen Plugins.</summary>
    public const float PageMargin = 32f;

    public const float SidebarIndent = 18f;
    public const float SidebarSectionGap = 18f;

    /// <summary>Vorläufiger Default (siehe Klassenkommentar) - Bestandscode hatte 14/16/18px für
    /// denselben "Abstand zwischen gestapelten Karten"-Zweck, je nach Seite. 16px war der Wert auf
    /// den beiden einfachsten/grundlegendsten Seiten (General, Overlay).</summary>
    public const float SectionGap = 16f;

    public const float DropdownRightMargin = 25f;

    // ---- Steuerelemente ----
    public const float ToggleWidth = 42f;
    public const float ToggleHeight = 22f;
    public const float RowHeight = 30f;

    /// <summary>Vorläufiger Default (siehe Klassenkommentar) - 34px (Database/Blacklist) vs. 32px (Log).</summary>
    public const float TableHeaderHeight = 34f;

    // ---- Verzierungen (Standardmaße - CodexOrnaments nutzt diese 1:1, andere IUiOrnaments-Implementierungen dürfen eigene verwenden) ----
    public const float CornerLenOverlay = 10f;
    public const float CornerThickOverlay = 1.5f;
    public const float CornerInsetOverlay = 4f;
    public const float CornerLenMenu = 18f;
    public const float CornerThickMenu = 2f;
    public const float CornerInsetMenu = 6f;
    public const float DividerOrnamentWidth = 160f;

    // ---- Schriftgrößen-Rollen (Basis-px, siehe UiFonts) ----
    public const float PageTitleSize = 33f;
    public const float CardTitleSize = 22f;
    public const float SectionLabelSize = 17f;
    public const float SubtitleSize = 20f;
    public const float BodySize = 13f;
    public const float BodyMediumSize = 18f;
    public const float TitleBarSize = 16f;
}
