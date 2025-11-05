using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Queries;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Entities.User;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;
using System.Net.WebSockets;
using RoomAggregate = Epam.ItMarathon.ApiService.Domain.Aggregate.Room.Room;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{
    public class DeleteUserHandler(IRoomRepository roomRepository)
        : IRequestHandler<DeleteUserRequest, Result<RoomAggregate, ValidationResult>>
    {
        ///<inheritdoc/>
        public async Task<Result<RoomAggregate, ValidationResult>> Handle(DeleteUserRequest request,
            CancellationToken cancellationToken)
        {
            var roomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);
            if (roomResult.IsFailure)
            {
                return roomResult;
            }

            var room = roomResult.Value;

            // Validate that the room is not closed
            var closedRoomCheck = room.ClosedOn.HasValue;
            if (closedRoomCheck)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new NotFoundError([
                    new ValidationFailure("room.ClosedOn", "The room is already closed.")
                ]));
            }

            // Validate that the chosen user id exists
            var userToDelete = room.Users.FirstOrDefault(user => user.Id == request.UserId);
            if (userToDelete is null)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new NotFoundError([
                    new ValidationFailure("user.Id", "User with the specified Id was not found in the room")
                ]));
            }

            // Validate that the chosen userCode exists
            var userCodeCheckResult = room.Users.FirstOrDefault(user => user.AuthCode == request.UserCode);
            if (userCodeCheckResult is null)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new NotFoundError([
                    new ValidationFailure("user.UserCode", "User with the specified UserCode was not found in the room.")
                ]));
            }

            // Validate that the chosen userCode is an Admin
            if (!userCodeCheckResult.IsAdmin)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new ForbiddenError([
                    new ValidationFailure("user.IsAdmin", "This user is not the admin.")
                ]));
            }

            // Validate that the chosen id and userCode is in the same room
            if (userToDelete.RoomId != userCodeCheckResult.RoomId)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure("user.Id", "This user is not in the same room as UserCode.")
                ]));
            }

            // Validate that the chosen id and userCode is not the same person
            if (userToDelete.AuthCode == request.UserCode)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure("user.Id", "Admin cannot delete himself in the room.")
                ]));
            }

            var deleteResult = room.DeleteUser(request.UserId);
            if (deleteResult.IsFailure)
            {
                return deleteResult;
            }

            var updatedResult = await roomRepository.UpdateAsync(room, cancellationToken);
            if (updatedResult.IsFailure)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(string.Empty, updatedResult.Error)
                ]));
            }

            var updatedRoomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);
            return updatedRoomResult;

        }
    }
}