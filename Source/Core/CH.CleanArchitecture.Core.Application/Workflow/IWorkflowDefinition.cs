using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application.Workflow
{
    public interface IWorkflowDefinition
    {
        Task<WorkflowBuilder> BuildAsync();
    }
}