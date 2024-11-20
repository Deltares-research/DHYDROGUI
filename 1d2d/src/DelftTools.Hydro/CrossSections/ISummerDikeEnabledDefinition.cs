namespace DelftTools.Hydro.CrossSections
{
    /// <summary>
    /// Interface indicating that the definition might be able to have a summerdike.
    /// This is implemented by ZW and Proxy.
    /// For Proxy the summerdike can only be used if the innerdefinition is ZW
    /// </summary>
    public interface ISummerDikeEnabledDefinition
    {
        bool CanHaveSummerDike { get; }

        SummerDike SummerDike { get;  }
    }
}