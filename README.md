README – WsSofterpRD
API REST ASP.NET Core · SQL Server · Entity Framework Core · JWT
1. Descripción
WsSofterpRD es una API desarrollada con ASP.NET Core para exponer servicios relacionados con la aplicación SofterpRD. El proyecto utiliza SQL Server como motor de base de datos, Entity Framework Core para el acceso a datos y autenticación/autorización mediante tokens JWT.
2. Tecnologías utilizadas
•	ASP.NET Core Web API
•	C#
•	Microsoft.Data.SqlClient
•	Entity Framework Core
•	SQL Server
•	JWT Bearer Authentication
•	Swagger / OpenAPI
•	CORS
•	Kestrel
•	Memory Cache
3. Configuración de la aplicación
La aplicación obtiene la clave JWT desde la configuración mediante la entrada "Jwt:Key". Si dicha clave no está configurada, la aplicación genera una excepción.
También utiliza la cadena de conexión "WsSofterpRDContext" para conectarse a SQL Server. La configuración de Kestrel se obtiene desde la sección "Kestrel" de la configuración.
4. Seguridad y autenticación
La API utiliza autenticación JWT Bearer. La validación contempla issuer, audience, vigencia del token y firma mediante una clave simétrica. Además, ClockSkew está configurado en cero, por lo que no se agrega tolerancia adicional al tiempo de expiración.
Cuando el token ha expirado, la aplicación agrega el encabezado HTTP "Token-Expired: true" durante el evento de autenticación fallida. Los endpoints protegidos responden con HTTP 401 cuando el usuario no está autorizado.
5. Middleware y pipeline HTTP
•	Redirección HTTPS.
•	Política CORS denominada AllowAngular.
•	Autenticación JWT.
•	Autorización.
•	Mapeo de controladores.
•	Swagger y Swagger UI.
El orden de UseAuthentication() antes de UseAuthorization() es importante para que la identidad del usuario sea establecida antes de evaluar las políticas de autorización.
6. Swagger / OpenAPI
Swagger está habilitado con un documento v1 denominado "Clientes API". La definición de seguridad permite introducir un token JWT utilizando el esquema Bearer.
Formato esperado en Swagger:
Bearer {tu_token}
7. Base de datos
El proyecto registra un DbContext denominado WsSofterpRDContext y utiliza SQL Server mediante UseSqlServer(). Adicionalmente, el endpoint CientesWS/CientesWS abre una conexión SqlConnection utilizando la misma cadena de conexión.
8. Endpoint CientesWS
Elemento	Valor
Método	GET
Ruta	/api/CientesWS/CientesWS
Autenticación	JWT Bearer
Nombre Swagger	GetDocumentosEstadoCuenta
Tag Swagger	Documentos
Parámetros recibidos por query string:
•	Idcodigo – int
•	Nombres1 – string
•	Nombres2 – string
•	Apellido1 – string
•	Apellido2 – string
•	Fechacreacion – DateTime
•	iduser – int
El endpoint abre una conexión SQL, ejecuta el objeto dbo.CientesWS y carga el resultado en un DataTable. Posteriormente transforma las filas obtenidas a objetos y devuelve la respuesta mediante Results.Ok().
9. Modelo ClientesWS
El modelo ClientesWS utiliza Idcodigo como clave primaria mediante el atributo [PrimaryKey(nameof(Idcodigo))].
Propiedad	Tipo	Descripción
Idcodigo	string	Clave primaria
Nombre1	string	Primer nombre
Nombre2	string	Segundo nombre
Apellido1	string?	Primer apellido
Apellido2	string?	Segundo apellido
Fechacreacion	DateTime	Fecha de creación
iduser	string	Identificador del usuario
10. Flujo general
Cliente → HTTPS → CORS → Autenticación JWT → Autorización → Endpoint → SQL Server → Transformación de resultados → HTTP 200 OK.
11. Recomendaciones de configuración
Las claves JWT y cadenas de conexión deben mantenerse en mecanismos seguros de configuración y no deben incluirse directamente en el código fuente. Los valores concretos de Jwt:Key, Jwt:Issuer, Jwt:Audience, Kestrel y WsSofterpRDContext no aparecen en los archivos proporcionados, por lo que deben configurarse según el ambiente correspondiente.
12. Archivos documentados
•	Program.cs – configuración principal de la API, autenticación, Swagger, CORS, SQL Server y endpoint.
•	ClientesWS.cs – modelo de datos ClientesWS y definición de su clave primaria.
13. Nota
Este README fue generado exclusivamente a partir del contenido proporcionado en Program.cs y ClientesWS.cs. No se documentan funcionalidades que no estén evidenciadas en esos archivos.
