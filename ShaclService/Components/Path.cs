namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF;

    public class Path : ViewComponent
    {
        public IViewComponentResult Invoke(INode node)
        {
            return View(node);
        }
    }
}
