using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Osnova.Markdown.FrontMatterExtractors;

public class YamlFrontMatterExtractor : IFrontMatterExtractor
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.UseYamlFrontMatter();
    }

    public bool Extract<T>(MarkdownDocument document, out T result)
    {
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

        if (yamlBlock != null)
        {
            var yaml = yamlBlock.Lines.ToString();

            // Deserialize the yaml block into a custom type
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var frontMatterModel = deserializer.Deserialize<T>(yaml);
            result = frontMatterModel;
            return true;
        }

        result = default;
        return false;
    }
}