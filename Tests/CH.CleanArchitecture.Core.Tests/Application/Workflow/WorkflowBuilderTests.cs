using FluentAssertions;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Workflow;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CH.CleanArchitecture.Core.Tests.Application.Workflow
{
    public class WorkflowBuilderTests
    {
        [Fact]
        public async Task ExecuteAsync_AllStepsSuccessful_ShouldReturnSuccess() {
            var builder = new WorkflowBuilder()
                .Step<string>("Step1", ctx => Task.FromResult(new Result<string>().Succeed().WithData("value1")), "key1")
                .Step<string>("Step2", ctx => Task.FromResult(new Result<string>().Succeed().WithData("value2")), "key2");

            var result = await builder.ExecuteAsync();

            result.IsSuccessful.Should().BeTrue();
            builder.Context.Get<string>("key1").Should().Be("value1");
            builder.Context.Get<string>("key2").Should().Be("value2");
        }

        [Fact]
        public async Task ExecuteAsync_StepFails_ShouldTriggerCompensation() {
            bool compensationCalled = false;

            var builder = new WorkflowBuilder()
                .Step<string>("Step1", ctx => Task.FromResult(new Result<string>().Succeed().WithData("value1")), "key1")
                    .WithCompensation(ctx =>
                    {
                        compensationCalled = true;
                        return Task.CompletedTask;
                    })
                .Step("FailingStep", ctx => Task.FromResult(new Result().Fail().WithError("Something broke")));

            var result = await builder.ExecuteAsync();

            result.IsSuccessful.Should().BeFalse();
            result.MessageWithErrors.Should().Contain("Step 'FailingStep' failed");

            compensationCalled.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteAsync_StepFailsWithoutCompensation_ShouldNotThrow() {
            var builder = new WorkflowBuilder()
                .Step("FailingStep", ctx => Task.FromResult(new Result().Fail().WithError("boom")));

            Func<Task> act = async () => await builder.ExecuteAsync();

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task WithRetry_ShouldRetryFailedStep() {
            int attempt = 0;

            var builder = new WorkflowBuilder()
                .Step<string>("FlakyStep", ctx =>
                {
                    attempt++;
                    if (attempt < 3)
                        return Task.FromResult(new Result<string>().Fail().WithError("temporary failure"));

                    return Task.FromResult(new Result<string>().Succeed().WithData("finally succeeded"));
                }, "flakyKey")
                .WithRetry(3, TimeSpan.FromMilliseconds(10));

            var result = await builder.ExecuteAsync();

            result.IsSuccessful.Should().BeTrue();
            attempt.Should().Be(3); // retried twice then succeeded
        }

        [Fact]
        public async Task WithRetry_ShouldFailAfterExceedingRetries() {
            int attempt = 0;

            var builder = new WorkflowBuilder()
                .Step<string>("AlwaysFailingStep", ctx =>
                {
                    attempt++;
                    return Task.FromResult(new Result<string>().Fail().WithError("always fails"));
                }, "failKey")
                .WithRetry(2, TimeSpan.FromMilliseconds(10)); // Only 2 retries

            var result = await builder.ExecuteAsync();

            result.IsSuccessful.Should().BeFalse();
            attempt.Should().Be(3); // initial attempt + 2 retries
        }
    }
}
