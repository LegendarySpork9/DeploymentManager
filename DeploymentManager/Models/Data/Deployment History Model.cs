// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Responses.Related;

namespace DeploymentManager.Models.Data
{
    /// <summary>
    /// Stores the deployment history data for a project. 
    /// </summary>
    public class DeploymentHistoryModel<T>
    {
        public required int Id { get; set; }
        public required DeploymentType Type { get; set; }
        public required ArtefactType ArtefactType { get; set; }
        public required Status Status { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required TimeSpan RunTime { get; set; }
        public required long ArtefactId { get; set; }
        public required string ArtefactName { get; set; }
        public required long ArtefactSize { get; set; }
        public required string BranchId { get; set; }
        public required string BranchName { get; set; }
        public DeploymentStage? FailedAtStage { get; set; }
        public required DeploymentConfigurationModel<T> DeploymentConfiguration { get; set; }
        public required List<StageModel> Stages { get; set; }

        /// <summary>
        /// Converts the model to an artefact type model.
        /// </summary>
        public DeploymentHistoryModel<ArtefactModel> ToArtefactDeployment()
        {
            return new()
            {
                Id = Id,
                Type = Type,
                ArtefactType = ArtefactType,
                Status = Status,
                StartDate = StartDate,
                EndDate = EndDate,
                RunTime = RunTime,
                ArtefactId = ArtefactId,
                ArtefactName = ArtefactName,
                ArtefactSize = ArtefactSize,
                BranchId = BranchId,
                BranchName = BranchName,
                FailedAtStage = FailedAtStage,
                DeploymentConfiguration = DeploymentConfiguration.ToArtefactDeployment(),
                Stages = Stages
            };
        }

        /// <summary>
        /// Converts the model to an asset type model.
        /// </summary>
        public DeploymentHistoryModel<AssetModel> ToAssetDeployment()
        {
            return new()
            {
                Id = Id,
                Type = Type,
                ArtefactType = ArtefactType,
                Status = Status,
                StartDate = StartDate,
                EndDate = EndDate,
                RunTime = RunTime,
                ArtefactId = ArtefactId,
                ArtefactName = ArtefactName,
                ArtefactSize = ArtefactSize,
                BranchId = BranchId,
                BranchName = BranchName,
                FailedAtStage = FailedAtStage,
                DeploymentConfiguration = DeploymentConfiguration.ToAssetDeployment(),
                Stages = Stages
            };
        }

        /// <summary>
        /// Converts the model to an upload type model.
        /// </summary>
        public DeploymentHistoryModel<UploadFileModel> ToUploadDeployment()
        {
            return new()
            {
                Id = Id,
                Type = Type,
                ArtefactType = ArtefactType,
                Status = Status,
                StartDate = StartDate,
                EndDate = EndDate,
                RunTime = RunTime,
                ArtefactId = ArtefactId,
                ArtefactName = ArtefactName,
                ArtefactSize = ArtefactSize,
                BranchId = BranchId,
                BranchName = BranchName,
                FailedAtStage = FailedAtStage,
                DeploymentConfiguration = DeploymentConfiguration.ToUploadDeployment(),
                Stages = Stages
            };
        }

        /// <summary>
        /// Converts the model to an object type model.
        /// </summary>
        public DeploymentHistoryModel<object> ToObjectDeployment()
        {
            return new()
            {
                Id = Id,
                Type = Type,
                ArtefactType = ArtefactType,
                Status = Status,
                StartDate = StartDate,
                EndDate = EndDate,
                RunTime = RunTime,
                ArtefactId = ArtefactId,
                ArtefactName = ArtefactName,
                ArtefactSize = ArtefactSize,
                BranchId = BranchId,
                BranchName = BranchName,
                FailedAtStage = FailedAtStage,
                DeploymentConfiguration = DeploymentConfiguration.ToObjectDeployment(),
                Stages = Stages
            };
        }
    }
}
