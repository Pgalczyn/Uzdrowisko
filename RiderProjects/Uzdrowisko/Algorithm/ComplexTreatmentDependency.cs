namespace Algorithm;

using System.Linq;
public class ComplexTreatmentDependency
{
    public string? DbFlag { get; set; }
    public List<string> SingularDependenciesFlags { get; set; }

    public ComplexTreatmentDependency(string dbFlag)
    {
        DbFlag = dbFlag;
        SingularDependenciesFlags = dbFlag == null
            ? new List<string>()
            : dbFlag.Split(';').ToList();
    }

    public override string ToString()
    {
        return DbFlag == null ? "" : DbFlag;
    }
}