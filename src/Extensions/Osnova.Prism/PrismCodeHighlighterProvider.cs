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
            string script = ReadPrismScript();
            engine.Execute(script);
        });
    }

    public ICodeHighlighter GetCodeHighlighter()
    {
        _cachedCodeHighlighter ??= new PrismCodeHighlighter(_jsEngineManager);
        return _cachedCodeHighlighter;
    }

    private string ReadPrismScript()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Osnova.Prism.prism.js";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        throw new Exception("Cant find 'prism.js' in embedded resources.");
    }
}