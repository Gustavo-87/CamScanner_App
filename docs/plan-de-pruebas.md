# Plan de pruebas - CamScanner

**Proyecto:** CamScanner - Android - .NET 10 / .NET MAUI
**Versión del plan:** 2.0 - Revisión basada en la implementacion actual
**Responsable de pruebas:** Javier Serna
**Documentación y calidad:** Richard Prado
**Especificación vigente:** [docs/especificacion.md](especificacion.md)

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
| CP-01 | RF-07        | Crear lote válido; cancelar; usar nombre vacío o repetido.             | Solo se crea con nombre válido y único; aparece en la lista.                       | Pendiente de validación funcional |
| CP-02 | RF-08/RF-09  | Consultar lotes; cambiar nombre y descripción; recargar.               | Lotes y cambios se muestran sin duplicarse y persisten.                            | Pendiente de validación funcional |
| CP-03 | RF-10        | Eliminar lote vacío, cancelar y eliminar lote con documentos.          | Vacio se elimina; cancelar conserva; lote con documentos se rechaza.               | Pendiente de validación funcional |
| CP-04 | RF-01        | Conceder permiso y capturar foto para un lote.                         | Imagen copiada, documento creado y visible en el lote.                             | Pendiente de validación funcional |
| CP-05 | RF-01/RF-18  | Denegar permiso o usar dispositivo sin cámara.                         | Mensaje claro, sin cierre y sin documento nuevo.                                   | Pendiente de validación funcional |
| CP-06 | RF-02        | Importar una imagen, varias, 10, mas de 10 y cancelar selector.        | Imagenes incorporadas, se respeta límite 10 y cancelar no cambia datos.            | Pendiente de validación funcional |
| CP-07 | RF-11/RF-12  | Revisar, abrir y consultar documento desde tarjeta y lote.             | ID, ruta, fecha, lote, imagen y orden son Pendiente de validación funcionals.                               | Pendiente de validación funcional |
| CP-08 | RF-03/RNF-03 | Ejecutar OCR sobre imagen legible sin Internet.                        | Texto reconocido localmente y asociado al documento.                               | Pendiente de validación funcional |
| CP-09 | RF-03/RF-18  | OCR sobre imagen borrosa, sin texto, inexistente o con error temporal. | Resultado/error controlado sin pérdida ni cierre.                                  | Pendiente de validación funcional |
| CP-10 | RF-04        | Abrir aviso de texto listo o documento con OCR existente.              | Editor visible y no se ejecuta OCR innecesariamente.                               | Pendiente de validación funcional |
| CP-11 | RF-06        | Editar, guardar, reabrir, copiar selección, copiar todo y limpiar.     | Correccion persiste; copiar y limpiar respetan la accion confirmada.               | Pendiente de validación funcional |
| CP-12 | RF-13        | Renombrar documento y modificar ID, lote, nombre y orden desde DB.     | Valores válidos se guardan; ID o nombre duplicado, o lote inexistente, se rechaza. | Pendiente de validación funcional |
| CP-13 | RF-13/RNF-09 | Mover un documento entre dos lotes con documentos existentes.          | Origen y destino quedan con órdenes `0..n-1`, sin repetidos ni huecos.             | Pendiente de validación funcional |
| CP-14 | RF-13/RNF-09 | Introducir órdenes negativos, duplicados y con huecos.                 | Normalización deja cada lote numerado desde `0`.                                   | Pendiente de validación funcional |
| CP-15 | RF-13        | Arrastrar tres documentos en vista interior y recargar.                | Orden visual persistente.                                                          | Pendiente de validación funcional |
| CP-16 | RF-14        | Eliminar documento confirmando y cancelando.                           | Confirmar elimina registro/imagen; cancelar conserva.                              | Pendiente de validación funcional |
| CP-17 | RF-15        | Crear, editar, mover y eliminar; cerrar y abrir aplicación.            | SQLite conserva cambios confirmados y no reaparecen eliminados.                    | Pendiente de validación funcional |
| CP-18 | RF-16        | Exportar documento desde detalle con y sin OCR; guardar y cancelar.    | XLSX legible; guardar funciona y cancelar no modifica datos.                       | Pendiente de validación funcional |
| CP-19 | RF-17        | Exportar uno o varios lotes con documentos.                            | Una hoja por lote, documentos ordenados por `Orden`.                               | Pendiente de validación funcional |
| CP-20 | RNF-10       | Exportar con nombres duplicados, inválidos y de mas de 31 caracteres.  | XLSX abre y sus hojas son validas, unicas y truncadas cuando procede.              | Pendiente de validación funcional |
| CP-21 | RF-18        | Compartir imagen existente y repetir con ruta inexistente.             | Compartir funciona o informa error controlado.                                     | Pendiente de validación funcional |
| CP-22 | RF-18        | Provocar fallo de almacenamiento, OCR o exportación.                   | Mensaje claro, sin cierre y datos confirmados conservados.                         | Pendiente de validación funcional |
| CP-23 | RF-17        | Generar ZIP con uno o varios lotes, Excel e imágenes existentes.        | ZIP válido con `CamScanner_Lotes.xlsx` y carpetas por lote.                        | Pendiente de validación funcional |
| CP-24 | RF-17        | Generar ZIP con lotes vacíos o imágenes inexistentes.                   | Lotes vacíos se omiten; el Excel se conserva y no se agregan imágenes inexistentes. | Pendiente de validación funcional |
| CP-25 | RF-17        | Intentar generar ZIP sin documentos.                                   | Se informa que no hay lotes con documentos y no se genera un archivo válido.        | Pendiente de validación funcional |
| CP-26 | RF-18        | Pulsar guardar/compartir Excel o ZIP en Android.                       | Se abre el selector del sistema o se guarda el archivo; no hay bloqueo ni pérdida.  | Manual |

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
| NFR-01 | RNF-01    | Usuario nuevo completa crear lote, importar, OCR, editar y exportar.  | Navegación y mensajes comprensibles.                   | Pendiente de validación funcional |
| NFR-02 | RNF-02    | Medir OCR con 1, 10 y 20 imágenes y exportación de varios lotes.      | Duración registrada, sin bloqueos; umbral por definir. | Pendiente de validación funcional |
| NFR-03 | RNF-03    | Ejecutar OCR y consultar datos sin red.                               | OCR y SQLite funcionan offline.                        | Pendiente de validación funcional |
| NFR-04 | RNF-04    | Reiniciar después de operaciones confirmadas.                         | Lotes, documentos, texto, rutas y órdenes persisten.   | Pendiente de validación funcional |
| NFR-05 | RNF-05    | Cancelar selectores, permisos, compartir, guardado y OCR.             | Sin cierres ni pérdida confirmada.                     | Pendiente de validación funcional |
| NFR-06 | RNF-06    | Probar Android API 23 o superior, dispositivo y emulador.             | Instala, inicia y completa flujos compatibles.         | Pendiente de validación funcional |
| NFR-07 | RNF-07    | Revisar separación de paginas, servicios, modelos y SQLite; compilar. | Solución compilable y responsabilidades localizables.  | Pendiente de validación funcional |
| NFR-08 | RNF-08    | Revisar permisos, almacenamiento, logs y tráfico.                     | Datos locales y sin transmisión no autorizada.         | Pendiente de validación funcional |
| NFR-09 | RNF-09    | Ejecutar movimientos, cambios de ID, eliminación y normalización.     | Sin huérfanos ni órdenes repetidas.                    | Pendiente de validación funcional |
| NFR-10 | RNF-10    | Abrir XLSX y comparar con la fuente.                                  | Hojas, texto, fechas, nombres y orden Pendiente de validación funcionals.       | Pendiente de validación funcional |

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

La última ejecución automatizada local del proyecto `tests/CamScanner.Tests` obtuvo 65 pruebas correctas, 0 fallidas y 0 omitidas. Este resultado cubre servicios puros y SQLite temporal, incluyendo la unicidad de nombres de lotes y documentos por lote, la generación de XLSX y ZIP; no sustituye las pruebas manuales de cámara, permisos, OCR nativo, navegación, selector de compartir o integración Android.

| ID                 | Fecha                        | Versión/commit | Dispositivo/emulador | Datos usados                | Resultado                              | Evidencia/incidencia                                         |
| ------------------ | ---------------------------- | -------------- | -------------------- | --------------------------- | -------------------------------------- | ------------------------------------------------------------ |
| Suite automatizada | Fecha pendiente de registrar | `net10.0`      | Ejecución local      | Servicios y SQLite temporal | 65 aprobadas / 0 fallidas / 0 omitidas | `dotnet test tests/CamScanner.Tests/CamScanner.Tests.csproj` |

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
| RF-01     | CP-04, CP-05                      | Pendiente de validación funcional                           |
| RF-02     | CP-06                             | Pendiente de validación funcional                           |
| RF-03     | CP-08, CP-09                      | Pendiente de validación funcional                           |
| RF-04     | CP-10                             | Pendiente de validación funcional                           |
| RF-05     | BG-01                             | Brecha no implementada             |
| RF-06     | CP-11                             | Pendiente de validación funcional                           |
| RF-07     | CP-01                             | Pendiente de validación funcional                           |
| RF-08     | CP-02, CP-07                      | Pendiente de validación funcional                           |
| RF-09     | CP-02                             | Pendiente de validación funcional                           |
| RF-10     | CP-03                             | Pendiente de validación funcional                           |
| RF-11     | CP-04, CP-06, CP-07               | Pendiente de validación funcional                           |
| RF-12     | CP-07, CP-10                      | Pendiente de validación funcional                           |
| RF-13     | CP-12, CP-13, CP-14, CP-15        | Pendiente de validación funcional; incluye normalización    |
| RF-14     | CP-16                             | Pendiente de validación funcional                           |
| RF-15     | CP-17, NFR-04                     | Pendiente de validación funcional                           |
| RF-16     | CP-18, BG-02                      | Parcial: menu contextual pendiente |
| RF-17     | CP-19, CP-20                      | Pendiente de validación funcional                           |
| RF-18     | CP-05, CP-06, CP-09, CP-21, CP-22 | Pendiente de validación funcional                           |

| RNF    | Caso asociado | Estado    |
| ------ | ------------- | --------- |
| RNF-01 | NFR-01        | Pendiente de validación funcional  |
| RNF-02 | NFR-02, BG-04 | Pendiente de validación funcional  |
| RNF-03 | NFR-03, BG-03 | Pendiente |
| RNF-04 | NFR-04        | Pendiente de validación funcional  |
| RNF-05 | NFR-05        | Pendiente de validación funcional  |
| RNF-06 | NFR-06        | Pendiente de validación funcional  |
| RNF-07 | NFR-07, BG-05 | Pendiente de validación funcional  |
| RNF-08 | NFR-08        | Pendiente de validación funcional  |
| RNF-09 | NFR-09        | Pendiente de validación funcional  |
| RNF-10 | NFR-10, CP-20 | Pendiente de validación funcional  |
