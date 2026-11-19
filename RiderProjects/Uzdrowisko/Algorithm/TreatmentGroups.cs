namespace Algorithm;

using Uzdrowisko.Infrastructure.DataBase.Entities;

// TEMPORARY - remove after adding categories and custom groups to database
public class TreatmentGroups
{
    public Dictionary<int, string> CategoryBasedGroups { get; }

    public Dictionary<int, (string Name, List<Treatment> Treatments)> CustomGroups { get; }

    private int NewCategoryIndex { get; set; }

    private int NewCustomGroupIndex { get; set; }

    public TreatmentGroups()
    {
        // TEMPORARY - change initialization after adding support for custom groups in database
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
        // END OF TEMPORARY PART
    }

    // Methods below are only for testing
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