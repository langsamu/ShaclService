namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF;

    public class Node : ViewComponent
    {
        public IViewComponentResult Invoke(INode node)
        {
            return View(node);
        }
    }
}
