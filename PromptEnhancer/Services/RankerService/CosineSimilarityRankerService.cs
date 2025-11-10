using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Services.RecordRankerService
{
    public class CosineSimilarityRankerService : IRankerService
    {
        //TODO maybe add some source/name that it is cosine similarity?
        public float GetSimilarityScore(ReadOnlyMemory<float> queryEmbed, ReadOnlyMemory<float> recordEmbed)
        {
            return TensorPrimitives.CosineSimilarity(queryEmbed.ToArray(), recordEmbed.ToArray());
        }
    }
}
