using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class QueryParserStep : PipelineStep
    {
        protected override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            //"""
            //SystemPrompt: You will receive a question that will be used as a search query, try to logically split the question so that
            // search can find relevant data for it. Split the parts by ';' and order them by priority.
            //"""

            if (context.QueryString is null)
            {
                return Task.FromResult<ErrorOr<bool>>(false);
            }

            context.QueryString = context.QueryString.Trim();
            return Task.FromResult<ErrorOr<bool>>(true);
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
