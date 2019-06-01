namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using VDS.RDF;
    using VDS.RDF.Writing;

    internal class GraphOutputFormatter : TextOutputFormatter
    {
        public GraphOutputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/turtle"));
            SupportedEncodings.Add(new UTF8Encoding(false));
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IGraph).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
        {
            return new TaskFactory().StartNew(() =>
            {
                var g = (IGraph)context.Object;

                using (var writer = context.WriterFactory(context.HttpContext.Response.Body, encoding))
                {
                    new CompressingTurtleWriter().Save(g, writer);
                }
            });
        }
    }
}