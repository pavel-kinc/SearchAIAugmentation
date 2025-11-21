using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    public abstract class PipelineStep : IPipelineStep
    {
        protected bool _isRequired = false;
        protected PipelineStep(bool isRequired = false)
        {
            _isRequired = isRequired;
        }
        public async Task<ErrorOr<bool>> ExecuteAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            var check = CheckExecuteConditions(context);
            if (check.IsError)
            {
                return check;
            }
            //TODO here try catch? also maybe delete cancellation token, i dont use it in my methods/services
            if (check.Value)
            {
                var res = await ExecuteStepAsync(settings, context, cancellationToken);
                return res.IsError ? res : (res.Value ? true : (_isRequired ? FailExecution() : false));
            }
            return _isRequired ? FailExecution() : false;
        }

        protected abstract Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default);

        //TODO: mostly for pipeline context conditions, maybe rename?
        protected virtual ErrorOr<bool> CheckExecuteConditions(PipelineRun context)
        {
            return true;
        }

        protected virtual ErrorOr<bool> FailCondition()
        {
            return _isRequired ? Error.Failure($"{GetType()}: Conditions check for this required step failed.") : false;
        }

        protected virtual ErrorOr<bool> FailExecution(string? reason = null)
        {
            return _isRequired ?
                (string.IsNullOrEmpty(reason) ? Error.Failure($"{GetType().Name}: Execution for this required step failed.") : Error.Failure(reason)) :
                false;
        }
    }
}
