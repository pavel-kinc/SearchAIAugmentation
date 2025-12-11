using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Tests.TestDataFactory
{
    public static class KernelMocks
    {
        public static Kernel GetRealKernelWithMocks(Mock<IEmbeddingGenerator<string, Embedding<float>>>? generatorMock = null, Mock<IChatClient>? chatClient = null)
        {
            var services = new ServiceCollection();

            if(generatorMock is not null)
            {
                services.AddSingleton(generatorMock.Object);
            }
            if(chatClient is not null)
            {
                services.AddSingleton(chatClient.Object);
            }
                
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            return kernel;
        }
    }
}
