# ForraControl API

API REST para **Forra Store** (tienda de forraje y alimento para ganado), construida en **.NET 9** con **PostgreSQL**, desplegada en **Railway**. Reemplaza al backend original en ASP.NET Web API 2 / SQL Server, replicando su funcionalidad al 100%.

Consumida por la app móvil Flutter ([`forra_store`](https://github.com/Johann-Yamil15/Forra_store)) vía `lib/core/constants/api_constants.dart`.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core 9 (Web API, Controllers) |
| ORM | Entity Framework Core 9 + `EFCore.NamingConventions` (snake_case automático) |
| Base de datos | PostgreSQL (hosteada en [Neon](https://neon.tech)) |
| Procesamiento de imágenes | SixLabors.ImageSharp 3.x |
| Contraseñas | BCrypt.Net-Next |
| Documentación interactiva | Swagger UI (`/swagger`) |
| Hosting | Railway (Docker, auto-deploy desde GitHub) |
| CORS | Abierto (`AllowAnyOrigin`) — pensado para consumo desde la app móvil |

---

## Arquitectura

Arquitectura por capas clásica (no CQRS/vertical-slice, a diferencia de otros proyectos del autor) — Controllers → Interfaces → Services → Data (EF Core) → PostgreSQL.

```
ForraControl.API/
├── Controllers/
│   ├── ApiControllerBase.cs          Ok<T>/Created<T>/Fail() → sobre { ok, data } / { ok, error }
│   ├── AuthController.cs             /api/auth
│   ├── ProductosController.cs        /api/productos      (catálogo trabajador)
│   ├── ClientesController.cs         /api/clientes       (dropdown trabajador)
│   ├── VentasController.cs           /api/ventas
│   ├── Admin/
│   │   ├── ProductosAdminController.cs       /api/admin/productos
│   │   ├── PresentacionesAdminController.cs  /api/admin/presentaciones
│   │   ├── ClientesAdminController.cs        /api/admin/clientes
│   │   ├── DashboardController.cs            /api/admin/dashboard
│   │   └── ReportesController.cs             /api/admin/reportes
│   └── Config/
│       └── ConfigController.cs       /api/config (categorías/subcategorías/unidades)
├── Interfaces/            IAuthService, IProductoService, IClienteService, IVentaService, IAdminService, IConfigService
├── Services/              Implementaciones EF Core async de las interfaces de arriba
├── Models/
│   ├── Entities/          Usuario, Producto, Presentacion, Cliente, PrecioEspecial, Venta, DetalleVenta
│   └── Dtos/               Un folder por feature: Auth, Productos, Clientes, Ventas, Admin, Config
├── Data/
│   ├── ForraDbContext.cs  DbSets + Fluent API (constraints, índices, precisión decimal)
│   └── DbInitializer.cs   Seed idempotente al arrancar (usuarios, productos, clientes de ejemplo)
├── Common/
│   └── UploadPaths.cs     Resuelve la carpeta raíz de imágenes subidas
├── Migrations/            Generadas por EF Core
├── Program.cs             DI, Npgsql, CORS, JSON, Swagger, puerto dinámico, uploads, migrate+seed
├── Dockerfile
└── railway.json
```

---

## Modelo de datos

7 tablas (snake_case en Postgres, generado por `EFCore.NamingConventions` a partir de las entidades en PascalCase):

| Tabla | Descripción |
|---|---|
| `usuarios` | Login (admin / trabajador). Password con hash BCrypt. |
| `productos` | Catálogo: nombre, categoría, subcategoría, uso, imagen. |
| `presentaciones` | Variantes de un producto: unidad, tamaño, precio, stock, stock mínimo. |
| `clientes` | Clientes con precios especiales. |
| `precios_especiales` | Precio acordado por cliente + presentación. |
| `ventas` | Cabecera de venta (usuario, cliente, totales). |
| `detalles_venta` | Líneas de una venta (cantidad, precio unitario/efectivo, subtotal). |

Detalles relevantes:
- `presentaciones.tamano` es `decimal(10,2)`.
- `detalles_venta` **no** guarda nombre de producto — se obtiene por join `presentaciones → productos` al leer historial/reportes.
- No hay validación de stock insuficiente al registrar una venta (gap conocido, heredado del diseño original).
- Índice parcial `ix_presentaciones_stock` solo sobre presentaciones activas.
- Constraint único `(id_cliente, id_presentacion)` en `precios_especiales`.

---

## Respuesta estándar

Todos los endpoints (excepto `/uploads/*`, que sirve archivos estáticos) devuelven el mismo sobre:

```json
// Éxito
{ "ok": true, "data": { ... } }

// Error
{ "ok": false, "error": "Descripción del error" }
```

| Código | Cuándo |
|---|---|
| 200 | GET / operación exitosa |
| 201 | POST exitoso (recurso creado) |
| 204 | DELETE exitoso |
| 400 | Datos inválidos / campo requerido faltante |
| 401 | Credenciales incorrectas |
| 404 | Recurso no encontrado |
| 500 | Error interno |

---

## Endpoints

### Auth
| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/login` | Login (`{username, password}`) → verifica con BCrypt |

### Trabajador (app móvil)
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/productos` | Catálogo activo con presentaciones |
| GET | `/api/clientes` | Dropdown de clientes con precios especiales |
| POST | `/api/ventas` | Registrar venta (transacción: inserta detalle + descuenta stock) |
| GET | `/api/ventas` | Historial (`?idUsuario=&periodo=hoy\|semana\|mes`) |
| GET | `/api/ventas/{id}` | Detalle de una venta |

### Admin — Productos
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/admin/productos` | Todos los productos (activos e inactivos) |
| POST | `/api/admin/productos` | Crear producto con presentaciones iniciales |
| PUT | `/api/admin/productos/{id}` | Actualizar datos generales |
| DELETE | `/api/admin/productos/{id}` | Desactivar producto (soft delete) |
| POST | `/api/admin/productos/{id}/presentaciones` | Agregar presentación |
| **POST** | **`/api/admin/productos/{id}/imagen`** | **Subir foto** (multipart, campo `imagen`) — ver sección abajo |

### Admin — Presentaciones
| Método | Ruta | Descripción |
|---|---|---|
| PUT | `/api/admin/presentaciones/{id}` | Actualizar presentación |
| DELETE | `/api/admin/presentaciones/{id}` | Eliminar presentación |
| PATCH | `/api/admin/presentaciones/{id}/stock` | Agregar stock (`{cantidad}`) |

### Admin — Clientes
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/admin/clientes` | Todos los clientes con precios especiales |
| POST | `/api/admin/clientes` | Crear cliente |
| PUT | `/api/admin/clientes/{id}` | Actualizar cliente |
| DELETE | `/api/admin/clientes/{id}` | Eliminar cliente (hard delete) |
| PUT | `/api/admin/clientes/{id}/precios` | Reemplazar precios especiales |

### Admin — Dashboard y Reportes
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/admin/dashboard` | KPIs: ventas hoy/semana, alertas de stock, top 3 productos, ventas recientes |
| GET | `/api/admin/reportes?periodo=hoy\|semana\|mes` | Reporte con desglose diario/semanal/mensual |

### Config (catálogos dinámicos)
| Método | Ruta | Descripción |
|---|---|---|
| GET/POST | `/api/config/categorias` | Lista / agrega categoría |
| GET/POST | `/api/config/subcategorias` | Lista / agrega subcategoría |
| GET/POST | `/api/config/unidades` | Lista / agrega unidad |

> Las categorías/subcategorías/unidades no tienen tabla propia — se leen como `DISTINCT` de `productos`/`presentaciones`. Los POST solo devuelven el valor tal cual (quedan disponibles al guardar el siguiente producto).

**Total: 28 endpoints**

---

## Subida de imágenes de productos

`POST /api/admin/productos/{id}/imagen` (multipart/form-data, campo `imagen`):

1. Rechaza archivos de más de **5 MB**.
2. Valida que el contenido sea una imagen real decodificándolo con ImageSharp (no confía en la extensión — un `.jpg` que en realidad es texto se rechaza).
3. Redimensiona a un máximo de **1600px** en el lado más largo (mantiene proporción).
4. **Elimina metadatos EXIF/IPTC/XMP** (ubicación GPS, modelo de cámara, fecha) — conserva el perfil ICC de color.
5. Guarda como JPEG (calidad 82) en `{Uploads:Path}/productos/{guid}.jpg` y borra la imagen anterior del producto si existía.
6. Actualiza `productos.imagen_url` con la ruta relativa (ej. `/uploads/productos/xxxx.jpg`) y la devuelve en la respuesta.

Las imágenes se sirven como archivos estáticos en `/uploads/...`. El campo `imagen_url` puede contener:
- Una ruta relativa (`/uploads/productos/...`) para fotos subidas desde la app.
- Una URL absoluta (`https://...`) para los productos semilla con placeholder externo.

El cliente Flutter normaliza ambos casos con `ApiConstants.resolveImageUrl()`.

**⚠️ Persistencia en Railway**: el disco del contenedor es efímero — se pierde en cada sleep/restart/redeploy. `Uploads:Path` (env var `Uploads__Path`) debe apuntar a un **Volume** de Railway montado (ver sección de despliegue) o las imágenes subidas se perderán.

---

## Usuarios semilla

`DbInitializer.cs` siembra estos usuarios la primera vez que la API corre contra una base vacía (contraseña en texto plano, se hashea con BCrypt al guardarse):

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `123456789` | admin |
| `usuario` | `123456789` | trabajador |
| `maria` | `123456789` | trabajador |

También siembra 6 productos, 14 presentaciones, 3 clientes y 4 precios especiales de ejemplo (mismos datos que el script SQL Server original).

---

## Variables de entorno

| Variable | Requerida | Descripción |
|---|---|---|
| `ConnectionStrings__Database` | Sí | Cadena de conexión Npgsql a PostgreSQL. Formato: `Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true` |
| `Uploads__Path` | Recomendada en prod | Carpeta raíz de imágenes subidas. Local: `uploads` (relativa, default). Railway: debe apuntar a un Volume, ej. `/data/uploads` |
| `PORT` | Automática (Railway) | Puerto en el que escucha Kestrel. Railway la inyecta sola — no configurar a mano. |
| `ASPNETCORE_ENVIRONMENT` | No | `Development` local / `Production` en Railway (default del contenedor) |

Los valores reales (contraseñas, connection string completo) **no están en este README** — ver `SECRETS.local.md` (no versionado, solo local) o directamente en Railway → servicio → **Variables**.

---

## Desarrollo local

```bash
# 1. Guardar el connection string real como user-secret (nunca en appsettings.json)
dotnet user-secrets set "ConnectionStrings:Database" "Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"

# 2. Aplicar migraciones (crea las tablas si no existen)
dotnet ef database update

# 3. Correr — el seed se aplica automáticamente si la base está vacía
dotnet run
```

Swagger UI: `http://localhost:5255/swagger` (o el puerto que asigne `dotnet run`).

### Comandos útiles de EF Core

```bash
dotnet ef migrations add NombreDeLaMigracion   # nueva migración tras cambiar entidades
dotnet ef migrations script --idempotent       # ver el SQL que se aplicaría
dotnet ef database drop -f                     # ⚠️ borra la base completa (cuidado en prod)
```

---

## Despliegue en Railway

1. **Conectar el repo**: Railway → New Project → Deploy from GitHub repo → `Johann-Yamil15/forracontrol-api`, rama `main`. Auto-deploy activado por defecto (cada push a `main` redeploya).
2. **Build**: usa el `Dockerfile` del repo (configurado en `railway.json`, builder `DOCKERFILE`).
3. **Variables** (servicio → pestaña Variables):
   - `ConnectionStrings__Database` = cadena de conexión de Neon (ver `SECRETS.local.md`)
   - `Uploads__Path` = `/data/uploads` (una vez creado el Volume, paso 5)
4. **Dominio público**: servicio → Settings → Networking → Public Networking → Generate Domain. Puerto objetivo: `8080` (coincide con `EXPOSE 8080` del Dockerfile; `Program.cs` respeta la variable `PORT` que Railway inyecta).
5. **Volume para imágenes** (persistencia — sin esto, las imágenes subidas se pierden en cada sleep/redeploy):
   - Ir a la vista de **canvas/arquitectura** del proyecto (no Project Settings) → click derecho sobre la cajita del servicio → **Attach Volume** / **New Volume**.
   - Mount path: `/data`.
   - Confirmar que `Uploads__Path=/data/uploads` esté seteado en Variables.
   - **Importante**: el Dockerfile corre el contenedor como **root** (sin `USER $APP_UID`) a propósito — Railway monta los Volumes nuevos con dueño `root`, y el usuario sin privilegios de la imagen base de .NET no puede escribir ahí. Si se reintroduce `USER $APP_UID`, el arranque va a fallar con `UnauthorizedAccessException` en `/data/uploads`.
6. **Serverless / Scale to zero** (opcional, ahorra recursos): Settings → Serverless → activar. El contenedor se duerme sin tráfico y despierta con la siguiente petición (cold start de unos segundos — el cliente Flutter tiene 20s de timeout, suficiente).

### Diagnóstico rápido si el deploy falla

`Program.cs` loguea explícitamente al arrancar:
- Si `ConnectionStrings:Database` llega vacía → lista las variables de entorno relacionadas que sí ve el contenedor, para detectar nombre mal escrito (debe ser `ConnectionStrings__Database`, doble guion bajo) o variable en el servicio/ambiente equivocado.
- Si la carpeta de uploads no se puede crear/escribir → loguea el error pero **no tumba la API** (solo esa función queda inutilizable hasta resolverlo).

---

## Notas de desarrollo

- **Passwords**: hash BCrypt (a diferencia del backend original en .NET Framework, que comparaba texto plano).
- **Auth**: sin JWT — el frontend gestiona la sesión localmente con lo que devuelve `/api/auth/login`.
- **Stock negativo**: no se valida que haya stock suficiente al registrar una venta (gap conocido, heredado del diseño original — agregar validación si se requiere).
- **Soft delete**: productos se desactivan (`activo = false`); clientes se borran físicamente (hard delete).
- **CORS**: abierto a cualquier origen — es una API consumida solo por la app móvil, no hay superficie web pública que lo vuelva un riesgo real hoy, pero restringirlo sería lo ideal si esto crece.
