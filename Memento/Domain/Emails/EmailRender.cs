using RazorLight;
using System.Threading.Tasks;

namespace Domain.Emails
{
    public class EmailRender
    {
        public async static Task<string> GetStringFromView(string key, string view, object model) {
            var razorEngine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(EmailRender)) // exception without this (or another project type)
                .UseMemoryCachingProvider()
                .Build();
            return await razorEngine.CompileRenderStringAsync(key, view, model);
        }
    }
}