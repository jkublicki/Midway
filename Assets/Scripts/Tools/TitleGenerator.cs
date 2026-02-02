using System.Collections.Generic;
using System.Linq;

public static class TitleGenerator
{
    private static List<string> times = new List<string> { "Sunrise", "Dawn", "Noon", "Midday", "Dusk", "Twilight" };
    private static List<string> actions = new List<string> { "Launch", "Scout", "Search", "Contact", "Tallyho", "Vector", "Launch All", "Strike",
        "Attack", "Intercept", "Rodeo", "Hunt", "Hey Rube", "Bogey", "Bandit Alert", "Dogfight", "Scramble", "Circus" };
    private static List<string> locations = new List<string> { "Tulagi", "Louisiades", "Point Buttercup", "Point Rye", "Point Option", "Misima", "Radar",
        "Shadows", "Jomard", "Deboyne", "Tagula", "Rossel", "Sudest", "Port Moresby", "Rabaul", "Woodlark", "Milne Bay", "Shortland", "Townsville" };

    private const float timeChance = 0.8f;
    private const float randomTimeChance = 0.6f;
    private const float longChance = 0.6f;

    public static string GetTitle()
    {
        string title = "";

        if (UnityEngine.Random.Range(0.0f, 1.0f) <= timeChance)
        {
            if (UnityEngine.Random.Range(0.0f, 1.0f) <= randomTimeChance)
            {
                title += UnityEngine.Random.Range(5, 20).ToString("00") + UnityEngine.Random.Range(0, 59).ToString("00") + " ";
            }
            else
            {
                title += times[UnityEngine.Random.Range(0, times.Count())] + " ";
            }
        }

        title += actions[UnityEngine.Random.Range(0, actions.Count())];


        if (UnityEngine.Random.Range(0.0f, 1.0f) <= longChance || title.Split(" ").Length < 2)
        {
            title += " " + locations[UnityEngine.Random.Range(0, locations.Count())];
        }

        return title;
    }



}
