# Dotnet api plus HTMX demo
Demo app for hypermedia driven web app with the [Fluid templating engine](https://github.com/sebastienros/fluid) and 
[HTMX](https://htmx.org/).  
This app uses a custom Fluid renderer to support [Template Fragments](https://htmx.org/essays/template-fragments/).

https://dotnet-api-plus-htmx.fly.dev/

## Setup
```sh
dotnet tool install --global dotnet-ef
dotnet ef database update
cd dotnet-api-plus-htmx/static
wget https://unpkg.com/htmx.org@2.0.3/dist/htmx.js -O htmx-2.0.3.js
wget https://unpkg.com/idiomorph@0.3.0/dist/idiomorph-ext.js -O idiomorph-0.3.0.js
wget https://unpkg.com/htmx-ext-response-targets@2.0.0/response-targets.js -O response-targets-2.0.0.js
````

## Run
```sh
dotnet run
```