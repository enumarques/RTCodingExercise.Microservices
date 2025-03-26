using RTCodingExercise.Microservices.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace RTCodingExercise.Microservices.Services
{
    public class PlateServiceClient : IPlateServiceClient
    {
        private HttpClient _httpClient {get;}
        private ILogger<PlateServiceClient> _logger {get;}

        const string addPlatePath = "/api/plate/{0}";
        const string getPlatesPath = "/api/plate/list";

        public PlateServiceClient(HttpClient httpClient, ILogger<PlateServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> AddPlateAsync(Plate newPlate)
        {

            _logger.LogInformation("Adding plate {PlateId} with registration {Registration}", newPlate.Id.ToString(), newPlate.Registration);
            var response = await _httpClient.PostAsJsonAsync(string.Format(addPlatePath, newPlate.Id.ToString()), newPlate);
            return response.IsSuccessStatusCode;
        }

        public async Task<PaginatedItemsServiceResponse<Plate>> GetPlatesAsync(
            int pageSize,
            int pageIndex,
            string? sortField,
            SortOrder sortOrder,
            PlateFilter? filter = null
        )
        {
            _logger.LogInformation("Calling plate storage web service");
            var queryParams = new Dictionary<string, string?>()
            {
                { "pageSize", $"{pageSize}" },
                { "pageIndex", $"{pageIndex}" }
            };
            if (! string.IsNullOrEmpty(sortField))
            {
                queryParams.Add("sortField", sortField);
                queryParams.Add("sortOrder", sortOrder.ToString());
            }
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Numbers)) queryParams.Add("numberFilter", filter.Numbers);
                if (!string.IsNullOrEmpty(filter.Letters)) queryParams.Add("letterFilter", filter.Letters);
            }

            var url = QueryHelpers.AddQueryString(getPlatesPath, queryParams);
            _logger.LogInformation("Calling service at {Url}", url);

            var response = await _httpClient.GetAsync(url);
            _logger.LogInformation("Got API response, {ResponseContent}", await response.Content.ReadAsStringAsync());

            var paginatedPlates = await response.Content.ReadFromJsonAsync<PaginatedItemsServiceResponse<Plate>>();
            if (paginatedPlates == null)
            {
                throw new NullReferenceException("Could not parse service response");
            }
            _logger.LogInformation("Retrieved {PageSize} results from {TotalResults}", paginatedPlates.PageSize, paginatedPlates.TotalCount);
            return paginatedPlates ?? new PaginatedItemsServiceResponse<Plate>();
        }
    }
}