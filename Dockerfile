# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# Sin USER $APP_UID: Railway monta los Volumes con dueño root, y el usuario
# sin privilegios de la imagen base no puede crear carpetas ahí dentro
# (causaba UnauthorizedAccessException en /data/uploads). Corre como root
# dentro del contenedor para poder escribir en el Volume montado.
# libfontconfig1 + fonts-dejavu-core: QuestPDF (SkiaSharp) necesita fontconfig
# y al menos una fuente instalada para poder renderizar los PDFs de reportes —
# la imagen base no trae ninguna de las dos, y sin esto la generación de PDF
# falla en Railway aunque funcione en local (Windows sí tiene fuentes).
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
# Railway asigna el puerto real via la variable de entorno PORT (ver Program.cs);
# este EXPOSE es solo documentación para Docker.
EXPOSE 8080


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ForraControl.API.csproj", "."]
RUN dotnet restore "./ForraControl.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ForraControl.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ForraControl.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForraControl.API.dll"]