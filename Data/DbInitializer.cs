using ForraControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Data;

// Seed idempotente — mismos datos que la sección 11 de database/forra_store_sqlserver.sql,
// pero con password_hash generado con BCrypt en vez de placeholder en texto plano.
public static class DbInitializer
{
    public static async Task SeedAsync(ForraDbContext db)
    {
        if (!await db.Usuarios.AnyAsync())
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456789");
            db.Usuarios.AddRange(
                new Usuario { Nombre = "Administrador", Username = "admin", PasswordHash = passwordHash, Rol = "admin" },
                new Usuario { Nombre = "Juan Trabajador", Username = "usuario", PasswordHash = passwordHash, Rol = "trabajador" },
                new Usuario { Nombre = "María Trabajadora", Username = "maria", PasswordHash = passwordHash, Rol = "trabajador" }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Productos.AnyAsync())
        {
            var maiz = new Producto
            {
                Nombre = "Maíz Amarillo",
                Descripcion = "Maíz amarillo de alta calidad para engorda de ganado bovino y porcino.",
                Categoria = "Alimento",
                Subcategoria = "Bovino",
                Uso = "Engorda",
                ImagenUrl = "https://example.com/maiz.jpg",
                Presentaciones =
                {
                    new Presentacion { Unidad = "Bulto", Tamano = 50, Precio = 420.00m, Stock = 120, StockMinimo = 10 },
                    new Presentacion { Unidad = "Bulto", Tamano = 25, Precio = 220.00m, Stock = 80, StockMinimo = 5 },
                    new Presentacion { Unidad = "Kg", Tamano = 1, Precio = 9.50m, Stock = 500, StockMinimo = 50 },
                }
            };

            var sorgo = new Producto
            {
                Nombre = "Sorgo Rojo",
                Descripcion = "Sorgo rojo en grano, ideal como complemento energético en dietas ganaderas.",
                Categoria = "Alimento",
                Subcategoria = "Bovino",
                Uso = "Engorda",
                ImagenUrl = "https://example.com/sorgo.jpg",
                Presentaciones =
                {
                    new Presentacion { Unidad = "Bulto", Tamano = 50, Precio = 380.00m, Stock = 90, StockMinimo = 10 },
                    new Presentacion { Unidad = "Kg", Tamano = 1, Precio = 8.00m, Stock = 400, StockMinimo = 50 },
                }
            };

            var alfalfa = new Producto
            {
                Nombre = "Alfalfa Achicalada",
                Descripcion = "Paca de alfalfa deshidratada de alta proteína para equinos y bovinos.",
                Categoria = "Alimento",
                Subcategoria = "Equino",
                Uso = "Mantenimiento",
                ImagenUrl = "https://example.com/alfalfa.jpg",
                Presentaciones =
                {
                    new Presentacion { Unidad = "Paca", Tamano = 1, Precio = 150.00m, Stock = 60, StockMinimo = 5 },
                    new Presentacion { Unidad = "Tonelada", Tamano = 1, Precio = 12000.00m, Stock = 3, StockMinimo = 1 },
                }
            };

            var alpiste = new Producto
            {
                Nombre = "Alpiste Nacional",
                Descripcion = "Alpiste de primera calidad para aves canoras y de ornato.",
                Categoria = "Alimento",
                Subcategoria = "Aves",
                Uso = "Engorda",
                ImagenUrl = "https://example.com/alpiste.jpg",
                Presentaciones =
                {
                    new Presentacion { Unidad = "Kg", Tamano = 1, Precio = 35.00m, Stock = 200, StockMinimo = 20 },
                    new Presentacion { Unidad = "Bolsa", Tamano = 5, Precio = 160.00m, Stock = 80, StockMinimo = 10 },
                }
            };

            var comedero = new Producto
            {
                Nombre = "Comedero de Pollo",
                Descripcion = "Comedero plástico resistente para pollos en diferentes tamaños.",
                Categoria = "Accesorios",
                Subcategoria = "Aves",
                Uso = "Equipamiento",
                ImagenUrl = "https://delagarzamateriasprimas.com/wp-content/uploads/2024/07/comedero_pollo.jpg",
                Presentaciones =
                {
                    new Presentacion { Unidad = "Pieza ch", Tamano = 1, Precio = 120.00m, Stock = 100, StockMinimo = 10 },
                    new Presentacion { Unidad = "Pieza med", Tamano = 1, Precio = 180.00m, Stock = 80, StockMinimo = 8 },
                    new Presentacion { Unidad = "Pieza gra", Tamano = 1, Precio = 250.00m, Stock = 60, StockMinimo = 5 },
                }
            };

            var bermuda = new Producto
            {
                Nombre = "Semilla de Pasto Bermuda",
                Descripcion = "Semilla certificada para establecimiento de praderas de bovinos.",
                Categoria = "Semillas",
                Subcategoria = "Bovino",
                Uso = "Praderas",
                ImagenUrl = "https://example.com/bermuda.jpg",
                Presentaciones =
                {
                    new Presentacion { Unidad = "Kg", Tamano = 1, Precio = 95.00m, Stock = 150, StockMinimo = 15 },
                    new Presentacion { Unidad = "Bolsa", Tamano = 10, Precio = 880.00m, Stock = 40, StockMinimo = 5 },
                }
            };

            db.Productos.AddRange(maiz, sorgo, alfalfa, alpiste, comedero, bermuda);
            await db.SaveChangesAsync();

            if (!await db.Clientes.AnyAsync())
            {
                var juan = new Cliente { Nombre = "Juan García (Rancho El Nogal)", Telefono = "867-100-0001" };
                var pedro = new Cliente { Nombre = "Pedro Martínez (Granja Avícola)", Telefono = "867-100-0002" };
                var silvia = new Cliente { Nombre = "Silvia Torres (Establo Las Palmas)", Telefono = "867-100-0003" };
                db.Clientes.AddRange(juan, pedro, silvia);
                await db.SaveChangesAsync();

                var maizBulto50 = maiz.Presentaciones.First(p => p.Unidad == "Bulto" && p.Tamano == 50);
                var maizBulto25 = maiz.Presentaciones.First(p => p.Unidad == "Bulto" && p.Tamano == 25);
                var alpisteKg = alpiste.Presentaciones.First(p => p.Unidad == "Kg");
                var alfalfaPaca = alfalfa.Presentaciones.First(p => p.Unidad == "Paca");

                db.PreciosEspeciales.AddRange(
                    new PrecioEspecial { IdCliente = juan.Id, IdPresentacion = maizBulto50.Id, Precio = 390.00m },
                    new PrecioEspecial { IdCliente = juan.Id, IdPresentacion = maizBulto25.Id, Precio = 200.00m },
                    new PrecioEspecial { IdCliente = pedro.Id, IdPresentacion = alpisteKg.Id, Precio = 30.00m },
                    new PrecioEspecial { IdCliente = silvia.Id, IdPresentacion = alfalfaPaca.Id, Precio = 130.00m }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
