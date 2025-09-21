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
    public class ProductosController : Controller
    {
        private readonly InventarioDbContext _context;

        public ProductosController(InventarioDbContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index(string searchString, int? categoriaId, int? proveedorId)
        {
            try
            {
                // Query base con relaciones
                var productosQuery = _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .AsQueryable();

                // Filtros de búsqueda
                if (!string.IsNullOrEmpty(searchString))
                {
                    productosQuery = productosQuery.Where(p =>
                        p.Nombre.Contains(searchString) ||
                        p.Codigo.Contains(searchString) ||
                        (p.Descripcion != null && p.Descripcion.Contains(searchString)));
                }

                if (categoriaId.HasValue)
                {
                    productosQuery = productosQuery.Where(p => p.CategoriaId == categoriaId);
                }

                if (proveedorId.HasValue)
                {
                    productosQuery = productosQuery.Where(p => p.ProveedorId == proveedorId);
                }

                // ViewData para filtros
                ViewData["SearchString"] = searchString;
                ViewData["Categorias"] = new SelectList(await _context.Categorias.Where(c => c.Activa).ToListAsync(), "CategoriaId", "Nombre");
                ViewData["Proveedores"] = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "ProveedorId", "Nombre");

                var productos = await productosQuery.OrderBy(p => p.Nombre).ToListAsync();
                return View(productos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los productos: " + ex.Message;
                return View(new List<Producto>());
            }
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de producto no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var producto = await _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .FirstOrDefaultAsync(m => m.ProductoId == id);

                if (producto == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(producto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Productos/Create
        public async Task<IActionResult> Create()
        {
            Console.WriteLine("Entró al método Create GET");
            try
            {
                await CargarSelectLists();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Codigo,Precio,Stock,CategoriaId,ProveedorId")] Producto producto)
        {
            try
            {
                // Verifica si ya existe un producto con el mismo código
                if (await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo))
                {
                    ModelState.AddModelError("Codigo", "Ya existe un producto con este código");
                }

                // Comprueba si el modelo es válido
                if (ModelState.IsValid)
                {
                    // Asigna fechas de creación y actualización
                    producto.FechaCreacion = DateTime.Now;
                    producto.FechaActualizacion = null;

                    // Añade el producto a la base de datos
                    _context.Add(producto);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Producto creado exitosamente";
                    return RedirectToAction(nameof(Index)); // Redirige a la lista de productos
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear el producto: " + ex.Message;
            }

            // Si hay errores, recarga las listas desplegables y vuelve a mostrar la vista
            await CargarSelectLists(producto.CategoriaId, producto.ProveedorId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de producto no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                await CargarSelectLists(producto.CategoriaId, producto.ProveedorId);
                return View(producto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductoId,Nombre,Descripcion,Codigo,Precio,Stock,CategoriaId,ProveedorId,FechaCreacion")] Producto producto)
        {
            if (id != producto.ProductoId)
            {
                TempData["ErrorMessage"] = "ID de producto no coincide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo && p.ProductoId != producto.ProductoId))
                {
                    ModelState.AddModelError("Codigo", "Ya existe otro producto con este código");
                }

                if (ModelState.IsValid)
                {
                    producto.FechaActualizacion = DateTime.Now;

                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Producto actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.ProductoId))
                {
                    TempData["ErrorMessage"] = "El producto ya no existe";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Error de concurrencia. El producto fue modificado por otro usuario";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el producto: " + ex.Message;
            }

            await CargarSelectLists(producto.CategoriaId, producto.ProveedorId);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de producto no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var producto = await _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .FirstOrDefaultAsync(m => m.ProductoId == id);

                if (producto == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(producto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Producto eliminado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el producto: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para cargar listas desplegables
        private async Task CargarSelectLists(int? categoriaSeleccionada = null, int? proveedorSeleccionado = null)
        {
            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync(),
                "CategoriaId", "Nombre", categoriaSeleccionada);

            ViewData["ProveedorId"] = new SelectList(
                await _context.Proveedores.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync(),
                "ProveedorId", "Nombre", proveedorSeleccionado);
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoId == id);
        }

        // Método adicional para obtener productos con stock bajo
        public async Task<IActionResult> StockBajo(int limite = 10)
        {
            try
            {
                var productosStockBajo = await _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .Where(p => p.Stock <= limite)
                    .OrderBy(p => p.Stock)
                    .ToListAsync();

                ViewBag.Limite = limite;
                return View("Index", productosStockBajo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al obtener productos con stock bajo: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
