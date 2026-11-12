# AD01 — Microservicios para Comercio Multibodega

**Carrera:** Desarrollo de Software  
**Asignatura:** Aplicaciones Distribuidas — Unidad 1, Práctica 1 (AD01)  
**Tema:** Diseño de Arquitecturas Distribuidas  
**Institución:** Instituto Superior Tecnológico Cordillera

## Descripción

Implementación académica de tres microservicios REST independientes para una empresa comercializadora de productos tecnológicos que administra su catálogo, las existencias en varias bodegas y los pedidos de clientes.

Cada microservicio se desarrolla como una **solución independiente de Visual Studio** y utiliza **su propia base de datos SQL Server**. Los tres proyectos se almacenan juntos en este repositorio únicamente para facilitar la revisión y la entrega.

## Arquitectura

```text
                     Clientes HTTP / Postman / Swagger
                                   |
               +-------------------+-------------------+
               |                   |                   |
          Catalogo.Api        Inventario.Api         Pedidos.Api
               |                   |                   |
          CatalogoDB           InventarioDB           PedidosDB
               \                   |                   /
                +--------- SQL Server ----------------+
```

> **Importante:** las referencias `IdProducto` de Inventario y Pedidos corresponden a productos administrados por Catálogo. Al estar en bases de datos independientes, estas referencias son **lógicas** y no llaves foráneas físicas entre bases de datos. No se debe asumir que existe comunicación HTTP automática entre servicios si no se ha implementado.

## Tecnologías

- C# y ASP.NET Core Web API (.NET 10).
- Microsoft SQL Server y Entity Framework Core (proveedor SQL Server).
- Swagger UI / Swashbuckle para explorar y probar endpoints.
- Postman para las pruebas de solicitudes HTTP.
- Visual Studio 2026 y Git/GitHub para desarrollo y versionamiento.

## Microservicios y modelos de datos

### 1. Catalogo.Api — `CatalogoDB`

Administra la información comercial de los productos.

| Entidad | Campos principales |
| --- | --- |
| `Categoria` | `IdCategoria`, `Nombre`, `Descripcion` |
| `Marca` | `IdMarca`, `Nombre`, `PaisOrigen` |
| `Producto` | `IdProducto`, `IdCategoria`, `IdMarca`, `Nombre`, `SKU`, `Precio`, `Estado` |

**Relaciones:** `Producto.IdCategoria` → `Categoria.IdCategoria`; `Producto.IdMarca` → `Marca.IdMarca`.

**Rutas base de los controladores:** `/api/Categorias`, `/api/Marcas`, `/api/Productos`.

### 2. Inventario.Api — `InventarioDB`

Administra bodegas, existencias, movimientos de inventario y reservas de stock.

| Entidad | Campos principales |
| --- | --- |
| `Bodega` | `IdBodega`, `Nombre`, `Ciudad`, `Direccion` |
| `Existencia` | `IdExistencia`, `IdProducto`, `IdBodega`, `Stock` |
| `MovimientoInventario` | `IdMovimiento`, `IdProducto`, `IdBodega`, `Tipo`, `Cantidad`, `Fecha` |
| `ReservaStock` | `IdReserva`, `IdProducto`, `IdBodega`, `Cantidad`, `Estado` |

**Relaciones físicas:** las entidades de existencias, movimientos y reservas referencian `Bodega.IdBodega`. **Referencia lógica:** `IdProducto` corresponde al catálogo.

**Rutas base previstas:** `/api/Bodegas`, `/api/Existencias`, `/api/MovimientosInventario`, `/api/ReservasStock`. Confirmar los nombres exactos en Swagger de esta solución.

**Reglas de negocio:** los movimientos afectan el stock; las reservas requieren disponibilidad. Determinadas operaciones de modificación o eliminación de movimientos/reservas pueden estar restringidas para proteger la trazabilidad.

### 3. Pedidos.Api — `PedidosDB`

Administra clientes, pedidos y detalles de pedido.

| Entidad | Campos principales |
| --- | --- |
| `Cliente` | `IdCliente`, `Nombres`, `Cedula`, `Correo` |
| `Pedido` | `IdPedido`, `IdCliente`, `Fecha`, `Total`, `Estado` |
| `DetallePedido` | `IdDetalle`, `IdPedido`, `IdProducto`, `Cantidad`, `PrecioUnitario` |

**Relaciones físicas:** `Pedido.IdCliente` → `Cliente.IdCliente`; `DetallePedido.IdPedido` → `Pedido.IdPedido`. **Referencia lógica:** `DetallePedido.IdProducto` corresponde al catálogo.

**Rutas base:** `/api/Clientes`, `/api/Pedidos`, `/api/DetallesPedido`.

El total de un pedido se calcula sumando `Cantidad × PrecioUnitario` de sus detalles.

## Estructura del repositorio

```text
AD01-Comercio-Multibodega/
├── Catalogo.Api/          # Solución y proyecto independientes
├── Inventario.Api/        # Solución y proyecto independientes
├── Pedidos.Api/           # Solución y proyecto independientes
├── Postman/
│   └── AD01_Coleccion_Postman.json
├── Documentacion/
│   ├── Declaracion_Uso_IA.pdf
│   └── Registro_Prompts_Fuentes.pdf
├── .gitignore
└── README.md
```

> Los nombres de archivos en `Postman/` y `Documentacion/` son los nombres sugeridos para la entrega. Ajustarlos a los nombres reales de los archivos exportados.

## Requisitos previos

1. Visual Studio con la carga de trabajo **ASP.NET y desarrollo web** y SDK de .NET 10.
2. SQL Server y una herramienta de administración como SQL Server Management Studio.
3. Postman para importar la colección de pruebas.
4. Las tres bases de datos: `CatalogoDB`, `InventarioDB` y `PedidosDB`, con sus tablas creadas.

## Configuración y ejecución local

1. Abrir **por separado** la solución de `Catalogo.Api`, `Inventario.Api` y `Pedidos.Api` en Visual Studio.
2. Verificar la cadena de conexión de cada servicio en su configuración local, con el nombre de servidor SQL Server correspondiente al equipo.
3. Restaurar paquetes NuGet y compilar cada solución.
4. Iniciar cada API y abrir su Swagger UI.
5. Importar la colección JSON de `Postman/` y ejecutar las solicitudes respetando las dependencias entre entidades.

### Direcciones HTTPS usadas durante el desarrollo

| Servicio | Swagger local |
| --- | --- |
| Catálogo | `https://localhost:7014/swagger` |
| Inventario | `https://localhost:7137/swagger` |
| Pedidos | `https://localhost:7204/swagger` |

Estos puertos corresponden al entorno local utilizado en el desarrollo; pueden cambiar en otra computadora. Revisar `Properties/launchSettings.json` en cada proyecto.

### Orden recomendado para las pruebas

1. Crear categorías y marcas, después productos.
2. Crear bodegas y existencias; después registrar movimientos y reservas según las reglas de negocio.
3. Crear clientes, luego pedidos y finalmente detalles de pedido.
4. Verificar consultas por ID, actualizaciones, eliminaciones permitidas y respuestas ante entradas inválidas o IDs inexistentes.

**Códigos HTTP de referencia:** `200 OK`, `201 Created`, `400 Bad Request` y `404 Not Found`, según la operación y la validación correspondiente.

> La colección de Postman sirve como conjunto de solicitudes de prueba; no constituye por sí sola evidencia de que todas las pruebas hayan sido ejecutadas con éxito. Los IDs de ejemplo deben revisarse antes de enviar solicitudes que alteren datos.

## Consideraciones de implementación

- No se crean llaves foráneas físicas entre bases de datos de distintos microservicios.
- Las relaciones dentro de una misma base de datos se gestionan con sus llaves foráneas.
- Algunas reglas de negocio impiden eliminar registros que mantienen relaciones activas.
- La validación remota de productos entre microservicios no debe darse por implementada sin comprobar el código correspondiente.
- El certificado HTTPS de desarrollo puede requerir confianza local (`dotnet dev-certs https --trust`). No se recomienda desactivar la validación TLS en producción.

## Evidencias y entrega

- Código fuente de las **tres soluciones independientes** en este repositorio.
- Archivo `.gitignore` que excluye `bin/`, `obj/`, `.vs/` y archivos temporales.
- Colección exportada de Postman (`.json`).
- Declaración de uso de inteligencia artificial en PDF y registro de consultas/fuentes, conforme a la política académica.

## Uso de inteligencia artificial

Se utilizó ChatGPT como apoyo para comprender conceptos, proponer estructuras, revisar código, interpretar errores y organizar pruebas y documentación. El trabajo asistido debe revisarse y comprobarse por el estudiante. Los prompts exactos, las decisiones adoptadas y las fuentes verificadas se registran en la documentación de entrega.

## Autoría

**Estudiante:** Mateo Moreno  
**Actividad:** AD01 — Diseño de Arquitecturas Distribuidas  
**Fecha de entrega:** 12/11/2026
