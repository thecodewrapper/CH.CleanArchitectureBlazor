using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Domain.User;
using CH.CleanArchitecture.Tests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CH.CleanArchitecture.Core.Tests.Application.Handlers
{
    public class AddRolesCommandHandlerTests : TestBase
    {
        private readonly AddRolesCommandHandler _handler;

        public AddRolesCommandHandlerTests() {
            var applicationUserService = ServiceProvider.GetService<IApplicationUserService>();
            _handler = new AddRolesCommandHandler(ServiceProvider, applicationUserService);
        }

        [Fact]
        public async Task AddRoles_WhenRoleIsValidAndNotAssigned_ShouldSucceed() {
            var command = new AddRolesCommand("basicUser", new List<string>
                {
                    RoleEnum.SuperAdmin.ToString()
                });

            var result = await _handler.HandleAsync(command);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task AddRoles_WhenRoleIsInvalid_ShouldFail() {
            var command = new AddRolesCommand("basicUser", new List<string>
                {
                    "InvalidRole"
                });

            var result = await _handler.HandleAsync(command);

            result.IsSuccessful.Should().BeFalse();
        }
    }
}
