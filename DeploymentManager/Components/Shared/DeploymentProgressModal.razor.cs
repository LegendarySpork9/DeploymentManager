// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Converters;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Shared
{
    public partial class DeploymentProgressModal
    {
        [Inject]
        private IClock _Clock { get; set; } = default!;

        [Parameter]
        public required DeploymentHistoryModel<object> Deployment { get; set; }
        [Parameter]
        public EventCallback OnClose { get; set; }
        [Parameter]
        public EventCallback<StageModel> OnShowErrors { get; set; }
        [Parameter]
        public EventCallback<StageModel> OnShowWarnings { get; set; }
        [Parameter]
        public bool AlwaysShowFooter { get; set; }
    }
}
