using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using VDS.RDF;

namespace ShaclService
{
    internal class GraphInputFormatter : TextInputFormatter
    {
        private readonly Action<IGraph, TextReader> read;

        public GraphInputFormatter(string mediaType, Action<IGraph, TextReader> read)
        {
            this.read = read;
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            this.SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var g = new Graph();

            using (var reader = context.ReaderFactory(context.HttpContext.Request.Body, encoding))
            {
                this.read(g, reader);
            }

            return InputFormatterResult.SuccessAsync(g);
        }

        protected override bool CanReadType(Type type)
        {
            return typeof(IGraph).IsAssignableFrom(type);
        }
    }
}