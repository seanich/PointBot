namespace PointBot.Data
{
    public static class DbInitializer
    {
        public static void Initialize(PointBotContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}