# Plan de pruebas — CamScanner

**Proyecto:** CamScanner · Android · .NET 10 / .NET MAUI  
**Etapa:** Seguimiento 1, 2 de noviembre de 2026  
**Responsable de pruebas:** Javier Serna  
**Documentación y calidad:** Richard Prado  
**Especificación vigente:** [docs/especificacion.md](especificacion.md)

## 1. Objetivo y alcance

Verificar incrementalmente la digitalización de documentos: captura/importación de imágenes, OCR local, selección y corrección del texto, gestión de lotes y documentos con SQLite y exportación individual o por lote a Excel.

Este documento distingue **casos diseñados**, **pruebas automatizadas aportadas** y **ejecuciones reportadas**. Una función planeada no se considera implementada ni verificada por el hecho de estar documentada. El alcance del proyecto **no incluye exportación ZIP/PDF**.

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
### Casos de prueba para requisitos no funcionales

| ID | Requisito | Descripción de la prueba | Resultado esperado | Estado |
|---|---|---|---|---|
| CP-27 | RNF-01 - Usabilidad | Revisar que las opciones principales sean comprensibles y permitan completar las operaciones sin instrucciones adicionales. | La navegación y las acciones principales son claras para el usuario. | Pendiente |
| CP-28 | RNF-05 - Confiabilidad | Interrumpir una operación controlada y volver a utilizar la aplicación. | La aplicación responde adecuadamente y no presenta pérdida inesperada de información previamente guardada. | Pendiente |
| CP-29 | RNF-06 - Compatibilidad Android | Instalar y ejecutar la aplicación en un dispositivo o emulador Android compatible. | La aplicación inicia y permite utilizar las funciones disponibles sin errores de compatibilidad. | Pendiente |
| CP-30 | RNF-07 - Mantenibilidad | Revisar la organización del código, separación de responsabilidades y facilidad para identificar los componentes principales. | El código presenta una estructura comprensible y permite localizar los componentes para futuras modificaciones. | Pendiente |
| CP-31 | RNF-08 - Privacidad | Revisar el almacenamiento y tratamiento de imágenes y documentos, así como los permisos y posibles transmisiones de información. | Los datos se manejan conforme a las condiciones de privacidad definidas para la aplicación. | Pendiente |
| CP-32 | RNF-09 - Integridad | Verificar la relación entre lotes y documentos al registrar, modificar o eliminar información. | No se generan asociaciones inconsistentes ni pérdida accidental de documentos. | Pendiente |

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
| RNF-01 - Usabilidad | CP-27 | Pendiente |
| RNF-02 - Desempeño | CP-25 | Pendiente |
| RNF-03 - Funcionamiento sin conexión | CP-23 | Pendiente |
| RNF-04 - Persistencia | CP-24 | Pendiente |
| RNF-05 - Confiabilidad | CP-28 | Pendiente |
| RNF-06 - Compatibilidad Android | CP-29 | Pendiente |
| RNF-07 - Mantenibilidad | CP-30 | Pendiente |
| RNF-08 - Privacidad | CP-31 | Pendiente |
| RNF-09 - Integridad | CP-32 | Pendiente |
| RNF-10 - Legibilidad de exportación | CP-26 | Pendiente |

### 9.2. Resultado de la revisión de trazabilidad

- Requisitos funcionales relacionados: 18 de 18.
- Criterios de aceptación relacionados: 18 de 18.
- Casos de prueba funcionales asociados: 22.
- Requisitos no funcionales con casos asociados: 10 de 10.
- Requisitos no funcionales pendientes de asociación: 0.

La trazabilidad documental de los requisitos funcionales está establecida. Sin embargo, la verificación de cumplimiento permanece pendiente hasta ejecutar los casos de prueba y registrar sus evidencias.
