using CSharpFunctionalExtensions;
using FluentValidation.Results;
using MediatR;
using RoomAggregate = Epam.ItMarathon.ApiService.Domain.Aggregate.Room.Room;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Commands
{
    /// <summary>
    /// Request for deleting User from Room.
    /// </summary>
    /// <param name="UserCode">User authorization code.</param>
    /// <param name="UserId">User's unique identifier.</param>
    public record DeleteUserRequest(string UserCode, ulong? UserId)
        : IRequest<Result<RoomAggregate, ValidationResult>>;
}