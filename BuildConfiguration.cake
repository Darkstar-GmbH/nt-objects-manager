public class BuildConfiguration
{
	public string Target { get; private set; }
	public string Architecture { get; private set; }
    public string Configuration { get; private set; }
    public List<string> Frameworks {get; set;}

    public static BuildConfiguration New()
    {
        return new BuildConfiguration();
    }

    public BuildConfiguration SetTarget(string target) 
    {
        this.Target = target;
        return this;
    }

    public BuildConfiguration SetArchitecture(string architecture) 
    {
		this.Architecture = architecture;
        return this;
    }

    public BuildConfiguration SetConfiguration(string configuration) 
    {
        this.Configuration = configuration;
        return this;
    }

    public BuildConfiguration SetFrameworks(string[] frameworks) 
    {
        this.Frameworks = new List<string>(frameworks);
        return this;
    }
}