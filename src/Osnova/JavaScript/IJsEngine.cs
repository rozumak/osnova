using Jint;

namespace Osnova.JavaScript;

public interface IJsEngine
{
    void Execute<T>(T context, Action<T, Engine> action);
}