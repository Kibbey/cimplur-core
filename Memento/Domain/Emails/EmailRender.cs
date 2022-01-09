using RazorEngine;
using RazorEngine.Templating;

namespace Domain.Emails
{
    public class EmailRender
    {
        public static string GetStringFromView(string View, object model){
            return Engine.Razor.RunCompile(View, "templateKey", null, model);
        }
    }
}