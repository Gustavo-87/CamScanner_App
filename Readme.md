# CamScanner — Seguimiento 1

Proyecto académico de Ingeniería de Software II (2026-2). Aplicación móvil desarrollada con **C#, .NET 10 y .NET MAUI**, con Android como plataforma principal.

## Objetivo del proyecto

Digitalizar documentos mediante captura con cámara o importación de imágenes, **OCR local**, revisión y corrección de la información reconocida, organización en **lotes**, persistencia con **SQLite** y exportación a **Excel**. Se trata del alcance previsto para el producto final; **no todas las funcionalidades están implementadas en Seguimiento 1**.

## Estado de la entrega — 2 de noviembre de 2026

| Componente | Estado |
|---|---|
| Estructura del proyecto .NET MAUI | Implementada; compilación Android comprobada |
| Captura de fotos y listado de documentos | Implementados y comprobados en dispositivo físico |
| Importación de imágenes de la galería | Código implementado; pendiente verificación independiente en la entrega |
| Previsualización y opciones básicas de documentos | Código incluido; cobertura manual parcial |
| OCR local | Integración y pantallas incorporadas como avance; validación completa pendiente |
| Modelos `Lote` y `Documento` | Implementados |
| Capa SQLite | Base inicial implementada; falta integrar gestión completa a la interfaz |
| Selección y corrección estructurada de información OCR | Pendiente de completar |
| Exportación individual y consolidada a Excel | Pendiente |
| Pruebas xUnit de nombres y almacenamiento de imágenes | **29 de 29 aprobadas** en la revisión de integración |
| Instalación y apertura en Android | Comprobadas en tableta |

La aplicación **no está terminada**. Esta entrega demuestra una base técnica, funcionalidades iniciales, pruebas y participación del equipo.

## Equipo y responsabilidades

| Integrante | Rol | Aporte en Seguimiento 1 |
|---|---|---|
| Gustavo Piedrahita | Líder técnico | Arquitectura, modelos, SQLite, integración y verificación |
| Javier Serna | Desarrollo y pruebas | Captura/importación, avance OCR, pruebas automatizadas |
| Richard Prado | Calidad y documentación | Especificación, criterios de aceptación, plan y documentación de calidad |

El historial de Git y los enlaces de las tarjetas de Trello documentan la participación individual.

## Estructura del repositorio

```text
CamScanner_App/
├── Readme.md
├── docs/
│   ├── especificacion.md
│   ├── plan-de-pruebas.md
│   └── informe-calidad.md
├── src/
│   ├── CamScanner.csproj
│   ├── MainPage.xaml
│   ├── MainPage.xaml.cs
│   ├── DetallePage.xaml
│   ├── TextoPage.xaml
│   ├── Models/
│   ├── Data/
│   └── Services/
└── tests/
    └── CamScanner.Tests/
```

## Requisitos para desarrollo

- SDK **.NET 10** y carga de trabajo .NET MAUI para Android.
- Android SDK y entorno de compilación correspondiente; en macOS, JDK compatible con el SDK Android.
- Tableta/teléfono Android con depuración USB o emulador.

## Ejecutar el proyecto

Clonar el repositorio y entrar a la carpeta:

```bash
git clone https://github.com/Gustavo-87/CamScanner_App.git
cd CamScanner_App
```

Restaurar dependencias (si la carga MAUI no está instalada, instalarla previamente):

```bash
dotnet workload restore src/CamScanner.csproj
dotnet restore src/CamScanner.csproj
```

Compilar para Android:

```bash
dotnet build src/CamScanner.csproj -f net10.0-android
```

Con un dispositivo Android configurado, ejecutar:

```bash
dotnet build src/CamScanner.csproj -f net10.0-android -t:Run
```

Ejecutar las pruebas automatizadas:

```bash
dotnet test tests/CamScanner.Tests/CamScanner.Tests.csproj
```

En la verificación del Seguimiento 1 se obtuvo **29 pruebas aprobadas, 0 fallidas y 0 omitidas**. La compilación Android fue exitosa con cuatro advertencias relacionadas con bibliotecas nativas OCR duplicadas; no impidieron generar ni ejecutar la aplicación.

## Documentación y gestión

- [Especificación y criterios de aceptación](docs/especificacion.md)
- [Plan de pruebas](docs/plan-de-pruebas.md)
- [Informe de calidad](docs/informe-calidad.md)
- [Tablero Trello del equipo](https://trello.com/b/5W9lnNH9/camscaner-trabajo-final)
- [Repositorio GitHub](https://github.com/Gustavo-87/CamScanner_App)

### Entregas

La versión del **Seguimiento 1** se identifica mediante la etiqueta `entrega-s1`. La versión del Seguimiento 2 y el producto final se entregarán en las etapas posteriores del cronograma. Los avances posteriores a la etiqueta no forman parte de ella, salvo que se publique una actualización explícita.

## Próximos pasos

Completar la gestión de lotes y documentos con SQLite, probar el OCR local en distintos escenarios, implementar la revisión/selección de datos y la exportación Excel; ampliar pruebas y registrar defectos y evidencias en Trello.
