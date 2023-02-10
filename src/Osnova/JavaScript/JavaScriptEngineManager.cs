using Jint;

namespace Osnova.JavaScript;

public class JavaScriptEngineManager : IDisposable
{
    private readonly Action<Engine> _initEngine;
    private readonly Lazy<JsEngine> _lazyEngine;

    public JavaScriptEngineManager(Action<Engine> initialize)
    {
        _initEngine = initialize;
        _lazyEngine = new Lazy<JsEngine>(CreateEngine);
    }

    public IJsEngine GetEngine()
    {
        return _lazyEngine.Value;
    }

    public void Dispose()
    {
        if (_lazyEngine.IsValueCreated)
            _lazyEngine.Value.Dispose();
    }

    private JsEngine CreateEngine()
    {
        Engine engine = new Engine();
        _initEngine(engine);

        return new JsEngine(engine);
    }

    private class JsEngine : IJsEngine, IDisposable
    {
        private readonly Engine _engine;

        public JsEngine(Engine engine)
        {
            _engine = engine;
        }

        public void Execute<T>(T context, Action<T,Engine> action)
        {
            lock (this)
            {
                action(context, _engine);
            }
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}