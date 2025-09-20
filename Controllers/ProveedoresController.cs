using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioProductos.Data;
using InventarioProductos.Models;

namespace InventarioProductos.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly InventarioDbContext _context;

        public ProveedoresController(InventarioDbContext context)
        {
            _context = context;
        }

        // GET: Proveedores
        public async Task<IActionResult> Index(string searchString, bool? soloActivos)
        {
            try
            {
                var proveedoresQuery = _context.Proveedores
                    .Include(p => p.Productos)
                    .AsQueryable();

                // Filtro por búsqueda
                if (!string.IsNullOrEmpty(searchString))
                {
                    proveedoresQuery = proveedoresQuery.Where(p =>
                        p.Nombre.Contains(searchString) ||
                        (p.Contacto != null && p.Contacto.Contains(searchString)) ||
                        (p.Email != null && p.Email.Contains(searchString)) ||
                        (p.Telefono != null && p.Telefono.Contains(searchString)));
                }

                // Filtro solo activos (por defecto true)
                if (soloActivos ?? true)
                {
                    proveedoresQuery = proveedoresQuery.Where(p => p.Activo);
                }

                ViewData["SearchString"] = searchString;
                ViewData["SoloActivos"] = soloActivos ?? true;

                var proveedores = await proveedoresQuery.OrderBy(p => p.Nombre).ToListAsync();
                return View(proveedores);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los proveedores: " + ex.Message;
                return View(new List<Proveedor>());
            }
        }

        // GET: Proveedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de proveedor no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var proveedor = await _context.Proveedores
                    .Include(p => p.Productos)
                    .FirstOrDefaultAsync(m => m.ProveedorId == id);

                if (proveedor == null)
                {
                    TempData["ErrorMessage"] = "Proveedor no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(proveedor);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Proveedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Contacto,Telefono,Email,Direccion,Activo")] Proveedor proveedor)
        {
            try
            {
                // Validaciones adicionales del lado servidor
                if (await _context.Proveedores.AnyAsync(p => p.Nombre == proveedor.Nombre))
                {
                    ModelState.AddModelError("Nombre", "Ya existe un proveedor con este nombre");
                }

                if (!string.IsNullOrEmpty(proveedor.Email) &&
                    await _context.Proveedores.AnyAsync(p => p.Email == proveedor.Email))
                {
                    ModelState.AddModelError("Email", "Ya existe un proveedor con este email");
                }

                if (!string.IsNullOrEmpty(proveedor.Telefono) &&
                    await _context.Proveedores.AnyAsync(p => p.Telefono == proveedor.Telefono))
                {
                    ModelState.AddModelError("Telefono", "Ya existe un proveedor con este teléfono");
                }

                if (ModelState.IsValid)
                {
                    // Establecer fecha de registro automáticamente
                    proveedor.FechaRegistro = DateTime.Now;

                    _context.Add(proveedor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Proveedor creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear el proveedor: " + ex.Message;
            }

            return View(proveedor);
        }

        // GET: Proveedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de proveedor no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                {
                    TempData["ErrorMessage"] = "Proveedor no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(proveedor);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProveedorId,Nombre,Contacto,Telefono,Email,Direccion,Activo,FechaRegistro")] Proveedor proveedor)
        {
            if (id != proveedor.ProveedorId)
            {
                TempData["ErrorMessage"] = "ID de proveedor no coincide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Validaciones adicionales: únicos excepto para el mismo proveedor
                if (await _context.Proveedores.AnyAsync(p => p.Nombre == proveedor.Nombre && p.ProveedorId != proveedor.ProveedorId))
                {
                    ModelState.AddModelError("Nombre", "Ya existe otro proveedor con este nombre");
                }

                if (!string.IsNullOrEmpty(proveedor.Email) &&
                    await _context.Proveedores.AnyAsync(p => p.Email == proveedor.Email && p.ProveedorId != proveedor.ProveedorId))
                {
                    ModelState.AddModelError("Email", "Ya existe otro proveedor con este email");
                }

                if (!string.IsNullOrEmpty(proveedor.Telefono) &&
                    await _context.Proveedores.AnyAsync(p => p.Telefono == proveedor.Telefono && p.ProveedorId != proveedor.ProveedorId))
                {
                    ModelState.AddModelError("Telefono", "Ya existe otro proveedor con este teléfono");
                }

                if (ModelState.IsValid)
                {
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Proveedor actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProveedorExists(proveedor.ProveedorId))
                {
                    TempData["ErrorMessage"] = "El proveedor ya no existe";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Error de concurrencia. El proveedor fue modificado por otro usuario";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el proveedor: " + ex.Message;
            }

            return View(proveedor);
        }

        // GET: Proveedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de proveedor no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var proveedor = await _context.Proveedores
                    .Include(p => p.Productos)
                    .FirstOrDefaultAsync(m => m.ProveedorId == id);

                if (proveedor == null)
                {
                    TempData["ErrorMessage"] = "Proveedor no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(proveedor);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores
                    .Include(p => p.Productos)
                    .FirstOrDefaultAsync(p => p.ProveedorId == id);

                if (proveedor == null)
                {
                    TempData["ErrorMessage"] = "Proveedor no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si tiene productos asociados
                if (proveedor.Productos != null && proveedor.Productos.Any())
                {
                    TempData["ErrorMessage"] = "No se puede eliminar el proveedor porque tiene productos asociados. Desactívelo en su lugar.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Proveedor eliminado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el proveedor: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Método para activar/desactivar proveedor (alternativa al delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                {
                    TempData["ErrorMessage"] = "Proveedor no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                proveedor.Activo = !proveedor.Activo;
                _context.Update(proveedor);
                await _context.SaveChangesAsync();

                string estado = proveedor.Activo ? "activado" : "desactivado";
                TempData["SuccessMessage"] = $"Proveedor {estado} exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cambiar el estado del proveedor: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Método para obtener proveedores con más productos
        public async Task<IActionResult> MasProductos()
        {
            try
            {
                var proveedores = await _context.Proveedores
                    .Include(p => p.Productos)
                    .Where(p => p.Activo)
                    .OrderByDescending(p => p.Productos!.Count())
                    .ToListAsync();

                ViewBag.Titulo = "Proveedores con más productos";
                return View("Index", proveedores);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al obtener proveedores con más productos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Método para obtener información de contacto
        public async Task<IActionResult> Contactos()
        {
            try
            {
                var proveedores = await _context.Proveedores
                    .Where(p => p.Activo && (!string.IsNullOrEmpty(p.Email) || !string.IsNullOrEmpty(p.Telefono)))
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                ViewBag.Titulo = "Directorio de contactos de proveedores";
                return View("Index", proveedores);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al obtener contactos de proveedores: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.ProveedorId == id);
        }
    }
}
//
