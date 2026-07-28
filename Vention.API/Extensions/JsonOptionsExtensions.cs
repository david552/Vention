using System.Text.Json;
using System.Text.Json.Serialization;
using Vention.API.Converters;

namespace Vention.API.Extensions
{
    public static class JsonOptionsExtensions
    {
        public static IMvcBuilder AddVentionJsonOptions(this IMvcBuilder builder)
        {
            return builder.AddJsonOptions(options =>
            {
                var json = options.JsonSerializerOptions;

                json.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                json.PropertyNameCaseInsensitive = true;

                json.NumberHandling = JsonNumberHandling.AllowReadingFromString;

                json.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                json.Converters.Add(new JsonStringEnumConverter(
                    namingPolicy: null,          
                    allowIntegerValues: false));

                json.Converters.Add(new BigIntegerConverter());

                json.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
        }
    }
}
