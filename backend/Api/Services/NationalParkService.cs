using Api.Models;

namespace Api.Services;

public sealed class NationalParkService : INationalParkService
{
    private static readonly IReadOnlyList<NationalPark> Parks =
    [
        new(1, "Acadia National Park", "ME", 1919, 49071, "Northeast", "Granite peaks, coastal carriage roads, and sunrise views from Cadillac Mountain.", ["Hiking", "Biking", "Tidepooling"]),
        new(2, "Arches National Park", "UT", 1971, 76679, "Intermountain", "More than 2,000 natural sandstone arches shaped by desert erosion.", ["Scenic drives", "Photography", "Stargazing"]),
        new(3, "Everglades National Park", "FL", 1947, 1508538, "Southeast", "A subtropical wetland protecting sawgrass marsh, mangroves, and rare wildlife.", ["Paddling", "Birding", "Wildlife viewing"]),
        new(4, "Grand Canyon National Park", "AZ", 1919, 1201647, "Intermountain", "Layered rock walls reveal immense geologic time along the Colorado River.", ["Hiking", "Rafting", "Scenic overlooks"]),
        new(5, "Great Smoky Mountains National Park", "TN", 1934, 522427, "Southeast", "Biodiverse forests, historic cabins, and misty Appalachian ridgelines.", ["Hiking", "History", "Wildflower walks"]),
        new(6, "Joshua Tree National Park", "CA", 1994, 795156, "Pacific West", "Mojave and Colorado Desert ecosystems meet among boulder fields and Joshua trees.", ["Climbing", "Camping", "Stargazing"]),
        new(7, "Rocky Mountain National Park", "CO", 1915, 265807, "Intermountain", "Alpine tundra, glacial lakes, and Trail Ridge Road above 12,000 feet.", ["Hiking", "Wildlife viewing", "Snowshoeing"]),
        new(8, "Yellowstone National Park", "WY", 1872, 2219791, "Intermountain", "The first national park protects geysers, hot springs, bison, wolves, and vast wilderness.", ["Geyser walks", "Wildlife viewing", "Backpacking"]),
        new(9, "Yosemite National Park", "CA", 1890, 761747, "Pacific West", "Granite cliffs, waterfalls, giant sequoias, and the Yosemite Valley landscape.", ["Hiking", "Climbing", "Waterfall viewing"]),
        new(10, "Zion National Park", "UT", 1919, 147243, "Intermountain", "Sandstone canyons and the Virgin River create one of Utah's landmark landscapes.", ["Canyoneering", "Hiking", "Scenic shuttle"])
    ];

    public IReadOnlyList<NationalPark> GetAll() => Parks;

    public NationalPark? GetById(int id) => Parks.FirstOrDefault(park => park.Id == id);

    public IReadOnlyList<NationalPark> Search(string? state, string? query)
    {
        var results = Parks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(state))
        {
            results = results.Where(park => park.State.Equals(state, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(park =>
                park.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                park.Highlight.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                park.Activities.Any(activity => activity.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        return results.ToList();
    }
}
