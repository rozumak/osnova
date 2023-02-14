using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    internal class Build
    {
        private const string Clean = "clean";
        private const string Restore = "restore";
        private const string Compile = "compile";
        private const string Test = "test";
        private const string Pack = "pack";
        private const string Publish = "publish";

        private static void Main(string[] args)
        {
            const string solutionName = "Osnova.sln";

            Target("default", DependsOn(Compile));

            Target(Clean, () =>
            {
                EnsureDirectoriesDeleted("./artifacts");
                Run("dotnet", $"clean {solutionName}");
            });

            Target(Restore, () =>
            {
                Run("dotnet", $"restore {solutionName}");
            });

            Target(Compile, DependsOn(Restore), () =>
            {
                Run("dotnet",
                    $"build {solutionName} --no-restore --framework net7.0");
            });

            Target("ci", DependsOn("default"));

            var nugetProjects = new string[]
            {
                "./src/Osnova",
                "./src/extensions/Osnova.Markdown",
                "./src/extensions/Osnova.Prism"
            };

            Target(Pack, DependsOn(Compile), ForEach(nugetProjects), project =>
                Run("dotnet", $"pack {project} -o ./artifacts --configuration Release"));

            RunTargetsAndExit(args);
        }

        private static void EnsureDirectoriesDeleted(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    DeleteDirectory(dir);
                }
            }
        }

        private static void DeleteDirectory(DirectoryInfo baseDir)
        {
            baseDir.Attributes = FileAttributes.Normal;
            foreach (var childDir in baseDir.GetDirectories())
                DeleteDirectory(childDir);

            foreach (var file in baseDir.GetFiles())
                file.IsReadOnly = false;

            baseDir.Delete(true);
        }

        private static Action IgnoreIfFailed(Action action)
        {
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            };
        }
    }
}
