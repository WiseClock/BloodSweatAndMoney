using System.Collections.Generic;

public class BuildingType
{
    public static readonly Dictionary<string, BuildingType> List = new Dictionary<string, BuildingType>();
    
    public string Name { get; }
    public int AmountOfWork { get; }
    public int WorkersNeeded { get; }
    public float Cohesion { get; }
    public float MoneyPower { get; }
    public float Stability { get; }
    public int Width { get; }
    public int Height { get; }

    public static void Create(string name, int amountOfWork, int workersNeeded, float cohesion, float moneyPower, float stability, int width = 1, int height = 1)
    {
        BuildingType bt = new BuildingType(name, amountOfWork, workersNeeded, cohesion, moneyPower, stability, width, height);
        List[name] = bt;
    }
    
    private BuildingType(string name, int amountOfWork, int workersNeeded, float cohesion, float moneyPower, float stability, int width, int height)
    {
        Name = name;
        AmountOfWork = amountOfWork;
        WorkersNeeded = workersNeeded;
        Cohesion = cohesion;
        MoneyPower = moneyPower;
        Stability = stability;
        Width = width;
        Height = height;
    }
}