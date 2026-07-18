# Demo Técnica API de Inventario

[![.NET CI](https://github.com/DiegoG-420/technical-demo-dotnet-inventory-api/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/DiegoG-420/technical-demo-dotnet-inventory-api/actions/workflows/dotnet-ci.yml)


API REST de demostración construida con .NET 9 para simular operaciones básicas de inventario, stock y movimientos dentro de un entorno empresarial.

Este proyecto forma parte de mi portafolio técnico profesional y demuestra habilidades en backend, diseño de APIs, documentación técnica, pruebas automatizadas y preparación para flujos DevOps.

## Objetivo

Demostrar una API pequeña, clara y funcional que permita:

- Consultar productos.
- Consultar un producto por identificador.
- Revisar el resumen de stock.
- Validar el estado del servicio.
- Registrar movimientos simulados de inventario.

## Stack técnico

- .NET 9
- ASP.NET Core Minimal API
- Swagger / Swashbuckle
- xUnit
- Git
- PowerShell

## Endpoints disponibles

| Método | Endpoint | Descripción |
|---|---|---|
| GET | /health | Valida el estado general del servicio. |
| GET | /api/productos | Obtiene el listado de productos simulados. |
| GET | /api/productos/{id} | Obtiene un producto por identificador. |
| GET | /api/stock/resumen | Obtiene un resumen general del inventario. |
| POST | /api/movimientos | Registra un movimiento simulado de inventario. |

## Ejemplo de respuesta: health

JSON:

{
  "estado": "Saludable",
  "servicio": "Inventory.Api",
  "fechaHoraUtc": "2026-07-18T04:45:21.1765428Z"
}

## Ejemplo de respuesta: resumen de stock

JSON:

{
  "totalProductos": 4,
  "productosActivos": 3,
  "unidadesTotales": 45,
  "productosSinStock": 1
}

## Ejemplo de request: crear movimiento

JSON:

{
  "productoId": 1,
  "tipo": "ENTRADA",
  "cantidad": 5,
  "descripcion": "Ajuste de inventario de demostración"
}

Valores válidos para tipo:

- ENTRADA
- SALIDA

## Cómo ejecutar localmente

Desde la raíz del repositorio:

dotnet restore
dotnet build

Para iniciar la API:

cd src/Inventory.Api
dotnet run

La API se ejecuta localmente en:

http://localhost:5087

Swagger UI:

http://localhost:5087/swagger


## Ejecución con Docker

El proyecto incluye un `Dockerfile` multi-stage para compilar y ejecutar la API en un contenedor.

Construir la imagen:

docker build -t inventory-api-demo .

Ejecutar el contenedor:

docker run --rm -p 8080:8080 inventory-api-demo

La API quedará disponible en:

http://localhost:8080

Swagger UI:

http://localhost:8080/swagger

Nota: esta sección queda preparada para ejecución con Docker Desktop o cualquier entorno compatible con Docker.

## Cómo ejecutar pruebas

Desde la raíz del repositorio:

dotnet test

## Estructura del proyecto

technical-demo-dotnet-inventory-api/
  src/
    Inventory.Api/
      Program.cs
      Inventory.Api.csproj
  tests/
    Inventory.Api.Tests/
      Inventory.Api.Tests.csproj
      UnitTest1.cs
  global.json
  TechnicalDemo.InventoryApi.slnx
  README.md


## Integración continua

El repositorio incluye un workflow de GitHub Actions para validar automáticamente el proyecto en cada push o pull request hacia la rama `main`.

El pipeline ejecuta:

- Restauración de dependencias.
- Compilación en modo Release.
- Ejecución de pruebas automatizadas.

Archivo del workflow:

.github/workflows/dotnet-ci.yml

## Estado actual

- API funcional.
- Swagger habilitado.
- Endpoints principales disponibles.
- Respuestas JSON en español.
- Build local validado.
- Pruebas automatizadas ejecutándose correctamente.
- Rama principal: main.

## Roadmap

Próximas mejoras planeadas:

- Agregar pruebas de integración para endpoints principales.
- Agregar Dockerfile.
- Agregar GitHub Actions para CI.
- Publicar badge de build en README.
- Agregar ejemplos curl.
- Agregar manejo centralizado de errores.
- Separar modelos y servicios en carpetas dedicadas.
- Evaluar persistencia con base de datos en una segunda versión.

## Autor

Diego Garrido E.

Software Engineer enfocado en backend, APIs, automatización, integración y sistemas reales.

Portafolio:

https://portfolio.ssitecnologicos.com



