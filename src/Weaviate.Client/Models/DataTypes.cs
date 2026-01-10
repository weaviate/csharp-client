namespace Weaviate.Client.Models;

/// <summary>
/// The geo coordinate
/// </summary>
public record GeoCoordinate(float Latitude, float Longitude);

/// <summary>
/// The phone number
/// </summary>
public partial record PhoneNumber(string Input = default!)
{
    /// <summary>
    /// Creates the international using the specified number
    /// </summary>
    /// <param name="number">The number</param>
    /// <returns>The phone number</returns>
    public static PhoneNumber FromInternational(string number) => new PhoneNumber(number);

    /// <summary>
    /// Creates the national using the specified country
    /// </summary>
    /// <param name="country">The country</param>
    /// <param name="number">The number</param>
    /// <returns>The phone number</returns>
    public static PhoneNumber FromNational(string country, string number) =>
        new PhoneNumber(number) { DefaultCountry = country };

    /// <summary>
    /// Read-only. Parsed result in the international format (e.g. +49 123 ...)
    /// </summary>
    public string? InternationalFormatted { get; internal set; } = default!;

    /// <summary>
    /// Optional. The ISO 3166-1 alpha-2 country code. This is used to figure out the correct countryCode and international format if only a national number (e.g. 0123 4567) is provided
    /// </summary>
    public string? DefaultCountry { get; set; } = default!;

    /// <summary>
    /// Read-only. The numerical country code (e.g. 49)
    /// </summary>
    public ulong? CountryCode { get; internal set; } = default!;

    /// <summary>
    /// Read-only. The numerical representation of the national part
    /// </summary>
    public ulong? National { get; internal set; } = default!;

    /// <summary>
    /// Read-only. Parsed result in the national format (e.g. 0123 456789)
    /// </summary>
    public string? NationalFormatted { get; internal set; } = default!;

    /// <summary>
    /// Read-only. Indicates whether the parsed number is a valid phone number
    /// </summary>
    public bool? Valid { get; internal set; } = default!;
}
