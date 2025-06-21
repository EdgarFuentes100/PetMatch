using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;
using PetMatch.Models;
using PetMatch.Services;
using System.Security.Claims;

namespace PetMatch.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ServiceTransactionHelper _tx;
        private readonly ServiceGoogleValidator _validator;

        public AccountController(AppDbContext db, ServiceTransactionHelper tx, ServiceGoogleValidator googleTokenValidator)
        {
            _db = db;
            _tx = tx;
            _validator = googleTokenValidator;
        }

        [HttpGet("Login")]
        public IActionResult Login(string returnUrl = "/", bool expired = false)
        {
            ViewData["Expired"] = expired;
            ViewData["ReturnUrl"] = returnUrl;
            return View(); // Muestra la vista personalizada
        }

        [HttpPost("login/google")]
        public IActionResult LoginWithGoogle(string returnUrl = "/")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ExternalLoginCallback), new { returnUrl })
            };
            return Challenge(props, "Google");
        }

        [HttpGet("externallogincallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
        {
            // 1) Recuperar id_token guardado en la cookie
            var authResult = await HttpContext.AuthenticateAsync();
            var idToken = authResult.Properties.GetTokenValue("id_token");

            if (string.IsNullOrEmpty(idToken))
                return RedirectToAction(nameof(Login));

            // 2) Validar con firma de Google
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await _validator.ValidarAsync(idToken);
            }
            catch
            {
                // Token inválido o expirado
                return RedirectToAction(nameof(Login));
            }

            // 3) Datos seguros extraídos del JWT
            var email = payload.Email;
            var nombre = payload.Name;

            // 4) Lógica de BD (igual que antes)

            var usuario = await _db.Usuario
                       .Include(u => u.Rol)
                       .FirstOrDefaultAsync(u => u.Email == email);

            await _tx.RunAsync(async () =>
            {
                if (usuario == null)
                {
                    usuario = new Usuario { Email = email, Nombre = nombre, RolId = 2 };
                    _db.Usuario.Add(usuario);
                    await _db.SaveChangesAsync();

                    _db.Perfil.Add(new Perfil
                    {
                        UsuarioId = usuario.UsuarioId,
                        Descripcion = $"¡Hola! Soy {nombre}"
                    });
                    await _db.SaveChangesAsync();

                    //cargar la navegación Rol del usuario recién insertado
                    await _db.Entry(usuario)
                             .Reference(u => u.Rol)
                             .LoadAsync();
                }
            });

            // 5) Crear claims locales
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , nombre ?? ""),
                new Claim(ClaimTypes.Email, email),
                new Claim("UserId", usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 6) Firmar la cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return LocalRedirect(returnUrl);
        }


        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        [HttpGet("acceso-denegado")]
        public IActionResult AccesoDenegado(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

    }
}
