using System.Reflection;
using Osnova.JavaScript;
using Osnova.Markdown.CodeHighlight;

namespace Osnova.Prism;

public class PrismCodeHighlighterProvider : ICodeHighlighterProvider
{
    private readonly JavaScriptEngineManager _jsEngineManager;
    private ICodeHighlighter? _cachedCodeHighlighter;

    /// <summary>
    /// TODO: add options to set other prism script
    /// </summary>
    public PrismCodeHighlighterProvider()
    {
        _jsEngineManager = new JavaScriptEngineManager(engine =>
        {
            string? script = ReadPrismScript("prism.js");
            if (script == null)
                throw new Exception($"Cant find 'prism.js' in embedded resources.");

            engine.Execute(script);
        });
    }

    public ICodeHighlighter GetCodeHighlighter()
    {
        _cachedCodeHighlighter ??= new PrismCodeHighlighter(_jsEngineManager, ReadPrismScript);
        return _cachedCodeHighlighter;
    }

    private string? ReadPrismScript(string script)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Osnova.Prism.{script}";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        return null;
    }
}