# Air Quality Proxy Server

A lightweight HTTP proxy server written in C# (.NET Framework 4.8) that fetches real-time air quality data for Serbian cities via the [IQAir API](https://www.iqair.com/air-pollution-data-api).

## Overview

The server listens on `localhost:5050`, accepts HTTP requests with a city name parameter, and returns the current AQI (Air Quality Index) value. Responses are cached in-memory for 1 hour to minimize redundant API calls.

## Architecture

```
Client → HttpListener (port 5050)
              │
              ▼
         In-Memory Cache (Kes)
              │ miss
              ▼
         IQAir API (airvisual.com)
```

- **Program.cs** — HTTP server, request routing, API communication
- **Kes.cs** — Thread-safe in-memory cache with TTL
- **IQAir.cs** — Data model for API responses

## Requirements

- .NET Framework 4.8
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) 13.0.3
- IQAir API key

## Configuration

Set your API key in `App.config`:

```xml
<appSettings>
    <add key="ApiKey" value="YOUR_API_KEY_HERE"/>
</appSettings>
```

## Usage

Build and run the project, then send a GET request:

```
GET http://localhost:5050/?city=Beograd
```

The server returns the AQI value as plain text.

**Supported regions:** Central Serbia and Autonomna Pokrajina Vojvodina. The server automatically tries both if the first lookup fails.

## Caching

Responses are cached per URL for **1 hour**. Expired entries are transparently refreshed on the next request.
