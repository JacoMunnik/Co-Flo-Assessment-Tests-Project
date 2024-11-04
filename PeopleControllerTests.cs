using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoFloAssessmentTestsProject
{
    public class PeopleControllerTests
    {
        private readonly PeopleContext _context;
        private readonly Mock<ILogger<PeopleController>> _mockLogger;
        private readonly PeopleController _controller;

        public PeopleControllerTests()
        {
            var options = new DbContextOptionsBuilder<PeopleContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new PeopleContext(options: options);
            _mockLogger = new Mock<ILogger<PeopleController>>();
            _controller = new PeopleController(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task GetPeople_ReturnsOkResult_WithListOfPeople()
        {
            // Arrange
            var people = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1990, 1, 1) },
                new Person { Id = 2, FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(1992, 2, 2) }
            };
            _context.People.AddRange(entities: people);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetPeople();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(@object: result);
            var returnValue = Assert.IsType<List<Person>>(@object: okResult.Value);
            Assert.Equal(expected: 2, actual: returnValue.Count);
        }

        [Fact]
        public async Task GetPerson_ReturnsNotFound_WhenPersonDoesNotExist()
        {
            // Act
            var result = await _controller.GetPerson(1);

            // Assert
            Assert.IsType<NotFoundResult>(@object: result);
        }

        [Fact]
        public async Task CreatePerson_ReturnsBadRequest_WhenPersonIsNull()
        {
            // Act
            var result = await _controller.CreatePerson(null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(@object: result);
        }

        [Fact]
        public async Task CreatePerson_ReturnsCreatedAtAction_WhenPersonIsCreated()
        {
            // Arrange
            var person = new Person { Id = 1, FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1990, 1, 1), DateCreated = DateTime.UtcNow };

            // Act
            var result = await _controller.CreatePerson(person: person);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(@object: result);
            var returnValue = Assert.IsType<Person>(@object: createdAtActionResult.Value);
            Assert.Equal(expected: person.Id, actual: returnValue.Id);
        }

        [Fact]
        public async Task UpdatePerson_ReturnsNoContent_WhenPersonIsUpdated()
        {
            // Arrange
            var person = new Person { Id = 1, FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1990, 1, 1), DateCreated = DateTime.UtcNow };
            _context.People.Add(entity: person);
            await _context.SaveChangesAsync();

            var updatedPerson = new Person { Id = 1, FirstName = "John", LastName = "Smith", DateOfBirth = new DateTime(1990, 1, 1), DateCreated = DateTime.UtcNow };

            // Act
            var result = await _controller.UpdatePerson(id: 1, person: updatedPerson);

            // Assert
            Assert.IsType<NoContentResult>(@object: result);
            var personInDb = await _context.People.FindAsync(1);
            Assert.Equal(expected: "Smith", actual: personInDb.LastName);
        }

        [Fact]
        public async Task UpdatePerson_ReturnsNotFound_WhenPersonDoesNotExist()
        {
            // Arrange
            var updatedPerson = new Person { Id = 1, FirstName = "John", LastName = "Smith", DateOfBirth = new DateTime(1990, 1, 1), DateCreated = DateTime.UtcNow };

            // Act
            var result = await _controller.UpdatePerson(id: 1, person: updatedPerson);

            // Assert
            Assert.IsType<NotFoundResult>(@object: result);
        }

        [Fact]
        public async Task DeletePerson_ReturnsNoContent_WhenPersonIsDeleted()
        {
            // Arrange
            var person = new Person { Id = 1, FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1990, 1, 1), DateCreated = DateTime.UtcNow };
            _context.People.Add(entity: person);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeletePerson(1);

            // Assert
            Assert.IsType<NoContentResult>(@object: result);
            var personInDb = await _context.People.FindAsync(1);
            Assert.Null(@object: personInDb);
        }

        [Fact]
        public async Task DeletePerson_ReturnsNotFound_WhenPersonDoesNotExist()
        {
            // Act
            var result = await _controller.DeletePerson(1);

            // Assert
            Assert.IsType<NotFoundResult>(@object:  result);
        }
    }
}
