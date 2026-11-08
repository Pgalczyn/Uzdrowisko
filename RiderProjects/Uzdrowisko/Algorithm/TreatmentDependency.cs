namespace Algorithm;

public class TreatmentDependency
{
    public string DbFlag { get; set; }
    public DependencyType Type { get; set; }
    public string Target { get; set; }
    public int Id { get; set; }

    public TreatmentDependency(string dbFlag)
    {
        DbFlag = dbFlag;

        (Type, Target, Id) = ParseDependencyParameters(dbFlag);
    }

    public override string ToString()
    {
        return DbFlag;
    }

    public static (DependencyType type, string target, int id) ParseDependencyParameters(string dbFlag)
    {
        string[] parameters = dbFlag.Split('_');

        if (!int.TryParse(parameters[0], out int typeValue) ||
            !Enum.IsDefined(typeof(DependencyType), typeValue))
        {
            throw new ArgumentException("Invalid dependency type");
        }

        DependencyType type = (DependencyType)typeValue;

        string target = parameters[1];

        if (target != "S" && target != "G")
        {
            throw new ArgumentException("Invalid Flag: second flag parameter must be 'S' or 'G'");
        }

        int id = int.Parse(parameters[2]);

        return (type, target, id);
    }
}