using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Catalog.API.Controllers;
using Catalog.API.Models;
using Catalog.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Catalog.UnitTests
{
    public class APIPlateControllerTests
    {
        private PlateController UnitUnderTest {get;}
        private IPlateRepository PlateRepository {get;}
        private ILogger<PlateController> Logger {get;}

        public APIPlateControllerTests()
        {
            PlateRepository = Substitute.For<IPlateRepository>();
            Logger = Substitute.For<ILogger<PlateController>>();

            UnitUnderTest = new PlateController(PlateRepository, Logger);
        }

        [Theory]
        [InlineData( 20, 0, null, SortOrder.Unspecified, null, null )]
        [InlineData( 20, 0, "Price", SortOrder.Ascending, null, null )]
        [InlineData( 20, 0, "Price", SortOrder.Descending, null, null )]
        [InlineData( 20, 0, null, SortOrder.Unspecified, "T", null )]
        [InlineData( 20, 0, null, SortOrder.Ascending, null, "3" )]
        [InlineData( 20, 0, "Price", SortOrder.Ascending, "C", "3" )]
        public void ListReturnsPaginatedPlates(int pageSize, int pageIndex, string? sortField, SortOrder sortOrder, string? letterFilter, string? numberFilter)
        {
            // Given
            PlateRepository.GetPlates(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<SortOrder>(), Arg.Any<SearchFilters>())
                .Returns(new PaginatedPlates(new List<Plate>(), 10, pageSize, pageIndex, sortField, sortOrder));

            // When
            var response = UnitUnderTest.List(pageSize, pageIndex, sortField, sortOrder, letterFilter, numberFilter);
            var result = response as ObjectResult;
            
            // Then
            PlateRepository.ReceivedWithAnyArgs(1).GetPlates(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<SortOrder>(), Arg.Any<SearchFilters>());
            Assert.True(result is OkObjectResult);
            Assert.IsType<PaginatedPlates>(result?.Value);
        }

        [Fact]
        public void AddPlateCallsRepositoryAndReturnsCreatedIfInsertionWorked()
        {
            // Given
            var plate = new Plate()
            {
                Id = Guid.NewGuid(),
                Registration = "TR46ICC",
                PurchasePrice = 500m,
                SalePrice = 750m,
                Letters = "TRAG",
                Numbers = 46
            };
            PlateRepository.AddPlate(Arg.Any<Guid>(), Arg.Any<Plate>())
                .Returns(new Result<Plate>(plate));

            // When
            var response = UnitUnderTest.AddPlate(plate.Id, plate);
            var result = response as ObjectResult;
            
            // Then
            PlateRepository.Received(1).AddPlate(plate.Id, plate);
            Assert.True(result is CreatedResult);
            Assert.IsType<Plate>(result?.Value);
        }

        [Fact]
        public void AddPlateCallsRepositoryAndReturnsConflictIfRegistrationExists()
        {
            // Given
            var plate = new Plate()
            {
                Id = Guid.NewGuid(),
                Registration = "TR46ICC",
                PurchasePrice = 500m,
                SalePrice = 750m,
                Letters = "TRAG",
                Numbers = 46
            };
            PlateRepository.AddPlate(Arg.Any<Guid>(), Arg.Any<Plate>())
                .Returns(new Result<Plate>(new ArgumentException()));

            // When
            var response = UnitUnderTest.AddPlate(plate.Id, plate);
            var result = response as ObjectResult;
            
            // Then
            PlateRepository.Received(1).AddPlate(plate.Id, plate);
            Assert.True(result is ConflictObjectResult);
        }

        [Fact]
        public void AddPlateCallsRepositoryAndReturnsBadRequestIfValidationFails()
        {
            // Given
            var plate = new Plate()
            {
                Id = Guid.NewGuid(),
                Registration = "TR46ICC",
                PurchasePrice = 500m,
                SalePrice = 750m,
                Letters = "TRAG",
                Numbers = 46
            };
            PlateRepository.AddPlate(Arg.Any<Guid>(), Arg.Any<Plate>())
                .Returns(new Result<Plate>(new ValidationException()));

            // When
            var response = UnitUnderTest.AddPlate(plate.Id, plate);
            var result = response as ObjectResult;
            
            // Then
            PlateRepository.Received(1).AddPlate(plate.Id, plate);
            Assert.True(result is BadRequestObjectResult);
        }
    }    
}
