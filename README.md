# CamScanner

Aplicación móvil académica desarrollada para el proyecto de Ingeniería de Software II (2026-2), utilizando C#, .NET 10 y .NET MAUI. Su objetivo es facilitar la digitalización, almacenamiento y organización de documentos mediante imágenes y reconocimiento óptico de caracteres (OCR).

## 1. Objetivo del proyecto

CamScanner permite capturar fotografías de documentos o importar imágenes desde la galería del dispositivo, almacenarlas localmente, organizarlas mediante lotes y reconocer su contenido textual mediante OCR.

La aplicación busca facilitar la consulta, edición y conservación de la información digitalizada, manteniendo los datos en el dispositivo mediante SQLite.

## 2. Estado actual de la aplicación

La siguiente tabla refleja las funcionalidades identificadas en el código fuente de la versión revisada.

| Funcionalidad | Estado |
|---|---|
| Creación de lotes | Implementada |
| Consulta de lotes | Implementada |
| Edición de lotes | Implementada |
| Eliminación de lotes | Implementada con validación de documentos asociados |
| Captura de fotografías | Implementada |
| Importación de imágenes desde galería | Implementada, hasta 10 imágenes por selección |
| Almacenamiento local de imágenes | Implementado |
| Registro de documentos en SQLite | Implementado |
| Consulta de documentos | Implementada |
| Renombrar documentos | Implementado |
| Eliminar documentos | Implementado |
| Visualización de imágenes | Implementada |
| Reconocimiento de texto OCR | Integrado en el código; validación funcional pendiente |
| Edición del texto reconocido | Implementada |
| Copia de texto al portapapeles | Implementada |
| Compartir imágenes | Implementado |
| Exportación a Excel | Implementada: individual y consolidada por lote, verificada en Android |

**Estado de validación:** la última ejecución local registró 65 pruebas aprobadas, 0 fallidas y 0 omitidas. Las pruebas incluyen almacenamiento, validación de nombres, SQLite y exportaciones Excel y ZIP. No representan una validación integral de todas las funcionalidades de la aplicación.

La compilación Android requiere configurar el Android SDK en el entorno de desarrollo. En la última verificación se obtuvo el error XA5300 por ausencia de dicha configuración.

## 3. Tecnologías y dependencias

| Tecnología | Uso |
|---|---|
| C# | Lenguaje de programación |
| .NET 10 | Plataforma de desarrollo |
| .NET MAUI | Desarrollo de la aplicación móvil |
| SQLite | Persistencia local |
| sqlite-net-pcl 1.11.285 | Acceso a la base de datos SQLite |
| Plugin.Maui.OCR 1.1.1 | Reconocimiento óptico de caracteres |
| Microsoft.Extensions.Logging.Debug 10.0.0 | Registro de eventos de depuración |
| xUnit | Pruebas automatizadas |

## 4. Requisitos de instalación

Para trabajar con el proyecto se requiere:

- Windows 10 o superior para el entorno de desarrollo descrito.
- .NET SDK 10.
- Carga de trabajo .NET MAUI.
- Android SDK y herramientas de compilación Android.
- Android Studio o Visual Studio con las herramientas de desarrollo MAUI.
- Dispositivo Android físico o emulador compatible.
- Cable USB y depuración USB habilitada, cuando se utilice un dispositivo físico.

El proyecto declara los siguientes destinos: Android, iOS, MacCatalyst y Windows. Android es la plataforma principal del desarrollo; los demás destinos requieren sus respectivos entornos y verificaciones.

## 5. Instalación del proyecto

Clonar el repositorio:

```bash
git clone https://github.com/Gustavo-87/CamScanner_App.git
```

Ingresar a la carpeta:

```bash
cd CamScanner_App
```

Restaurar las cargas de trabajo:

```bash
dotnet workload restore src/CamScanner.csproj
```

Restaurar las dependencias:

```bash
dotnet restore src/CamScanner.csproj
```

## 6. Compilación y ejecución

Compilar el proyecto para Android:

```bash
dotnet build src/CamScanner.csproj -f net10.0-android
```

Para ejecutar la aplicación, configurar un dispositivo Android o emulador y utilizar las opciones de implementación de Visual Studio o Android Studio con las herramientas MAUI correspondientes.

**Nota:** la compilación Android no podrá completarse si el Android SDK no está instalado o configurado correctamente.

## 7. Ejecución de pruebas automatizadas

El proyecto cuenta con un conjunto de pruebas desarrollado con xUnit.

Ejecutar todas las pruebas:

```bash
dotnet test tests/CamScanner.Tests/CamScanner.Tests.csproj
```

### Componentes evaluados

- `AlmacenImagenesTests.cs`: almacenamiento, extensión de archivos, generación de rutas únicas, eliminación y manejo de errores.
- `ValidadorNombresTests.cs`: generación de nombres consecutivos, normalización, validación de nombres vacíos y detección de duplicados.

### Último resultado registrado

| Indicador | Resultado |
|---|---:|
| Pruebas ejecutadas | 29 |
| Aprobadas | 29 |
| Fallidas | 0 |
| Omitidas | 0 |

Estos resultados corresponden a la ejecución automatizada registrada durante la revisión del 17 de noviembre de 2026.

## 8. Estructura del repositorio

```text
CamScanner_App/
├── README.md
├── docs/
│   ├── especificacion.md
│   ├── plan-de-pruebas.md
│   └── informe-calidad.md
├── src/
│   ├── CamScanner.csproj
│   ├── App.xaml
│   ├── AppShell.xaml
│   ├── MainPage.xaml
│   ├── MainPage.xaml.cs
│   ├── DetallePage.xaml
│   ├── DetallePage.xaml.cs
│   ├── TextoPage.xaml
│   ├── TextoPage.xaml.cs
│   ├── Models/
│   │   ├── Documento.cs
│   │   └── Lote.cs
│   ├── Data/
│   │   └── CamScannerDatabase.cs
│   ├── Services/
│   │   ├── AlmacenImagenes.cs
│   │   └── ValidadorNombres.cs
│   └── Platforms/
│       ├── Android/
│       ├── iOS/
│       ├── MacCatalyst/
│       └── Windows/
└── tests/
    └── CamScanner.Tests/
        ├── AlmacenImagenesTests.cs
        └── ValidadorNombresTests.cs
```

## 9. Documentación del proyecto

La documentación técnica se encuentra en la carpeta `docs/`:

- [Especificación de requisitos](docs/especificacion.md): requisitos funcionales, no funcionales y criterios de aceptación.
- [Plan de pruebas](docs/plan-de-pruebas.md): casos de prueba, resultados y trazabilidad.
- [Informe de calidad](docs/informe-calidad.md): evaluación técnica, hallazgos, riesgos y resultados de verificación.

## 10. Gestión del proyecto

El seguimiento de actividades, asignación de responsabilidades y organización del trabajo del equipo se realiza mediante Trello.

- [Tablero Trello de CamScanner](https://trello.com/b/5W9lnNH9/camscaner-trabajo-final)
- [Repositorio GitHub](https://github.com/Gustavo-87/CamScanner_App)

## 11. Estado de entrega y consideraciones

La versión documentada contiene funcionalidades para la captura e importación de imágenes, organización mediante lotes, almacenamiento local, gestión de documentos y reconocimiento de texto mediante OCR.

La exportación individual y consolidada a Excel está implementada y fue verificada en una tableta Android.

Los resultados de las pruebas automatizadas no sustituyen las pruebas funcionales de aceptación. La verificación en dispositivos Android y la validación integral de los flujos de negocio deben respaldarse con sus respectivas evidencias.

La documentación de esta versión debe mantenerse sincronizada con los cambios del código fuente, los resultados de las pruebas y los defectos registrados por el equipo.
