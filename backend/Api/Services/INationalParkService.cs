using Api.Models;

namespace Api.Services;

public interface INationalParkService
{
    IReadOnlyList<NationalPark> GetAll();

    NationalPark? GetById(int id);

    IReadOnlyList<NationalPark> Search(string? state, string? query);
}
