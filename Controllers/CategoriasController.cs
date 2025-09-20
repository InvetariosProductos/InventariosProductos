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
    public class CategoriasController : Controller
    {
        private readonly InventarioDbContext _context;

        public CategoriasController(InventarioDbContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> Index(string searchString, bool? soloActivas)
        {
            try
            {
                var categoriasQuery = _context.Categorias
                    .Include(c => c.Productos)
                    .AsQueryable();

                // Filtro por búsqueda
                if (!string.IsNullOrEmpty(searchString))
                {
                    categoriasQuery = categoriasQuery.Where(c =>
                        c.Nombre.Contains(searchString) ||
                        (c.Descripcion != null && c.Descripcion.Contains(searchString)));
                }

                // Filtro solo activas (por defecto true)
                if (soloActivas ?? true)
                {
                    categoriasQuery = categoriasQuery.Where(c => c.Activa);
                }

                ViewData["SearchString"] = searchString;
                ViewData["SoloActivas"] = soloActivas ?? true;

                var categorias = await categoriasQuery.OrderBy(c => c.Nombre).ToListAsync();
                return View(categorias);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar las categorías: " + ex.Message;
                return View(new List<Categoria>());
            }
        }

        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de categoría no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var categoria = await _context.Categorias
                    .Include(c => c.Productos)
                    .FirstOrDefaultAsync(m => m.CategoriaId == id);

                if (categoria == null)
                {
                    TempData["ErrorMessage"] = "Categoría no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la categoría: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Activa")] Categoria categoria)
        {
            try
            {
                // Validación adicional: nombre único
                if (await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
                {
                    ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre");
                }

                if (ModelState.IsValid)
                {
                    // Establecer fecha de creación automáticamente
                    categoria.FechaCreacion = DateTime.Now;

                    _context.Add(categoria);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Categoría creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la categoría: " + ex.Message;
            }

            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de categoría no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                {
                    TempData["ErrorMessage"] = "Categoría no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la categoría: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Nombre,Descripcion,Activa,FechaCreacion")] Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                TempData["ErrorMessage"] = "ID de categoría no coincide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Validación adicional: nombre único excepto para la misma categoría
                if (await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre && c.CategoriaId != categoria.CategoriaId))
                {
                    ModelState.AddModelError("Nombre", "Ya existe otra categoría con este nombre");
                }

                if (ModelState.IsValid)
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Categoría actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaExists(categoria.CategoriaId))
                {
                    TempData["ErrorMessage"] = "La categoría ya no existe";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Error de concurrencia. La categoría fue modificada por otro usuario";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar la categoría: " + ex.Message;
            }

            return View(categoria);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de categoría no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var categoria = await _context.Categorias
                    .Include(c => c.Productos)
                    .FirstOrDefaultAsync(m => m.CategoriaId == id);

                if (categoria == null)
                {
                    TempData["ErrorMessage"] = "Categoría no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la categoría: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var categoria = await _context.Categorias
                    .Include(c => c.Productos)
                    .FirstOrDefaultAsync(c => c.CategoriaId == id);

                if (categoria == null)
                {
                    TempData["ErrorMessage"] = "Categoría no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si tiene productos asociados
                if (categoria.Productos != null && categoria.Productos.Any())
                {
                    TempData["ErrorMessage"] = "No se puede eliminar la categoría porque tiene productos asociados. Desactívela en su lugar.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Categoría eliminada exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la categoría: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Método para activar/desactivar categoría (alternativa al delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                {
                    TempData["ErrorMessage"] = "Categoría no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                categoria.Activa = !categoria.Activa;
                _context.Update(categoria);
                await _context.SaveChangesAsync();

                string estado = categoria.Activa ? "activada" : "desactivada";
                TempData["SuccessMessage"] = $"Categoría {estado} exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cambiar el estado de la categoría: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Método para obtener categorías con más productos
        public async Task<IActionResult> MasProductos()
        {
            try
            {
                var categorias = await _context.Categorias
                    .Include(c => c.Productos)
                    .Where(c => c.Activa)
                    .OrderByDescending(c => c.Productos!.Count())
                    .ToListAsync();

                ViewBag.Titulo = "Categorías con más productos";
                return View("Index", categorias);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al obtener categorías con más productos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaId == id);
        }
    }
}