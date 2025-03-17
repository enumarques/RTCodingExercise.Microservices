using System.ComponentModel.DataAnnotations;
using System.Net;
using Catalog.API.Models;

namespace Catalog.API.Controllers 
{
    [Route("/api/plate")]
    [ApiController]
    public class PlateController : Controller
    {
        private ILogger<PlateController> _logger {get;set;}
        private IPlateRepository _plateRepository {get; }

        public PlateController(IPlateRepository repository, ILogger<PlateController> logger)
        {
            _plateRepository = repository;
            _logger = logger;
        }

        [HttpGet]
        [Route("list")]
        [ProducesResponseType(typeof(PaginatedPlates), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ListAsync(
            [FromQuery]int pageSize = 20,
            [FromQuery]int pageIndex = 0,
            [FromQuery]string? sortField = null,
            [FromQuery]SortOrder sortOrder = SortOrder.Unspecified
        )
        {
            _logger.LogInformation("Retrieving {PageSize} plates from page {PageIndex} by {SortField}", pageSize, pageIndex, sortField ?? "no order");
            return Ok(await _plateRepository.GetPlatesAsync(pageIndex, pageSize, sortField, sortOrder));
        }

        [HttpPost]
        [Route("{Id}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> AddPlateAsync(
            [FromRoute]Guid Id,
            [FromBody]Plate plateData
        )
        {
            var result = await _plateRepository.AddPlateAsync(Id, plateData);

            if (!result.IsSuccess)
            {
                if (result.Exception is ValidationException)
                {
                    return BadRequest(result.Exception.Message);
                }
                if (result.Exception is ArgumentException)
                {
                    return Conflict(result.Exception.Message);
                }
            }

            return Created($"/api/plate/{Id}", result.Value);
        }


    }

}
