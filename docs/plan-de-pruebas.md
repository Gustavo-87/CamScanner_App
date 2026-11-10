# Plan de pruebas — CamScanner

## 1. Objetivo
Definir casos para verificar captura/importación, OCR local, selección y corrección, gestión de lotes/documentos, SQLite y exportación Excel. Este documento diseña pruebas; no informa resultados ejecutados.

## 2. Estado inicial
Todos los casos están **Pendientes de ejecución**. La revisión estática identificó configuración MAUI, modelos `Lote` y `Documento`, creación de tablas SQLite, consulta de lotes e inserción/actualización de lotes. La pantalla principal sigue siendo la plantilla MAUI y la búsqueda realizada no encontró referencias a OCR, cámara, selección de archivos o Excel.

## 3. Técnicas
- Partición de equivalencia: imágenes compatibles/no compatibles y datos válidos/no válidos.
- Valores límite: tamaños, cantidades y longitudes de campos.
- Pruebas de flujo: captura/importación, OCR, revisión, guardado y exportación.
- Persistencia: cerrar y reabrir la aplicación.
- Pruebas negativas: permisos denegados, imágenes sin texto y errores de almacenamiento/exportación.

## 4. Casos de prueba

| ID | Requisito | Escenario | Resultado esperado | Estado |
|---|---|---|---|---|
| CP-01 | RF-01 | Capturar fotografía con permiso concedido. | Imagen capturada y disponible. | Pendiente |
| CP-02 | RF-01 | Denegar permiso de cámara. | Mensaje claro, sin cierre inesperado. | Pendiente |
| CP-03 | RF-02 | Importar imagen compatible. | Imagen incorporada al flujo. | Pendiente |
| CP-04 | RF-02 | Cancelar selector de imágenes. | Se conserva el estado actual. | Pendiente |
| CP-05 | RF-03 | Procesar imagen legible sin Internet. | OCR local reconoce texto sin conexión. | Pendiente |
| CP-06 | RF-03 | Procesar imagen borrosa o sin texto. | Se informa resultado parcial o ausencia de texto. | Pendiente |
| CP-07 | RF-04 | Revisar resultado OCR. | Texto reconocido visible. | Pendiente |
| CP-08 | RF-05 | Seleccionar algunos datos. | Se conservan solo los datos elegidos. | Pendiente |
| CP-09 | RF-06 | Corregir un dato reconocido. | Valor corregido se conserva. | Pendiente |
| CP-10 | RF-07 | Crear lote válido. | Lote registrado. | Pendiente |
| CP-11 | RF-08 | Consultar lotes. | Se muestran lotes existentes. | Pendiente |
| CP-12 | RF-09 | Modificar lote. | Cambios guardados. | Pendiente |
| CP-13 | RF-10 | Eliminar lote sin documentos. | Lote deja de aparecer. | Pendiente |
| CP-14 | RF-10 | Eliminar lote con documentos asociados. | Se aplica regla de integridad definida. | Pendiente |
| CP-15 | RF-11 | Crear documento y asociarlo a lote. | Documento registrado y relacionado. | Pendiente |
| CP-16 | RF-12 | Consultar documentos de un lote. | Se muestran los documentos correspondientes. | Pendiente |
| CP-17 | RF-13 | Modificar documento. | Cambios guardados. | Pendiente |
| CP-18 | RF-14 | Eliminar documento. | Documento deja de aparecer. | Pendiente |
| CP-19 | RF-15 | Guardar y reiniciar aplicación. | Registros confirmados persisten en SQLite. | Pendiente |
| CP-20 | RF-16 | Exportar documento individual. | Excel con datos seleccionados. | Pendiente |
| CP-21 | RF-17 | Exportar lote. | Excel con datos de documentos del lote. | Pendiente |
| CP-22 | RF-18 | Provocar error controlado. | Mensaje claro y datos confirmados conservados. | Pendiente |
| CP-23 | RNF-03 | Ejecutar OCR sin conexión. | OCR funciona sin Internet. | Pendiente |
| CP-24 | RNF-04 | Reiniciar después de guardar. | Datos disponibles nuevamente. | Pendiente |
| CP-25 | RNF-02 | Procesar conjunto representativo de imágenes. | Duración registrada y sin bloqueos inaceptables. | Pendiente |
| CP-26 | RNF-10 | Abrir archivo Excel generado. | Datos legibles y correspondientes a la selección. | Pendiente |

## 5. Registro de ejecución

| ID | Fecha | Dispositivo/emulador | Resultado | Evidencia/incidencia |
|---|---|---|---|---|
| CP-__ | __/__/____ | __ | Aprobado/Fallido | __ |

## 6. Criterios de entrada
- Aplicación compilada e instalada en dispositivo/emulador Android compatible.
- Datos de prueba preparados.
- Permisos y recursos disponibles.
- Imágenes de referencia con texto conocido para OCR.
- Documentos de prueba para exportación.

## 7. Criterios de salida
- Casos ejecutados y resultados registrados.
- Evidencias asociadas a los casos.
- Defectos clasificados.
- Fallos críticos corregidos o reportados.

## 8. Evidencias recomendadas
- Captura o importación de imagen.
- OCR ejecutado sin conexión.
- Selección y corrección de datos.
- CRUD de lotes y documentos.
- Persistencia después de reiniciar.
- Archivo Excel generado y contenido revisado.
- Manejo de errores y permisos denegados.

No marcar un caso como aprobado sin evidencia de ejecución.


## 9. Matriz de trazabilidad de requisitos

La siguiente matriz relaciona los requisitos funcionales definidos en `docs/especificacion.md` con sus criterios de aceptación y los casos de prueba correspondientes.

Esta relación representa la trazabilidad documental. La ejecución y aprobación de los casos se registra independientemente en el apartado 5.

| Requisito | Criterio de aceptación | Caso(s) de prueba | Estado de verificación |
|---|---|---|---|
| RF-01 | CA-01 | CP-01, CP-02 | Pendiente |
| RF-02 | CA-02 | CP-03, CP-04 | Pendiente |
| RF-03 | CA-03 | CP-05, CP-06 | Pendiente |
| RF-04 | CA-04 | CP-07 | Pendiente |
| RF-05 | CA-05 | CP-08 | Pendiente |
| RF-06 | CA-06 | CP-09 | Pendiente |
| RF-07 | CA-07 | CP-10 | Pendiente |
| RF-08 | CA-08 | CP-11 | Pendiente |
| RF-09 | CA-09 | CP-12 | Pendiente |
| RF-10 | CA-10 | CP-13, CP-14 | Pendiente |
| RF-11 | CA-11 | CP-15 | Pendiente |
| RF-12 | CA-12 | CP-16 | Pendiente |
| RF-13 | CA-13 | CP-17 | Pendiente |
| RF-14 | CA-14 | CP-18 | Pendiente |
| RF-15 | CA-15 | CP-19 | Pendiente |
| RF-16 | CA-16 | CP-20 | Pendiente |
| RF-17 | CA-17 | CP-21 | Pendiente |
| RF-18 | CA-18 | CP-22 | Pendiente |

### 9.1. Trazabilidad de requisitos no funcionales

| Requisito | Caso de prueba asociado | Estado |
|---|---|---|
| RNF-02 - Desempeño | CP-25 | Pendiente |
| RNF-03 - Funcionamiento sin conexión | CP-23 | Pendiente |
| RNF-04 - Persistencia | CP-24 | Pendiente |
| RNF-10 - Legibilidad de exportación | CP-26 | Pendiente |

Los requisitos no funcionales RNF-01, RNF-05, RNF-06, RNF-07, RNF-08 y RNF-09 requieren casos de prueba específicos para verificar su cumplimiento.

### 9.2. Resultado de la revisión de trazabilidad

- Requisitos funcionales relacionados: 18 de 18.
- Criterios de aceptación relacionados: 18 de 18.
- Casos de prueba funcionales asociados: 22 de 22.
- Requisitos no funcionales con casos asociados: 4 de 10.
- Requisitos no funcionales pendientes de asociación: 6 de 10.

La trazabilidad documental de los requisitos funcionales está establecida. Sin embargo, la verificación de cumplimiento permanece pendiente hasta ejecutar los casos de prueba y registrar sus evidencias.
