using System.Globalization;
using System.Text.Json;
using HomemadeFood.Api.DTOs.Address;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public sealed class GoogleGeocodingService : IGoogleGeocodingService
    {
        private const string FieldMask =
            "results.formattedAddress," +
            "results.addressComponents," +
            "results.types," +
            "results.granularity";

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoogleGeocodingService> _logger;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public GoogleGeocodingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleGeocodingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiKey =
                configuration["GoogleMaps:GeocodingApiKey"]
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException(
                    "GoogleMaps:GeocodingApiKey değeri bulunamadı.");
            }
        }

        public async Task<ReverseGeocodeResponse?> ReverseGeocodeAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            var latitudeText =
                latitude.ToString(
                    "R",
                    CultureInfo.InvariantCulture);

            var longitudeText =
                longitude.ToString(
                    "R",
                    CultureInfo.InvariantCulture);

            var requestUri =
                "v4/geocode/location" +
                "?location.latitude=" +
                latitudeText +
                "&location.longitude=" +
                longitudeText +
                "&languageCode=tr" +
                "&regionCode=TR";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    requestUri);

            // API key URL'e yazılmaz; header ile gönderilir.
            request.Headers.TryAddWithoutValidation(
                "X-Goog-Api-Key",
                _apiKey);

            request.Headers.TryAddWithoutValidation(
                "X-Goog-FieldMask",
                FieldMask);

            using var response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google Geocoding isteği başarısız. HTTP status: {StatusCode}",
                    (int)response.StatusCode);

                throw new HttpRequestException(
                    "Google Geocoding servisi başarısız cevap döndürdü.",
                    inner: null,
                    response.StatusCode);
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            GoogleGeocodeLocationResponse? googleResponse;

            try
            {
                googleResponse =
                    await JsonSerializer.DeserializeAsync<
                        GoogleGeocodeLocationResponse>(
                            stream,
                            JsonOptions,
                            cancellationToken);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Google Geocoding cevabı ayrıştırılamadı.");

                throw new HttpRequestException(
                    "Google Geocoding cevabı işlenemedi.",
                    exception);
            }

            var result =
                googleResponse?
                    .Results?
                    .FirstOrDefault(
                        item =>
                            item.AddressComponents
                                is { Count: > 0 });

            if (result == null)
            {
                return null;
            }

            var components =
                result.AddressComponents ?? [];

            var city =
                FindComponent(
                    components,
                    "administrative_area_level_1")
                ?? string.Empty;

            var district =
                FindComponent(
                    components,
                    "administrative_area_level_2",
                    "locality",
                    "administrative_area_level_3")
                ?? string.Empty;

            if (
                district.Equals(
                    city,
                    StringComparison.OrdinalIgnoreCase))
            {
                district =
                    FindComponent(
                        components,
                        "locality",
                        "administrative_area_level_3")
                    ?? string.Empty;
            }

            var neighborhood =
                FindComponent(
                    components,
                    "neighborhood",
                    "sublocality_level_1",
                    "sublocality",
                    "administrative_area_level_4",
                    "administrative_area_level_3")
                ?? string.Empty;

            if (
                neighborhood.Equals(
                    district,
                    StringComparison.OrdinalIgnoreCase))
            {
                neighborhood =
                    FindComponent(
                        components,
                        "neighborhood",
                        "sublocality_level_1",
                        "sublocality",
                        "administrative_area_level_4")
                    ?? string.Empty;
            }

            return new ReverseGeocodeResponse
            {
                City = city,
                District = district,
                Neighborhood = neighborhood,

                Street =
                    FindComponent(
                        components,
                        "route")
                    ?? string.Empty,

                BuildingNo =
                    FindComponent(
                        components,
                        "street_number")
                    ?? string.Empty,

                FormattedAddress =
                    result.FormattedAddress
                    ?? string.Empty,

                Granularity =
                    result.Granularity
                    ?? string.Empty
            };
        }

        private static string? FindComponent(
            IReadOnlyCollection<GoogleAddressComponent> components,
            params string[] types)
        {
            foreach (var type in types)
            {
                var component =
                    components.FirstOrDefault(
                        item =>
                            item.Types?
                                .Contains(
                                    type,
                                    StringComparer.Ordinal)
                            == true);

                if (
                    !string.IsNullOrWhiteSpace(
                        component?.LongText))
                {
                    return component.LongText.Trim();
                }
            }

            return null;
        }

        private sealed class GoogleGeocodeLocationResponse
        {
            public List<GoogleGeocodeResult>? Results { get; set; }
        }

        private sealed class GoogleGeocodeResult
        {
            public string? FormattedAddress { get; set; }
            public string? Granularity { get; set; }
            public List<GoogleAddressComponent>? AddressComponents { get; set; }
            public List<string>? Types { get; set; }
        }

        private sealed class GoogleAddressComponent
        {
            public string? LongText { get; set; }
            public string? ShortText { get; set; }
            public List<string>? Types { get; set; }
            public string? LanguageCode { get; set; }
        }
    }
}