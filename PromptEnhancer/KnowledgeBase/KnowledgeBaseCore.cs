using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Services;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.Search.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.KnowledgeBase
{

    public abstract class KnowledgeBaseBase
    {
        protected readonly IChunkGenerator _chunkGenerator;

        protected KnowledgeBaseBase(IChunkGenerator chunkGenerator)
        {
            _chunkGenerator = chunkGenerator;
        }
    }

}
