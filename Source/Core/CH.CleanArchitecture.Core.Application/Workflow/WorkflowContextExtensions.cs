using System;

namespace CH.CleanArchitecture.Core.Application.Workflow
{
    public static class WorkflowContextExtensions
    {
        public static T GetOrThrow<T>(this WorkflowContext ctx, string key) {
            var value = ctx.Get<T>(key);
            if (value == null)
                throw new InvalidOperationException($"Context missing required key '{key}'.");

            return value;
        }
    }
}
