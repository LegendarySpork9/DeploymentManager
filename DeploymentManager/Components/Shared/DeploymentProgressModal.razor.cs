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

        private string FormatTime(DateTime dateTime)
        {
            return DateTimeConverter.FormatTime(dateTime, _Clock.DefaultDate);
        }

        private string FormatRunTime(TimeSpan runTime)
        {
            return DateTimeConverter.FormatRunTime(runTime, _Clock.DefaultTimeSpan);
        }
    }
}
