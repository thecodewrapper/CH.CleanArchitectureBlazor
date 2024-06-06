using Newtonsoft.Json.Linq;

namespace CH.CleanArchitecture.Presentation.WebApp.Helpers
{
    public static class JsonStringHelper
    {
        public static string FormatJSONForView(string json) {
            try {
                JToken jt = JToken.Parse(json);
                return jt.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch {
                return json;
            }
        }
    }
}
