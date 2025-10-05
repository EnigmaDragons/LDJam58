using System;
using System.Collections.Generic;
using System.Linq;

public static class VisitorGenerator
{
    //TODO: move to global rules
    private static int minPeople = 1;
    private static int maxPeople = 4;
    private static int groupFascinations = 3;
    private static int groupDisinterests = 2;

    private static ExhibitTag[] allTags;

    private static ExhibitTag[] AllTags()
    {
        if (allTags == null)
            allTags = Enum.GetValues(typeof(ExhibitTag)).Cast<ExhibitTag>().Where(x => x != ExhibitTag.None).ToArray();
        return allTags;
    }

    public static Group Generate(ExhibitTag fascination, HashSet<ExhibitTag> exhibitTags)
    {
        var takeAmount = fascination == ExhibitTag.None ? groupFascinations : groupFascinations - 1;
        var fascinations = allTags.Where(x => fascination != x).TakeRandom(takeAmount).ToList();
        if (fascination != ExhibitTag.None)
            fascinations.Add(fascination);
        var disinterests = allTags.Where(x => !exhibitTags.Contains(x)).TakeRandom(groupDisinterests).ToList();
        return new Group { peopleCount = Rng.Int(minPeople, maxPeople + 1), Fascinations = fascinations.ToArray(), Disinterests = disinterests.ToArray() };
    }
}