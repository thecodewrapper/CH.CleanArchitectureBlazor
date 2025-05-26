using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using Polly;

namespace CH.CleanArchitecture.Core.Application.Workflow
{
    public class WorkflowStep
    {
        public string Name { get; }
        public Func<WorkflowContext, Task<Result>> Action { get; }
        public Func<WorkflowContext, Task> Compensation { get; private set; }
        public IAsyncPolicy<Result> RetryPolicy { get; private set; }

        public WorkflowStep(string name, Func<WorkflowContext, Task<Result>> action) {
            Name = name;
            Action = action;
        }

        public void SetCompensation(Func<WorkflowContext, Task> compensation) {
            Compensation = compensation;
        }

        public void SetRetryPolicy(IAsyncPolicy<Result> retryPolicy) {
            RetryPolicy = retryPolicy;
        }
    }
}
