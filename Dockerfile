FROM mcr.microsoft.com/dotnet/sdk:8.0 AS core
WORKDIR /src
COPY DacpacDiff.sln **/*.csproj ./
# re-creates the folder structure for all csproj files
RUN for from in ./*.csproj; do to=$(echo "$from" | sed 's/\/\([^/]*\)\.csproj$/\/\1&/') \
  && mkdir -p "$(dirname "$to")" && mv "$from" "$to"; done

# packages are restored to /packages to ensure packages are only pulled when .sln or .csproj files are changed
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore --packages /packages
COPY . .

FROM core AS build
ENV NUGET_PACKAGES=/packages
RUN dotnet build --no-restore

FROM build AS publish
RUN dotnet publish -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "dotnet", "dacpac-diff.dll" ]

