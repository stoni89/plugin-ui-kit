# PluginUiKit

Wiederverwendbares ImGui/Dalamud-UI-Paket, herausgelöst aus dem "Codex"-Theme von
**The Explorer's Codex** (dunkles Tinten-Braun mit Gold-Akzent). Enthält alle Farben, Schriften,
Maße und Zeichen-Bausteine, die dieses Theme ausmachen - aber ohne Bindung an ein bestimmtes
Plugin oder eine bestimmte Farbpalette.

## Architektur

- **`UiTheme`** - eine Farbpalette (Instanz, keine statische Klasse). Jedes Plugin erstellt seine
  eigene (`new UiTheme { BgWindow = ..., ... }`) oder nutzt die mitgelieferte `UiTheme.Codex`.
  Gesetzt wird sie einmal über `UiTheme.Active = meinePalette;` - alle Widgets in diesem Paket
  lesen ausschließlich über `UiTheme.Active`, nie über eine fest verdrahtete Palette.
- **`UiMetrics`** - Abstände/Größen als `const float`, plugin-übergreifend EINHEITLICH (nur Farbe/
  Logo/Verzierungen unterscheiden sich laut Vorgabe zwischen Plugins). Alle Werte sind Basis-px
  ohne Skalierung - jeder Aufrufer multipliziert selbst mit `ImGuiHelpers.GlobalScale`.
- **`UiFonts`** - lädt die mitgelieferten Cinzel-/Alegreya-Schriften über Dalamuds Font-Atlas.
  `UiFonts.Initialize(pluginInterface)` einmal beim Start aufrufen, danach `UiFonts.Preload()`
  optional (vermeidet einen Ruckler beim ersten Öffnen des Menüs). Zusätzliche, feinere
  Rollen-Schriften (wie die ~70 Handles in TheExplorersCodex) baut jedes Plugin selbst über das
  öffentliche `UiFonts.BuildHandle(datei, px, mergeCjk)` - das Paket selbst stellt nur ein paar
  grobe Standardrollen bereit (`PageTitle`, `CardTitle`, `SectionLabel`, `Subtitle`, `Body`,
  `BodyMedium`, `TitleBar`, `Mono`, `PluginInterfaceIconFont`).
- **`IUiOrnaments`** - austauschbare Verzierungen (Eck-Verzierung, Trenn-Ornament,
  Seitenleisten-Bildmarke). `CodexOrnaments` ist die mitgelieferte Codex-Variante (goldene
  L-Striche, Linie-Raute-Linie, Kompass). Ein zweites Plugin schreibt seine eigene Implementierung
  für seinen eigenen Look.
- **`UiWidgets`** / **`UiWidgetsExtra`** / **`UiNav`** - die eigentlichen Bausteine (Karte,
  Schalter, Zeilen-Layouts für Einstellungszeilen/Dropdowns, Badge, Fortschrittsbalken,
  Tastenkürzel-Box, fließende Knopfreihe, zentrierter/umbrochener Text, Trennlinie mit
  Beschriftung, Tabellenkopf-Zelle, Titelleisten-Icon-Knopf, Akzent-Knopf, Seitenleisten-
  Menüpunkt, Seitenleisten-Statuskarte, Tooltip). Kein Baustein hat eigene Farb-/Maß-Literale -
  alles kommt aus `UiTheme.Active`/`UiMetrics`.

Mehrere Widgets (`ToggleRow`, `BeginDropdownRow`, `CardGroupLabel`, `ShortcutHint`,
`FlowButtons`) nehmen optionale `IFontHandle`-Parameter entgegen, die den sonst genutzten
Kit-Standardfont überschreiben - wichtig, falls ein Plugin (wie TheExplorersCodex) für dieselbe
Rolle eine eigene, abweichende Schriftgröße pflegt. Ohne Angabe wird der Kit-Standard aus
`UiFonts` verwendet.

## Farb-Tokens (`UiTheme`)

Werte der mitgelieferten `UiTheme.Codex`-Palette - eine eigene Palette setzt hier eigene
Hex-Werte, die Rollen (was wofür verwendet wird) bleiben gleich.

| Token | Codex-Wert | Verwendung |
|---|---|---|
| `BgWindow` | `#1A150F` | Fenster-Hintergrund |
| `BgSidebar` | `#130F0A` | Seitenleisten-Hintergrund |
| `BgCard` | `#221C14` | Karten-Füllung |
| `BgInput` | `#17120C` | Eingabefelder, Scrollbar-Bahn |
| `BgPopup` | `#1D1812` | Popup-/Dropdown-/Tooltip-Hintergrund |
| `BgSelected` | `#2E2516` | Hover-/Auswahl-Füllung |
| `BgSelectedStrong` | `#3A2F1E` | Aktiv-/Gedrückt-Füllung |
| `LineFrame` | `#6A5638` | Rahmen (Zierecken, Tastenkürzel-Box, Scrollbar-Griff) |
| `LineControl` | `#5A4A33` | Rahmen auf Schaltern/Knöpfen/Dropdowns |
| `LineCard` | `#43372A` | Kartenrahmen, Tabellenrahmen |
| `LineSubtle` | `#362C1F` | Feine Trennlinien innerhalb einer Karte |
| `LineRow` | `#2A2219` | Zeilentrenner (Tabellen) |
| `LineDisabled` | `#3E3324` | Rahmen im deaktivierten Zustand |
| `TextHeading` | `#F6E9C8` | Überschriften |
| `TextPrimary` | `#F1E6CF` | Primärer Fließtext |
| `TextCardTitle` | `#E9D7AE` | Kartentitel |
| `TextValue` | `#E2D3B2` | Werte/Daten-Text |
| `TextSecondary` | `#C2B396` | Sekundärer Text |
| `TextTertiary` | `#B8A88A` | Abschnitts-Label, Tabellenkopf |
| `TextMuted` | `#A8987A` | Gedämpfter Text (Beschreibungen) |
| `TextDim` | `#8A7B62` | Sehr gedämpfter Text (Meta-Angaben) |
| `TextDisabled` | `#6E604A` | Text im deaktivierten Zustand |
| `TextOnAccent` | `#1A1208` | Text auf einer Akzentfläche (z.B. Toggle-Knopf an) |
| `Accent` | `#D4A94F` | Die Akzentfarbe selbst (Gold) |
| `OkFg`/`OkBg`/`OkLine` | `#9BD3A2`/`#1C2A1D`/`#35513A` | Erfolg-Semantik |
| `WarnFg`/`WarnBg`/`WarnLine` | `#E9C46A`/`#30271A`/`#5C4A26` | Warnung-Semantik |
| `ErrFg`/`ErrBg`/`ErrLine` | `#EE9A86`/`#331C17`/`#5A2E26` | Fehler-Semantik |
| `InfoFg`/`InfoBg`/`InfoLine` | `#9CC4E4`/`#18242E`/`#2C4458` | Info-Semantik |

## Maße (`UiMetrics`)

Alle Werte in px, ohne `GlobalScale`-Multiplikation (die macht jeder Aufrufer selbst).

| Konstante | Wert | Verwendung |
|---|---|---|
| `SidebarWidth` | 236 | Breite der Seitenleiste |
| `WindowRadius` | 8 | Fenster-Rundung |
| `CardRadius` | 6 | Karten-/Reihen-Rundung |
| `OverlayRadius` | 6 | Overlay-Fenster-Rundung |
| `ControlRadius` | 3 | Knopf-/Badge-/Chip-Rundung *(vorläufiger Default, siehe unten)* |
| `GrabRadius` | 999 | Pillenform (Scrollbar-/Slider-Griff) |
| `BorderThickness` | 1 | Standard-Rahmenstärke |
| `CardPadX` / `CardPadY` | 16 / 14 | Karten-Innenabstand |
| `PageMargin` | 32 | Seiteninhalt zu Fensterrand (links+rechts) |
| `SidebarIndent` | 18 | Seitenleisten-Einzug |
| `SidebarSectionGap` | 18 | Abstand zwischen Seitenleisten-Gruppen |
| `SectionGap` | 16 | Abstand zwischen gestapelten Karten *(vorläufiger Default)* |
| `DropdownRightMargin` | 25 | Rechter Randabstand von Dropdowns/Schaltern in einer Zeile |
| `ToggleWidth` / `ToggleHeight` | 42 / 22 | Schalter-Größe |
| `RowHeight` | 30 | Seitenleisten-Menüpunkt-Höhe |
| `TableHeaderHeight` | 34 | Tabellenkopf-Höhe *(vorläufiger Default)* |
| `CornerLenMenu` / `ThickMenu` / `InsetMenu` | 18 / 2 / 6 | Zierecken, Menü-Fenster |
| `CornerLenOverlay` / `ThickOverlay` / `InsetOverlay` | 10 / 1.5 / 4 | Zierecken, Overlay-Fenster |
| `DividerOrnamentWidth` | 160 | Breite des Trenn-Ornaments unter Seitentiteln |
| `PageTitleSize` | 33 | Schriftgröße: Seitentitel |
| `CardTitleSize` | 22 | Schriftgröße: Kartentitel |
| `SectionLabelSize` | 17 | Schriftgröße: Abschnitts-Label |
| `SubtitleSize` | 20 | Schriftgröße: Untertitel |
| `BodySize` | 13 | Schriftgröße: Fließtext |
| `BodyMediumSize` | 18 | Schriftgröße: Nav-/Zeilen-Label |
| `TitleBarSize` | 16 | Schriftgröße: Titelleiste |

**Vorläufige Defaults:** `ControlRadius`, `SectionGap` und `TableHeaderHeight` sind als
"vorläufiger Default" markiert - im Bestandscode von TheExplorersCodex gab es für dieselbe Rolle
an verschiedenen Stellen leicht unterschiedliche Werte (siehe "Offene
Vereinheitlichungs-Entscheidungen" unten). TheExplorersCodex selbst nutzt an den betroffenen
Stellen weiterhin seine Originalwerte als eigene Literale statt dieser Kit-Defaults (siehe dessen
`CodexTheme.cs`-Klassenkommentar) - die Vereinheitlichung ist eine separate, noch offene
Entscheidung und betrifft nur NEUE Plugins, die den Kit-Default direkt übernehmen.

## Beispiel: Fenster mit Titelleiste, Seitenleiste und einer Seite

```csharp
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using PluginUiKit;

public class MyPluginWindow : Window
{
    private readonly string[] pages = { "Allgemein", "Einstellungen" };
    private int activePage;

    public MyPluginWindow() : base("##MyPluginWindow", ImGuiWindowFlags.NoTitleBar)
    {
        Size = new System.Numerics.Vector2(700f, 500f);
    }

    public override void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;

        // Titelleiste: Icon + Name + Einklappen/Schließen
        if (UiNav.IconButton(FontAwesomeIcon.Times, "##Close", new(17f * scale, 17f * scale), UiFonts.PluginInterfaceIconFont, "Schließen"))
            IsOpen = false;

        ImGui.SameLine(0f, 0f);

        // Seitenleiste
        ImGui.BeginChild("##Sidebar", new(UiMetrics.SidebarWidth * scale, 0f));
        ImGui.Indent(UiMetrics.SidebarIndent * scale);
        for (var i = 0; i < pages.Length; i++)
        {
            if (UiNav.NavItem($"##Nav{i}", FontAwesomeIcon.Gear, pages[i], activePage == i, scale,
                    UiFonts.PluginInterfaceIconFont, UiFonts.BodyMedium))
                activePage = i;
        }
        UiNav.SidebarStatusCard("Bereit", "v1.0.0", scale, UiFonts.BodyMedium, UiFonts.Body);
        ImGui.Unindent(UiMetrics.SidebarIndent * scale);
        ImGui.EndChild();

        ImGui.SameLine(0f, 0f);

        // Seiteninhalt
        ImGui.BeginChild("##Content");
        ImGui.Indent(UiMetrics.PageMargin * scale);
        using (UiFonts.PageTitle.Push())
            ImGui.TextColored(UiTheme.Active.TextHeading, pages[activePage]);
        UiWidgets.DrawDividerOrnament(myOrnaments, scale);

        UiWidgets.BeginCard(scale, rightMargin: UiMetrics.PageMargin * scale);
        ImGui.TextColored(UiTheme.Active.TextPrimary, "Kartenhalt hier.");
        UiWidgets.EndCard();

        ImGui.Unindent(UiMetrics.PageMargin * scale);
        ImGui.EndChild();

        UiWidgets.DrawWindowCorners(myOrnaments, 6f * scale);
    }
}
```

## Eigene Palette und eigene Verzierungen übergeben

**Eigene Palette:** eine neue `UiTheme`-Instanz mit den eigenen Hex-Werten erstellen und einmal
beim Plugin-Start aktivieren:

```csharp
private static readonly UiTheme MyTheme = new()
{
    BgWindow = new Vector4(0.05f, 0.08f, 0.10f, 1f), // oder über denselben Hex()-Helfer wie UiTheme.Codex
    Accent = new Vector4(0.3f, 0.7f, 0.9f, 1f),
    // ... alle übrigen Token siehe Tabelle oben
};

// Im Plugin-Konstruktor:
UiTheme.Active = MyTheme;
```

Ab da liefern alle `UiWidgets`/`UiWidgetsExtra`/`UiNav`-Aufrufe automatisch die neue Palette -
kein Widget-Aufruf muss sich ändern.

**Eigene Verzierungen:** `IUiOrnaments` selbst implementieren (drei Methoden: `DrawCorner`,
`DrawDivider`, `DrawSidebarLogo`) statt `CodexOrnaments` zu verwenden:

```csharp
public sealed class MyOrnaments : IUiOrnaments
{
    public void DrawCorner(Vector2 corner, int sx, int sy) { /* eigene Eckenzeichnung */ }
    public void DrawDivider(Vector2 pos, float width) { /* eigene Trennlinie */ }
    public void DrawSidebarLogo(float size) { /* eigenes Logo-Symbol */ }
}
```

Eine Instanz davon wird dann überall dort übergeben, wo bisher `CodexOrnaments` (bzw. eine
`MenuOrnaments`/`OverlayOrnaments`-Instanz davon) stand - z.B. `UiWidgets.DrawWindowCorners(myOrnaments, inset)`.
Farbe/Größe der Verzierung liest `CodexOrnaments` selbst aus `UiTheme.Active.Accent` bzw. seinem
eigenen Konstruktor (`cornerLength`/`cornerThickness`) - eine eigene `IUiOrnaments`-Implementierung
ist darin völlig frei.

## Einbindung in ein zweites Plugin (z.B. Big Fish Helper)

**Empfehlung: eigenes Git-Repo + einfache `<ProjectReference>` über einen relativen Pfad**, NICHT
Git-Submodule, NICHT NuGet.

Begründung (geringster Pflegeaufwand für diesen Workflow): beide Plugins liegen bereits als
Geschwisterordner unter `C:\FF14Mods\` (`TheExplorersCodex`, `BigFishHelper`, `PluginUiKit`). Eine
`ProjectReference` ist nur ein Dateipfad - sie braucht keine gemeinsame Repo-Struktur und keinen
Veröffentlichungsschritt. Eine Änderung in PluginUiKit wirkt sich beim nächsten `dotnet build`
eines der beiden Plugins sofort aus, ohne Commit/Push/Versions-Bump/Paket-Republish dazwischen -
angesichts der sehr kleinteiligen, häufigen Iterationsweise in diesem Projekt (viele einzelne
Pixel-/Farbanpassungen pro Sitzung) wäre jeder zusätzliche Schritt pro Änderung hier reine
Reibung. PluginUiKit bleibt trotzdem ein eigenes Git-Repo (eigene Historie/Sicherung, unabhängig
versionierbar), nur der *Konsum* läuft über den Dateipfad statt über Submodule/Paket-Feed.

Nachteil, bewusst in Kauf genommen: wer `BigFishHelper` allein (ohne `PluginUiKit` als
Geschwisterordner) auschecken würde, kann es nicht bauen. Da beide Plugins von derselben Person
gepflegt werden, ist das ein geringer Preis gegenüber dem Reibungsverlust von Submodule/NuGet.
Falls das Projekt später für andere auschecker reproduzierbar sein muss, lässt sich jederzeit
NACHTRÄGLICH ein Git-Submodule-Link ergänzen, ohne den Code selbst anzufassen.

In der `.csproj` des zweiten Plugins:

```xml
<ItemGroup>
  <ProjectReference Include="..\PluginUiKit\PluginUiKit.csproj" />
</ItemGroup>
```

Im Plugin-Konstruktor (vor dem ersten Fenster-`Draw()`):

```csharp
UiFonts.Initialize(PluginInterface, Log);
UiFonts.Preload();
UiTheme.Active = UiTheme.Codex; // oder eine eigene Palette, siehe oben
```

## Was bewusst NICHT Teil dieses Pakets ist

- Seitenspezifische/einmalige Farben (z.B. Log-Level-Farben, Sammelobjekt-Typ-Etiketten) - bleiben
  im jeweiligen Plugin als eigene Konstanten neben der `UiTheme`-Instanz.
- Feine, seitenspezifische Schriftgrößen (wie die ~70 Handles in TheExplorersCodex) - jedes Plugin
  baut die über `UiFonts.BuildHandle` selbst.
- Seiten-/Menüverwaltung (welche Seiten es gibt, welche aktiv ist) - das bleibt im jeweiligen
  Plugin; `UiNav.NavItem`/`UiNav.SidebarStatusCard` sind nur die Zeichen-Bausteine dafür.

## Offene Vereinheitlichungs-Entscheidungen

Mehrere Werte in `UiMetrics` sind als "vorläufiger Default" markiert (siehe Kommentare dort und
Tabelle oben) - im Bestandscode von TheExplorersCodex gab es für dieselbe Rolle an verschiedenen
Stellen leicht unterschiedliche Werte:

- **Badge-/Chip-Rundung**: 3px (die meisten Stellen) vs. 3.5px (nur Nav-Item-Auswahl) - Kit-Default: 3px.
- **Abstand zwischen gestapelten Karten**: 14/16/18px je nach Seite - Kit-Default: 16px.
- **Tabellenkopf-Höhe**: 32px (Log-Seite) vs. 34px (Database/Blacklist) - Kit-Default: 34px.

Diese Entscheidungen stehen noch aus und betreffen nur NEUE Plugins, die den jeweiligen
Kit-Default direkt übernehmen - TheExplorersCodex selbst behält an diesen Stellen seine
Originalwerte (siehe `CodexTheme.cs`).
