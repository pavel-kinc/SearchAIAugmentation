using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using PromptEnhancer.AIUtility.ChatHistory;
using PromptEnhancer.CustomJsonResolver;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;
using PromptEnhancer.SK.Interfaces;
using System.Collections.Concurrent;
using System.Text;

namespace PromptEnhancer.Services.EnhancerService
{
    public class EnhancerService : IEnhancerService
    {
        private readonly ISemanticKernelManager _semanticKernelManager;
        private readonly IPipelineOrchestrator _pipelineOrchestrator;
        private readonly Kernel? _kernel;
        private readonly IServiceProvider _serviceProvider;

        public EnhancerService(ISemanticKernelManager semanticKernelManager, IPipelineOrchestrator pipelineOrchestrator, IServiceProvider serviceProvider, Kernel? kernel = null)
        {
            _semanticKernelManager = semanticKernelManager;
            _pipelineOrchestrator = pipelineOrchestrator;
            _kernel = kernel;
            _serviceProvider = serviceProvider;
        }

        // supports single completion and embedding
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string? embeddingModel = "text-embedding-3-small")
        {
            var enhancerConfiguration = new EnhancerConfiguration();
            enhancerConfiguration.KernelConfiguration = new KernelConfiguration
            {
                AIApiKey = aiApiKey,
                Model = aiModel,
                Provider = aiProvider,
                EmbeddingModel = embeddingModel,
                UseLLMConfigForEmbeddings = embeddingModel is not null && aiProvider == AIProviderEnum.OpenAI,
            };
            return enhancerConfiguration;
        }

        public async Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true)
        {
            var json = GetConfigurationJson(configuration, hideSecrets);
            await File.WriteAllTextAsync("filePath", json);
        }

        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true)
        {
            var json = GetConfigurationJson(configuration, hideSecrets);
            return Encoding.UTF8.GetBytes(json);
        }

        public async Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<EnhancerConfiguration>(json);
        }

        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes)
        {
            var json = Encoding.UTF8.GetString(jsonBytes);
            return JsonConvert.DeserializeObject<EnhancerConfiguration>(json);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(PipelineSettings settings, PipelineContext context, CancellationToken ct = default)
        {
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            List<ChatMessage> history = ChatHistoryUtility.AddToChatHistoryPipeline(context);
            context.ChatHistory = history;
            if (ChatHistoryUtility.GetHistoryLength(history) > settings.Settings.MaximumInputLength)
            {
                throw new ArgumentOutOfRangeException(nameof(context), ChatHistoryUtility.GetInputSizeExceededLimitMessage(nameof(GetStreamingResponse)));
            }

            return chatClient.GetStreamingResponseAsync(history, settings.Settings.ChatOptions, ct);
        }

        public async Task<ErrorOr<IList<ResultModel>>> ProcessPipelineAsync(PipelineModel pipeline, IEnumerable<PipelineContext> entries, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!entries.Any())
                {
                    return Error.Unexpected("No input specified, nothing to proccess");
                }
                var cb = new ConcurrentBag<ResultModel>();
                await Parallel.ForEachAsync(entries, async (context, _) =>
                {
                    var pipelineRes = await _pipelineOrchestrator.RunPipelineAsync(pipeline, context, cancellationToken);
                    var resultModel = new ResultModel
                    {
                        Result = context,
                        Errors = pipelineRes.ErrorsOrEmptyList,
                    };
                    cb.Add(resultModel);
                });
                return cb.ToList();
            }
            catch (Exception ex)
            {
                return Error.Failure($"{nameof(ProcessPipelineAsync)} failed", ex.Message);
            }

        }

        public ErrorOr<PipelineSettings> CreatePipelineSettingsFromConfig(PromptConfiguration promptConf, PipelineAdditionalSettings pipelineSettings, KernelConfiguration? kernelData = null, Kernel? kernel = null)
        {
            var sk = kernel ?? _kernel;
            if (sk is null && kernelData is not null)
            {
                var configs = _semanticKernelManager.ConvertConfig(kernelData);
                if (configs.IsError)
                {
                    return configs.Errors;
                }
                var createdKernel = _semanticKernelManager.CreateKernel(configs.Value);
                if (createdKernel.IsError)
                {
                    return createdKernel.Errors;
                }
                sk = createdKernel.Value;
            }

            if (sk is null)
            {
                return Error.Failure("No kernel could be created or resolved.");
            }
            return new PipelineSettings(sk, _serviceProvider, pipelineSettings, promptConf);
        }

        //TODO here i need pipeline, figure out configs
        public async Task<ErrorOr<IList<ResultModel>>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var settings = CreatePipelineSettingsFromConfig(config.PromptConfiguration, config.PipelineAdditionalSettings, config.KernelConfiguration, kernel);

            if (settings.IsError)
            {
                return settings.Errors;
            }

            var pipeline = new PipelineModel(settings.Value, config.Steps);

            return await ProcessPipelineAsync(pipeline, entries.Select(x => new PipelineContext(x)), cancellationToken);

            //TODO should this be here? (maybe like sk.invoke and hope there are some plugins? - since there are 3 ways to kernel here, also i would need some plugins in my creation, but i could resolve plugins by injection (common interface))
            //TODO it could also require check for openai, options and some uniform way to work with results, or just put it outside of this method and just work with it there, but it requires same arguments prolly
            //TODO ye just put it outside of here, this is bad, just if else with this config i guess
            //if (config.UseAutomaticFunctionCalling)
            //{
            //    return await HandleAutomaticFunctionCalling(sk, entries.FirstOrDefault());
            //}

            //if(config.PipeLineSteps?.Any() != true)
            //{
            //    return Error.Unexpected("No pipeline steps defined in configuration.");
            //}

            //delete
            //_pipelineContextService.SetCurrentContext(context);



            //change to list of results, deal with errors in result creation

            // will be needed to search by params/config
            // refactor into pipeline
            //var sk = _semanticKernelManager.CreateKernel(skData!);
            //_semanticKernelManager.AddPluginToSemanticKernel<SemanticSlicerChunkGenerator>(sk!);
            //sk!.Plugins.TryGetPlugin(typeof(SemanticSlicerChunkGenerator).Name, out var plugin);
            //var service = sk.GetRequiredService<IChunkGenerator>();



            //var prompt = "{{SemanticSlicerChunkGenerator.generate_chunks_from_string $rawText}}";
            //var arg = "Multiple Implementations: If there are multiple implementations of generate_chunks_from_string, ensure that the correct one is invoked by specifying the appropriate function name or handling the selection logic.\n\nError Handling: Implement robust error handling to manage scenarios where the function might not be available or the invocation fails.\n\nFunction Signatures: Ensure that the function signatures match the expected parameters to avoid runtime errors.";
            //var c = new PromptTemplateConfig
            //{
            //    Template = prompt,
            //    TemplateFormat = PromptTemplateConfig.SemanticKernelTemplateFormat,
            //    Name = "ChunkGenTemplate"
            //};
            //var factory = new KernelPromptTemplateFactory();
            //var b = factory.TryCreate(c, out var promptTemplate);
            //var rendered = await promptTemplate.RenderAsync(sk, new KernelArguments { { "rawText", arg } });
            //var res = await sk.InvokePromptAsync(prompt, new KernelArguments { { "rawText", arg } });
            //var result = await sk.InvokeAsync(typeof(SemanticSlicerChunkGenerator).Name, "generate_chunks_from_string", new KernelArguments{ { "rawText", arg } });
            //var result2 = await sk.InvokePromptAsync($"Generate chunks from this string and return only list of strings: {arg}", new(settings));
            //var val1 = result.GetValue<IList<string>>();
            //var val = result2.GetValue<string>();
            //var val = res.GetValue<string>();
            //var textSearch = _searchProviderManager.CreateTextSearch(searchData!)!;

            //var strings = new List<string> { "Pes šel do lesa.", "Kočka spí v koši.", "Auto jede rychle." };
            //var embeddingGenerator = sk.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            //var emb = await embeddingGenerator.GenerateAsync("závodní vůz.");
            //var vectorStore = new InMemoryVectorStore();
            //var collection = new InMemoryCollection<string, VectorStore>("myCollection");
            //await collection.EnsureCollectionExistsAsync();
            ////var embeddings = await embeddingGenerator.GenerateAsync(strings.ToArray());

            //for (int i = 0; i < strings.Count; i++)
            //{
            //    var e = await embeddingGenerator.GenerateAsync(strings[i]);
            //    await collection.UpsertAsync(new VectorStore
            //    {
            //        Id = $"doc-{i}",
            //        Text = strings[i],
            //        Embedding = e.Vector
            //    });
            //}
            //var searchResult = collection.SearchAsync(emb, top: 3);

            //await foreach (var record in searchResult)
            //{
            //    Console.WriteLine($"Best match: {record.Record.Text}");
            //    Console.WriteLine($"Score: {record.Score}");
            //}

            await Parallel.ForEachAsync(entries, async (entry, _) =>
            {
                //var query = entry.QueryString!;
                //var res = await _searchProviderManager.GetSearchResults(textSearch, query);
                //var searchResults = await res.Results.ToListAsync();
                //var usedUrls = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => x.Link!);
                //temporary
                //if (useScraper)
                //{
                //    //will be needed some specifications from config what to search for maybe?
                //    var rawScrapedContent = await _searchWebScraper.ScrapeDataFromUrlsAsync(usedUrls);
                //    var chunks = _chunkGenerator.GenerateChunksFromData(rawScrapedContent);
                //    resultView.SearchResult = _chunkRanker.ExtractRelevantDataFromChunks(chunks, query);
                //}
                //else
                //{
                //    //this uses snippets from search only
                //    resultView.SearchResult = string.Join('\n', searchResults.Select(x => x.Value));
                //}
                //resultView.Prompt = PromptUtility.BuildPrompt(promptConf, query, resultView.SearchResult);
                ////resultView.AIResult = await _semanticKernelManager.GetAICompletionResult(sk!, resultView.Prompt);
                //resultView.AIResult.UsedURLs = usedUrls;
            });
        }

        private string GetConfigurationJson(EnhancerConfiguration configuration, bool hideSecrets = true)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            if (hideSecrets)
            {
                settings.ContractResolver = new SensitiveContractResolver();
            }

            return JsonConvert.SerializeObject(configuration, settings);
        }
    }
}
