namespace ShaclService
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using VDS.RDF;

    public class Parameters
    {
        [Display(Name = "Data graph URI")]
        [DataType(DataType.Url)]
        public Uri DataGraphUri { get; set; }

        [Display(Name = "Data graph RDF")]
        [DataType(DataType.MultilineText)]
        public string DataGraphRdf { get; set; }

        [Display(Name = "Shapes graph URI")]
        [DataType(DataType.Url)]
        public Uri ShapesGraphUri { get; set; }

        [Display(Name = "Shapes graph RDF")]
        [DataType(DataType.MultilineText)]
        public string ShapesGraphRdf { get; set; }

        public string Format { get; set; }

        internal IGraph DataGraph =>
            Load(DataGraphUri, DataGraphRdf);

        internal IGraph ShapesGraph =>
            Load(ShapesGraphUri, ShapesGraphRdf);

        private IGraph Load(Uri uri, string rdf)
        {
            var g = new Graph();

            if (uri is null)
            {
                g.LoadFromString(rdf);
            }
            else
            {
                g.LoadFromUri(uri);
            }

            return g;
        }
    }
}
