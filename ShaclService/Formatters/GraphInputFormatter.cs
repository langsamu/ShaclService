namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using VDS.RDF;
    using VDS.RDF.Parsing;

    internal class GraphInputFormatter : TextInputFormatter
    {
        public GraphInputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/turtle"));
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        }

        protected override bool CanReadType(Type type)
        {
            return typeof(IGraph).IsAssignableFrom(type);
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var g = new Graph();

            using (var reader = new StreamReader(context.HttpContext.Request.Body, encoding))
            {
                new TurtleParser().Load(g, reader);
            }

            return InputFormatterResult.SuccessAsync(g);
        }
    }
}