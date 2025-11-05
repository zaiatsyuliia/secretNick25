using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Queries;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentAssertions;
using FluentValidation.Results;
using NSubstitute;
using NSubstitute.Core;

namespace Epam.ItMarathon.ApiService.Application.Tests.UserCases.Queries
{
    /// <summary>
    /// Unit tests for the <see cref="DeleteUserHandler"/> class.
    /// </summary>
    public class DeleteUserHandlerTests
    {
        private readonly IRoomRepository _roomRepositoryMock;
        private readonly DeleteUserHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteUserHandlerTests"/> class with mocked dependencies.
        /// </summary>
        public DeleteUserHandlerTests()
        {
            _roomRepositoryMock = Substitute.For<IRoomRepository>();
            _handler = new DeleteUserHandler(_roomRepositoryMock);
        }

        /// <summary>
        /// Tests that the handler returns a failure when the room is already closed.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoomIsClosed()
        {
            // Arrange
            var request = new DeleteUserRequest("asdfgh", 5);
            var room = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .Generate();
            var roomClosed = DataFakers.RoomFaker
                .RuleFor(room => room.ClosedOn, _ => DateTime.UtcNow)
                .Generate();

            _roomRepositoryMock.GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(roomClosed);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Errors.First().ErrorMessage.Should().Be("The room is already closed.");
        }

        /// <summary>
        /// Tests that the handler returns a failure when the chosen user id is not found in the room
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenChosenUserIdNotExists()
        {
            // Arrange
            var request = new DeleteUserRequest("asdfgh", 5);
            var adminUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.IsAdmin, _ => true)
                .Generate();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.Users, _ => [ adminUser ])
                .Generate();

            _roomRepositoryMock.GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(room);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Errors.First().ErrorMessage.Should().Be("User with the specified Id was not found in the room");
        }

        /// <summary>
        /// Tests that the handler returns a failure when the chosen userCode is not found in the room
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenChosenUserCodeNotExists()
        {
            // Arrange
            var request = new DeleteUserRequest("asdfgh", 2);
            var adminUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.Id, _ => 1UL)
                .RuleFor(user => user.AuthCode, _ => "asdf")
                .RuleFor(user => user.IsAdmin, _ => true)
                .Generate();

            var commonUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.Id, _ => 2UL)
                .Generate();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.Users, _ => [adminUser, commonUser])
                .Generate();

            _roomRepositoryMock.GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(room);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Errors.First().ErrorMessage.Should().Be("User with the specified UserCode was not found in the room.");
        }

        /// <summary>
        /// Tests that the handler returns a failure when the chosen user is not an admin
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenChosenUserIsNotAnAdmin()
        {
            // Arrange
            var request = new DeleteUserRequest("asdfgh", 1);
            var adminUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.Id, _ => 1UL)
                .RuleFor(user => user.AuthCode, _ => "asdfgh")
                .RuleFor(user => user.IsAdmin, _ => false)
                .Generate();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.Users, _ => [adminUser])
                .Generate();

            _roomRepositoryMock.GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(room);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Errors.First().ErrorMessage.Should().Be("This user is not the admin.");
        }

        /// <summary>
        /// Tests that the handler returns a failure when the chosen user id and userCode is not found in the same room
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenChosenUserIdAndUserCodeIsNotInTheSameRoom()
        {
            // Arrange
            var request = new DeleteUserRequest("asdfgh", 2);
            var adminUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.Id, _ => 1UL)
                .RuleFor(user => user.AuthCode, _ => "asdfgh")
                .RuleFor(user => user.IsAdmin, _ => true)
                .Generate();

            var commonUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 2UL)
                .RuleFor(user => user.Id, _ => 2UL)
                .Generate();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.Users, _ => [adminUser, commonUser])
                .Generate();

            _roomRepositoryMock.GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(room);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Errors.First().ErrorMessage.Should().Be("This user is not in the same room as UserCode.");
        }

        /// <summary>
        /// Tests that the handler returns a failure when the chosen user id and userCode is not the same person
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenChosenUserIdAndUserCodeIsNotTheSamePerson()
        {
            // Arrange
            var request = new DeleteUserRequest("asdfgh", 1);
            var adminUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.Id, _ => 1UL)
                .RuleFor(user => user.AuthCode, _ => "asdfgh")
                .RuleFor(user => user.IsAdmin, _ => true)
                .Generate();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.Users, _ => [adminUser])
                .Generate();

            _roomRepositoryMock.GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(room);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Errors.First().ErrorMessage.Should().Be("Admin cannot delete himself in the room.");
        }

        /// <summary>
        /// Tests that the handler returns a success when the chosen user is deleted successfully
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserIsDeletedSuccessfully()
        {
            // Arrange
            var request = new DeleteUserRequest("asdfgh", 2);
            var adminUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.Id, _ => 1UL)
                .RuleFor(user => user.AuthCode, _ => "asdfgh")
                .RuleFor(user => user.IsAdmin, _ => true)
                .Generate();

            var commonUser = DataFakers.UserFaker
                .RuleFor(user => user.RoomId, _ => 1UL)
                .RuleFor(user => user.Id, _ => 2UL)
                .Generate();

            var room = DataFakers.RoomFaker
                .RuleFor(room => room.Users, _ => [adminUser, commonUser])
                .Generate();

            _roomRepositoryMock.GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(room);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}