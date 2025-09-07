using MediatR;
using FunnyActivities.Application.Commands.UnitOfMeasureManagement;
using FunnyActivities.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UnitOfMeasureManagement
{
    /// <summary>
    /// Handler for deleting a unit of measure.
    /// </summary>
    public class DeleteUnitOfMeasureCommandHandler : IRequestHandler<DeleteUnitOfMeasureCommand>
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly ILogger<DeleteUnitOfMeasureCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteUnitOfMeasureCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="logger">The logger.</param>
        public DeleteUnitOfMeasureCommandHandler(
            IUnitOfMeasureRepository unitOfMeasureRepository,
            ILogger<DeleteUnitOfMeasureCommandHandler> _logger)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            this._logger = _logger;
        }

        /// <summary>
        /// Handles the delete unit of measure command.
        /// </summary>
        /// <param name="request">The delete unit of measure command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Handle(DeleteUnitOfMeasureCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting unit of measure with ID: {Id}", request.Id);

            var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(request.Id);
            if (unitOfMeasure == null)
            {
                _logger.LogWarning("Unit of measure with ID {Id} not found", request.Id);
                throw new KeyNotFoundException($"Unit of measure with ID '{request.Id}' was not found.");
            }

            await _unitOfMeasureRepository.DeleteAsync(unitOfMeasure);

            _logger.LogInformation("Unit of measure deleted successfully with ID: {Id}", request.Id);
        }
    }
}