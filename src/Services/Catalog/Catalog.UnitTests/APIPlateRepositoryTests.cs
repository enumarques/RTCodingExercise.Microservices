using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.EntityFrameworkCore;
using Catalog.Domain;
using Catalog.API.Data;
using Catalog.API.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;

namespace Catalog.UnitTests
{
    public class APIPlateRepositoryTests
    {
        // set up model isolation
        private PlateRepository UnitUnderTest {get; set;}
        private DbSet<Plate> PlatesDouble {get;}
        private ApplicationDbContext Context {get;}
        private ILogger<PlateRepository> Logger {get;}

        private IQueryable<Plate> MockData {get;}

        public APIPlateRepositoryTests()
        {
            // set up db context
            MockData = GetMockData();

            PlatesDouble = Substitute.For<DbSet<Plate>, IQueryable<Plate>>();

            PlatesDouble.AsQueryable<Plate>().Provider.Returns(MockData.Provider);
            PlatesDouble.AsQueryable<Plate>().Expression.Returns(MockData.Expression);
            PlatesDouble.AsQueryable<Plate>().ElementType.Returns(MockData.ElementType);
            PlatesDouble.AsQueryable<Plate>().GetEnumerator().Returns(MockData.GetEnumerator());

            var options = new DbContextOptions<ApplicationDbContext>();
            Context = Substitute.For<ApplicationDbContext>(options);
            Context.Plates.Returns(PlatesDouble);

            Logger = Substitute.For<ILogger<PlateRepository>>();

            UnitUnderTest = new PlateRepository(Context, Logger);
        }


        // test scenarios
        // request unfiltered list should return page size elements
        [Fact]
        public void GetPlatesShouldReturnPaginatedResults()
        {
            // Given
            const int expectedPageSize = 10;

            // When
            var plates = UnitUnderTest.GetPlates(pageIndex: 0, pageSize: expectedPageSize);

            // Then
            Assert.Equal(MockData.Count(), plates.TotalCount);
            Assert.Equal(expectedPageSize, plates.Items.Count());
        }

        // second page and first page should not repeat elements (check if pagination being applied correctly)
        [Fact]
        public void GetPlatesShouldReturnDifferentResultsOnEachPage()
        {
            // Given
            const int expectedPageSize = 10;

            // When
            var firstPage = UnitUnderTest.GetPlates(pageIndex: 0, pageSize: expectedPageSize);
            var secondPage = UnitUnderTest.GetPlates(pageIndex: 1, pageSize: expectedPageSize);

            // Then
            Assert.Equal(firstPage.TotalCount, secondPage.TotalCount);
            Assert.Equal(1, secondPage.PageIndex);
            Assert.Equal(expectedPageSize, secondPage.Items.Count());
            Assert.All(secondPage.Items, item => Assert.DoesNotContain(item, firstPage.Items));
        }

        // filter results should match filter condition (check filter applied correctly)
        [Theory]
        [InlineData("S", null, 7)]
        [InlineData(null, 33, 4)]
        [InlineData("", -1, 24)]
        [InlineData("S", 2, 1)]
        public void GetPlatesShouldFilterResultsToMatchRequest(string? letterFilter, int? numberFilter, int expectedMatches)
        {
            // Given
            const int pageSize = 5;
            SearchFilters filters = new (){ Letters = letterFilter };
            if (numberFilter is not null) filters.Numbers = (int)numberFilter;

            // When
            var plates = UnitUnderTest.GetPlates(pageIndex: 0, pageSize: pageSize, filters: filters);

            // Then
            var expectedItems = expectedMatches>pageSize? pageSize: expectedMatches;
            Assert.Equal(expectedItems, plates.Items.Count());
            Assert.Equal(expectedMatches, plates.TotalCount);
            if (!string.IsNullOrEmpty(letterFilter))
                Assert.All(plates.Items, item => item.Letters?.Contains(letterFilter));
            if (numberFilter > -1)
                Assert.All(plates.Items, item => Assert.True( item.Numbers == numberFilter ) );
        }

        // check if total count matches number of filtered results
        [Fact]
        public void GetPlatesShouldCountFilteredResults()
        {
            // Given
            const int expectedPageSize = 5;
            SearchFilters filters = new (){ Letters = "S" };

            // When
            var plates = UnitUnderTest.GetPlates(pageIndex: 0, pageSize: expectedPageSize, filters: filters);

            // Then
            Assert.Equal(expectedPageSize, plates.Items.Count());
            Assert.All(plates.Items, item => item.Letters?.Contains(filters.Letters));
        }

        // check if added plate shows up in filtered list
        [Fact]
        public void AddPlateShouldAddPlateToRepository()
        {
            // Given
            var newId = Guid.NewGuid();
            var newPlate = new Plate()
            {
                Id = newId,
                Registration = "C8LYN",
                PurchasePrice = 530.01m,
                SalePrice = 2300.00m,
                Letters = "CAIT",
                Numbers = 8
            };

            // If I have to do all this, this should probably become an integration test at some point
#pragma warning disable EF1001 // Internal EF Core API usage.
            var internalEntityEntry = new InternalEntityEntry(
                Substitute.For<IStateManager>(), 
                new RuntimeEntityType("T", typeof(Plate), false, null, null, null, ChangeTrackingStrategy.Snapshot, null, false),
                newPlate);
#pragma warning restore EF1001 // Internal EF Core API usage.
            var entityEntry = Substitute.For<EntityEntry<Plate>>(internalEntityEntry);
            entityEntry.Entity.Returns(newPlate);

            PlatesDouble.Add(Arg.Any<Plate>()).Returns(entityEntry);

            // When
            var result = UnitUnderTest.AddPlate(newId, newPlate);

            // Then
            Assert.True(result.IsSuccess);
            Assert.IsType<Plate>(result.Value);
        }

        [Fact]
        public void AddPlateShouldCheckPlateDoesNotExistBeforeAdding()
        {
            // Given
            var newId = Guid.NewGuid();
            var newPlate = new Plate()
            {
                Id = newId,
                Registration = "THX 1138",
                PurchasePrice = 530.01m,
                SalePrice = 2300.00m,
                Letters = "THX",
                Numbers = 1138 
            };

            // When
            var result = UnitUnderTest.AddPlate(newId, newPlate);

            // Then
            Assert.False(result.IsSuccess);
            Assert.IsType<ArgumentException>(result.Exception);
        }

        [Fact]
        public void AddPlateShouldApplyValidationRules()
        {
            // Given
            var newId = Guid.NewGuid();
            var newPlate = new Plate()
            {
                Id = newId,
                Registration = "C8LYN",
                PurchasePrice = 2530.01m,
                SalePrice = 2300.00m,
                Letters = "CAIT",
                Numbers = 8
            };

            // When
            var result = UnitUnderTest.AddPlate(newId, newPlate);

            // Then
            Assert.False(result.IsSuccess);
            Assert.IsType<ValidationException>(result.Exception);
        }

        private IQueryable<Plate> GetMockData()
        {
            return new List<Plate>()
            {
                new (){ Registration = "THX 1138", PurchasePrice = 100m, SalePrice = 200m, Letters = "THX", Numbers = 1138, Id = Guid.NewGuid() },
                new (){ Registration = "SLE3T", PurchasePrice = 1000m, SalePrice = 2000m, Letters = "SLE", Numbers = 3, Id = Guid.NewGuid() },
                new (){ Id = new Guid("fe4ae31c-c1a4-4df6-94c3-458525280d76"), Registration = "SPA2E", PurchasePrice = 1037.65m, SalePrice = 14995.00m, Letters = "SPA", Numbers = 2},
                new (){ Id = new Guid("4ab05451-b550-407f-a97c-470175d8d038"), Registration = "B35ANT", PurchasePrice = 626.82m, SalePrice = 9995.00m, Letters = "BES", Numbers = 35},
                new (){ Id = new Guid("5e790bb1-599c-4d5f-9a0f-00a91070ab6d"), Registration = "EVE777E", PurchasePrice = 2832.73m, SalePrice = 9995.00m, Letters = "EVE", Numbers = 777},
                new (){ Id = new Guid("5cd629c6-7cfc-4e1e-8d5b-013635a2f7f0"), Registration = "K325HAW", PurchasePrice = 522.61m, SalePrice = 19995.00m, Letters = "KER", Numbers = 325},
                new (){ Id = new Guid("35e0265d-2473-4797-b412-0186c02babdc"), Registration = "V33REY", PurchasePrice = 981.54m, SalePrice = 8995.00m, Letters = "VER", Numbers = 33},
                new (){ Id = new Guid("9a42f831-35eb-43e3-bcde-0371da997861"), Registration = "P247LEY", PurchasePrice = 416.06m, SalePrice = 14995.00m, Letters = "PRA", Numbers = 247},
                new (){ Id = new Guid("f0415997-6365-45d9-93e3-03ace9ee5e4b"), Registration = "T33ALL", PurchasePrice = 523.97m, SalePrice = 6995.00m, Letters = "TEA", Numbers = 33},
                new (){ Id = new Guid("51d99aa5-71fd-4289-a542-03d730e045eb"), Registration = "FEN8Y", PurchasePrice = 441.77m, SalePrice = 14995.00m, Letters = "FEN", Numbers = 8},
                new (){ Id = new Guid("f9f512c0-87c0-45c4-ac29-0432284842b5"), Registration = "W33MAY", PurchasePrice = 561.43m, SalePrice = 4995.00m, Letters = "MAY", Numbers = 33},
                new (){ Id = new Guid("606f1e69-c199-424f-9d61-04387a2f4822"), Registration = "DUG135S", PurchasePrice = 2961.43m, SalePrice = 6995.00m, Letters = "DUG", Numbers = 135},
                new (){ Id = new Guid("84119004-ae9c-4de5-81a6-0487743818bb"), Registration = "V333RMA", PurchasePrice = 2852.05m, SalePrice = 6995.00m, Letters = "VER", Numbers = 333},
                new (){ Id = new Guid("6bde891e-0fc0-49cc-88d7-04fc09bfafec"), Registration = "B42TON", PurchasePrice = 306.74m, SalePrice = 14995.00m, Letters = "BAR", Numbers = 42},
                new (){ Id = new Guid("d98d9c29-4b0e-4e9c-8350-0550fc1d9a1a"), Registration = "KES53L", PurchasePrice = 3530.77m, SalePrice = 14995.00m, Letters = "KES", Numbers = 53},
                new (){ Id = new Guid("c4e76158-ba7a-4f21-923d-056cf029d491"), Registration = "MEA10R", PurchasePrice = 372.32m, SalePrice = 19995.00m, Letters = "MEA", Numbers = 10},
                new (){ Id = new Guid("d2654a84-54d5-42ec-808c-05e06aec279c"), Registration = "DOB8B", PurchasePrice = 35.00m, SalePrice = 8995.00m, Letters = "DOB", Numbers = 8},
                new (){ Id = new Guid("78db220d-1489-452c-bd6d-05fce3f785aa"), Registration = "WAS13Y", PurchasePrice = 59.13m, SalePrice = 14995.00m, Letters = "WAS", Numbers = 13},
                new (){ Id = new Guid("696be0d6-a1ef-4aba-ade1-069fbcecdf1b"), Registration = "BAS4L", PurchasePrice = 999.57m, SalePrice = 7995.00m, Letters = "ASA", Numbers = 4},
                new (){ Id = new Guid("460cc02b-c970-4e40-82d2-07f9893d8cac"), Registration = "L33AHY", PurchasePrice = 96.70m, SalePrice = 7995.00m, Letters = "LEA", Numbers = 33},
                new (){ Id = new Guid("e8f7ea71-325d-4226-bc83-0813aab92bd0"), Registration = "PEX70N", PurchasePrice = 42.62m, SalePrice = 24995.00m, Letters = "PEX", Numbers = 70},
                new (){ Id = new Guid("52b8e39a-0950-422f-908f-088faba82f43"), Registration = "DOL88Y", PurchasePrice = 1098.97m, SalePrice = 8995.00m, Letters = "DOL", Numbers = 88},
                new (){ Id = new Guid("3b79e459-0009-495d-8ab6-094f9091a30f"), Registration = "FAG8N", PurchasePrice = 877.36m, SalePrice = 14995.00m, Letters = "FAG", Numbers = 8},
                new (){ Id = new Guid("f39bba68-c7fc-4c58-910e-0991c016e13f"), Registration = "VOS93R", PurchasePrice = 7540.03m, SalePrice = 9995.00m, Letters = "VOS", Numbers = 93}
            }.AsQueryable();
        }
    }
}