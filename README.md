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
bash init.sh # Download js libs
npm install # Install Tailwind & TS
````

## Run
```sh
dotnet watch
```

### TODO
- [x] Deploy to fly.io and try volumes with a Sqlite db
- [ ] Make this project some sort of Thuisbezorgd clone 
