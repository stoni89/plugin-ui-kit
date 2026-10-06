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
UiTheme.Active = UiTheme.Codex; // oder eine eigene Palette
```

## Was bewusst NICHT Teil dieses Pakets ist

- Seitenspezifische/einmalige Farben (z.B. Log-Level-Farben, Sammelobjekt-Typ-Etiketten) - bleiben
  im jeweiligen Plugin als eigene Konstanten neben der `UiTheme`-Instanz.
- Feine, seitenspezifische Schriftgrößen (wie die ~70 Handles in TheExplorersCodex) - jedes Plugin
  baut die über `UiFonts.BuildHandle` selbst.
- Seiten-/Menüverwaltung (welche Seiten es gibt, welche aktiv ist) - das bleibt im jeweiligen
  Plugin; `UiNav.NavItem`/`UiNav.SidebarStatusCard` sind nur die Zeichen-Bausteine dafür.

## Offene Vereinheitlichungs-Entscheidungen

Mehrere Werte in `UiMetrics` sind als "vorläufiger Default" markiert (siehe Kommentare dort) - im
Bestandscode von TheExplorersCodex gab es für dieselbe Rolle an verschiedenen Stellen leicht
unterschiedliche Werte (z.B. Karten-Abstand 14/16/18px, Tabellenkopf-Höhe 32/34px, Badge-Rundung
3f/3.5f). Diese Entscheidungen stehen noch aus.
