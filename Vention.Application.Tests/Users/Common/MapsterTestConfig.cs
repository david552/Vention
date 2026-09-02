using Mapster;

namespace Vention.Application.Tests.Users.Common
{
    public static class MapsterTestConfig
    {
        private static bool _configured;

        public static void EnsureConfigured()
        {
            if (_configured)
                return;

            TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
            _configured = true;
        }
    }
}