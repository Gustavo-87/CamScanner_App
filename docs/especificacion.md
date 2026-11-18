# Especificación de requisitos --- CamScanner

**Proyecto:** CamScanner · Ingeniería de Software II (2026-2)\
**Plataforma principal:** Android · .NET 10 · .NET MAUI · C#

## 1. Introducción

### 1.1 Propósito

Definir los requisitos funcionales, no funcionales y criterios de
aceptación de CamScanner para orientar el desarrollo, las pruebas y la
evaluación del producto.

### 1.2 Alcance acordado

CamScanner es una aplicación móvil orientada a digitalizar y organizar
documentos. Permite capturar fotografías o importar imágenes,
almacenarlas localmente, agrupar documentos en lotes y utilizar
reconocimiento óptico de caracteres (OCR) para obtener texto que puede
revisarse y editarse.

La exportación de información a Excel forma parte del alcance previsto,
pero permanece pendiente de implementación en la versión revisada.

### 1.3 Definiciones

  -----------------------------------------------------------------------
  Término                             Definición
  ----------------------------------- -----------------------------------
  OCR                                 Reconocimiento óptico de caracteres
                                      a partir de una imagen.

  Lote                                Agrupación lógica de documentos.

  CRUD                                Crear, consultar, actualizar y
                                      eliminar registros.

  SQLite                              Base de datos relacional local.

  Exportación                         Generación de un archivo Excel con
                                      información seleccionada.
  -----------------------------------------------------------------------

## 2. Perspectiva y restricciones

La aplicación se desarrolla con .NET MAUI y C#. El proyecto declara
destinos Android, iOS, MacCatalyst y Windows; Android es la plataforma
principal. SQLite se utiliza para persistencia local y `Plugin.Maui.OCR`
está integrado para reconocimiento de texto.

La disponibilidad de una dependencia o la existencia de código no
demuestra por sí sola que una función haya superado pruebas funcionales
en un dispositivo. Por ello, este documento distingue entre
implementación identificada y validación pendiente.

## 3. Requisitos funcionales

  -----------------------------------------------------------------------
  ID                      Requisito               Prioridad
  ----------------------- ----------------------- -----------------------
  RF-01                   Capturar una fotografía Alta
                          mediante la cámara del
                          dispositivo.

  RF-02                   Importar una o varias   Alta
                          imágenes desde el
                          dispositivo.

  RF-03                   Reconocer texto         Alta
                          mediante OCR local.

  RF-04                   Mostrar el texto        Alta
                          reconocido para
                          revisión.

  RF-05                   Permitir seleccionar la Alta
                          información reconocida
                          que se desea conservar.

  RF-06                   Permitir corregir o     Alta
                          editar la información
                          extraída.

  RF-07                   Crear lotes de          Alta
                          documentos.

  RF-08                   Consultar lotes         Alta
                          registrados.

  RF-09                   Modificar información   Alta
                          de un lote.

  RF-10                   Eliminar lotes          Media
                          respetando la
                          integridad de
                          documentos asociados.

  RF-11                   Registrar documentos y  Alta
                          asociarlos a un lote.

  RF-12                   Consultar documentos    Alta
                          almacenados.

  RF-13                   Modificar información   Alta
                          de documentos.

  RF-14                   Eliminar documentos.    Media

  RF-15                   Almacenar localmente en Alta
                          SQLite la información
                          de lotes y documentos.

  RF-16                   Exportar a Excel la     Alta
                          información
                          seleccionada de un
                          documento individual.

  RF-17                   Exportar a Excel la     Alta
                          información de los
                          documentos de un lote.

  RF-18                   Informar errores        Alta
                          durante captura,
                          importación, OCR,
                          almacenamiento o
                          exportación.
  -----------------------------------------------------------------------

## 4. Requisitos no funcionales

  -----------------------------------------------------------------------
  ID                      Característica          Requisito
  ----------------------- ----------------------- -----------------------
  RNF-01                  Usabilidad              Interfaz comprensible
                                                  para captura, revisión,
                                                  organización y
                                                  exportación.

  RNF-02                  Rendimiento             OCR y exportación deben
                                                  ejecutarse en tiempo
                                                  razonable sin bloquear
                                                  innecesariamente la
                                                  interfaz.

  RNF-03                  Funcionamiento sin      El reconocimiento OCR
                          conexión                debe poder ejecutarse
                                                  sin Internet.

  RNF-04                  Persistencia            Los datos confirmados
                                                  deben permanecer
                                                  disponibles al reabrir
                                                  la aplicación.

  RNF-05                  Confiabilidad           Manejar errores sin
                                                  cierres inesperados ni
                                                  pérdida injustificada
                                                  de información.

  RNF-06                  Compatibilidad          Funcionar en las
                                                  versiones Android
                                                  definidas como
                                                  objetivo.

  RNF-07                  Mantenibilidad          Separar, en lo posible,
                                                  interfaz, lógica de
                                                  negocio y acceso a
                                                  datos.

  RNF-08                  Privacidad              Procesar y almacenar
                                                  localmente los datos
                                                  según el alcance; no
                                                  enviarlos a servicios
                                                  externos sin
                                                  autorización.

  RNF-09                  Integridad              Conservar consistencia
                                                  entre lotes y
                                                  documentos durante
                                                  operaciones CRUD.

  RNF-10                  Exportación             Los Excel deben
                                                  contener los datos
                                                  seleccionados de forma
                                                  legible y organizada.
  -----------------------------------------------------------------------

## 5. Criterios de aceptación

  -----------------------------------------------------------------------
  ID                                  Criterio
  ----------------------------------- -----------------------------------
  CA-01                               Con permisos concedidos, se puede
                                      tomar una fotografía y agregarla al
                                      flujo.

  CA-02                               Se pueden seleccionar imágenes del
                                      dispositivo e incorporarlas al
                                      flujo.

  CA-03                               Una imagen compatible puede
                                      procesarse por OCR con el
                                      dispositivo sin Internet.

  CA-04                               El texto reconocido se presenta
                                      para revisión.

  CA-05                               Se pueden seleccionar los datos que
                                      se desean conservar.

  CA-06                               Se puede editar un dato reconocido
                                      y conservar el valor corregido.

  CA-07                               Se puede crear un lote y
                                      encontrarlo en la consulta.

  CA-08                               La consulta de un lote muestra sus
                                      datos y documentos asociados.

  CA-09                               Los cambios de un lote permanecen
                                      después de guardarlos.

  CA-10                               Se puede eliminar un lote conforme
                                      a la regla de integridad definida.

  CA-11                               Se puede registrar un documento y
                                      asociarlo a un lote.

  CA-12                               Se puede consultar un documento
                                      almacenado.

  CA-13                               Se puede modificar un documento y
                                      consultar los cambios.

  CA-14                               Se puede eliminar un documento y
                                      comprobar que ya no aparece.

  CA-15                               Los datos guardados permanecen
                                      después de cerrar y abrir la
                                      aplicación.

  CA-16                               La exportación individual genera
                                      Excel con los datos seleccionados.

  CA-17                               La exportación por lote genera
                                      Excel con los datos de sus
                                      documentos.

  CA-18                               Ante errores se informa al usuario
                                      y no se pierden datos previamente
                                      confirmados.
  -----------------------------------------------------------------------

## 6. Decisiones y restricciones pendientes

-   Verificar el funcionamiento real del OCR en dispositivos Android y
    confirmar su comportamiento sin conexión.
-   Definir formatos y límites de imagen admitidos.
-   Definir idiomas reconocidos y campos que se extraen.
-   Precisar si la selección de información OCR será por texto completo
    o por campos estructurados.
-   Seleccionar e integrar una biblioteca Excel.
-   Definir la estructura de los archivos Excel.
-   Definir la versión mínima Android y los dispositivos objetivo.
-   Evaluar imágenes sin texto, borrosas o de baja calidad.
-   Confirmar los permisos y el tratamiento de datos en el entorno
    Android.

## 7. Estado de implementación revisado

La siguiente clasificación se basa en la revisión del código fuente
compartido el 17 de noviembre de 2026. No equivale a una aprobación de
pruebas funcionales.

  -----------------------------------------------------------------------
  Requisito               Estado de               Validación
                          implementación
                          observado
  ----------------------- ----------------------- -----------------------
  RF-01                   Captura mediante        Pendiente en
                          `MediaPicker` y         dispositivo
                          solicitud de permiso de
                          cámara presentes.

  RF-02                   Selección múltiple      Pendiente en
                          desde galería, con      dispositivo
                          límite configurado de
                          10 imágenes.

  RF-03                   Integración de          Pendiente de comprobar
                          `Plugin.Maui.OCR` en    OCR y modo sin conexión
                          pantalla de detalle.

  RF-04                   Resultado OCR guardado  Pendiente en
                          y disponible para abrir dispositivo
                          editor.

  RF-05                   Selección de campos     Pendiente / alcance por
                          estructurados no        precisar
                          evidenciada; el editor
                          trabaja con texto
                          completo.

  RF-06                   Edición y guardado del  Pendiente en
                          texto reconocido        dispositivo
                          presentes.

  RF-07                   Creación de lotes       Pendiente en
                          presente.               dispositivo

  RF-08                   Consulta de lotes       Pendiente en
                          presente.               dispositivo

  RF-09                   Edición de lotes        Pendiente en
                          presente.               dispositivo

  RF-10                   Eliminación de lote     Pendiente en
                          bloqueada cuando        dispositivo
                          existen documentos
                          asociados.

  RF-11                   Registro de documentos  Pendiente en
                          asociados a un lote     dispositivo
                          presente.

  RF-12                   Consulta de documentos  Pendiente en
                          presente.               dispositivo

  RF-13                   Actualización de nombre Pendiente en
                          y texto de documento    dispositivo
                          presente.

  RF-14                   Eliminación de          Pendiente en
                          documento y archivo de  dispositivo
                          imagen presente.

  RF-15                   Inicialización de       Pendiente de prueba de
                          tablas y operaciones de reinicio
                          persistencia SQLite
                          presentes.

  RF-16                   Exportación individual  Pendiente
                          a Excel no
                          implementada; la
                          interfaz muestra aviso
                          de próxima
                          disponibilidad.

  RF-17                   Exportación por lote no Pendiente
                          implementada.

  RF-18                   Existen mensajes de     Parcial / pendiente
                          error en varios flujos;
                          cobertura integral no
                          demostrada.
  -----------------------------------------------------------------------

## 8. Trazabilidad

Los criterios CA-01 a CA-18 corresponden respectivamente a RF-01 a
RF-18. Los casos CP-01 a CP-32 se encuentran relacionados en
`plan-de-pruebas.md`.

La trazabilidad documental no implica que el requisito haya sido
probado. Cada ejecución debe registrar fecha, entorno, resultado y
evidencia. Los resultados de pruebas unitarias se documentan por
separado de las pruebas funcionales y de aceptación.
