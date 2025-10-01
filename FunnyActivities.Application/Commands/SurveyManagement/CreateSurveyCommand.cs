using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Commands.SurveyManagement
{
    /// <summary>
    /// Command for creating a new survey.
    /// </summary>
    public class CreateSurveyCommand : IRequest<SurveyDto>
    {
        /// <summary>
        /// Gets or sets the title of the survey.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the survey.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the start date of the survey.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the survey.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of participants allowed.
        /// </summary>
        public int? MaxParticipants { get; set; }

        /// <summary>
        /// Gets or sets the activity IDs to include in the survey.
        /// </summary>
        public List<Guid> ActivityIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the ID of the user creating the survey.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSurveyCommand"/> class.
        /// </summary>
        public CreateSurveyCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSurveyCommand"/> class with specified parameters.
        /// </summary>
        /// <param name="title">The title of the survey.</param>
        /// <param name="description">The description of the survey.</param>
        /// <param name="startDate">The start date of the survey.</param>
        /// <param name="endDate">The end date of the survey.</param>
        /// <param name="maxParticipants">The maximum number of participants.</param>
        /// <param name="activityIds">The activity IDs to include in the survey.</param>
        /// <param name="createdByUserId">The ID of the user creating the survey.</param>
        public CreateSurveyCommand(string title, string description, DateTime startDate, DateTime? endDate, int? maxParticipants, List<Guid> activityIds, Guid createdByUserId)
        {
            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            MaxParticipants = maxParticipants;
            ActivityIds = activityIds ?? new List<Guid>();
            CreatedByUserId = createdByUserId;
        }
    }
}