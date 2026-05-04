namespace Api.Models;

public sealed record NationalPark(
    int Id,
    string Name,
    string State,
    int EstablishedYear,
    int Acres,
    string Region,
    string Highlight,
    IReadOnlyList<string> Activities);
