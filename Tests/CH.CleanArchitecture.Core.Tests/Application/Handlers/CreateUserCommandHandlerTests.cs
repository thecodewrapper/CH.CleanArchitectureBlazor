using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Tests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CH.CleanArchitecture.Core.Tests.Application.Handlers
{
    public class CreateUserCommandHandlerTests : TestBase
    {
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests() {
            var applicationUserService = ServiceProvider.GetService<IApplicationUserService>();
            _handler = new CreateUserCommandHandler(ServiceProvider, applicationUserService);
        }

        [Fact]
        public async Task CreateUser_WhenPasswordIsNull_ShouldFail() {
            var command = new CreateUserCommand(
                "testUser",
                "testName",
                "testSurname",
                "test@test.com",
                null,
                null);

            Result result = await _handler.HandleAsync(command);

            result.IsSuccessful.Should().BeFalse();
            result.Exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUser_WhenRequestIsValid_ShouldSucceed() {
            var command = new CreateUserCommand(
                "testUser",
                Guid.NewGuid().ToString("N"),
                Guid.NewGuid().ToString("N"),
                 "test@test.com",
                "test test",
                null);

            Result result = await _handler.HandleAsync(command);

            result.IsSuccessful.Should().BeTrue();
            result.Exception.Should().BeNull();
        }
    }
}
