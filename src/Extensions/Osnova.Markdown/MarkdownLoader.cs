using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Osnova.Markdown;

public class MarkdownLoader
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownLoader()
    {
        //TODO: extract this to be configured outside of ctor
        _pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            //.UseHighlightCode(new PrismCodeHighlighterProvider())
            .Build();
    }

    public async Task<MarkdownWithFrontMatter<T>> LoadMarkdownPage<T>(string fileName)
    {
        string markdown = await File.ReadAllTextAsync(fileName);

        var document = Markdig.Markdown.Parse(markdown, _pipeline);

        // extract the front matter from markdown document
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        T? frontMatterModel = default;

        if (yamlBlock != null)
        {
            var yaml = yamlBlock.Lines.ToString();

            // deserialize the yaml block into a custom type
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            frontMatterModel = deserializer.Deserialize<T>(yaml);
        }

        //TODO: make file name relative
        var content = new MarkdownContent(_pipeline, document, fileName);

        return new MarkdownWithFrontMatter<T>(content, frontMatterModel);
    }
}