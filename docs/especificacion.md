# Especificación de requisitos — CamScanner

## 1. Introducción

### 1.1 Propósito
Definir los requisitos funcionales, no funcionales y criterios de aceptación de CamScanner para orientar el desarrollo y las pruebas de Ingeniería de Software II.

### 1.2 Alcance acordado
Aplicación móvil Android desarrollada con .NET 10, .NET MAUI y C#. Permitirá capturar o importar fotografías de documentos, reconocer texto mediante OCR local sin depender de Internet para el reconocimiento, seleccionar y corregir la información detectada, organizar documentos por lotes, almacenar datos en SQLite y exportarlos a Excel.

### 1.3 Definiciones

| Término | Definición |
|---|---|
| OCR | Reconocimiento óptico de caracteres a partir de una imagen. |
| Lote | Agrupación lógica de documentos. |
| CRUD | Crear, consultar, actualizar y eliminar registros. |
| SQLite | Base de datos relacional local. |
| Exportación | Generación de un archivo Excel con información seleccionada. |

## 2. Perspectiva y restricciones
La aplicación se desarrolla con .NET MAUI y C#, con destino Android. SQLite se utilizará para persistencia local. El OCR debe ejecutarse en el dispositivo sin conexión a Internet.

En los archivos revisados se identifican modelos `Lote` y `Documento`, creación de tablas SQLite, consulta de lotes e inserción/actualización de lotes. Los demás requisitos pertenecen al alcance acordado y no se consideran implementados hasta contar con código y evidencia.

## 3. Requisitos funcionales

| ID | Requisito | Prioridad |
|---|---|---|
| RF-01 | Capturar una fotografía mediante la cámara del dispositivo. | Alta |
| RF-02 | Importar una o varias imágenes desde el dispositivo. | Alta |
| RF-03 | Reconocer texto mediante OCR local sin requerir Internet para el reconocimiento. | Alta |
| RF-04 | Mostrar el texto reconocido para revisión. | Alta |
| RF-05 | Permitir seleccionar la información reconocida que se desea conservar. | Alta |
| RF-06 | Permitir corregir o editar la información extraída. | Alta |
| RF-07 | Crear lotes de documentos. | Alta |
| RF-08 | Consultar lotes registrados. | Alta |
| RF-09 | Modificar información de un lote. | Alta |
| RF-10 | Eliminar lotes respetando la integridad de documentos asociados. | Media |
| RF-11 | Registrar documentos y asociarlos a un lote. | Alta |
| RF-12 | Consultar documentos almacenados. | Alta |
| RF-13 | Modificar información de documentos. | Alta |
| RF-14 | Eliminar documentos. | Media |
| RF-15 | Almacenar localmente en SQLite la información de lotes y documentos. | Alta |
| RF-16 | Exportar a Excel la información seleccionada de un documento individual. | Alta |
| RF-17 | Exportar a Excel la información de los documentos de un lote. | Alta |
| RF-18 | Informar errores durante captura, importación, OCR, almacenamiento o exportación. | Alta |

## 4. Requisitos no funcionales

| ID | Característica | Requisito |
|---|---|---|
| RNF-01 | Usabilidad | Interfaz comprensible para captura, revisión, organización y exportación. |
| RNF-02 | Rendimiento | OCR y exportación deben ejecutarse en tiempo razonable sin bloquear innecesariamente la interfaz. |
| RNF-03 | Funcionamiento sin conexión | El reconocimiento OCR debe poder ejecutarse sin Internet. |
| RNF-04 | Persistencia | Los datos confirmados deben permanecer disponibles al reabrir la aplicación. |
| RNF-05 | Confiabilidad | Manejar errores sin cierres inesperados ni pérdida injustificada de información. |
| RNF-06 | Compatibilidad | Funcionar en las versiones Android definidas como objetivo. |
| RNF-07 | Mantenibilidad | Separar, en lo posible, interfaz, lógica de negocio y acceso a datos. |
| RNF-08 | Privacidad | Procesar y almacenar localmente los datos según el alcance; no enviarlos a servicios externos sin autorización. |
| RNF-09 | Integridad | Conservar consistencia entre lotes y documentos durante operaciones CRUD. |
| RNF-10 | Exportación | Los Excel deben contener los datos seleccionados de forma legible y organizada. |

## 5. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-01 | Con permisos concedidos, se puede tomar una fotografía y agregarla al flujo. |
| CA-02 | Se pueden seleccionar imágenes del dispositivo e incorporarlas al flujo. |
| CA-03 | Una imagen compatible puede procesarse por OCR con el dispositivo sin Internet. |
| CA-04 | El texto reconocido se presenta para revisión. |
| CA-05 | Se pueden seleccionar los datos que se desean conservar. |
| CA-06 | Se puede editar un dato reconocido y conservar el valor corregido. |
| CA-07 | Se puede crear un lote y encontrarlo en la consulta. |
| CA-08 | La consulta de un lote muestra sus datos y documentos asociados. |
| CA-09 | Los cambios de un lote permanecen después de guardarlos. |
| CA-10 | Se puede eliminar un lote conforme a la regla de integridad definida. |
| CA-11 | Se puede registrar un documento y asociarlo a un lote. |
| CA-12 | Se puede consultar un documento almacenado. |
| CA-13 | Se puede modificar un documento y consultar los cambios. |
| CA-14 | Se puede eliminar un documento y comprobar que ya no aparece. |
| CA-15 | Los datos guardados permanecen después de cerrar y abrir la aplicación. |
| CA-16 | La exportación individual genera Excel con los datos seleccionados. |
| CA-17 | La exportación por lote genera Excel con los datos de sus documentos. |
| CA-18 | Ante errores se informa al usuario y no se pierden datos previamente confirmados. |

## 6. Decisiones por detallar
- Motor OCR y versión.
- Biblioteca Excel y formato de salida.
- Formatos y límites de imagen.
- Idiomas reconocidos.
- Campos que se extraen y reglas de selección.
- Validaciones de lotes/documentos.
- Comportamiento al eliminar lotes con documentos.
- Versión mínima Android y dispositivos objetivo.
- Tratamiento de imágenes sin texto o de baja calidad.

## 7. Estado de implementación
En la revisión de la rama `documentacion-richard` se comprobó la existencia de los modelos, inicialización y creación de tablas SQLite, consulta de lotes e inserción/actualización de lotes. Los demás requisitos siguen siendo parte del alcance, pero no se consideran implementados ni verificados por esa revisión.

## 8. Trazabilidad
Los criterios CA-01 a CA-18 corresponden respectivamente a RF-01 a RF-18. Registrar ejecución, fecha, entorno, resultado y evidencia en `plan-de-pruebas.md`.
