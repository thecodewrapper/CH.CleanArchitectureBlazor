using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Tests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CH.CleanArchitecture.Core.Tests.Application.Handlers
{
    public class ActivateUserCommandHandlerTests : TestBase
    {
        private readonly ActivateUserCommandHandler _handler;

        public ActivateUserCommandHandlerTests() {
            var applicationUserService = ServiceProvider.GetService<IApplicationUserService>();
            _handler = new ActivateUserCommandHandler(ServiceProvider, applicationUserService);
        }

        [Fact]
        public async Task ActivateUser_WhenUserDoesNotExit_ShouldFailWithMessage() {
            var activateUserCommand = new ActivateUserCommand("unknownUser");

            var result = await _handler.HandleAsync(activateUserCommand);

            result.IsFailed.Should().BeTrue();
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task ActivateUser_WhenUserActive_ShouldFailWithMessage() {
            var activateUserCommand = new ActivateUserCommand("activeUser");
            var result = await _handler.HandleAsync(activateUserCommand);

            result.IsFailed.Should().BeTrue();
            result.Message.Should().Be("User is already active.");
        }

        [Fact]
        public async Task ActivateUser_WhenUserInactive_ShouldSucceed() {
            var activateUserCommand = new ActivateUserCommand("inactiveUser");
            var result = await _handler.HandleAsync(activateUserCommand);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task ActivateUser_WhenServiceThrowsException_ShouldFail() {

            var activateUserCommand = new ActivateUserCommand("throwsException");
            var result = await _handler.HandleAsync(activateUserCommand);

            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Be("Error while trying to activate user.");
            result.Exception.Should().NotBeNull();
        }
    }
}
