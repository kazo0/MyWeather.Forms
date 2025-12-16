You are an expert in Xamarin.Forms, .NET, Uno Platform, WinUI, and cross-platform migrations. Migrate an existing Xamarin.Forms app to Uno Platform by creating a new Uno solution (keep the original XF project untouched). Use the MCP exactly as follows: Uno Platform Docs MCP must be queried before every decision.

## Strict constraints

- Do not remove features; if a gap exists, ask Docs MCP for guidance first.
- You MUST use your available tools to complete the task
- You MUST list your available tools before starting the work
- You MUST extensively use uno_platform_docs_search and uno_platform_docs_fetch for best guidance on usage
- You MUST use all tools available from the UnoApp MCP Server
- You MUST initialize the agent rules with uno_platform_agent_rules_init and uno_platform_usage_rules_init
Process:
- Inventory: scan the repository and list all Xamarin.Forms Pages, ViewModels, Models, Services, Resource Dictionaries, Converters, Behaviors, Effects, and any Shell/NavigationPage/TabbedPage usage.
- Plan: for each area (views, navigation, DI, platform services, resources), propose a migration plan. For every uncertain mapping, first ask Uno Platform Docs MCP, then proceed.
- Create Uno app: generate a new Uno application and named it `MyWeatherApp.UnoFull`.
- Navigation: Migrate the Xamarin.Forms navigation to use the Uno Navigation Extensions and use Uno Toolkit for the TabBar navigation. Confirm each mapping with Docs MCP before changing.
- You MUST use the same permissions in the AndroidManifest.xml as in the original Xamarin.Forms project.
- Migrate the ViewModels/Commands to use the MVUX pattern
- Essentials/plugins: for each API (Preferences, Permissions, Geolocation, File access, Connectivity, Media, etc.), ask Docs MCP how to map it in Uno.
- Resources: migrate Resource Dictionaries, styles, and converters as-is. Do not invent keys or change color/typography systems. If something must change to compile/run, confirm with Docs MCP and document the minimal fix.
- Iteration loop (repeat per subsystem/file): show original file → show migrated file → cite the exact Docs MCP response used → build the Uno app with Uno App MCP → run on desktop target → verify navigation, bindings, and feature parity. Fix compile/runtime issues before proceeding.
- Completion: ensure the Uno app builds cleanly, runs, and matches the original behavior and structure. Provide a final checklist covering navigation flows, bindings, platform services, and resource usage, plus a list of any gaps with links to the Docs MCP answers.
Execution rules:
- Always query Uno Platform Docs MCP before replacing any XF API/control/navigation/binding. Include the citation of the doc answer in your explanation for each change.
- Make changes in small atomic commits; after each major step, compile and run.
- Do not delete or modify the original Xamarin.Forms project.
- You MUST add the new Uno csproj to the same solution file as the original Xamarin.Forms project for easier comparison.

## Supported Form Factors

- **Mobile** (iOS, Android): Optimized with bottom navigation tabs and vertical layouts
- **Desktop** (Windows, macOS, Linux): Features side navigation rail and wider multi-column layouts
- **Web** (WebAssembly): Full browser support with same responsive behavior
- **Tablet**: Adaptive layouts that respond to screen width

## Responsive Behavior

- **Narrow screens** (mobile): Single column, bottom tab bar, vertical stacking
- **Wide screens** (desktop/tablet landscape): Multi-column grids (up to 8 columns), side navigation rail, horizontal content layouts
- Breakpoint-based responsive switching between layouts
- Adaptive image aspect ratios and content density