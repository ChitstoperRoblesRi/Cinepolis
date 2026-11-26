# Cinépolis Clone

Aplicación local con React 19, ASP.NET Core 8, PostgreSQL y Kong en modo declarativo. El navegador carga React en `http://localhost:5173`. Las solicitudes HTTP y SignalR pasan por `http://localhost:8000`. Dentro del contenedor, Kong alcanza Kestrel mediante `http://host.docker.internal:5000`.

## Arranque en PowerShell

Abre tres terminales en la raíz del repositorio. Necesitas el SDK de .NET 8, Node.js 20.19+ de la rama 20 o 22.12+, npm, Docker Desktop con contenedores Linux y acceso a la base existente descrita en `AGENTS.md`.

### 1. API y conexión a PostgreSQL

El perfil `Development` lee `backend/Cinepolis.API/appsettings.Development.json`, escucha en `0.0.0.0:5000` y permite el origen `http://localhost:5173`. Define la cadena Npgsql de tu instalación en esta terminal. El usuario de la aplicación necesita acceso al esquema y permisos para las operaciones de venta, reservas y Club.

```powershell
$conexion = Read-Host 'Cadena Npgsql completa (Host, Port, Database, Username y Password)' -AsSecureString
$env:ConnectionStrings__Postgres = [System.Net.NetworkCredential]::new('', $conexion).Password
```

Para registro e inicio de sesión configura también un servidor SMTP. Introduce un remitente autorizado por ese servidor. Este ejemplo usa STARTTLS en el puerto 587. Si usas un capturador local, ajusta el puerto y TLS según su configuración y omite las credenciales si acepta correo sin autenticación.

```powershell
$env:Smtp__Host = Read-Host 'Host SMTP'
$env:Smtp__De = Read-Host 'Correo del remitente'
$env:Smtp__Puerto = '587'
$env:Smtp__Tls = 'true'
$env:Smtp__Usuario = Read-Host 'Usuario SMTP'
$claveSmtp = Read-Host 'Contraseña SMTP' -AsSecureString
$env:Smtp__Clave = [System.Net.NetworkCredential]::new('', $claveSmtp).Password
dotnet run --project backend/Cinepolis.API --launch-profile Development
```

En desarrollo, la API genera una clave JWT al iniciar. Si necesitas conservar sesiones después de reiniciar, establece `Jwt__ClaveBase64` con una clave estable de al menos 32 bytes codificada en Base64.

### 2. Kong Gateway

Con Docker Desktop en ejecución, usa la segunda terminal.

```powershell
docker compose config --quiet
docker compose up -d kong
docker compose ps
```

Kong lee `kong.yml` y conserva las rutas `/api/v1` y `/hubs/sala` al reenviarlas al backend. La ruta del hub admite la negociación HTTP y la actualización a WebSocket.

### 3. Frontend

En la tercera terminal, instala las dependencias fijadas en el archivo de bloqueo e inicia Vite.

```powershell
Set-Location frontend
npm.cmd ci
npm.cmd run dev -- --host localhost
```

Abre [Club Cinépolis](http://localhost:5173/club). En `/auth` puedes solicitar el código de seis dígitos para registrarte o iniciar sesión. El panel muestra las visitas del semestre y el siguiente nivel configurado en PostgreSQL. Cada compra abre su comprobante QR.

## Verificación

Con los tres procesos activos, ejecuta estas comprobaciones desde la raíz. Ambos health checks deben devolver `Healthy`. Un `503` indica que la comprobación de PostgreSQL falló. El perfil sin JWT debe responder `401`. El preflight debe devolver el origen de Vite y permitir credenciales.

```powershell
curl.exe -i http://localhost:5000/api/v1/health
curl.exe -i http://localhost:8000/api/v1/health
curl.exe -i http://localhost:8000/api/v1/club/perfil
curl.exe -i -X OPTIONS http://localhost:8000/api/v1/club/compras -H "Origin: http://localhost:5173" -H "Access-Control-Request-Method: GET" -H "Access-Control-Request-Headers: authorization"
```

Para comprobar SignalR sin reservar asientos, ejecuta este cliente desde la raíz.

```powershell
Push-Location frontend
node -e "const s=require('@microsoft/signalr'); (async()=>{const h=new s.HubConnectionBuilder().withUrl('http://localhost:8000/hubs/sala',{transport:s.HttpTransportType.WebSockets}).build(); try {await h.start(); await h.invoke('UnirseAFuncion',1); console.log('Grupo conectado')} finally {await h.stop()}})().catch(e=>{console.error(e.message);process.exitCode=1})"
Pop-Location
```

Para compilar la solución y el paquete de producción de React desde la raíz:

```powershell
dotnet build backend/CinepolisClone.sln -warnaserror
Push-Location frontend
npm.cmd run build
Pop-Location
```

Detén Vite y la API con Ctrl+C en sus respectivas terminales. Para detener Kong ejecuta `docker compose stop kong` desde la raíz.
