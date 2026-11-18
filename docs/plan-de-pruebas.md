# Plan de pruebas --- CamScanner

**Proyecto:** CamScanner · Android · .NET 10 / .NET MAUI\
**Revisión documental:** 17 de noviembre de 2026\
**Responsable de pruebas reportado en la documentación del equipo:**
Javier Serna\
**Documentación y calidad:** Richard Prado\
**Especificación:** [docs/especificacion.md](especificacion.md)

## 1. Objetivo y alcance

Registrar los casos de prueba definidos para CamScanner y distinguir
entre: - implementación identificada mediante revisión del código; -
pruebas automatizadas ejecutadas; - pruebas funcionales o de aceptación
ejecutadas en dispositivo; - pruebas pendientes de ejecución o
evidencia.

No se recibió un reporte adicional de resultados funcionales de Javier
para esta actualización. Por tanto, no se atribuyen aprobaciones
funcionales a casos que no cuentan con evidencia de ejecución.

El alcance incluye captura e importación de imágenes, OCR, revisión y
edición de texto, gestión de lotes y documentos, persistencia SQLite y
exportación Excel. La exportación Excel permanece pendiente de
implementación. No se incluye exportación ZIP/PDF.

## 2. Entorno y herramientas

  -----------------------------------------------------------------------
  Elemento                            Información disponible
  ----------------------------------- -----------------------------------
  Lenguaje y plataforma               C# / .NET 10

  Interfaz                            .NET MAUI

  Plataforma principal                Android

  Persistencia                        SQLite mediante `sqlite-net-pcl`
                                      1.11.285

  OCR                                 `Plugin.Maui.OCR` 1.1.1

  Pruebas automatizadas               xUnit

  SDK .NET reportado por ejecución    .NET 10.0.14 en salida de pruebas

  Entorno de ejecución funcional      No reportado para esta
                                      actualización

  Android SDK                         No localizado durante el intento de
                                      compilación Android; error XA5300
  -----------------------------------------------------------------------

## 3. Clasificación de estados

-   **Implementado; pendiente de prueba funcional:** se identificó
    código para la función, pero no se aportó evidencia de ejecución en
    dispositivo.
-   **Parcial:** existe parte del comportamiento, pero no se demuestra
    el cumplimiento completo del requisito.
-   **Pendiente de implementación:** no se identificó implementación
    funcional en el código revisado.
-   **Aprobado:** existe resultado de ejecución verificable para el caso
    concreto.
-   **No ejecutado:** no se aportó evidencia de ejecución del caso.

La clasificación de implementación no reemplaza el resultado de una
prueba.

## 4. Casos de prueba funcionales

  ---------------------------------------------------------------------------------
  ID             Requisito      Escenario      Resultado esperado  Estado actual
  -------------- -------------- -------------- ------------------- ----------------
  CP-01          RF-01          Capturar       Imagen capturada y  Implementado;
                                fotografía con disponible en un    pendiente de
                                permiso        lote.               prueba funcional
                                concedido.

  CP-02          RF-01          Denegar        Mensaje claro, sin  Manejo previsto
                                permiso de     cierre inesperado.  en código;
                                cámara.                            pendiente de
                                                                   prueba

  CP-03          RF-02          Importar       Imagen incorporada  Implementado;
                                imagen         al lote             pendiente de
                                compatible.    seleccionado.       prueba funcional

  CP-04          RF-02          Cancelar       Se conserva el      Manejo de
                                selector de    estado actual.      cancelación
                                imágenes.                          presente;
                                                                   pendiente de
                                                                   prueba

  CP-05          RF-03          Procesar       OCR local reconoce  OCR integrado;
                                imagen legible texto sin conexión. modo sin
                                sin Internet.                      conexión
                                                                   pendiente de
                                                                   validar

  CP-06          RF-03          Procesar       Se informa          Manejo de
                                imagen borrosa resultado parcial o resultado sin
                                o sin texto.   ausencia de texto.  texto presente;
                                                                   pendiente de
                                                                   prueba

  CP-07          RF-04          Revisar        Texto reconocido    Implementado en
                                resultado OCR. visible para el     código;
                                               usuario.            pendiente de
                                                                   prueba

  CP-08          RF-05          Seleccionar    Se conservan solo   Selección
                                algunos datos  los datos elegidos. estructurada no
                                reconocidos.                       evidenciada;
                                                                   pendiente

  CP-09          RF-06          Corregir texto Texto corregido se  Editor y evento
                                reconocido.    conserva.           de guardado
                                                                   presentes;
                                                                   pendiente de
                                                                   prueba

  CP-10          RF-07          Crear lote     Lote registrado.    Implementado;
                                válido.                            pendiente de
                                                                   prueba funcional

  CP-11          RF-08          Consultar      Se muestran lotes   Implementado;
                                lotes.         existentes.         pendiente de
                                                                   prueba funcional

  CP-12          RF-09          Modificar      Cambios guardados.  Implementado;
                                lote.                              pendiente de
                                                                   prueba funcional

  CP-13          RF-10          Eliminar lote  Lote deja de        Implementado en
                                sin            aparecer.           código;
                                documentos.                        pendiente de
                                                                   prueba

  CP-14          RF-10          Eliminar lote  Se impide la        Validación
                                con documentos eliminación y se    presente;
                                asociados.     informa al usuario. pendiente de
                                                                   prueba

  CP-15          RF-11          Crear          Documento           Implementado;
                                documento y    registrado y        pendiente de
                                asociarlo a    relacionado.        prueba funcional
                                lote.

  CP-16          RF-12          Consultar      Se muestran los     Consulta general
                                documentos de  documentos          y por lote
                                un lote.       correspondientes.   presente en
                                                                   datos; pendiente
                                                                   de prueba

  CP-17          RF-13          Modificar      Cambios guardados.  Renombrado y
                                documento.                         actualización de
                                                                   texto presentes;
                                                                   pendiente de
                                                                   prueba

  CP-18          RF-14          Eliminar       Registro y archivo  Implementado en
                                documento.     de imagen dejan de  código;
                                               estar disponibles.  pendiente de
                                                                   prueba

  CP-19          RF-15          Guardar y      Registros           Persistencia
                                reiniciar      confirmados         implementada;
                                aplicación.    persisten en        prueba de
                                               SQLite.             reinicio
                                                                   pendiente

  CP-20          RF-16          Exportar       Excel con datos     Pendiente de
                                documento      seleccionados.      implementación
                                individual.

  CP-21          RF-17          Exportar lote. Excel con datos de  Pendiente de
                                               documentos del      implementación
                                               lote.

  CP-22          RF-18          Provocar error Mensaje claro y     Manejo de
                                controlado.    datos confirmados   errores parcial;
                                               conservados.        pendiente de
                                                                   prueba integral
  ---------------------------------------------------------------------------------

## 5. Casos de prueba no funcionales

  --------------------------------------------------------------------------------------
  ID             Requisito      Descripción de la    Resultado esperado Estado actual
                                prueba
  -------------- -------------- -------------------- ------------------ ----------------
  CP-23          RNF-03         Ejecutar OCR sin     OCR funciona sin   Pendiente de
                                conexión.            Internet.          validación

  CP-24          RNF-04         Reiniciar después de Datos disponibles  Persistencia en
                                guardar.             nuevamente.        código; prueba
                                                                        pendiente

  CP-25          RNF-02         Procesar conjunto    Duración           Pendiente
                                representativo de    registrada y sin
                                imágenes.            bloqueos
                                                     inaceptables.

  CP-26          RNF-10         Abrir archivo Excel  Datos legibles y   Pendiente de
                                generado.            correspondientes a implementación y
                                                     la selección.      prueba

  CP-27          RNF-01         Revisar que las      Navegación y       Pendiente de
                                opciones principales acciones           evaluación
                                sean comprensibles.  principales
                                                     claras.

  CP-28          RNF-05         Interrumpir una      Sin pérdida        Pendiente
                                operación controlada inesperada de
                                y volver a utilizar  información
                                la aplicación.       confirmada.

  CP-29          RNF-06         Instalar y ejecutar  La aplicación      Pendiente;
                                en dispositivo o     inicia y permite   compilación
                                emulador Android     utilizar las       bloqueada por
                                compatible.          funciones          XA5300 en el
                                                     disponibles.       entorno
                                                                        reportado

  CP-30          RNF-07         Revisar organización Estructura         Revisión
                                del código y         comprensible y     estática
                                separación de        mantenible.        parcial;
                                responsabilidades.                      pendiente de
                                                                        evaluación
                                                                        formal

  CP-31          RNF-08         Revisar              Tratamiento        Pendiente
                                almacenamiento,      conforme a las
                                permisos y           condiciones de
                                transmisiones de     privacidad.
                                información.

  CP-32          RNF-09         Verificar relación   Sin asociaciones   Validaciones en
                                entre lotes y        inconsistentes ni  código;
                                documentos al        pérdida            pendiente de
                                registrar, modificar accidental.        prueba funcional
                                o eliminar.
  --------------------------------------------------------------------------------------

## 6. Registro de pruebas automatizadas

El 17 de noviembre de 2026 se ejecutó el comando:

``` powershell
dotnet test .\tests\CamScanner.Tests\CamScanner.Tests.csproj
```

Resultado reportado por la consola:

  Indicador                                            Resultado
  -------------------------------------------------- -----------
  Total de pruebas                                            29
  Aprobadas                                                   29
  Fallidas                                                     0
  Omitidas                                                     0
  Resultado de compilación del proyecto de pruebas      Correcto

### 6.1. Archivos de pruebas identificados

**`AlmacenImagenesTests.cs`** - Copia del contenido al almacenamiento. -
Conservación de extensiones conocidas y uso de `.jpg` como
alternativa. - Creación de carpeta cuando no existe. - Generación de
rutas diferentes para archivos con el mismo nombre. - Propagación de
errores de lectura sin dejar archivos incompletos. - Eliminación de
archivos existentes. - Manejo de archivos inexistentes y rutas vacías.

**`ValidadorNombresTests.cs`** - Generación de nombres consecutivos. -
Tratamiento de consecutivos inválidos. - Generación del siguiente nombre
disponible. - Omisión de nombres ocupados. - Normalización de mayúsculas
y espacios. - Validación de listas vacías, nombres vacíos y
duplicados. - Aceptación de nombres únicos.

Las 29 pruebas automatizadas corresponden a estos componentes. No deben
contabilizarse como aprobación de los casos funcionales CP-01 a CP-22 ni
de los casos no funcionales CP-23 a CP-32.

## 7. Verificación de compilación

Se ejecutó:

``` powershell
dotnet build .\src\CamScanner.csproj -f net10.0-android
```

Resultado: compilación no completada. Se reportó el error `XA5300`,
debido a que no se encontró el directorio Android SDK.

Esta incidencia corresponde a la configuración del entorno de
compilación. No demuestra por sí sola un defecto funcional de la
aplicación.

## 8. Registro de ejecución funcional

No se recibió un reporte adicional de ejecución funcional de Javier para
esta actualización. Los siguientes campos deben completarse cuando se
realicen pruebas en dispositivo:

  ID        Fecha            Dispositivo/emulador   Resultado          Evidencia/incidencia
  --------- ---------------- ---------------------- ------------------ ----------------------
  CP-\_\_   **/**/\_\_\_\_   \_\_                   Aprobado/Fallido   \_\_

No marcar un caso como aprobado sin evidencia de ejecución.

## 9. Criterios de entrada

-   Aplicación compilada e instalada en dispositivo o emulador Android
    compatible.
-   Datos de prueba preparados.
-   Permisos y recursos disponibles.
-   Imágenes de referencia con texto conocido para OCR.
-   Documentos de prueba para exportación, cuando esta funcionalidad
    esté implementada.

## 10. Criterios de salida

-   Casos ejecutados y resultados registrados.
-   Evidencias asociadas a los casos.
-   Defectos clasificados y vinculados con los casos correspondientes.
-   Fallos críticos corregidos o reportados.
-   Resultados automatizados y funcionales informados por separado.

## 11. Evidencias recomendadas

-   Captura de fotografía con cámara.
-   Importación de una y varias imágenes.
-   Creación, edición y eliminación de lotes.
-   Registro, consulta, renombrado y eliminación de documentos.
-   OCR ejecutado con imagen de referencia.
-   OCR ejecutado sin conexión, si se confirma el requisito.
-   Edición y guardado del texto reconocido.
-   Persistencia después de cerrar y abrir la aplicación.
-   Archivo Excel individual y por lote, cuando estén implementados.
-   Manejo de permisos denegados y errores.
-   Salida de consola de las 29 pruebas automatizadas.

## 12. Matriz de trazabilidad de requisitos

### 12.1. Requisitos funcionales

  Requisito   Criterio de aceptación   Caso(s) de prueba
  ----------- ------------------------ -------------------
  RF-01       CA-01                    CP-01, CP-02
  RF-02       CA-02                    CP-03, CP-04
  RF-03       CA-03                    CP-05, CP-06
  RF-04       CA-04                    CP-07
  RF-05       CA-05                    CP-08
  RF-06       CA-06                    CP-09
  RF-07       CA-07                    CP-10
  RF-08       CA-08                    CP-11
  RF-09       CA-09                    CP-12
  RF-10       CA-10                    CP-13, CP-14
  RF-11       CA-11                    CP-15
  RF-12       CA-12                    CP-16
  RF-13       CA-13                    CP-17
  RF-14       CA-14                    CP-18
  RF-15       CA-15                    CP-19
  RF-16       CA-16                    CP-20
  RF-17       CA-17                    CP-21
  RF-18       CA-18                    CP-22

### 12.2. Requisitos no funcionales

  Requisito                              Caso de prueba
  -------------------------------------- ----------------
  RNF-01 - Usabilidad                    CP-27
  RNF-02 - Desempeño                     CP-25
  RNF-03 - Funcionamiento sin conexión   CP-23
  RNF-04 - Persistencia                  CP-24
  RNF-05 - Confiabilidad                 CP-28
  RNF-06 - Compatibilidad Android        CP-29
  RNF-07 - Mantenibilidad                CP-30
  RNF-08 - Privacidad                    CP-31
  RNF-09 - Integridad                    CP-32
  RNF-10 - Legibilidad de exportación    CP-26

### 12.3. Resultado de trazabilidad

-   Requisitos funcionales relacionados con criterios y pruebas: 18 de
    18.
-   Requisitos no funcionales asociados a casos: 10 de 10.
-   Casos definidos: 32.
-   Casos funcionales aprobados con evidencia individual en esta
    actualización: ninguno reportado.
-   Pruebas automatizadas aprobadas: 29 de 29, registradas por separado.

La trazabilidad documental está establecida. El cumplimiento funcional
debe determinarse mediante la ejecución de los casos y el registro de
evidencias.
