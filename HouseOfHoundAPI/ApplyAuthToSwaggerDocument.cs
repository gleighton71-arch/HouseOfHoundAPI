using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;

public class ApplyAuthToSwaggerDocument : IDocumentFilter
{
    public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
    {
        swaggerDoc.security = new List<IDictionary<string, IEnumerable<string>>>
        {
            new Dictionary<string, IEnumerable<string>>
            {
                { "Authorization", new string[] { } }
            }
        };
    }
}