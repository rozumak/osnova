using Jint;
using Osnova.JavaScript;
using Osnova.Markdown.CodeHighlight;

namespace Osnova.Prism;

internal class PrismCodeHighlighter : ICodeHighlighter
{
    private readonly JavaScriptEngineManager _jsEngineManager;
    private readonly ExecuteContext _executeContext = new();

    public PrismCodeHighlighter(JavaScriptEngineManager jsEngineManager)
    {
        _jsEngineManager = jsEngineManager;
    }

    public string Highlight(string code, string languageCode)
    {
        _executeContext.Code = code;
        _executeContext.LanguageCode = languageCode;

        var jsEngine = _jsEngineManager.GetEngine();
        jsEngine.Execute(_executeContext, (ctx, engine) =>
        {
            engine.SetValue("code", ctx.Code);
            engine.SetValue("lang", ctx.LanguageCode);

            engine.Execute(
                $"result = Prism.highlight(code, Prism.languages.{ctx.LanguageCode}, lang)");
            ctx.Result = engine.GetValue("result").AsString();
        });

        return _executeContext.Result;
    }

    private class ExecuteContext
    {
        public string Result { get; set; } = null!;

        public string Code { get; set; } = null!;

        public string LanguageCode { get; set; } = null!; 
    }
}