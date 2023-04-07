using Jint;
using Jint.Native;
using Osnova.JavaScript;
using Osnova.Markdown.CodeHighlight;

namespace Osnova.Prism;

internal class PrismCodeHighlighter : ICodeHighlighter
{
    private static readonly Dictionary<string, string> s_langAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {"html", "markup"},
            {"xml", "markup"},
            {"svg", "markup"},
            {"mathml", "markup"},
            {"ssml", "markup"},
            {"atom", "markup"},
            {"rss", "markup"},
            {"js", "javascript"},
            {"g4", "antlr4"},
            {"ino", "arduino"},
            {"arm-asm", "armasm"},
            {"art", "arturo"},
            {"adoc", "asciidoc"},
            {"avs", "avisynth"},
            {"avdl", "avro-idl"},
            {"gawk", "awk"},
            {"sh", "bash"},
            {"shell", "bash"},
            {"shortcode", "bbcode"},
            {"rbnf", "bnf"},
            {"oscript", "bsl"},
            {"cs", "csharp"},
            {"dotnet", "csharp"},
            {"cfc", "cfscript"},
            {"cilk-c", "cilkc"},
            {"cilk-cpp", "cilkcpp"},
            {"cilk", "cilkcpp"},
            {"coffee", "coffeescript"},
            {"conc", "concurnas"},
            {"jinja2", "django"},
            {"dns-zone", "dns-zone-file"},
            {"dockerfile", "docker"},
            {"gv", "dot"},
            {"eta", "ejs"},
            {"xlsx", "excel-formula"},
            {"xls", "excel-formula"},
            {"gamemakerlanguage", "gml"},
            {"po", "gettext"},
            {"gni", "gn"},
            {"ld", "linker-script"},
            {"go-mod", "go-module"},
            {"hbs", "handlebars"},
            {"mustache", "handlebars"},
            {"hs", "haskell"},
            {"idr", "idris"},
            {"gitignore", "ignore"},
            {"hgignore", "ignore"},
            {"npmignore", "ignore"},
            {"webmanifest", "json"},
            {"kt", "kotlin"},
            {"kts", "kotlin"},
            {"kum", "kumir"},
            {"tex", "latex"},
            {"context", "latex"},
            {"ly", "lilypond"},
            {"emacs", "lisp"},
            {"elisp", "lisp"},
            {"emacs-lisp", "lisp"},
            {"md", "markdown"},
            {"moon", "moonscript"},
            {"n4jsd", "n4js"},
            {"nani", "naniscript"},
            {"objc", "objectivec"},
            {"qasm", "openqasm"},
            {"objectpascal", "pascal"},
            {"px", "pcaxis"},
            {"pcode", "peoplecode"},
            {"plantuml", "plant-uml"},
            {"pq", "powerquery"},
            {"mscript", "powerquery"},
            {"pbfasm", "purebasic"},
            {"purs", "purescript"},
            {"py", "python"},
            {"qs", "qsharp"},
            {"rkt", "racket"},
            {"razor", "cshtml"},
            {"rpy", "renpy"},
            {"res", "rescript"},
            {"robot", "robotframework"},
            {"rb", "ruby"},
            {"sh-session", "shell-session"},
            {"shellsession", "shell-session"},
            {"smlnj", "sml"},
            {"sol", "solidity"},
            {"sln", "solution-file"},
            {"rq", "sparql"},
            {"sclang", "supercollider"},
            {"t4", "t4-cs"},
            {"trickle", "tremor"},
            {"troy", "tremor"},
            {"trig", "turtle"},
            {"ts", "typescript"},
            {"tsconfig", "typoscript"},
            {"uscript", "unrealscript"},
            {"uc", "unrealscript"},
            {"url", "uri"},
            {"vb", "visual-basic"},
            {"vba", "visual-basic"},
            {"webidl", "web-idl"},
            {"mathematica", "wolfram"},
            {"nb", "wolfram"},
            {"wl", "wolfram"},
            {"xeoracube", "xeora"},
            {"yml", "yaml"}
        };

    private static readonly Dictionary<string, object> s_langDependencies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {"javascript", "clike"},
            {"actionscript", "javascript"},
            {"apex", new List<string> {"clike", "sql"}},
            {"arduino", "cpp"},
            {"aspnet", new List<string> {"markup", "csharp"}},
            {"birb", "clike"},
            {"bison", "c"},
            {"c", "clike"},
            {"csharp", "clike"},
            {"cpp", "c"},
            {"cfscript", "clike"},
            {"chaiscript", new List<string> {"clike", "cpp"}},
            {"cilkc", "c"},
            {"cilkcpp", "cpp"},
            {"coffeescript", "javascript"},
            {"crystal", "ruby"},
            {"css-extras", "css"},
            {"d", "clike"},
            {"dart", "clike"},
            {"django", "markup-templating"},
            {"ejs", new List<string> {"javascript", "markup-templating"}},
            {"etlua", new List<string> {"lua", "markup-templating"}},
            {"erb", new List<string> {"ruby", "markup-templating"}},
            {"fsharp", "clike"},
            {"firestore-security-rules", "clike"},
            {"flow", "javascript"},
            {"ftl", "markup-templating"},
            {"gml", "clike"},
            {"glsl", "c"},
            {"go", "clike"},
            {"gradle", "clike"},
            {"groovy", "clike"},
            {"haml", "ruby"},
            {"handlebars", "markup-templating"},
            {"haxe", "clike"},
            {"hlsl", "c"},
            {"idris", "haskell"},
            {"java", "clike"},
            {"javadoc", new List<string> {"markup", "java", "javadoclike"}},
            {"jolie", "clike"},
            {"jsdoc", new List<string> {"javascript", "javadoclike", "typescript"}},
            {"js-extras", "javascript"},
            {"json5", "json"},
            {"jsonp", "json"},
            {"js-templates", "javascript"},
            {"kotlin", "clike"},
            {"latte", new List<string> {"clike", "markup-templating", "php"}},
            {"less", "css"},
            {"lilypond", "scheme"},
            {"liquid", "markup-templating"},
            {"markdown", "markup"},
            {"markup-templating", "markup"},
            {"mongodb", "javascript"},
            {"n4js", "javascript"},
            {"objectivec", "c"},
            {"opencl", "c"},
            {"parser", "markup"},
            {"php", "markup-templating"},
            {"phpdoc", new List<string> {"php", "javadoclike"}},
            {"php-extras", "php"},
            {"plsql", "sql"},
            {"processing", "clike"},
            {"protobuf", "clike"},
            {"pug", new List<string> {"markup", "javascript"}},
            {"purebasic", "clike"},
            {"purescript", "haskell"},
            {"qsharp", "clike"},
            {"qml", "javascript"},
            {"qore", "clike"},
            {"racket", "scheme"},
            {"cshtml", new List<string> {"markup", "csharp"}},
            {"jsx", new List<string> {"markup", "javascript"}},
            {"tsx", new List<string> {"jsx", "typescript"}},
            {"reason", "clike"},
            {"ruby", "clike"},
            {"sass", "css"},
            {"scss", "css"},
            {"scala", "java"},
            {"shell-session", "bash"},
            {"smarty", "markup-templating"},
            {"solidity", "clike"},
            {"soy", "markup-templating"},
            {"sparql", "turtle"},
            {"sqf", "clike"},
            {"squirrel", "clike"},
            {"stata", new List<string> {"mata", "java", "python"}},
            {"t4-cs", new List<string> {"t4-templating", "csharp"}},
            {"t4-vb", new List<string> {"t4-templating", "vbnet"}},
            {"tap", "yaml"},
            {"tt2", new List<string> {"clike", "markup-templating"}},
            {"textile", "markup"},
            {"twig", "markup-templating"},
            {"typescript", "javascript"},
            {"v", "clike"},
            {"vala", "clike"},
            {"vbnet", "basic"},
            {"velocity", "markup"},
            {"wiki", "markup"},
            {"xeora", "markup"},
            {"xml-doc", "markup"},
            {"xquery", "markup"}
        };

    private readonly JavaScriptEngineManager _jsEngineManager;
    private readonly Func<string, string?> _scriptReader;

    private readonly ExecuteContext _executeContext = new();

    public PrismCodeHighlighter(JavaScriptEngineManager jsEngineManager, 
        Func<string, string?> scriptReader)
    {
        _jsEngineManager = jsEngineManager;
        _scriptReader = scriptReader;
    }

    public string Highlight(string code, string languageCode)
    {
        _executeContext.Code = code;
        _executeContext.LanguageCode = languageCode;

        var jsEngine = _jsEngineManager.GetEngine();
        jsEngine.Execute(_executeContext, Execute);

        return _executeContext.Result;
    }

    private void Execute(ExecuteContext ctx, Engine engine)
    {
        string languageCode = ctx.LanguageCode;
        if (s_langAliases.TryGetValue(languageCode, out string? lang))
        {
            languageCode = lang;
        }

        LoadLanguage(engine, languageCode);

        engine.SetValue("code", ctx.Code);
        engine.SetValue("lang", languageCode);
        engine.Execute($"prismLang = Prism.languages['{languageCode}']");

        engine.Execute("result = Prism.highlight(code, prismLang, lang)");

        ctx.Result = engine.GetValue("result").AsString();
    }

    private void LoadLanguage(Engine engine, string languageCode)
    {
        // Check if language loaded by calling js and checking if prop is defined
        var targetLangJsObject = engine
            .Execute($"targetLang = Prism.languages['{languageCode}']")
            .GetValue("targetLang");

        if (targetLangJsObject is JsUndefined)
        {
            // Load lang deps
            if (s_langDependencies.TryGetValue(languageCode, out var deps))
            {
                if (deps is string depsLang)
                {
                    LoadLanguage(engine, depsLang);
                }
                else if (deps is List<string> depsLangs)
                {
                    foreach (var lang in depsLangs)
                    {
                        LoadLanguage(engine, lang);
                    }
                }
            }

            // Do actual loading of required lang
            string? script = _scriptReader.Invoke($"languages.prism-{languageCode}.min.js");
            if (script == null)
            {
                throw new UnsupportedPrismLanguageException($"Unknown or unsupported language code '{languageCode}'");
            }

            engine.Execute(script);
        }
    }

    private class ExecuteContext
    {
        public string Result { get; set; } = null!;

        public string Code { get; set; } = null!;

        public string LanguageCode { get; set; } = null!; 
    }
}