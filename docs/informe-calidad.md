# Informe de calidad — CamScanner

## 1. Objetivo

Identificar el estado comprobable del producto, los riesgos técnicos y los aspectos de calidad que deben atenderse durante el desarrollo y la verificación de CamScanner, mediante revisión estática del código fuente, documentación y resultados de compilación.

## 2. Alcance y limitaciones de la revisión

Se revisaron los siguientes archivos del proyecto:

- `src/CamScanner.csproj`
- `src/Models/Documento.cs`
- `src/Models/Lote.cs`
- `src/Data/CamScannerDatabase.cs`
- `src/MainPage.xaml`
- `src/MainPage.xaml.cs`
- `src/MauiProgram.cs`
- `src/App.xaml`
- `src/AppShell.xaml`
- `src/Platforms/Android/AndroidManifest.xml`
- `docs/especificacion.md`
- `docs/plan-de-pruebas.md`

También se realizó una búsqueda de referencias a OCR, cámara, selección de archivos y Excel dentro de `src`, excluyendo las carpetas `bin` y `obj`.

La revisión incluye los resultados de restauración y compilación obtenidos con .NET SDK 10.0.403 y la carga de trabajo MAUI 10.0.110.

**Limitaciones:**

- No se completaron pruebas funcionales en un dispositivo Android.
- No se verificó el comportamiento de la aplicación durante su ejecución.
- No se midió el rendimiento del OCR ni de la exportación.
- No se aportaron evidencias de pruebas automatizadas.
- La revisión estática no demuestra por sí sola que una funcionalidad esté ausente de todos los archivos del proyecto.

Por lo tanto, los resultados representan un diagnóstico del estado comprobable y no una certificación integral de calidad.

## 3. Estado técnico comprobado

### 3.1. Revisión del código fuente

| Área | Evidencia | Resultado |
|---|---|---|
| Plataforma | `.csproj` declara `net10.0-android` y `UseMaui=true`. | Configuración inicial presente. |
| Dependencias | Se declara `sqlite-net-pcl` versión 1.11.285. | Dependencia SQLite presente. |
| Modelo Lote | Contiene Id, Nombre, FechaCreacion y Descripcion. | Implementado. |
| Modelo Documento | Contiene Id, LoteId, Nombre, RutaImagen, TextoExtraido y FechaCreacion. | Implementado. |
| Inicialización SQLite | `InitAsync()` crea las tablas `Lote` y `Documento`. | Implementada en código. |
| Consulta de lotes | `ObtenerLotesAsync()`. | Implementada en código. |
| Guardado de lotes | `GuardarLoteAsync()` realiza inserción o actualización según el Id. | Implementado en código. |
| Eliminación de lotes | No se identificó un método de eliminación en la clase revisada. | No evidenciado. |
| Gestión de documentos | No se identificaron métodos para insertar, consultar, actualizar o eliminar documentos. | No evidenciada. |
| Relación lote-documento | `Documento` contiene `LoteId` con atributo `[Indexed]`. | Índice presente; no se evidencia clave foránea. |
| Interfaz | La pantalla principal muestra «Hello, World!» y un contador. | Interfaz de negocio no evidenciada. |
| OCR, cámara e importación | La búsqueda de referencias no encontró implementaciones en los archivos revisados. | No evidenciados. |
| Exportación Excel | No se encontraron referencias ni dependencias de Excel en el archivo de proyecto revisado. | No evidenciada. |
| Pruebas automatizadas | No se identificó una carpeta de pruebas en el listado versionado compartido. | No evidenciadas en ese listado. |

### 3.2. Resultados de restauración y compilación

Se realizaron las siguientes verificaciones:

| Verificación | Resultado | Observación |
|---|---|---|
| `dotnet --list-sdks` | Aprobado | SDK 10.0.403 instalado. |
| Instalación de carga MAUI | Aprobado | Carga MAUI instalada correctamente. |
| `dotnet restore src/CamScanner.csproj` | Aprobado | Restauración completada. |
| Compilación Android | Fallida | Error XA5300: no se encontró el directorio Android SDK. |
| Compilación iOS Simulator | Aprobada | Compilación completada. |
| Compilación MacCatalyst | Aprobada | Compilación completada. |
| Compilación Windows | Aprobada | Compilación completada. |
| Compilación individual Windows | Aprobada | `net10.0-windows10.0.19041.0` compiló correctamente. |
| Ejecución mediante `-t:Run` | No completada | Se produjo el error MSB3073, código de salida 9009. |

**Interpretación de resultados:**

La compilación multi-target obtuvo tres destinos compilados correctamente de cuatro intentados. Android no pudo compilar debido a la configuración del entorno de desarrollo, específicamente la ausencia del Android SDK.

La compilación satisfactoria de Windows demuestra que el proyecto puede generarse para ese destino, pero no confirma que sus funcionalidades operen correctamente.

La compilación de iOS y MacCatalyst tampoco equivale a una prueba de ejecución en esos sistemas operativos.

El intento de ejecución con `-t:Run` no se completó y requiere diagnóstico adicional.

### 3.3. Diagnóstico de la capa de datos

La clase `CamScannerDatabase` inicializa una base de datos local en `FileSystem.AppDataDirectory` con el nombre `camscanner.db3`.

Se identificaron estas operaciones:

- Creación de la tabla `Lote`.
- Creación de la tabla `Documento`.
- Consulta de todos los lotes.
- Inserción de un lote nuevo.
- Actualización de un lote existente.

No se identificaron métodos de gestión de documentos ni operaciones de eliminación.

Aunque `Documento` tiene un campo `LoteId` marcado como índice, esto no constituye por sí solo una relación de clave foránea ni garantiza que no existan documentos asociados a un lote eliminado.

## 4. Evaluación por características de calidad

| Característica | Situación observada | Acción recomendada |
|---|---|---|
| Adecuación funcional | Interfaz de plantilla y persistencia parcial de lotes. | Implementar y verificar los flujos funcionales del alcance. |
| Eficiencia de desempeño | Sin mediciones de OCR, procesamiento de imágenes o exportación. | Medir tiempos y consumo cuando se incorporen esas funciones. |
| Compatibilidad | Se declara Android y existen otros destinos de compilación. Android no pudo compilar en el entorno revisado. | Configurar Android SDK y probar en dispositivos compatibles. |
| Usabilidad | La pantalla inspeccionada conserva la plantilla inicial de MAUI. | Diseñar y evaluar las pantallas de captura, revisión, lotes y exportación. |
| Confiabilidad | Capa de datos limitada y sin pruebas de recuperación aportadas. | Incorporar manejo de excepciones y pruebas de persistencia e integridad. |
| Seguridad y privacidad | La base local utiliza `FileSystem.AppDataDirectory`. No se evidenció el flujo completo de protección de documentos. | Revisar permisos, almacenamiento y tratamiento de datos. |
| Mantenibilidad | Existe separación inicial entre modelos, datos e interfaz. | Mantener responsabilidades separadas e incorporar servicios específicos. |
| Portabilidad | El proyecto declara varios destinos condicionados en el archivo de proyecto. | Priorizar Android y documentar las plataformas efectivamente verificadas. |

## 5. Riesgos identificados

Las valoraciones son preliminares y deben revisarse cuando se ejecuten pruebas funcionales.

| ID | Riesgo | Probabilidad | Impacto | Acción recomendada |
|---|---|---|---|---|
| R-01 | Imágenes borrosas, inclinadas u oscuras pueden reducir la precisión del OCR. | Alta | Alto | Probar condiciones variadas, permitir corrección manual y comunicar resultados parciales. |
| R-02 | El OCR local puede consumir recursos elevados en dispositivos limitados. | Media | Alto | Medir consumo y tiempo de procesamiento y evitar bloquear la interfaz. |
| R-03 | Permisos denegados o archivos incompatibles pueden impedir captura e importación. | Media | Medio | Validar permisos y formatos y mostrar mensajes claros. |
| R-04 | La capa SQLite no expone el CRUD completo requerido. | Alta | Alto | Completar las operaciones para lotes y documentos y probarlas. |
| R-05 | Las eliminaciones pueden dejar relaciones lote-documento inconsistentes. | Media | Alto | Definir integridad referencial y probar lotes con documentos asociados. |
| R-06 | La interfaz actual no ofrece los flujos de negocio previstos. | Alta | Alto | Implementar pantallas de captura, revisión, lotes y exportación. |
| R-07 | La exportación puede generar archivos incompletos o desordenados. | Media | Alto | Definir la estructura del Excel y validar exportaciones individuales y por lote. |
| R-08 | La ausencia de pruebas automatizadas visibles dificulta detectar regresiones. | Media | Alto | Crear un proyecto de pruebas y automatizar validaciones de modelos y persistencia. |
| R-09 | La configuración de permisos Android puede no cubrir las capacidades futuras. | Media | Medio | Revisar el manifiesto y los permisos en tiempo de ejecución. |
| R-10 | Las dependencias OCR y Excel pueden presentar incompatibilidades o restricciones de licencia. | Media | Medio | Evaluar versiones, compatibilidad y licencias antes de integrarlas. |
| R-11 | La ausencia del Android SDK en el entorno impide verificar la compilación Android. | Alta | Medio | Instalar y configurar Android SDK y repetir la compilación. |

## 6. Métricas y resultados de verificación

### 6.1. Métricas de compilación

| Métrica | Resultado |
|---|---|
| SDK instalado | .NET 10.0.403 |
| Carga MAUI | 10.0.110 |
| Restauración de dependencias | Correcta |
| Destinos compilados en la prueba multi-target | 3 de 4 |
| Destinos fallidos en la prueba multi-target | 1 de 4 |
| Compilación individual Windows | Correcta |
| Ejecución funcional de la aplicación | No completada |

El porcentaje de destinos compilados correctamente en la prueba multi-target fue:

\[
\frac{3}{4}\times100=75\%
\]

Este porcentaje corresponde exclusivamente a los destinos de compilación intentados. No representa el porcentaje de requisitos funcionales implementados ni el porcentaje de pruebas funcionales aprobadas.

### 6.2. Métricas funcionales pendientes

No se suministraron resultados de ejecución funcional. Se deben recopilar:

- Porcentaje de requisitos probados: requisitos con pruebas ejecutadas / requisitos totales × 100.
- Porcentaje de pruebas aprobadas: casos aprobados / casos ejecutados × 100.
- Tiempo de OCR y exportación en dispositivos identificados.
- Exactitud del OCR comparando resultados con transcripciones de referencia.
- Persistencia después de cerrar y abrir la aplicación.
- Defectos por severidad.
- Compatibilidad por modelo y versión Android.

Los umbrales de aceptación deben acordarse antes de interpretar las mediciones.

## 7. Hallazgos de revisión

Los siguientes hallazgos corresponden a revisión estática y verificación de compilación. No deben confundirse con defectos confirmados mediante pruebas funcionales.

| ID | Hallazgo | Severidad sugerida | Estado | Evidencia |
|---|---|---|---|---|
| H-01 | La pantalla principal conserva la plantilla inicial de MAUI. | Alta | Abierto | `MainPage.xaml` y `MainPage.xaml.cs`. |
| H-02 | La capa de datos revisada solo consulta e inserta/actualiza lotes. | Alta | Abierto | `CamScannerDatabase.cs`. |
| H-03 | No se encontraron referencias a OCR, cámara, selección de archivos o Excel en la búsqueda realizada. | Alta | Abierto | Búsqueda en `src`. |
| H-04 | No se identificó una carpeta de pruebas en el listado versionado compartido. | Media | Abierto | Salida de `git ls-files`. |
| H-05 | El campo `LoteId` está indexado, pero no se evidencia una regla de clave foránea en el modelo revisado. | Media | Abierto | `Documento.cs`. |
| H-06 | La compilación Android no pudo completarse por falta del Android SDK. | Media | Pendiente | Error XA5300 durante `dotnet build`. |
| H-07 | El intento de ejecución con `-t:Run` no se completó. | Media | Pendiente de diagnóstico | Error MSB3073, código 9009. |

### 7.1. Clasificación de los hallazgos

- **Hallazgos del producto:** H-01 a H-05.
- **Incidencias del entorno o de verificación:** H-06 y H-07.

La ausencia del Android SDK corresponde a una limitación del entorno de pruebas, no a un defecto funcional demostrado del producto.

## 8. Acciones de mejora

1. Sustituir la interfaz de plantilla por los flujos de negocio de CamScanner.
2. Completar las operaciones CRUD para lotes y documentos.
3. Definir y aplicar reglas de integridad para la relación entre lotes y documentos.
4. Incorporar captura e importación de imágenes con los permisos Android correspondientes.
5. Seleccionar e integrar una solución de OCR local y documentar su paquete y versión.
6. Incorporar selección y edición del texto reconocido.
7. Seleccionar e integrar una biblioteca para exportación Excel.
8. Crear pruebas automatizadas para modelos y persistencia.
9. Configurar el Android SDK y repetir la compilación Android.
10. Diagnosticar el error de ejecución MSB3073 y verificar la aplicación en un entorno compatible.
11. Ejecutar los casos del plan de pruebas y adjuntar evidencias reales.
12. Actualizar los estados de los requisitos y hallazgos conforme se obtengan resultados.

## 9. Conclusión

La revisión confirma que CamScanner cuenta con una estructura inicial desarrollada en .NET MAUI, modelos para lotes y documentos y una infraestructura básica de persistencia SQLite. Se identificaron operaciones para crear las tablas, consultar lotes e insertar o actualizar lotes.

Sin embargo, la interfaz principal conserva elementos de la plantilla inicial de MAUI y no se evidenció la integración de las funcionalidades centrales de digitalización, captura, importación, OCR y exportación Excel en los archivos revisados. Tampoco se identificó la gestión CRUD completa de documentos.

En las verificaciones técnicas, la restauración de dependencias fue satisfactoria y la compilación de Windows, iOS Simulator y MacCatalyst finalizó correctamente. La compilación Android quedó bloqueada por la ausencia del Android SDK. La ejecución de la aplicación tampoco pudo completarse mediante el objetivo `Run`.

En consecuencia, el proyecto presenta una implementación parcial respecto al alcance definido en la especificación. Se recomienda priorizar la implementación de las funcionalidades pendientes, completar la configuración del entorno Android y ejecutar los casos del plan de pruebas antes de emitir una valoración definitiva sobre la calidad funcional del producto.