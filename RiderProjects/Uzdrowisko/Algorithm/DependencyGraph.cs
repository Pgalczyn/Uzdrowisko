namespace Algorithm;

using Uzdrowisko.Infrastructure.DataBase.Entities;

public class DependencyGraph
{
    public Dictionary<Treatment, List<TreatmentDependency>> Graph { get; set; }

    public DependencyGraph(List<Treatment> treatmentList)
    {
        Graph = new Dictionary<Treatment, List<TreatmentDependency>>();

        foreach (Treatment treatment in treatmentList)
        {
            Graph[treatment] = new List<TreatmentDependency>();

            ComplexTreatmentDependency complexDependency =
                new ComplexTreatmentDependency(treatment.Flag);

            foreach (string singularFlag in complexDependency.SingularDependenciesFlags)
            {
                TreatmentDependency dependency = new TreatmentDependency(singularFlag);
                Graph[treatment].Add(dependency);
            }
        }
    }
}