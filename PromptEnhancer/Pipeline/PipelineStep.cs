using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    public abstract class PipelineStep : IPipelineStep
    {
        protected bool _isRequired = false;
        public async Task<ErrorOr<bool>> ExecuteAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var check = CheckExecuteConditions(context);
            if (check.IsError)
            {
                return check;
            }
            //TODO here try catch?
            return check.Value ? await ExecuteStepAsync(settings, context, cancellationToken) : false;
        }

        protected abstract Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default);

        //TODO: mostly for pipeline context conditions, maybe rename?
        protected virtual ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            return true;
        }

        protected virtual ErrorOr<bool> FailCondition()
        {
            return _isRequired ? Error.Failure($"{GetType()}: Conditions check for this required step failed.") : false;
        }
    }
}
