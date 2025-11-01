using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.PromptEnhancerSteps
{
    public class PreprocessStep : PipelineStep
    {
        protected override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineContext context, CancellationToken cancellationToken = default)
        {
            if(context.QueryString is null)
            {
                return Task.FromResult<ErrorOr<bool>>(false);
            }

            context.QueryString = context.QueryString.Trim();
            return Task.FromResult<ErrorOr<bool>>(true);
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context, bool isRequired = false)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition(isRequired);
        }
    }
}
