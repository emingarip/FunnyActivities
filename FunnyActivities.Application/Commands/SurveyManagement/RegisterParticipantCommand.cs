using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Commands.SurveyManagement
{
    /// <summary>
    /// Command for registering a survey participant.
    /// </summary>
    public class RegisterParticipantCommand : IRequest<SurveyParticipantDto>
    {
        /// <summary>
        /// Gets or sets the survey ID.
        /// </summary>
        public Guid SurveyId { get; set; }

        /// <summary>
        /// Gets or sets the participant's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the participant's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the number of children.
        /// </summary>
        public int ChildrenCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterParticipantCommand"/> class.
        /// </summary>
        public RegisterParticipantCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterParticipantCommand"/> class with specified parameters.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <param name="firstName">The participant's first name.</param>
        /// <param name="lastName">The participant's last name.</param>
        /// <param name="childrenCount">The number of children.</param>
        public RegisterParticipantCommand(Guid surveyId, string firstName, string lastName, int childrenCount)
        {
            SurveyId = surveyId;
            FirstName = firstName;
            LastName = lastName;
            ChildrenCount = childrenCount;
        }
    }
}