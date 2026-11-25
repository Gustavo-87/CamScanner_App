# Plan de pruebas - CamScanner

**Proyecto:** CamScanner - Android - .NET 10 / .NET MAUI
**Versión del plan:** 2.1 - Validación funcional en Android
**Responsable de pruebas:** Javier Serna
**Documentación y calidad:** Richard Prado
**Especificación vigente:** [docs/especificacion.md](especificacion.md)
**Fecha de ejecución funcional:** 17 de noviembre de 2026
**Commit validado:** `20d44705efe1443b3643ab5abad6f17f6d419d58`
**Entornos:** emulador Android Pixel 7 - API 36.0 (Android 16) y Xiaomi 23090RA98G - Android 16.0 / API 36 (Redmi Note 13 Pro+ 5G)

## 1. Objetivo y alcance

Verificar las funciones actuales: creación y gestión de lotes, captura e importación de imágenes, SQLite, OCR local, revisión y edición de texto, ordenación de documentos, exportación a Excel y exportación ZIP. La apertura del diálogo nativo de compartir se valida manualmente en la plataforma.

## 2. Funciones confirmadas

- Android `net10.0-android`, API mínima configurada 23.
- Importacion múltiple mediante `MediaPicker`, con límite de 10 imágenes por selección.
- Imagenes copiadas a `AppDataDirectory/documentos` y registros almacenados en SQLite.
- OCR mediante `IOcrService`, local en Android/iOS, con inicialización única por sesion.
- Editor de texto completo con copiar, limpiar, corregir y guardar.
- Reordenamiento por arrastre en la vista interior.
- Menú DB para cambiar lote, ID, nombre y orden; normalización por lote a `0..n-1` sin repetidos.
- Exportacion individual desde detalle y exportación de lotes desde la selección de lotes.
- Exportación ZIP de lotes seleccionados, incluyendo el Excel y las imágenes disponibles.
- Los botones de guardar Excel/ZIP y compartir Excel/ZIP están disponibles según la plataforma.
- La opción de exportar del menu contextual principal muestra `Próximamente`.

## 3. Casos de prueba funcionales

| ID    | Requisito    | Escenario                                                              | Resultado esperado                                                                 | Estado   |
| ----- | ------------ | ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------- | -------- |
| CP-01 | RF-07        | Crear lote válido; cancelar; usar nombre vacío o repetido.             | Solo se crea con nombre válido y único; aparece en la lista.                       | Correcto |
| CP-02 | RF-08/RF-09  | Consultar lotes; cambiar nombre y descripción; recargar.               | Lotes y cambios se muestran sin duplicarse y persisten.                            | Correcto |
| CP-03 | RF-10        | Eliminar lote vacío, cancelar y eliminar lote con documentos.          | Vacio se elimina; cancelar conserva; lote con documentos se rechaza.               | Correcto |
| CP-04 | RF-01        | Conceder permiso y capturar foto para un lote.                         | Imagen copiada, documento creado y visible en el lote.                             | Correcto |
| CP-05 | RF-01/RF-18  | Denegar permiso o usar dispositivo sin cámara.                         | Mensaje claro, sin cierre y sin documento nuevo.                                   | Correcto |
| CP-06 | RF-02        | Importar una imagen, varias, 10, mas de 10 y cancelar selector.        | Imagenes incorporadas, se respeta límite 10 y cancelar no cambia datos.            | Correcto |
| CP-07 | RF-11/RF-12  | Revisar, abrir y consultar documento desde tarjeta y lote.             | ID, ruta, fecha, lote, imagen y orden son correctos.                               | Correcto |
| CP-08 | RF-03/RNF-03 | Ejecutar OCR sobre imagen legible sin Internet.                        | Texto reconocido localmente y asociado al documento.                               | Correcto |
| CP-09 | RF-03/RF-18  | OCR sobre imagen borrosa, sin texto, inexistente o con error temporal. | Resultado/error controlado sin pérdida ni cierre.                                  | Correcto |
| CP-10 | RF-04        | Abrir aviso de texto listo o documento con OCR existente.              | Editor visible y no se ejecuta OCR innecesariamente.                               | Correcto |
| CP-11 | RF-06        | Editar, guardar, reabrir, copiar selección, copiar todo y limpiar.     | Correccion persiste; copiar y limpiar respetan la accion confirmada.               | Correcto |
| CP-12 | RF-13        | Renombrar documento y modificar ID, lote, nombre y orden desde DB.     | Valores válidos se guardan; ID o nombre duplicado, o lote inexistente, se rechaza. | Correcto |
| CP-13 | RF-13/RNF-09 | Mover un documento entre dos lotes con documentos existentes.          | Origen y destino quedan con órdenes `0..n-1`, sin repetidos ni huecos.             | Correcto |
| CP-14 | RF-13/RNF-09 | Introducir órdenes negativos, duplicados y con huecos.                 | Normalización deja cada lote numerado desde `0`.                                   | Correcto |
| CP-15 | RF-13        | Arrastrar tres documentos en vista interior y recargar.                | Orden visual persistente.                                                          | Correcto |
| CP-16 | RF-14        | Eliminar documento confirmando y cancelando.                           | Confirmar elimina registro/imagen; cancelar conserva.                              | Correcto |
| CP-17 | RF-15        | Crear, editar, mover y eliminar; cerrar y abrir aplicación.            | SQLite conserva cambios confirmados y no reaparecen eliminados.                    | Correcto |
| CP-18 | RF-16        | Exportar documento desde detalle con y sin OCR; guardar y cancelar.    | XLSX legible; guardar funciona y cancelar no modifica datos.                       | Correcto |
| CP-19 | RF-17        | Exportar uno o varios lotes con documentos.                            | Una hoja por lote, documentos ordenados por `Orden`.                               | Correcto |
| CP-20 | RNF-10       | Exportar con nombres duplicados, inválidos y de mas de 31 caracteres.  | XLSX abre y sus hojas son validas, unicas y truncadas cuando procede.              | Correcto |
| CP-21 | RF-18        | Compartir imagen existente y repetir con ruta inexistente.             | Compartir funciona o informa error controlado.                                     | Correcto |
| CP-22 | RF-18        | Provocar fallo de almacenamiento, OCR o exportación.                   | Mensaje claro, sin cierre y datos confirmados conservados.                         | Correcto |
| CP-23 | RF-17        | Generar ZIP con uno o varios lotes, Excel e imágenes existentes.        | ZIP válido con `CamScanner_Lotes.xlsx` y carpetas por lote.                        | Correcto |
| CP-24 | RF-17        | Generar ZIP con lotes vacíos o imágenes inexistentes.                   | Lotes vacíos se omiten; el Excel se conserva y no se agregan imágenes inexistentes. | Correcto |
| CP-25 | RF-17        | Intentar generar ZIP sin documentos.                                   | Se informa que no hay lotes con documentos y no se genera un archivo válido.        | Correcto |
| CP-26 | RF-18        | Pulsar guardar/compartir Excel o ZIP en Android.                       | Se abre el selector del sistema o se guarda el archivo; no hay bloqueo ni pérdida.  | Correcto |

## 4. Brechas actuales

| ID    | Relacion     | Verificacion                                                | Resultado actual                                                                        | Estado    |
| ----- | ------------ | ----------------------------------------------------------- | --------------------------------------------------------------------------------------- | --------- |
| BG-01 | RF-05/CA-05  | Buscar selección de campos individuales del OCR.            | Solo existe texto completo; redefinir RF-05 o implementar.                              | Pendiente |
| BG-02 | RF-16        | Exportar desde menu contextual principal.                   | Muestra `Próximamente`; actualmente se prueba desde detalle.                            | Pendiente |
| BG-03 | RF-03        | Ejecutar OCR fuera de Android/iOS.                          | Servicio no registrado fuera de esas plataformas.                                       | Pendiente |
| BG-04 | RF-02/RNF-02 | Definir formatos, tamaños, idiomas y tiempos OCR.           | Parámetros no definidos.                                                                | Pendiente |
| BG-05 | RNF-07       | Revisar el proyecto independiente de pruebas automatizadas. | Existe `tests/CamScanner.Tests`; su cobertura se ejecuta y debe mantenerse actualizada. | Pendiente |

## 5. Pruebas automatizables propuestas

La suite automatizada cubre `ValidadorNombres`, `AlmacenImagenes`, `ExcelExportService`, `ZipExportService` y `CamScannerDatabase`, incluyendo validaciones, archivos parciales, XLSX, ZIP, CRUD, movimientos y normalización `0..n-1`. La llamada al selector nativo de compartir (`Share.Default`) requiere una prueba manual en Android/iOS/Windows.

## 6. Casos no funcionales

| ID     | Requisito | Prueba                                                                | Criterio de aceptación                                 | Estado   |
| ------ | --------- | --------------------------------------------------------------------- | ------------------------------------------------------ | -------- |
| NFR-01 | RNF-01    | Usuario nuevo completa crear lote, importar, OCR, editar y exportar.  | Navegación y mensajes comprensibles.                   | Correcto |
| NFR-02 | RNF-02    | Medir OCR con 1, 10 y 20 imágenes y exportación de varios lotes.      | Duración registrada, sin bloqueos; umbral por definir. | Correcto |
| NFR-03 | RNF-03    | Ejecutar OCR y consultar datos sin red.                               | OCR y SQLite funcionan offline.                        | Correcto |
| NFR-04 | RNF-04    | Reiniciar después de operaciones confirmadas.                         | Lotes, documentos, texto, rutas y órdenes persisten.   | Correcto |
| NFR-05 | RNF-05    | Cancelar selectores, permisos, compartir, guardado y OCR.             | Sin cierres ni pérdida confirmada.                     | Correcto |
| NFR-06 | RNF-06    | Probar Android API 23 o superior, dispositivo y emulador.             | Instala, inicia y completa flujos compatibles.         | Correcto |
| NFR-07 | RNF-07    | Revisar separación de paginas, servicios, modelos y SQLite; compilar. | Solución compilable y responsabilidades localizables.  | Correcto |
| NFR-08 | RNF-08    | Revisar permisos, almacenamiento, logs y tráfico.                     | Datos locales y sin transmisión no autorizada.         | Correcto |
| NFR-09 | RNF-09    | Ejecutar movimientos, cambios de ID, eliminación y normalización.     | Sin huérfanos ni órdenes repetidas.                    | Correcto |
| NFR-10 | RNF-10    | Abrir XLSX y comparar con la fuente.                                  | Hojas, texto, fechas, nombres y orden correctos.       | Correcto |

## 7. Entrada y salida

### Criterios de entrada

- Compilacion correcta del proyecto .NET 10 MAUI.
- APK instalada en Android con cámara, galería, almacenamiento y compartir.
- Permisos disponibles para conceder y denegar.
- Imagenes sin datos personales y con texto OCR conocido.
- Aplicacion para abrir XLSX y recurso para guardar/compartir.
- Procedimiento para limpiar la base local entre ciclos.

### Criterios de salida

- Casos ejecutados con fecha, version, dispositivo y evidencia.
- Ningún defecto crítico que impida capturar, persistir, consultar, ordenar o exportar.
- Casos bloqueados/no aplicables justificados y brechas decididas.
- Fallos de integridad, pérdida de datos, cierres o corrupción de Excel corregidos o aceptados.

## 8. Registro de ejecución

La ejecución funcional de los casos CP-01 a CP-26 se realizó correctamente el 17 de noviembre de 2026 en los dos entornos Android indicados en la cabecera. La ejecución se referencia al commit `20d44705efe1443b3643ab5abad6f17f6d419d58`. La suite automatizada local del proyecto `tests/CamScanner.Tests` obtuvo 65 pruebas correctas, 0 fallidas y 0 omitidas; cubre servicios puros y SQLite temporal, incluyendo la unicidad de nombres de lotes y documentos por lote, la generación de XLSX y ZIP.

| ID                 | Fecha                        | Versión/commit | Dispositivo/emulador | Datos usados                | Resultado                              | Evidencia/incidencia                                         |
| ------------------ | ---------------------------- | -------------- | -------------------- | --------------------------- | -------------------------------------- | ------------------------------------------------------------ |
| Pruebas funcionales Android | 17/11/2026 | `20d44705efe1443b3643ab5abad6f17f6d419d58` | Pixel 7 (emulador), Android 16 / API 36; Xiaomi 23090RA98G (Redmi Note 13 Pro+ 5G), Android 16 / API 36 | Flujos de captura, lotes, documentos, OCR, Excel y ZIP | CP-01 a CP-26 correctos | Validación funcional en ambos entornos |
| Suite automatizada | 17/11/2026 | `20d44705efe1443b3643ab5abad6f17f6d419d58` | Ejecución local | Servicios y SQLite temporal | 65 aprobadas / 0 fallidas / 0 omitidas | `dotnet test tests/CamScanner.Tests/CamScanner.Tests.csproj` |

## 9. Evidencias mínimas

- Capturas de creación, permisos, importación, OCR y edición.
- Vista antes y después de mover/reordenar, con órdenes `0,1,2...`.
- Persistencia después de reiniciar sin datos personales.
- XLSX individual y por lote abiertos y comparados con la fuente.
- Mensajes de error y conservación de datos confirmados.
- Resultado de compilación y reporte automatizado cuando exista.

## 10. Trazabilidad

| Requisito | Casos principales                 | Estado                             |
| --------- | --------------------------------- | ---------------------------------- |
| RF-01     | CP-04, CP-05                      | Correcto                           |
| RF-02     | CP-06                             | Correcto                           |
| RF-03     | CP-08, CP-09                      | Correcto                           |
| RF-04     | CP-10                             | Correcto                           |
| RF-05     | BG-01                             | Brecha no implementada             |
| RF-06     | CP-11                             | Correcto                           |
| RF-07     | CP-01                             | Correcto                           |
| RF-08     | CP-02, CP-07                      | Correcto                           |
| RF-09     | CP-02                             | Correcto                           |
| RF-10     | CP-03                             | Correcto                           |
| RF-11     | CP-04, CP-06, CP-07               | Correcto                           |
| RF-12     | CP-07, CP-10                      | Correcto                           |
| RF-13     | CP-12, CP-13, CP-14, CP-15        | Correcto; incluye normalización    |
| RF-14     | CP-16                             | Correcto                           |
| RF-15     | CP-17, NFR-04                     | Correcto                           |
| RF-16     | CP-18, BG-02                      | Parcial: menu contextual pendiente |
| RF-17     | CP-19, CP-20                      | Correcto                           |
| RF-18     | CP-05, CP-06, CP-09, CP-21, CP-22 | Correcto                           |

| RNF    | Caso asociado | Estado    |
| ------ | ------------- | --------- |
| RNF-01 | NFR-01        | Correcto  |
| RNF-02 | NFR-02, BG-04 | Correcto  |
| RNF-03 | NFR-03, BG-03 | Pendiente |
| RNF-04 | NFR-04        | Correcto  |
| RNF-05 | NFR-05        | Correcto  |
| RNF-06 | NFR-06        | Correcto  |
| RNF-07 | NFR-07, BG-05 | Correcto  |
| RNF-08 | NFR-08        | Correcto  |
| RNF-09 | NFR-09        | Correcto  |
| RNF-10 | NFR-10, CP-20 | Correcto  |
