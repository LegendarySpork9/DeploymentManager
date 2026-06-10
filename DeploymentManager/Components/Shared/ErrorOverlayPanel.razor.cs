// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Data.Related;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Shared
{
    public partial class ErrorOverlayPanel
    {
        [Parameter]
        public StageModel? Stage { get; set; }
        [Parameter]
        public EventCallback OnClose { get; set; }
    }
}
