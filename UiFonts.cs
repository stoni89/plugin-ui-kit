using System;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PluginUiKit;

/// <summary>
/// Lädt die mitgelieferten Cinzel-/Alegreya-Sans-/Alegreya-Italic-Schriften über Dalamuds
/// Font-Atlas - 1:1 dasselbe Verfahren wie TheExplorersCodex.CodexTheme.BuildHandle, nur ohne
/// Bindung an EIN bestimmtes Plugin: <see cref="Initialize"/> muss von jedem Plugin, das dieses
/// Paket einbindet, einmal beim Start mit der eigenen PluginInterface aufgerufen werden.
///
/// Stellt zusätzlich ein paar Rollen-Standardgrößen (siehe UiMetrics.*Size) als fertige Handles
/// bereit - ein Plugin mit zusätzlichem, feinerem Bedarf (wie TheExplorersCodex mit ~70 Handles je
/// Seite/Element) baut eigene Handles einfach über das öffentliche <see cref="BuildHandle"/> on top,
/// im eigenen, plugin-spezifischen Theme-Erweiterungscode außerhalb dieses Pakets.
/// </summary>
public static class UiFonts
{
    private static IDalamudPluginInterface? pluginInterface;
    private static IPluginLog? log;

    /// <summary>Einmal beim Pluginstart aufrufen, bevor irgendein Font-Handle dieser Klasse verwendet wird.</summary>
    public static void Initialize(IDalamudPluginInterface pi, IPluginLog? logger = null)
    {
        pluginInterface = pi;
        log = logger;
    }

    private static IDalamudPluginInterface PluginInterface =>
        pluginInterface ?? throw new InvalidOperationException("UiFonts.Initialize(pluginInterface) wurde noch nicht aufgerufen.");

    private static IFontAtlas Atlas => PluginInterface.UiBuilder.FontAtlas;
    private static float ScaledPx(float px) => px * ImGuiHelpers.GlobalScale;

    private static string FontPath(string fileName) =>
        System.IO.Path.Combine(PluginInterface.AssemblyLocation.DirectoryName!, "Data", "Fonts", fileName);

    /// <summary>
    /// Baut ein Font-Handle aus einer der mitgelieferten .ttf-Dateien. mergeCjk=true merged
    /// zusätzlich Dalamuds eigene Noto-Sans-CJK-Schrift als Fallback für japanische Glyphen (für
    /// Handles, die rohe Spieldaten anzeigen können, die bei japanischem Client japanische Zeichen
    /// enthalten) - bewusst standardmäßig AUS, da jeder Merge beim Atlas-Bau Tausende CJK-Glyphen
    /// neu rastert (siehe TheExplorersCodex-Vorfall: >1 Minute Ladezeit bei ~70 Handles, die alle
    /// standardmäßig mergten).
    /// </summary>
    public static IFontHandle BuildHandle(string fileName, float sizePx, bool mergeCjk = false) =>
        Atlas.NewDelegateFontHandle(e => e.OnPreBuild(tk =>
        {
            var baseFont = tk.AddFontFromFile(FontPath(fileName), new SafeFontConfig { SizePx = ScaledPx(sizePx) });
            if (!mergeCjk)
                return;

            try
            {
                tk.AddDalamudAssetFont(Dalamud.DalamudAsset.NotoSansCjkRegular, new SafeFontConfig
                {
                    SizePx = ScaledPx(sizePx),
                    MergeFont = baseFont,
                });
            }
            catch (Exception ex)
            {
                log?.Warning(ex, $"[PluginUiKit.UiFonts] Japanisch-Fallback für {fileName} konnte nicht gemergt werden - Schrift bleibt ohne CJK-Fallback.");
            }
        }));

    // ---- Rollen-Standardgrößen (siehe UiMetrics) ----
    private static IFontHandle? pageTitle;
    public static IFontHandle PageTitle => pageTitle ??= BuildHandle("Cinzel-Bold.ttf", UiMetrics.PageTitleSize);

    private static IFontHandle? cardTitle;
    public static IFontHandle CardTitle => cardTitle ??= BuildHandle("Cinzel.ttf", UiMetrics.CardTitleSize);

    private static IFontHandle? sectionLabel;
    public static IFontHandle SectionLabel => sectionLabel ??= BuildHandle("Cinzel.ttf", UiMetrics.SectionLabelSize);

    private static IFontHandle? subtitle;
    public static IFontHandle Subtitle => subtitle ??= BuildHandle("AlegreyaItalic.ttf", UiMetrics.SubtitleSize);

    private static IFontHandle? body;
    public static IFontHandle Body => body ??= BuildHandle("AlegreyaSans-Regular.ttf", UiMetrics.BodySize);

    private static IFontHandle? bodyMedium;
    public static IFontHandle BodyMedium => bodyMedium ??= BuildHandle("AlegreyaSans-Medium.ttf", UiMetrics.BodyMediumSize);

    private static IFontHandle? titleBar;
    public static IFontHandle TitleBar => titleBar ??= BuildHandle("Cinzel-Bold.ttf", UiMetrics.TitleBarSize);

    private static IFontHandle? mono;

    /// <summary>Monospace - delegiert an Dalamuds eigenes UiBuilder.MonoFontHandle statt eine eigene
    /// Monospace-Schrift mitzuliefern (identischer pragmatischer Fallback wie in TheExplorersCodex.CodexTheme.FontMono12).</summary>
    public static IFontHandle Mono => mono ??= PluginInterface.UiBuilder.MonoFontHandle;

    /// <summary>FontAwesome-Icon-Schrift - delegiert an Dalamuds eigenes UiBuilder.IconFontHandle (für FontAwesomeIcon.X.ToIconString()-Glyphen wie ▸/✓, die kein regulärer Text-Font kennt).</summary>
    public static IFontHandle PluginInterfaceIconFont => PluginInterface.UiBuilder.IconFontHandle;

    /// <summary>Alle hier definierten Rollen-Handles einmal referenzieren, damit Dalamud sie beim
    /// Atlas-Bau mit erstellt statt erst beim ersten Zeichnen (vermeidet einen Ruckler beim ersten
    /// Öffnen des Menüs) - vom Plugin einmal nach Initialize() aufrufen.</summary>
    public static void Preload()
    {
        _ = PageTitle;
        _ = CardTitle;
        _ = SectionLabel;
        _ = Subtitle;
        _ = Body;
        _ = BodyMedium;
        _ = TitleBar;
    }
}
