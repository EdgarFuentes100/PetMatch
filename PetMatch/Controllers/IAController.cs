using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PetMatch.Services;


//[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
//Este atributo evita que la respuesta se guarde en caché. Es útil en páginas donde hay datos sensibles o personalizados por usuario.
[Authorize(Policy = "All")]
public class IAController : Controller
{
    private readonly ServiceIA _iaService;

    public IAController(ServiceIA iaService)
    {
        _iaService = iaService;
    }

    // GET: muestra el formulario
    [HttpGet]
    public IActionResult HacerMatch()
    {
        return View();
    }

    // POST: procesa la entrada y muestra respuesta en la misma vista
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HacerMatch(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            ModelState.AddModelError(string.Empty, "La descripción no puede estar vacía.");
            return View(); // vuelve al formulario con error
        }

        string respuesta = await _iaService.ObtenerRespuestaAsync(descripcion);
        ViewBag.Respuesta = respuesta;

        return View(); // vuelve a la misma vista con la respuesta
    }
}
