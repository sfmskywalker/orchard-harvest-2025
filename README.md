# Orchard Harvest 2025
A reference Orchard Core ASP.NET project, with Elsa Workflows integration

> A showcase & learning sandbox combining **Orchard Core CMS**, **Elsa Workflows 3.x**, **Elsa Agents**, and a **Blazor WebAssembly workflow designer** – including examples of custom activities, agent plugins, AI assisted content workflows, and multi‑tenant configuration.

## Why This Repo Exists
Use this project to:
- See a realistic Orchard Core + Elsa integration (modules, designer & runtime).
- Learn how to host the Elsa Workflow Designer (Blazor WASM) inside your own Orchard Core app.
- Explore custom workflow activities (`Sum`, `ProductAgent`) and agent plugins (Web search, JSON diffing, human‑language field detection).
- Experiment with AI powered content workflows using OpenAI and Google Custom Search.
- Understand multi‑tenant setup and per‑tenant Elsa configuration overrides.

## High‑Level Architecture
```
+--------------------------------------------------------------+
| ASP.NET Core Host (net9)                                     |
|  - OrchardCore CMS (Default tenant pre-provisioned)          |
|  - Elsa Modules (runtime, UI, timers, data, queries, contents)|
|  - Custom Startup: registers activities, agents, plugins     |
|  - Static file rewrite for embedded Blazor WASM designer     |
|                                                              |
|    Blazor WASM App (Elsa Designer + Agents Module)           |
|     - References OrchardCore.Elsa.Designer packages          |
|     - Provides activity icon overrides                      |
|                                                              |
|    Workflows & Agents                                        |
|     - Custom Code Activities: Sum, ProductAgent              |
|     - Agents (Poet, Proofreader, LatestNewsReporter, etc.)   |
|     - Plugins: WebSearch, JsonDiff, JsonHumanFieldDetector   |
+--------------------------------------------------------------+
```

## Notable Features
- Orchard Core CMS configured with a running `Default` tenant out of the box (`App_Data/tenants.json`).
- Elsa Workflow modules and designer integrated seamlessly (no manual static asset copying; WASM referenced via project reference).
- Custom workflow activities with icons in the designer.
- Elsa Agents configured via `appsettings.json` for AI powered operations (poetry, proofreading with structured diffs, news summarization, sentiment analysis, code explanation, recipe generation, summarization, subtitle generation).
- Agent plugins extending Semantic Kernel with:
  - `WebSearchPlugin` (Google Custom Search) – function: `search_web`.
  - `JsonDiffPlugin` – function: `json_diff` (Patch‑like diff).
  - `JsonHumanFieldDetectorPlugin` – function: `json_human_fields` (heuristics for natural language fields).
- Periodic workflow commit strategy example (“Every 10 seconds”).
- JavaScript expression engine with custom functions (`greet`, `sayHelloWorld`) + C# expressions.
- Serilog logging (console + rolling file under `App_Data/log`).

## Repository Layout (Key Parts)
```
src/
  apps/
    OrchardHarvest2025.Web/          # Orchard Core + Elsa host
      Activities/                    # Custom workflow activities
      Plugins/                       # Agent plugin providers & SK plugin classes
      Options/                       # Strongly typed option classes
      Startup.cs                     # Elsa & plugin registration, commit strategies
      Program.cs                     # Host wiring, Serilog, Quartz, WASM static file rewrite
      appsettings*.json              # Configuration (Elsa, Agents, OpenAI, GoogleSearch, per-tenant overrides)
      App_Data/                      # Tenant & logs
    OrchardHarvest2025.Web.Blazor/   # Blazor WASM Elsa Designer host
      ActivityIconProvider.cs        # Custom icons for activities
      CustomIcons.cs                 # SVG icon markup
      wwwroot/appsettings.json       # Designer backend URL (same-origin by default)
```

## Custom Workflow Activities
### `Sum`
Simple `CodeActivity<double>` demonstrating input binding & returning a result.
Inputs: `A`, `B` → Result: `A + B`.
Use Case: Intro to writing & registering activities (`elsa.AddActivitiesFrom<Program>()`).

### `ProductAgent`
Invokes OpenAI’s Responses API to produce persuasive product description copy.
Inputs: `ProductName`, `Description` → Result: AI‑generated text.
Highlights:
- Uses named HttpClient (`"OpenAI"`) with bearer token.
- Simple request body returned as JSON; extracts first text segment.
- Showcases async `ExecuteAsync` & option binding (`OpenAIOptions`).

## Elsa Agents & Plugins
Agents defined in `appsettings.json > OrchardCore > Default > Elsa > Agents > Agents` include examples like `Poet`, `Proofreader`, `LatestNewsReporter`, `SentimentAnalyzer`, etc.
Each agent:
- Declares a function name and a prompt template.
- Lists input/output variable contracts.
- Optionally references plugins (e.g., Proofreader uses `JsonHumanFieldDetector` + `JsonDiff`).
- Has execution settings (temperature, max tokens, response format).  

### Plugin Functions
| Plugin | Function | Purpose |
|--------|----------|---------|
| WebSearch | `search_web` | Fetch top Google Custom Search results & format brief list |
| JsonDiff | `json_diff` | Produce Patch-like diff entries (add/replace/remove) between two JSON objects |
| JsonHumanFieldDetector | `json_human_fields` | Identify JSON Pointer paths likely containing natural language text |

## Blazor WASM Elsa Designer Integration
The WASM project references:
- `OrchardCore.Elsa.Designer`
- `OrchardCore.Elsa.Designer.BlazorWasm`
- `Elsa.Studio.Agents`
`Program.cs` calls `builder.AddElsaDesigner();` then registers the Agents module with backend configuration.
Custom icons are provided via `ActivityIconProvider` (e.g., Robot for `OrchardHarvest.ProductAgent`, Sigma for `OrchardHarvest.Sum`).
The ASP.NET Core host calls `app.RewriteElsaStudioWebAssemblyAssets();` to serve the designer assets.

## Quick Start
Prerequisites: .NET SDK 9.x

```bash
# Clone
git clone https://github.com/your-org/orchard-harvest-2025.git
cd orchard-harvest-2025

# Restore & build (solution root)
dotnet restore
dotnet build

# Run the web host (includes WASM designer assets)
dotnet run --project src/apps/OrchardHarvest2025.Web/OrchardHarvest2025.Web.csproj
```
Visit the site (default Kestrel port – if unspecified, look at console output). Sign into Orchard Core Admin (the setup should be pre‑provisioned; if prompted, follow standard Orchard setup). Enable or verify that Elsa modules are active. Open the Workflows UI to launch the designer.

## Configuration Overview
Primary configuration file: `src/apps/OrchardHarvest2025.Web/appsettings.json`.
Sections:
- `Logging` / `Serilog`
- `Elsa` (root) – Identity token settings, provider API keys.
- `OrchardCore:Default:Elsa` – Per‑tenant overrides (BasePath, BaseUrl, Agents definitions & secrets).
- `OpenAI`, `GoogleSearch` – Option classes bound in `Program.cs`.

### Agent Secrets
Agents use `ApiKeys` & `Services` for referencing keys indirectly. Replace sample keys with real values via user secrets or environment variables for production. (Never commit real secrets.)

### Designer Backend URL
`OrchardHarvest2025.Web.Blazor/wwwroot/appsettings.json` contains:
```json
{"Backend": {"Url": ""}}
```
Empty = same-origin. Update if hosting WASM externally.

## Multi‑Tenancy
`App_Data/tenants.json` shows a pre‑running `Default` tenant. You can add additional tenants via the Orchard admin. Each tenant can have its own Elsa configuration block under `OrchardCore:{TenantName}:Elsa` enabling distinct BaseUrl, agents, or external service keys.

Example snippet:
```json
"OrchardCore": {
  "Acme": {
    "Elsa": {
      "Http": { "BasePath": "/wf", "BaseUrl": "https://localhost:9095/acme" }
    }
  }
}
```

## Scheduling & Commit Strategies
`Startup.cs` registers a custom periodic commit strategy:
```csharp
strategies.Add("Every 10 seconds", new PeriodicWorkflowStrategy(TimeSpan.FromSeconds(10)));
```
Use this to demonstrate background workflow execution without manual triggers.

## Expressions (JavaScript & C#)
JavaScript engine configured to allow CLR access and provides helper functions:
```js
function greet(name) { return `Hello ${name}!`; }
function sayHelloWorld() { return greet('World'); }
```
Also enables C# expression evaluation via `elsa.UseCSharp();`.

## Adding Your Own Activity
1. Create class deriving from `CodeActivity<T>` (or other base) in `Activities/`.
2. Annotate with `[Activity("Category", "Display Name", "Description")]` & `[Input]` / `[Output]` attributes.
3. Ensure assembly scanned via `elsa.AddActivitiesFrom<Program>()` (already done).
4. (Optional) Add icon entry in `ActivityIconProvider`.

## Extending Agents
- Implement a `PluginProvider` returning descriptors for new plugins.
- Provide methods annotated with `[KernelFunction]` for Semantic Kernel.
- Register provider via `services.AddPluginProvider<YourPluginProvider>();` in `Startup.cs`.

## Logging
Logs written to `App_Data/log/orchard.log` (rolling daily) and console. Adjust levels in Serilog config (`Override` section controlling module noise).

## Troubleshooting
| Issue | Possible Fix |
|-------|--------------|
| Designer not loading | Confirm WASM project reference & `RewriteElsaStudioWebAssemblyAssets()` executed before `UseStaticFiles()` or after (as configured). Clear browser cache. |
| Activities missing | Ensure build succeeded & attributes are correct; check category filter in designer. |
| OpenAI errors | Replace placeholder API key, verify model `gpt-4o` availability. |
| Google search returns error | Validate API key & Custom Search Engine ID (cx). Limit requests to quota. |
| Tenant config ignored | Confirm you edited the correct tenant section (`OrchardCore:Default:Elsa`). Restart app after changes. |

## Suggested Next Steps / Ideas
- Add sample workflows (export JSON) demonstrating each agent.
- Introduce persistence providers (e.g., EF Core) if not already enabled by modules.
- Add unit tests for plugins & activities.
- Secure agent endpoints with API key or OAuth.
- Add Dockerfile & dev container config.

## Contributing
Open a PR with clear description. For new activities/plugins, include brief README section updates & (ideally) tests.

## License
See `LICENSE` (MIT unless specified otherwise).

---
Happy harvesting workflows! Feel free to adapt this setup to kickstart your own Orchard Core + Elsa powered applications.
