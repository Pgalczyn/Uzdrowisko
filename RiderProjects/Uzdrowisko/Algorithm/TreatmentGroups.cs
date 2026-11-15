namespace Algorithm;

using Uzdrowisko.Infrastructure.DataBase.Entities;

public class TreatmentGroups
{
    public Dictionary<int, string> CategoryBasedGroups { get; set; }

    public Dictionary<int, (string Name, List<Treatment> Treatments)> CustomGroups { get; set; }

    public int NewCategoryIndex { get; set; }

    public int NewCustomGroupIndex { get; set; }

    public TreatmentGroups()
    {
        CategoryBasedGroups = new Dictionary<int, string>
        {
            { 0, "all" },
            { 1, "kinezytherapy" },
            { 2, "thermotherapy" },
            { 3, "gymnastics" },
            { 4, "massage" },
            { 5, "balneotherapy" },
            { 6, "physicotherapy" },
            { 7, "hydrotherapy" },
            { 8, "other" }
        };

        NewCategoryIndex = CategoryBasedGroups.Keys.Max() + 1;

        CustomGroups = new Dictionary<int, (string Name, List<Treatment> Treatments)>
        {
            { -1, ("electrotherapy", new List<Treatment>()) }
        };

        NewCustomGroupIndex = CategoryBasedGroups.Keys.Min() - 1;
    }

    public void AddNewCategory(string categoryName)
    {
        CategoryBasedGroups[NewCategoryIndex] = categoryName;
        NewCategoryIndex++;
    }

    public void AddNewCustomGroup(string groupName)
    {
        CustomGroups[NewCustomGroupIndex] = (groupName, new List<Treatment>());
        NewCustomGroupIndex--;
    }

    public void AddTreatmentToCustomGroup(Treatment treatment, int groupId)
    {
        if (groupId <= NewCustomGroupIndex || !CustomGroups.ContainsKey(groupId))
        {
            throw new ArgumentException("No group with given id");
        }

        if (!CustomGroups[groupId].Treatments.Contains(treatment))
        {
            CustomGroups[groupId].Treatments.Add(treatment);
        }
    }
}