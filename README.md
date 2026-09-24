# ⛏️ CaMine — Minecraft Launcher

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows/)
[![Architecture](https://img.shields.io/badge/Architecture-MVP-orange)](#-arquitectura-del-software)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

> Un lanzador de escritorio ligero, moderno y directo al grano para **Minecraft**, desarrollado en **C# (.NET Framework)** con Windows Forms y soporte automatizado para **Minecraft Forge**.

---

## 📌 ¿Qué es CaMine?

**CaMine** es un lanzador personalizado de Minecraft diseñado para ofrecer una experiencia sencilla, rápida y sin configuraciones engorrosas. Permite explorar versiones oficiales, instalarlas en segundo plano, añadir mods con facilidad y lanzar el juego con un nombre de usuario personalizado y la memoria RAM adecuada para tu equipo.

El proyecto está pensado tanto para jugadores que buscan una alternativa ligera como para grupos de amigos que desean compartir un entorno de juego unificado y aislado.

---

## ✨ Características Principales

- **🎮 Detección y Catálogo de Versiones**:
  - Detección inmediata de las versiones instaladas localmente en el equipo.
  - Carga asíncrona del catálogo oficial de versiones de Minecraft disponibles en línea.
  - Indicadores visuales de color en el selector para diferenciar versiones descargadas vs. pendientes por descargar.

- **🛠️ Instalación de Forge en un Clic**:
  - Opción de activar **Minecraft Forge** mediante una simple casilla.
  - Si Forge no está instalado para la versión elegida, el lanzador lo descarga y configura automáticamente sin necesidad de instaladores externos.

- **📂 Carpeta de Mods Directa**:
  - Botón integrado para abrir directamente la carpeta de `mods`, facilitando la personalización del juego.

- **⚡ Control de Recursos (RAM)**:
  - Selector de memoria RAM asignada a Minecraft (mínimo 512 MB, recomendado 4096 MB / 4 GB) para garantizar fluidez durante la partida.

- **📊 Barra de Progreso y Estados en Vivo**:
  - Muestra el estado del proceso en tiempo real (*Descargando*, *Instalando Forge*, *Lanzando...*) junto con una barra de progreso de tareas.

- **📁 Directorio Aislado**:
  - Almacena los datos y archivos en `%AppData%\.minecraft-launcher-amigos`, manteniendo tu instalación limpia e independiente de otros lanzadores.

---

## 📦 Dependencias Utilizadas

El proyecto aprovecha varias librerías especializadas del ecosistema .NET para resolver la gestión de archivos, descargas concurrentes y el ciclo de vida del juego:

| Librería | Versión | Propósito en el Proyecto |
| :--- | :---: | :--- |
| **`CmlLib.Core`** | `4.0.6` | **Motor principal del lanzador.** Gestiona la resolución de versiones, descarga de assets, librerías nativas, validación de archivos del juego y la construcción del proceso de ejecución de Minecraft. |
| **`CmlLib.Core.Installer.Forge`** | `1.1.1` | **Automatización de Forge.** Se encarga de descargar, validar e instalar los perfiles de Minecraft Forge de manera desatendida. |
| **`CmlLib.Core.Commons`** | `4.0.0` | Clases y componentes comunes de soporte para el ecosistema de *CmlLib*. |
| **`SharpZipLib`** | `1.4.2` | Manejo de descompresión y extracción de archivos empaquetados (`.zip`, `.jar`) necesarios para las librerías del juego. |
| **`LZMA-SDK`** | `19.0.0` | Algoritmos de compresión y descompresión LZMA utilizados en la extracción de paquetes de instalación de Forge. |
| **`HtmlAgilityPack`** | `1.11.48` | Parser HTML para la resolución y lectura de páginas o enlaces web de recursos externos. |
| **`System.Text.Json`** | `8.0.5` | Procesamiento y lectura de alta velocidad de los manifiestos JSON de versiones de Minecraft y metadatos del juego. |
| **`System.Threading.Tasks.Dataflow`** | `8.0.1` | Control de flujos de datos asíncronos y concurrencia para la descarga eficiente de archivos en segundo plano. |
| **`System.Buffers` & `System.Memory`** | — | Optimización de memoria y rendimiento en operaciones intensivas de I/O y lectura de archivos. |

---

## 🏛️ Arquitectura del Software

Para mantener el código ordenado, mantenible y desacoplado, **CaMine** implementa el patrón de diseño **MVP (Model-View-Presenter)**:

```
CaMine
 ├── Views/         -> Interfaz de usuario (Form1 e interfaz IMinecraftLauncherView)
 ├── Presenters/    -> Lógica de presentación y coordinación de eventos (MainPresenter)
 ├── Services/      -> Servicios del motor de juego y verificación (MinecraftService, VersionChecker)
 └── Models/        -> Modelos de estado de versión y controles (VersionStatus, ControlBuffer)
```

- **View (`IMinecraftLauncherView`)**: Define las acciones que la interfaz gráfica expone, permitiendo que la UI solo se encargue de mostrar información y capturar eventos del usuario.
- **Presenter (`MainPresenter`)**: Conecta la vista con los servicios; decide cuándo verificar versiones, cuándo iniciar descargas y cuándo invocar la ejecución del juego.
- **Services (`MinecraftService`, `VersionChecker`)**: Encapsulan la interacción con *CmlLib*, la verificación de archivos en disco (`.jar`, `.json`), el progreso de descargas y la construcción del proceso de Java.

---

## 🖥️ Requisitos del Sistema

- **Sistema Operativo**: Windows 10 o Windows 11 (64 bits).
- **Entorno .NET**: .NET Framework 4.7.2 o superior instalado.
- **Java**: Java Runtime Environment (JRE) o Java Development Kit (JDK) instalado y configurado en el sistema (versión correspondiente a la versión de Minecraft a ejecutar, típicamente Java 8 para versiones clásicas o Java 17/21 para versiones modernas).

---

## 🚀 Cómo Ejecutar o Compilar el Proyecto

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/carlos456dddd/CaMine.git
   ```

2. **Abrir la solución**:
   - Abre el archivo `CaMine.slnx` o el proyecto `CaMine.csproj` en **Visual Studio 2022** (con la carga de trabajo de *Desarrollo de escritorio de .NET* instalada).

3. **Restaurar paquetes NuGet**:
   - Visual Studio restaurará automáticamente las dependencias definidas en `packages.config` al compilar, o puedes ejecutar:
   ```bash
   nuget restore
   ```

4. **Compilar y Ejecutar**:
   - Selecciona la configuración `Debug` o `Release` (x64 o AnyCPU) y presiona `F5` o haz clic en **Iniciar**.

---
<img width="1652" height="812" alt="image" src="https://github.com/user-attachments/assets/87734abd-49fc-4588-b85d-d857d7b73856" />

## 📄 Licencia

Este proyecto está bajo la Licencia **MIT**. Consulta el archivo [LICENSE](LICENSE) para más detalles.
