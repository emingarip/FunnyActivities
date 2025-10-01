using MediatR;

namespace FunnyActivities.Application.Commands.SurveyManagement
{
    /// <summary>
    /// Command for deleting a survey.
    /// </summary>
    public class DeleteSurveyCommand : IRequest<bool>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the survey to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user requesting the deletion.
        /// </summary>
        public Guid RequestedByUserId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteSurveyCommand"/> class.
        /// </summary>
        public DeleteSurveyCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteSurveyCommand"/> class with specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to delete.</param>
        /// <param name="requestedByUserId">The ID of the user requesting the deletion.</param>
        public DeleteSurveyCommand(Guid id, Guid requestedByUserId)
        {
            Id = id;
            RequestedByUserId = requestedByUserId;
        }
    }
}