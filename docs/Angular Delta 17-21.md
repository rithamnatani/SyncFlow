---
created: 2025-12-24 15:57
category:
  - "[[LLM Coding]]"
status:
---
# Angular Delta 17-21

**Condensed Angular v21 Guide**

**Change:** use `provideZonelessChangeDetection()` and remove `zone.js` imports for default zoneless apps 
**Change:** use native `async/await` for async operations instead of Zone-patched promises 
**Change:** omit `standalone: true` in metadata as it is now the default 
**Change:** use `resource()` or `httpResource()` for async data fetching instead of complex RxJS flows 
**Change:** use `linkedSignal()` for mutable state dependent on upstream signals 
**Change:** use `input()`, `output()`, and `viewChild()` signals instead of `@Input`/`@Output` decorators 
**Change:** use `host` property in component metadata instead of `@HostBinding` or `@HostListener` decorators 
**Change:** use `@let` syntax for local variable declarations in templates 
**Change:** use built-in `@if`, `@for`, `@switch` blocks instead of structural directives 
**Change:** use Vitest via `npm i vitest` instead of Karma/Jasmine for unit testing 
**Change:** use `mat.theme` mixin for simplified Material theming instead of component-specific mixins 
**Change:** use `inputBinding` and `outputBinding` with `createComponent` for type-safe dynamic components 
**Feature:** use experimental `@angular/forms/signals` with `form()` and `[field]` binding 
**Feature:** use `@angular/aria` for unstyled, accessible UI primitives (Headless UI) 
**Feature:** use regex literals directly in templates (e.g., `/\d+/.test(val)`) 
**Feature:** use `**` exponential and `in` operators directly in template expressions 
**Feature:** use `ServerRoute` config with `getPrerenderParams` for per-route rendering modes **Feature:** use `@defer (hydrate on viewport)` for incremental hydration 
**Feature:** use `withEventReplay()` to capture and replay user events during hydration 
**Deprecated:** don't use `*ngIf`, `*ngFor`, or `*ngSwitch` (deprecated in v20) 
**Deprecated:** don't use `ControlValueAccessor` for custom form components (use Signal Forms) 
**Deprecated:** don't append `Component` or `Service` suffixes to filenames/classes (style guide update)

### 1. Core Architecture & Change Detection

**NP (New Pattern): Default to Zoneless.** Angular v21 applications do not include `zone.js` by default. Use `provideZonelessChangeDetection()` in the application bootstrap and remove `zone.js` from polyfills. **AP (Antipattern): Relying on `zone.js` for async tracking.** Do not rely on Zone to patch browser APIs. Use native `async/await` which is now fully supported without downleveling to promises.

**NP: Implicit Standalone Components.** `standalone: true` is the default setting. Omit the `standalone` property in `@Component`, `@Directive`, and `@Pipe` metadata unless you specifically need `standalone: false`.

### 2. Reactivity & State Management

**NP: Use the `resource` API for Asynchronous Data.** Do not use complex RxJS flows for basic data fetching. Use the `resource()` or `httpResource()` APIs (experimental) to link signals to asynchronous requests.

- _Code:_ `userResource = resource({ request: this.id, loader: ({request}) => fetch(...) })`.

**NP: Use `linkedSignal` for Dependent State.** Use `linkedSignal` for mutable state that must reset or update when a localized upstream signal changes (e.g., resetting a selection when a list changes).

**AP: Using `@Input` decorators.** **NP: Use Signal Inputs.** Use `input()`, `input.required()`, and `output()` instead of decorator-based inputs/outputs.

### 3. Forms (Experimental Signal Forms)

**NP: Use Signal Forms.** For new forms, use the experimental `@angular/forms/signals` package. Define a model signal, pass it to `form()`, and bind using the `[field]` directive.

**AP: Implementing `ControlValueAccessor`.** Signal Forms handle custom component bindings natively. Do not implement `ControlValueAccessor` when using this new paradigm.

### 4. Templates & Control Flow

**AP BC (Antipattern / Future Breaking Change): Structural Directives.** `*ngIf`, `*ngFor`, and `*ngSwitch` are deprecated as of v20. Do not use them. **NP: Built-in Control Flow.** Strictly use `@if`, `@for`, and `@switch` syntax.

**NP: Local Variables & Regex.** Use `@let` to declare local variables in templates. You may now use regular expressions directly in templates (e.g., `@let isNum = /\d+/.test(val)`).

**NP: Expanded Expression Syntax.** Use the `**` (exponential) and `in` operators directly in template expressions.

### 5. Components & Accessibility

**NP: Angular Aria (Headless UI).** Use `@angular/aria` for unstyled, accessible UI primitives (Accordion, Menu, Tabs) rather than building complex widgets from scratch. Style them via CSS.

**NP: Simplified Host Bindings.** Use the `host` property in component metadata rather than `@HostBinding` or `@HostListener` decorators. V21 enforces type checking on these bindings.

**NP: Clean File Naming.** Do not append suffixes (like `Component` or `Service`) to class names or files if following the updated v20 style guide. The CLI no longer generates them by default.

### 6. Server-Side Rendering (SSR)

**NP: Incremental Hydration & Event Replay.** Use `provideClientHydration(withIncrementalHydration(), withEventReplay())`. Use `@defer (hydrate on viewport)` to lazily hydrate components.

**NP: Route-Level Render Modes.** Use `ServerRoute` configuration to define rendering modes (`Server`, `Client`, `Prerender`) per route, including parameter resolution via `getPrerenderParams`.

### 7. Testing

**NP: Vitest.** Use **Vitest** as the default test runner. Do not use Karma (deprecated) or Jasmine for new projects.

- _Code:_ `import { describe, it, expect } from 'vitest';`.

**Analogy:** Coding in Angular v17 vs. v21 is like shifting from a manual transmission car (v17) to an autonomous electric vehicle (v21). In v17, you had to manually shift gears (manage `zone.js`, `ControlValueAccessor`, RxJS subscriptions) to keep the engine running. In v21, the system handles the transmission (Zoneless, Signal Forms, Resource API) automatically, allowing you to focus entirely on the destination (business logic).