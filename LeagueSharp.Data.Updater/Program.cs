namespace LeagueSharp.Data.Updater
{
    internal class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            new Updater().Run().Wait();
        }

        #endregion
    }
}