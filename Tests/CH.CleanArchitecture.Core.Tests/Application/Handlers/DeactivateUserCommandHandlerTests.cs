using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Tests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CH.CleanArchitecture.Core.Tests.Application.Handlers
{
    public class DeactivateUserCommandHandlerTests : TestBase
    {
        private readonly DeactivateUserCommandHandler _handler;

        public DeactivateUserCommandHandlerTests() {
            var applicationUserService = ServiceProvider.GetService<IApplicationUserService>();
            _handler = new DeactivateUserCommandHandler(ServiceProvider, applicationUserService);
        }

        [Fact]
        public async Task DeactivateUser_WhenUserDoesNotExit_ShouldFailWithMessage() {
            var deactivateUserCommand = new DeactivateUserCommand("unknownUser");
            var result = await _handler.HandleAsync(deactivateUserCommand);

            result.IsFailed.Should().BeTrue();
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task DeactivateUser_WhenUserNotActive_ShouldFailWithMessage() {
            var deactivateUserCommand = new DeactivateUserCommand("inactiveUser");
            var result = await _handler.HandleAsync(deactivateUserCommand);

            result.IsFailed.Should().BeTrue();
            result.Message.Should().Be("User is not active.");
        }

        [Fact]
        public async Task DeactivateUser_WhenUserActive_ShouldSucceed() {
            var deactivateUserCommand = new DeactivateUserCommand("activeUser");
            var result = await _handler.HandleAsync(deactivateUserCommand);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeactivateUser_WhenServiceThrowsException_ShouldFail() {

            var deactivateUserCommand = new DeactivateUserCommand("throwsException");
            var result = await _handler.HandleAsync(deactivateUserCommand);

            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Be("Error while trying to deactivate user.");
            result.Exception.Should().NotBeNull();
        }
    }
}
