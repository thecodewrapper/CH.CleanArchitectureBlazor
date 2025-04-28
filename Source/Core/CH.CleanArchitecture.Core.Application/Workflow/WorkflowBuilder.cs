using CH.CleanArchitecture.Common;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application.Workflow
{
    public class WorkflowBuilder
    {
        private readonly List<WorkflowStep> _steps = new();
        private WorkflowStep _lastStep;
        private readonly WorkflowContext _context = new();

        public WorkflowBuilder Step<T>(string name, Func<WorkflowContext, Task<Result<T>>> action, string outputKey) {
            var step = new WorkflowStep(name, async ctx =>
            {
                var result = await action(ctx);
                if (result.IsSuccessful) {
                    ctx.Set(outputKey, result.Unwrap());
                    return new Result().Succeed();
                }
                return new Result().Fail().WithError(result.MessageWithErrors);
            });

            _steps.Add(step);
            _lastStep = step;
            return this;
        }

        public WorkflowBuilder Step(string name, Func<WorkflowContext, Task<Result>> action) {
            var step = new WorkflowStep(name, action);
            _steps.Add(step);
            _lastStep = step;
            return this;
        }

        public WorkflowBuilder WithCompensation(Func<WorkflowContext, Task> compensation) {
            if (_lastStep == null)
                throw new InvalidOperationException("WithCompensation must be called after a Step.");

            _lastStep.SetCompensation(compensation);
            return this;
        }

        public WorkflowBuilder WithRetry(int retryCount, TimeSpan? retryDelay = null) {
            if (_lastStep == null)
                throw new InvalidOperationException("WithRetry must be called after a Step.");

            var policy = Policy<Result>
                .HandleResult(r => !r.IsSuccessful)
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => retryDelay ?? TimeSpan.FromMilliseconds(200),
                    (outcome, timespan, retryNumber, context) =>
                    {
                        Console.WriteLine($"Retry {retryNumber} for step '{_lastStep.Name}' after {timespan.TotalMilliseconds}ms due to failure: {outcome.Result.MessageWithErrors}");
                    });

            _lastStep.SetRetryPolicy(policy);

            return this;
        }

        public async Task<Result> ExecuteAsync() {
            var executedSteps = new Stack<WorkflowStep>();

            foreach (var step in _steps) {
                Result result;

                if (step.RetryPolicy != null) {
                    result = await step.RetryPolicy.ExecuteAsync(() => step.Action(_context));
                }
                else {
                    result = await step.Action(_context);
                }

                if (!result.IsSuccessful) {
                    await CompensateAsync(executedSteps);
                    return new Result().Fail().WithError($"Step '{step.Name}' failed: {result.MessageWithErrors}");
                }

                executedSteps.Push(step);
            }

            return new Result().Succeed();
        }

        private async Task CompensateAsync(Stack<WorkflowStep> executedSteps) {
            while (executedSteps.Count > 0) {
                var step = executedSteps.Pop();
                if (step.Compensation != null) {
                    try {
                        await step.Compensation(_context);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Compensation for step '{step.Name}' failed: {ex.Message}");
                    }
                }
            }
        }

        public WorkflowContext Context => _context;
    }
}
