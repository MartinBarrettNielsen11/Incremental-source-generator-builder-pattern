namespace Benchmarks.Simple;

public class ModelBase
{
    public string CreatedBy { get; set; }
}

public class Country
{
    public string CountryName { get; set; }
}

public class Citizen : ModelBase
{
    public Citizen()
    {
        Countries = new List<Country>();
        OtherCountries = new List<Country>();
    }
	
    public Guid CitizenId { get; set; }
    public string Name { get; set; }
    public bool ModifiedByPostBuildAction { get; set; }
    public IList<Country> Countries { get; }
    public IList<Country> OtherCountries { get; }

    public int CountryCount
    {
        get
        {
            return Countries.Count;
        }
    }

    public string Case { get; set; }
    public string Build { get; set; }
}


[Builder(typeof(Citizen))]
public partial class CitizenTestBuilder
{
    public CitizenTestBuilder()
    {
        _postBuildActions
            .Add(citizen => { citizen.ModifiedByPostBuildAction = true; });
    }
}