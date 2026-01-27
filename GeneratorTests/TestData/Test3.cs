namespace GeneratorTests.TestData;

public partial class Country
{
    public long Id { get; }
    public string CountryName { get; set; }
}

public partial class Country
{
    public void UpdateCountryName(string countryName)
    {
        CountryName = countryName;
    }
}

[Builder(typeof(Country))]
public partial class EntityBuilder { }