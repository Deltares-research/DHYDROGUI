namespace DelftTools.Hydro
{
    public enum Friction
    {
        Chezy = 1,
        TabbedDischarge = 2,
        TabbedLevel = 3,
        //Mannings = 4,
        Manning = 4,
        //Nikuradse = 5,
        StricklerNikuradse = 5,
        Strickler = 6,
        WhiteColebrook = 7,
        //BosBijkerk = 9,
        DeBosBijkerk = 9,
        WallLawNikuradse = 10
    }
}