# Informe de calidad --- CamScanner

**Proyecto:** CamScanner · Ingeniería de Software II (2026-2)\
**Fecha de actualización:** 17 de noviembre de 2026\
**Rama revisada:** `documentacion-richard`

## 1. Objetivo

Evaluar el estado comprobable de CamScanner a partir de la revisión del
código fuente, las dependencias, la documentación y los resultados de
pruebas automatizadas y compilación disponibles. El informe diferencia
la implementación identificada de la validación funcional efectivamente
ejecutada.

## 2. Alcance y limitaciones

Se revisaron los archivos y componentes compartidos de la versión
actual:

-   `src/CamScanner.csproj`
-   `src/MainPage.xaml` y `src/MainPage.xaml.cs`
-   `src/DetallePage.xaml` y `src/DetallePage.xaml.cs`
-   `src/TextoPage.xaml` y `src/TextoPage.xaml.cs`
-   `src/Data/CamScannerDatabase.cs`
-   `src/Models/Documento.cs`
-   `src/Models/Lote.cs`
-   `src/Services/AlmacenImagenes.cs`
-   `src/Services/ValidadorNombres.cs`
-   `tests/CamScanner.Tests/AlmacenImagenesTests.cs`
-   `tests/CamScanner.Tests/ValidadorNombresTests.cs`
-   `docs/especificacion.md`
-   `docs/plan-de-pruebas.md`

También se consideraron las salidas de `dotnet test` y `dotnet build`
compartidas el 17 de noviembre de 2026.

**Limitaciones de la evaluación:** - No se recibió un reporte adicional
de resultados funcionales de Javier. - No se aportaron evidencias
individuales de ejecución de los casos CP-01 a CP-32. - La compilación
Android no se completó por falta de configuración del Android SDK. - No
se confirmó mediante prueba en dispositivo el comportamiento del OCR,
cámara, galería o persistencia. - No se implementó ni verificó la
exportación Excel. - La revisión de código no sustituye pruebas de
aceptación ni una evaluación integral de calidad.

Por tanto, este informe representa una evaluación técnica documental y
automatizada parcial, no una certificación integral del producto.

## 3. Estado técnico comprobado

### 3.1. Funcionalidades identificadas en el código

  -------------------------------------------------------------------------------------------
  Área                    Evidencia revisada                          Evaluación
  ----------------------- ------------------------------------------- -----------------------
  Plataforma              `CamScanner.csproj` declara .NET 10 y MAUI. Configuración presente.

  Dependencias            `sqlite-net-pcl` 1.11.285 y                 Dependencias
                          `Plugin.Maui.OCR` 1.1.1.                    declaradas.

  Gestión de lotes        `MainPage.xaml.cs` permite crear,           Implementación
                          consultar, editar y solicitar eliminación.  identificada.

  Integridad de lotes     `EliminarLoteAsync` comprueba si existen    Validación de negocio
                          documentos asociados antes de eliminar.     presente.

  Captura                 Uso de                                      Implementación
                          `MediaPicker.Default.CapturePhotoAsync()` y identificada.
                          solicitud de permiso de cámara.

  Importación             Uso de `PickPhotosAsync`, con límite de     Implementación
                          selección de 10 imágenes.                   identificada.

  Almacenamiento de       Copia de archivos al directorio local de la Implementación
  imágenes                aplicación.                                 identificada.

  Gestión de documentos   Registro, consulta, actualización y         Implementación
                          eliminación en `CamScannerDatabase`.        identificada.

  Relación lote-documento El documento almacena `LoteId`; el registro Validación en código;
                          valida que el lote exista.                  no se demuestra clave
                                                                      foránea SQLite.

  Visualización           `DetallePage` muestra la imagen asociada al Implementación
                          documento.                                  identificada.

  OCR                     `OcrPlugin.Default.InitAsync()` y           Integración
                          `RecognizeTextAsync(bytes)`.                identificada; ejecución
                                                                      funcional pendiente.

  Edición de texto        `TextoPage` permite editar y guardar el     Implementación
                          texto reconocido.                           identificada.

  Copiar texto            Uso de `Clipboard.Default.SetTextAsync`.    Implementación
                                                                      identificada.

  Compartir imagen        Uso de `Share.Default.RequestAsync`.        Implementación
                                                                      identificada.

  Exportación Excel       Los manejadores muestran mensajes de        Pendiente de
                          próxima disponibilidad; no se identifica    implementación.
                          biblioteca Excel.
  -------------------------------------------------------------------------------------------

### 3.2. Dependencias identificadas

  --------------------------------------------------------------------------------------
  Dependencia                            Versión declarada       Uso
  -------------------------------------- ----------------------- -----------------------
  `Microsoft.Maui.Controls`              `$(MauiVersion)`        Interfaz MAUI

  `Microsoft.Extensions.Logging.Debug`   10.0.0                  Registro de depuración

  `Plugin.Maui.OCR`                      1.1.1                   Reconocimiento de texto

  `sqlite-net-pcl`                       1.11.285                Persistencia SQLite
  --------------------------------------------------------------------------------------

### 3.3. Capa de datos

La clase `CamScannerDatabase` inicializa `camscanner.db3` dentro de
`FileSystem.AppDataDirectory` y crea las tablas `Lote` y `Documento`.

Se identifican operaciones para: - consultar lotes; - insertar o
actualizar lotes; - guardar documentos con validación de existencia del
lote; - consultar documentos; - consultar documentos por lote; -
eliminar documentos; - impedir la eliminación de lotes que tengan
documentos asociados.

La relación se controla mediante el identificador `LoteId` y consultas
de aplicación. La revisión no demuestra una restricción de clave foránea
declarada en SQLite.

## 4. Resultados de pruebas automatizadas

El 17 de noviembre de 2026 se ejecutó:

``` powershell
dotnet test .\tests\CamScanner.Tests\CamScanner.Tests.csproj
```

Resultado reportado:

  Métrica                                 Resultado
  ------------------------------------- -----------
  Pruebas ejecutadas                             29
  Aprobadas                                      29
  Fallidas                                        0
  Omitidas                                        0
  Compilación del proyecto de pruebas      Correcta

### 4.1. Cobertura identificada

**AlmacenImagenesTests** - Copia de contenido. - Manejo de
extensiones. - Creación de directorios. - Generación de rutas únicas. -
Manejo de errores durante la copia. - Eliminación de archivos. -
Tratamiento de rutas vacías o archivos inexistentes.

**ValidadorNombresTests** - Generación de nombres consecutivos. - Manejo
de consecutivos inválidos. - Búsqueda del siguiente nombre disponible. -
Normalización de mayúsculas y espacios. - Validación de listas vacías. -
Detección de nombres vacíos y duplicados. - Validación de nombres
únicos.

Las 29 pruebas aprobadas corresponden a los componentes automatizados
indicados. No demuestran por sí solas el funcionamiento de la cámara,
OCR, interfaz MAUI, base de datos SQLite, exportación Excel ni
compatibilidad en dispositivos.

## 5. Resultados de compilación

Se ejecutó:

``` powershell
dotnet build .\src\CamScanner.csproj -f net10.0-android
```

Resultado: compilación no completada.

**Error reportado:** `XA5300`, no se encontró el directorio Android SDK.

  -----------------------------------------------------------------------
  Verificación                        Resultado
  ----------------------------------- -----------------------------------
  Ejecución de pruebas automatizadas  Correcta

  Compilación del proyecto de pruebas Correcta

  Compilación Android en el entorno   Fallida por Android SDK no
  reportado                           localizado

  Ejecución funcional Android         No demostrada mediante esta salida
  -----------------------------------------------------------------------

La incidencia XA5300 corresponde a la configuración del entorno. No se
clasifica como defecto funcional confirmado del producto.

## 6. Evaluación de características de calidad

  -----------------------------------------------------------------------
  Característica          Estado observado        Acción recomendada
  ----------------------- ----------------------- -----------------------
  Adecuación funcional    Existen flujos de       Completar Excel y
                          lotes, documentos,      ejecutar pruebas de
                          imágenes y OCR en el    aceptación.
                          código; Excel está
                          pendiente.

  Eficiencia de desempeño No hay mediciones de    Medir tiempos y consumo
                          OCR ni tiempos de       en dispositivos
                          procesamiento.          objetivo.

  Compatibilidad          El proyecto declara     Configurar Android SDK
                          Android y otros         y validar en
                          destinos; Android no    dispositivos.
                          compiló en el entorno
                          reportado.

  Usabilidad              Existen pantallas y     Ejecutar pruebas de
                          acciones de negocio. No navegación y
                          se aportó evaluación de comprensión.
                          usuarios.

  Confiabilidad           Hay manejo de errores   Ampliar pruebas de
                          en varios flujos y      persistencia, permisos
                          pruebas unitarias de    e interrupciones.
                          servicios.

  Seguridad y privacidad  Se utiliza              Revisar permisos,
                          almacenamiento local de almacenamiento y
                          aplicación; no se       tratamiento de
                          realizó auditoría       imágenes.
                          completa.

  Mantenibilidad          Hay separación entre    Mantener
                          páginas, modelos, datos responsabilidades
                          y servicios.            separadas y ampliar
                                                  pruebas.

  Portabilidad            Se declaran varios      Verificar cada
                          destinos de plataforma. plataforma antes de
                                                  afirmar compatibilidad.

  Integridad              Se comprueba la         Probar escenarios
                          existencia del lote al  límite y consistencia
                          guardar documentos y se de archivos y
                          bloquea eliminar lotes  registros.
                          con documentos.
  -----------------------------------------------------------------------

## 7. Riesgos identificados

  ---------------------------------------------------------------------------------------
  ID             Riesgo               Probabilidad   Impacto        Acción recomendada
  -------------- -------------------- -------------- -------------- ---------------------
  R-01           Imágenes borrosas,   Alta           Alto           Probar condiciones
                 inclinadas u oscuras                               variadas y permitir
                 pueden reducir la                                  corrección manual.
                 precisión del OCR.

  R-02           El OCR local puede   Media          Alto           Medir consumo y
                 consumir recursos                                  tiempo de
                 elevados en                                        procesamiento.
                 dispositivos
                 limitados.

  R-03           Permisos denegados o Media          Medio          Validar permisos y
                 archivos                                           formatos en
                 incompatibles pueden                               dispositivo.
                 impedir captura e
                 importación.

  R-04           Las operaciones      Media          Alto           Ampliar pruebas de
                 SQLite pueden                                      persistencia y CRUD.
                 presentar errores no
                 detectados por las
                 pruebas actuales.

  R-05           La relación          Media          Alto           Probar integridad y
                 lote-documento                                     definir si se
                 depende de                                         requiere restricción
                 validaciones de                                    referencial.
                 aplicación.

  R-06           La exportación Excel Alta           Alto           Seleccionar
                 aún no está                                        biblioteca,
                 implementada.                                      implementar y probar
                                                                    exportación
                                                                    individual y por
                                                                    lote.

  R-07           No hay evidencia     Alta           Alto           Ejecutar casos con
                 funcional de OCR en                                imágenes de
                 dispositivo.                                       referencia y
                                                                    registrar resultados.

  R-08           Las pruebas          Alta           Alto           Ampliar pruebas de
                 automatizadas                                      datos y aceptación.
                 actuales no cubren
                 todos los flujos de
                 negocio.

  R-09           La configuración de  Media          Medio          Revisar manifiesto y
                 permisos Android                                   permisos en tiempo de
                 puede no cubrir                                    ejecución.
                 todas las
                 capacidades
                 requeridas.

  R-10           Dependencias OCR     Media          Medio          Verificar versiones,
                 pueden presentar                                   compatibilidad y
                 incompatibilidades                                 licencias.
                 según dispositivo o
                 plataforma.

  R-11           La ausencia del      Alta           Medio          Instalar/configurar
                 Android SDK impide                                 Android SDK y repetir
                 verificar la                                       la compilación.
                 compilación Android
                 en el entorno
                 reportado.
  ---------------------------------------------------------------------------------------

## 8. Hallazgos de calidad

Los hallazgos siguientes se basan en revisión de código y resultados de
herramientas. No todos representan defectos funcionales reproducidos.

  -----------------------------------------------------------------------
  ID                Hallazgo          Severidad         Estado
                                      sugerida
  ----------------- ----------------- ----------------- -----------------
  H-01              La exportación    Alta              Pendiente de
                    Excel muestra                       implementación
                    mensajes de
                    próxima
                    disponibilidad y
                    no cuenta con
                    implementación
                    identificada.

  H-02              OCR integrado en  Alta              Pendiente de
                    el código, pero                     validación
                    sin evidencia de
                    prueba funcional
                    en dispositivo.

  H-03              La compilación    Media             Incidencia de
                    Android no se                       entorno pendiente
                    completó por
                    error XA5300.

  H-04              Los 29 tests      Media             Ampliación
                    automatizados                       recomendada
                    cubren
                    almacenamiento de
                    imágenes y
                    validación de
                    nombres, no todos
                    los flujos de
                    negocio.

  H-05              No se recibió     Media             Sin reporte
                    reporte adicional                   disponible
                    de resultados
                    funcionales de
                    Javier.

  H-06              La relación       Media             Revisión técnica
                    lote-documento se                   pendiente
                    valida mediante
                    lógica de
                    aplicación; no se
                    comprobó una
                    clave foránea
                    SQLite.

  H-07              No se aportaron   Media             Pendiente de
                    evidencias                          ejecución y
                    individuales de                     registro
                    ejecución de
                    CP-01 a CP-32.
  -----------------------------------------------------------------------

### 8.1. Incidencias de entorno y defectos del producto

-   **Incidencia de entorno:** H-03, por ausencia del Android SDK.
-   **Pendientes de producto:** H-01, H-02 y H-06.
-   **Pendientes de verificación/documentación:** H-04, H-05 y H-07.

La ausencia de evidencia no debe interpretarse automáticamente como un
defecto reproducido.

## 9. Trazabilidad y estado de verificación

La especificación contiene 18 requisitos funcionales y 10 requisitos no
funcionales. El plan de pruebas relaciona los requisitos funcionales con
los criterios CA-01 a CA-18 y con los casos CP-01 a CP-22; también
asocia los requisitos no funcionales con CP-23 a CP-32.

La relación documental está establecida, pero no se cuenta con
resultados individuales para declarar aprobados los casos funcionales y
no funcionales.

Las 29 pruebas automatizadas se registran de forma independiente y no
deben sumarse como casos de aceptación aprobados.

## 10. Acciones de mejora priorizadas

1.  Configurar el Android SDK y repetir la compilación para
    `net10.0-android`.
2.  Instalar y ejecutar la aplicación en un dispositivo o emulador
    Android compatible.
3.  Validar captura e importación, incluyendo permisos y cancelaciones.
4.  Ejecutar pruebas de OCR con imágenes de referencia y comprobar el
    funcionamiento sin conexión.
5.  Validar edición y persistencia del texto reconocido.
6.  Probar el CRUD de lotes y documentos, incluyendo integridad de
    asociaciones.
7.  Implementar la exportación individual y por lote a Excel.
8.  Ampliar pruebas automatizadas para la capa de datos y reglas de
    negocio.
9.  Ejecutar los casos CP-01 a CP-32 que correspondan y registrar fecha,
    entorno, resultado y evidencia.
10. Registrar defectos reproducibles y vincularlos con requisitos y
    casos de prueba.
11. Actualizar los estados documentales después de cada nueva ejecución.

## 11. Conclusión

La revisión del 17 de noviembre de 2026 identifica una versión más
avanzada que la descrita en el informe anterior. El código incluye
gestión de lotes y documentos, captura e importación de imágenes,
almacenamiento local, integración OCR, edición de texto, copia al
portapapeles y uso de funciones para compartir imágenes.

La exportación Excel permanece pendiente. La integración OCR está
identificada en el código, pero su comportamiento debe validarse
funcionalmente en dispositivos. La capa de datos contiene operaciones
para lotes y documentos y validaciones para evitar eliminar lotes que
tengan documentos asociados.

Las pruebas automatizadas reportaron 29 aprobadas, 0 fallidas y 0
omitidas. Su cobertura corresponde a almacenamiento de imágenes y
validación de nombres, por lo que no certifica el funcionamiento
integral de la aplicación.

La compilación Android no pudo completarse debido al error XA5300 por
ausencia o falta de configuración del Android SDK en el entorno
reportado. No se recibió un reporte adicional de resultados funcionales
de Javier ni evidencias individuales para los casos de aceptación.

En consecuencia, CamScanner presenta una implementación parcial avanzada
respecto al alcance definido. Antes de declarar la entrega
funcionalmente validada, se requiere configurar el entorno Android,
ejecutar pruebas en dispositivo, verificar OCR, completar Excel y
registrar las evidencias y defectos correspondientes.
