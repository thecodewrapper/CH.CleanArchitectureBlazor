using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class DeleteResourceCommand : BaseCommand<Result>
    {
        public string Id { get; private set; }
        public bool ForceDelete { get; init; }

        public DeleteResourceCommand(string id) {
            Id = id;
        }
    }

    public class DeleteResourceCommandHandler : BaseCommandHandler<DeleteResourceCommand, Result>
    {
        private readonly IResourcesService _resourcesService;

        public DeleteResourceCommandHandler(IServiceProvider serviceProvider, IResourcesService resourcesService) : base(serviceProvider)
        {
            _resourcesService = resourcesService;
        }

        public override async Task<Result> HandleAsync(DeleteResourceCommand command)
        {
            return await _resourcesService.DeleteResourceAsync(command.Id, command.ForceDelete);
        }
    }
}