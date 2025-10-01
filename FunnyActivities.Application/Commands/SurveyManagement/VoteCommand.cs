using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Commands.SurveyManagement
{
    /// <summary>
    /// Command for voting on survey activities.
    /// </summary>
    public class VoteCommand : IRequest<VoteDto>
    {
        /// <summary>
        /// Gets or sets the survey activity ID to vote for.
        /// </summary>
        public Guid SurveyActivityId { get; set; }

        /// <summary>
        /// Gets or sets the vote value (1-5 scale).
        /// </summary>
        public int VoteValue { get; set; }

        /// <summary>
        /// Gets or sets the ID of the survey participant voting.
        /// </summary>
        public Guid SurveyParticipantId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteCommand"/> class.
        /// </summary>
        public VoteCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteCommand"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID to vote for.</param>
        /// <param name="voteValue">The vote value (1-5).</param>
        /// <param name="surveyParticipantId">The ID of the survey participant voting.</param>
        public VoteCommand(Guid surveyActivityId, int voteValue, Guid surveyParticipantId)
        {
            SurveyActivityId = surveyActivityId;
            VoteValue = voteValue;
            SurveyParticipantId = surveyParticipantId;
        }
    }
}