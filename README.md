
**contracts/wiring-contract.md**

Every file write validated by the resolver before it lands. Every reference must resolve. Resolver map maintained live, updated on every write.

Reference types checked: imports, images, routes, api_calls, tokens, env_vars, i18n keys, components.

Resolution rules:
- trivial referent missing → create it now
- feature not built yet → wire to placeholder that renders "coming soon"
- typo → fix
- unresolvable → halt, report specific reference

No file ships with an unresolved reference. Ever.

Enforced by `wiring-check` tool (node 12b, 16a, 24). Tool repo: wiring-check.

**contracts/integrity-contract.md**

Runs continuously, not in batch:
- type integrity: tsc --noEmit on every write
- env integrity: every process.env.X has matching declaration
- i18n integrity: every t('key') exists in every locale file
- route integrity: every manifest route has a component; every component has a route or is used
- state integrity: no localStorage read before write; no context consumed without provider
- leak integrity: no console.log in prod; no stack traces in 500s; no tokens in URLs; no unvalidated input to DB; no unhandled promises; no orphaned listeners; no uncleared intervals
- asset integrity: every image has dimensions; every font has font-display; no 404s during runtime verification
- a11y integrity: alt on images; labels on inputs; keyboard reachable; WCAG AA contrast; visible focus

A failed check blocks the write.

**contracts/hardening-contract.md**

Default deny. Allow explicitly. Validate everything. Leak nothing.

Thirteen categories, all enforced on every write + on deploy:

1. SECRETS — never reach client. NEXT_PUBLIC_/VITE_ prefix enforcement. Secret pattern scanner blocks before commit. .env* never committed. No secrets in URLs, logs, or stack traces.
2. SECURITY HEADERS — HSTS (2y, includeSubDomains, preload), X-Content-Type-Options: nosniff, X-Frame-Options: DENY, Referrer-Policy strict-origin-when-cross-origin, Permissions-Policy denying camera/mic/geo, COOP same-origin, CORP same-origin, CSP nonce-based (no unsafe-inline, no unsafe-eval), X-Powered-By removed.
3. AUTH — default private. Server-side check every request. Refresh tokens httpOnly+Secure+SameSite=Lax/Strict cookies. CSRF token on state-changing. Rate limits on login/register/reset. No user enumeration. Session invalidated on logout/password change/role change. MFA-ready flag.
4. INPUT — schema validation server-side. Path traversal sanitized. File uploads MIME+size+extension whitelist + re-encode. No raw SQL. No eval, no new Function, no child_process.exec with user input. JSON size limit.
5. OUTPUT — React auto-escapes. dangerouslySetInnerHTML requires DOMPurify; linter flags every use. Whitelisted response shapes. No stack traces in 500s. No source maps in prod. No directory listings.
6. CORS — never * on authenticated routes. Allowlist explicit. Credentials only with specific origin.
7. DEPENDENCIES — npm audit --production on scaffold. High/critical advisories block. No abandoned packages (>3y since publish) without override. Lockfiles committed. SRI on CDN scripts.
8. INFRASTRUCTURE — .git, .env, .DS_Store, dumps rejected. /admin, /wp-admin, /phpmyadmin, /server-status 404 unless real. robots.txt clean. sitemap.xml excludes private. No /health leaking version or DB status. GraphQL introspection disabled in prod. Playground disabled.
9. RUNTIME — every async has error handling. Every fetch has timeout (10s). Every interval/listener/subscription cleared on unmount.
10. LOGGING — no PII. No secrets. Structured with redaction for password/token/secret/authorization/cookie keys. No console.debug in prod.
11. COOKIES — Secure (prod), HttpOnly, SameSite=Lax/Strict. __Host- prefix where possible. No wildcard domain.
12. EMAIL/EXTERNAL — SPF/DKIM/DMARC verified. Webhook signatures verified. Timeout + retry + backoff on outbound. SSRF allowlist.
13. BUILD-TIME — tree-shaking works. No .map files. No test files in prod build. No source paths in errors.

Enforced by `harden` tool (nodes 18d, 18e, 20d).

**contracts/completeness-contract.md**

The tree builds a website, not pages.

- system.json (node 01d) enumerates actors, journeys, routes, states, entities, integrations before any code.
- manifest.json (node 04a) is the checklist. Nothing ships until 100%.
- anticipatory architecture (node 03): layouts abstracted, auth as context, routes declared, data fetching uniform, tokens consumed via classes, components accept future props, empty directories reserved for pages not yet built.
- test: if user adds login in 3 months, does it require rework or wiring?

**contracts/consistency-contract.md**

Design is contractual, not improvisational.

- design-contract.md (node 11d) defines allowed colors, type scale, spacing, radii, shadows, animations.
- ESLint enforces: no hardcoded hex, no arbitrary Tailwind values, no inline styles outside tokens.
- consistency check (node 13a) per page: shared layout, only tokens, reuse component kit, match reference section structure, match previous pages' spacing rhythm.
- Visual regression between pages catches drift.

**contracts/load-once-contract.md**

The skill is loaded once. Silent operation is the default:
- no node announcements
- no state-block printing
- no unlock requests
- no "may I proceed" prompts

Every build request flows through the tree automatically.

Debug mode toggle: `debug on` / `debug off`. When on, prints state block (wire format `[AS] 1:01-06 2:07-11,11b 3:12-14 | 4,5`), node names, decisions, tool invocations.

Recovery: if 3 consecutive responses fail to produce artifacts, restart from node 01. Log reason to session.log.

**contracts/maintenance-contract.md**

Full text in MAINTAINING.md:

- Schema stability: frozen, versioned via $id, breaking changes require new version, old versions supported 2 years.
- Tool replaceability: no node depends on specific tool by name, only on contract. Every tool has tools/{name}.md with alternatives.
- Model-agnostic: no node contains model-specific tuning. Platform behavior lives only in integrations/.
- State accumulation: history.json persists across sessions. Every build appends. New sessions inherit.
- Golden test: every node has at least one golden test. New model must pass 90%+ before marked supported in skill-model-matrix.json.
- Integration modularity: adding new platform = one new file. Never modify nodes.
- Quarterly checklist: run golden tests against latest models, refresh references/, audit tool dependencies, review session.log for most-failed nodes, rewrite nodes > 20% failure rate, add new integration files, version-bump schemas if changed.
- Deprecation: retired nodes kept with # DEPRECATED header + replacement pointer. Retired schemas move to schemas/archive/. Never deleted.
- Reference-vault sync: weekly GitHub Action. references/index.json has last_verified per entry. Stale (>90 days) flagged in node 02.

**contracts/repair-contract.md**

Node 25 runs at end of every session. Reactive only.

Trigger conditions:
- any tool failure this session
- any schema mismatch on tool output
- any timeout or hang
- any fallback invoked (e.g., SPA-Ripper → SiteMap-X)
- any node failed more than once

Instructions:
1. Read session.log. Collect every tool failure with invocation, input, error.
2. Extract minimal reproduction. Write to tests/failures/{tool}-{hash}/.
3. Read failing tool's source. Identify specific bug. Do not modify working code.
4. Propose patch. Apply to tool repo.
5. Verify: run tool's tests/run.py + tests/verify.py + the new repro. All three must pass.
6. Pass → keep patch, add repro as permanent regression test. Fail → revert, log "unfixed" with error.
7. Write repair-report.json:
   {
     "tools_touched": [...],
     "fixes_applied": [...],
     "regressions_added": [...],
     "unfixed": [...],
     "skipped": "no failures this session" | null
   }
8. Report to user: one line per tool touched, one line per unfixed.

Do not:
- modify a tool that didn't fail this session
- refactor for style
- change a public CLI, schema, or output format without version bump
- apply any fix that breaks existing tests
- merge silently
- "fix" by disabling the feature that was failing

If the AI cannot fix a tool in one pass: stop. Report. Move on.

=============================================================
D. skill-tree.json — MACHINE-READABLE TREE
=============================================================

Every node entry:
{
  "id": "07",
  "name": "API Surface",
  "tier": 2,
  "prereqs": ["06"],
  "parallel_with": ["08", "09", "10", "11"],
  "input": "reference.json",
  "output": "api_client.{ts,py} + types.{ts,py}",
  "activates_when": "reference.json exists AND api_surface is non-empty",
  "skill_file": "nodes/t2/07-api-surface.md",
  "artifact_schema": "schema/endpoints.schema.json",
  "model": "sonnet",
  "max_response_tokens": 6000,
  "version": "1.0.0",
  "consumers": ["14", "17"]
}

Extend for all 42 nodes. Declare parallelism. Declare model. Declare budget.

compatibility.json:
{
  "node_06": {"produces": "prompt.md@v1", "consumers": ["12","13","14"]},
  "node_11": {"produces": "design-tokens.json@v1", "consumers": ["12"]},
  "node_11b": {"produces": "slop-report.json@v1", "consumers": ["12","13"]},
  "node_02": {"produces": "reference.json@v1", "consumers": ["07","08","09","10","11"]},
  "node_07a": {"produces": "api_research.json@v1", "consumers": ["07c","07d","17a"]},
  "node_24b": {"produces": "wiring_report.json@v1 + security_report.json@v1", "consumers": ["23b"]},
  "node_25": {"produces": "repair-report.json@v1", "consumers": []}
}

skill-model-matrix.json:
{
  "claude-sonnet-4.5": "supported",
  "claude-opus-4.5": "supported",
  "claude-haiku-4.5": "supported",
  "gpt-5": "supported",
  "gpt-5-mini": "degraded",
  "gemini-2.5-pro": "supported",
  "gemini-2.5-flash": "degraded",
  "cursor-composer-1": "partial"
}

=============================================================
E. NODE FILE FORMAT — uniform across all 42
=============================================================

Every node file:

# Node NN — Name

Tier: N
Prereqs: [list]
Parallel with: [list]
Input: [artifact name and shape]
Output: [artifact name and shape]
Model: haiku|sonnet|opus
Budget: N tokens

## Working Contract
(verbatim — see contracts/working-contract.md)

## Instructions
[numbered, dense, imperative. no hedging. no "you should".]

## Output Contract
[exact shape of artifact emitted]

## If this fails
[recovery paths, explicit. no improvising.]

## Do not
[banned behaviors]

## Example output
[one real snippet, 10-30 lines]

Write all 42 nodes in this format. No stubs. No TODOs.

=============================================================
F. SCHEMAS — FROZEN CONTRACTS
=============================================================

Produce all 18 schemas. Each is JSON Schema draft 2020-12. Each strict. Each with $defs.

structure.schema.json (master acquisition contract):
Required keys: reference_url, acquired_at, acquirer, stack, routes, sections, components, design_tokens, api_surface, external_hosts.
- stack: {framework, css, auth, cdn, analytics, payments, error_tracking} — each {name, confidence, evidence[]}
- routes: [{path, title, parent_path, status}]
- sections: {page_path: [{order, type (enum), contents[], layout, height}]}
- components: [{name, selector, variants[], snippet}]
- design_tokens: {colors{}, fonts{}, spacing[], radii[]}
- api_surface: [{path, method, source, params[], auth_hint}]

api_research.schema.json — matches api-researcher tool output. Verified.

wiring_report.schema.json — unresolved[], summary{}, exit_code.

security_report.schema.json — 13 categories with checks[], pass/fail/n/a, patches[].

slop-report.schema.json — per-block {before /5, after /5, tells[]}.

production_checklist.schema.json — pages[], legal[], technical[] — each pass/fail/n/a.

repair-report.schema.json — tools_touched[], fixes_applied[], regressions_added[], unfixed[], skipped.

session.schema.json — session_id, started_at, intent, scope, active_skills, artifacts, checkpoints[], budget{}, why_log[].

node.schema.json — describes node file shape. Includes working_contract: true, model, max_response_tokens.

=============================================================
G. THE SEVEN TOKEN LEVERS — tools/token-efficiency.md
=============================================================

1. Semantic density — strip every token that doesn't change output. +8.4pp accuracy.
2. Prompt caching — cache SKILL.md, schemas, templates, node files. ~90% static cost cut.
3. Context hygiene — keep artifact, drop conversation. 20-86% trim.
4. Output verbosity control — enforce artifact shape. No prose where JSON will do. 40-75% cut.
5. Model routing — haiku (mechanical 01-04, 11b-lint) / sonnet (06-20) / opus (21-25 meta).
6. Structured inter-node output — pipe-delimited or JSON. 70-80% cut.
7. Symbolic instruction encoding — "Prereqs: 06" not prose.

One-line rule: every token must survive "if I delete this, does the model's output change?"

=============================================================
H. THE SELF-IMPROVEMENT LOOP
=============================================================

Every node failure logged to session.log:
{"node": "13a", "input_hash": "abc123", "error": "...", "model": "sonnet", "at": "..."}

/stats command reads all session.log files, surfaces top-5 most-failed nodes, shows which models failed them most.

--report-failure writes tests/failures/{node-id}-{input-hash}/ with minimal reproduction + expected + actual.

CI runs golden tests + all failures/ cases on every commit. Once fixed, the case becomes a permanent regression test.

Loop: fail → log → reproduce → test → fix → protected forever.

=============================================================
I. TIER 1 — FOUNDATION (13 nodes)
=============================================================

01 — Intent Parse. Input: free text. Output: intent.json {product_type, target_user, key_flows[], reference_url, vibe}. Model: haiku. Budget: 1500.
Steps: extract product type, target user, 2-5 key flows, reference URL if provided, vibe (playful/professional/technical/minimal).
If fails: ask user for missing field directly.
Do not: invent reference URL. Invent flows not implied by ask.

01a — Suitability Check. Prereqs: 01. Output: suitability.json {match: bool, reason, alternative_references[]}. Model: haiku. Budget: 800.
Compare product_type to reference type. Warn on mismatch (game vs SaaS, static blog vs dashboard). Never block.

01b — Scope Negotiation. Prereqs: 01, 01a. Output: scope.json {deliverable (landing-page|mvp|full-product|custom), tier_stop (2|3|4), routes_include[], routes_exclude[], time_budget_minutes}. Model: haiku. Budget: 800.
Ask user to pick deliverable. Set tier_stop. Collect route include/exclude.
If fails: default mvp / tier_stop 3.

01c — Budget Estimate. Prereqs: 01b. Output: estimate.json {nodes_count, est_tokens, est_minutes, est_cost_usd}. Model: haiku. Budget: 500.
Advisory only. Never refuses.

01d — System Model. Prereqs: 01b. Input: intent + scope + reference. Output: system.json — actors[], journeys[], routes by category (public/auth/app/admin/error), states{auth,data,theme,network,device}, entities[], integrations[]. Model: sonnet. Budget: 4000.
This is the completeness foundation. Enumerate everything the site will ever need — even if the reference doesn't have it.

02 — Reference Load. Prereqs: 01. Invokes SPA-Ripper. CLI: `spa-ripper --target URL --out DIR --extract structure,endpoints,tokens,stack --depth 3 --render`. Validates against structure.schema.json. Output: reference.json. Model: sonnet. Budget: 3000.
Fallback chain if fails: SiteMap-X → HAR → manual paste → screenshots.
Check references/index.json first for pre-scraped version.

02b — Deep Reference Load. Prereqs: 01. Invokes SiteMap-X for deep crawl or >500 routes. Same output as 02.

03 — Code Style Lock. Prereqs: 02. Output: .editorconfig + style-rules.md + anticipatory-rules.md. Model: haiku. Budget: 800.
Anticipatory rules: layouts abstracted, auth as context, routes declared, data fetching uniform, tokens via classes, components accept future props.

04 — File Tree Gen. Prereqs: 03. Output: file-tree.md. Respects scope.routes_include. Model: sonnet. Budget: 2500.

04a — Completeness Manifest. Prereqs: 04. Output: manifest.json — required_pages[], required_states_per_page{}, required_components[], required_flows[], required_layouts[]. Model: sonnet. Budget: 2000.
Every node from 12 onward updates this. Node 24 fails if anything is pending.

05 — Repo Scaffolder. Prereqs: 04. Output: package.json / pyproject.toml with pinned versions + hardened defaults (.gitignore with .env*, .env.example template, next.config.js with security headers, auth middleware default-deny, R8/terser minification, sourcemaps off in prod).

06 — Prompt Composer. Prereqs: 02, 03, 04, 05. Output: prompt.md + handoff.md. Model: sonnet. Budget: 5000.
Fill templates/prompt-template.md with: REFERENCE STACK, ROUTE MAP, HOMEPAGE SECTIONS, KEY COMPONENTS, DESIGN TOKENS, API SURFACE, MY PRODUCT, BUILD INSTRUCTIONS (don't invent pages not in route map, don't use Lorem ipsum), OUTPUT FORMAT.
handoff.md: what the user does with prompt.md.

06a — Design Commitment. Prereqs: 06. Output: design-commitment.md — aesthetic direction, font pairing (Inter/Roboto/Arial/Space Grotesk banned), color palette (purple gradients banned), layout strategy. User approves or redirects.

=============================================================
J. TIER 2 — STRUCTURE (17 nodes)
=============================================================

07 — API Surface. Output: typed client + types. Consumes reference.json.api_surface. Model: sonnet.

07a — API Research. Invokes api-researcher tool. CLI: `apiresearch --endpoints ./endpoints.txt --out ./api_research --auth-header "$TOKEN"`. Output: api_research.json. Model: sonnet.

07b — Third-Party API Research. For each integration in system.json, pulls OpenAPI spec if available (Stripe, OpenAI, Anthropic, Resend, Clerk publish theirs), else docs. Output: integrations/{name}.md + working client scaffold with auth wired + stubbed functions. No hallucinated APIs.

07c — Backend Architecture. Output: backend-decision.md — runtime, framework, API style (REST/GraphQL/tRPC), data layer, hosting, caching, queue. Reasoning for match vs diverge from reference.

07d — API Design. Output: openapi.yaml (or schema.graphql or trpc-router.ts) — frozen contract frontend and backend develop against in parallel. Includes: every route, request shapes, response shapes (success + error), auth per route, pagination strategy, error contract, versioning, rate limit policy.

07e — Backend Scaffold. Server entry, router, middleware (auth/logging/error/rate-limit/CORS), validation layer, stubbed handlers for every route.

07f — Handler Implementations. Validation + DB query + response shaping per route.

07g — Integration Wiring. Webhook handler + signature verification + retry + idempotency keys (Stripe needs these) + event log per third-party.

08 — Stack Fingerprint. Output: stack-decision.md. Model: haiku.

09 — Route Extract. Output: router config for chosen framework.

10 — Section Detect. Output: sections.md + sections.json.

11 — Design Tokens. Output: design-tokens.json (OKLCH) + tailwind.config.js + tokens_reconcile.json. Model: sonnet.

11a — Design System Extract. Invokes design-extract tool. Renders page with Playwright, scrolls to trigger lazy content, extracts computed styles, produces semantically named tokens + component patterns + animation keyframes. Fills gaps nodes 02/02b can't.

11b — Copy Cleanse. Three linters: SlopMonster (writing) + signs-of-ai-design (design) + signs-of-ai-trust (fake proof). Rewrite pass + rival-model cleanse. Ship at 5/5. Never invent proof — write [needs number] instead.

11c — Design Slop Lint. Runs merged anti-pattern registry (references/signs-of-ai-design.md — pulls from Impeccable + design-on + slop-detect + deep-slop): purple gradients, pill buttons everywhere, emoji icons, cursor animations, scroll-overload, AI-looking images. Blocks on critical.

11d — Design Contract. Output: design-contract.md + ESLint rules + Tailwind preset. Defines ONLY allowed colors, type scale, spacing, radii, shadows, animations. Enforced at lint. No hardcoded hex, no arbitrary Tailwind values, no inline styles.

=============================================================
K. TIER 3 — COMPOSITION (9 nodes)
=============================================================

12 — Component Kit. Reusable Button, Card, Nav, Modal, Input, Toast, Skeleton, EmptyState, ErrorBoundary, AuthGuard. Output goes through wiring resolver.

12a — Motion Layer. Entrance/hover/page-transition/stagger/spring. Respects prefers-reduced-motion. Timing per windags motion tree: hover 100-150ms ease-out, toggle 200-250ms ease-in-out, reveal 300-400ms spring, modal 400-600ms. Spring: snappy 400/28, smooth 200/25.

12b — Static Verify (Bug Fixer pass 1). tsc --noEmit + eslint with design contract + prettier + next build. Fix every error. Warnings become errors. No shipping with warnings.

13 — Route Builder. Per-route page files. After each page, runs 13a before next page.

13a — Consistency Check. Does this page use shared layout? Only tokens? Reuse component kit? Match reference section structure? Match previous pages' spacing rhythm? Fix before moving on.

14 — Data Fetching. Hooks with loading/error/success states. React Query, SWR, or vanilla. Uniform shape everywhere.

15 — Form Logic. Validation, submit, error display, optimistic updates, field-level errors, disabled-while-submitting.

16 — State Patterns. Global state (Zustand, Redux, Context, Jotai). Auth, theme, cart, whatever.

16a — Runtime Verify (Bug Fixer pass 2). Playwright: open every route, capture console errors, run each flow (signup/login/navigate/form submit), capture video, compare screenshots to reference.

=============================================================
L. TIER 4 — PRODUCT (17 nodes)
=============================================================

17 — DB Schema. Infer entities from api_surface, produce migrations.

17a — Schema Deep Infer. system.json + api_research.json + integrations → full schema with tables, columns, relations, indexes, migrations, seed strategy.

17b — Migration Strategy. Versioned, backward-compatible, blue/green deploy compatible.

18 — Auth Layer. Login, signup, session, protected routes. Hardened defaults from hardening contract.

18b — Security Pass. JWT storage, CSRF, CORS, rate limits, input validation, SQL injection, auth-bypass paths. Patches straightforward, lists the rest. Output: security-report.json.

18c — Edge Verify (Bug Fixer pass 3). Empty states, loading states, error states, offline, mobile 375px, dark mode, keyboard-only, screen reader. Failed checks become bug tickets.

18d — Security Hardening. 13 categories from hardening contract. Headers, cookies, CSP, CORS, rate limits, input validation, output encoding, secret scan, dependency audit, .git/.env rejection, no source maps, GraphQL introspection disabled in prod. Invokes `harden` tool.

18e — Security Verify. Playwright + ZAP against preview build. Blocks if any critical check fails.

19 — Test Scaffold. Unit + integration for components and API calls.

19a — Backend Test Suite. Contract tests (impl matches OpenAPI), integration tests, auth tests, rate limit tests, webhook signature tests.

20 — Deploy Wiring. Dockerfile, CI yaml, env var list, one-command local dev, headers config, WAF if applicable, secret rotation plan.

20a — Production Readiness. Pages (404, 500, Privacy, ToS, Cookie Policy, Refund if selling, Contact). Legal (cookie consent banner, form consent checkboxes, analytics disclosure, third-party embeds audited, image licensing verified, real business details in footer, only minimum data collected). Technical (favicon + apple-touch-icon + og:image, WCAG AA contrast, alt text, keyboard nav, verb button labels, no unsupported claims, custom domain configured).

20b — Performance Pass. LCP ≤ 2.5s (hero image fetchpriority="high"), INP ≤ 200ms, CLS ≤ 0.1 (no layout animations, explicit image dimensions, font-display swap/optional, skeleton placeholders). Bundle size check. Lighthouse run.

20c — API Deployment. env vars, secrets management, rate limit config, cache config, log shipping, health check endpoint, readiness probe.

20d — Deploy Hardening. Env var audit, secret rotation plan, WAF config, backup strategy, incident response runbook.

24 — Build Verify. Scaffold output in temp dir. npm install. Start dev server. curl every route in scope. Report status codes + errors. Never claim success if any route fails.

24b — Bug Loop Orchestrator. Runs 12b → 16a → 18c in sequence. Three rounds max. If any bug survives three passes → halt, report, ask user.

=============================================================
M. TIER 5 — META (5 nodes)
=============================================================

21 — Multi-Agent. Split work across sub-agents (frontend, backend, tests, content). Coordinator merges. Only when concurrent work is possible.

22 — Diff-Merge. Compute fidelity score per category (routes, sections, tokens, components, endpoints). Output: diff-report.json + correction-prompt.md for anything below 90%.

23 — Self-Refine. LLM critiques output against reference + scope, then revises.

23b — Final Ship Gate. Run SlopMonster across entire output. Run design + trust linters. Run security verification. Block if anything below threshold. Output: ship-report.json.

25 — Post-Session Repair. Prereqs: 23b. Runs at end of every session. Output: repair-report.json + patched tool repos (gated).
Triggers: any tool failure this session, any schema mismatch on tool output, any timeout or hang, any fallback invoked, any node failed more than once.
Steps: read session.log, extract minimal repro, identify specific bug, propose patch, verify against tool's tests + new repro (all three must pass), keep or revert, write repair-report.json.
Reactive only. Never touch a tool that didn't fail this session. Every change reported to user. If AI cannot fix a tool in one pass: stop, report, move on.

=============================================================
N. TOOLS — EXTERNAL INTEGRATIONS
=============================================================

Each tool has a tools/{name}.md integration doc covering: what it is, why used, install, CLI invocation, output schema it satisfies, how the tree consumes it, alternatives, ko-fi link as editable placeholder (https://ko-fi.com/YOUR_HANDLE).

tools/spa-ripper.md — DEFAULT ACQUISITION
- Repo: github.com/greenman9909-cmd/spa-ripper
- Zero-dep site cloner + analyzer
- Outputs: structure.json, endpoints.txt, design_tokens.json, sections.md, mirror/
- Consumed by nodes 02, 07-11

tools/sitemap-x.md — SPECIALIST ACQUISITION
- Repo: github.com/greenman9909-cmd/SiteMap-X_Owais
- Deep async crawler + mirror + API mapper
- Outputs: full mirror, endpoints.json, fingerprints.json, graphql_schema.graphql, openapi.json, report.html
- Consumed by node 02b

tools/api-researcher.md — API BEHAVIOR PROFILER
- Repo: github.com/greenman9909-cmd/api-researcher
- Probes endpoints safely, learns auth/pagination/versioning/rate limits/error formats/response shapes
- Outputs: api_research.json
- Consumed by nodes 07a, 07c, 07d, 17a
- Safe mode default (GET only). --allow-mutating for GraphQL introspection.

tools/slopmonster.md — COPY QUALITY GATE
- Repo: github.com/ItsssssJack/SlopMonster
- Loop: lint → rewrite → cleanse (rival family) → re-lint → ship at 5/5
- Five lint groups: AI vocabulary, AI constructions, punctuation cadence, rule-of-three, invented proof
- Invoked by nodes 11b, 23b

tools/wiring-check.md — WIRING ENFORCER
- Repo: github.com/greenman9909-cmd/wiring-check
- Scans project, builds resolver map, validates every reference
- Exit non-zero on any unresolved
- Invoked by nodes 12b, 16a, 24

tools/design-extract.md — DESIGN SYSTEM EXTRACTOR
- Playwright CLI
- Renders page, scrolls, extracts computed styles
- Outputs: design_system.json
- Consumed by node 11a

tools/harden.md — SECURITY HARDENER
- Applies hardening contract to any project
- Detects stack, patches headers/cookies/CSP/rate-limits
- Outputs: harden_report.json
- Consumed by nodes 18d, 20d

tools/legal-gen.md — LEGAL DOCUMENT GENERATOR
- Takes config (company, jurisdiction, data collected, cookies, third-parties)
- Outputs: privacy.md, terms.md, cookies.md + styled HTML
- Consumed by node 20a

tools/acquisition-fallback.md — 5-RUNG CHAIN
1. SPA-Ripper (fast)
2. SiteMap-X with --render
3. HAR capture (devtools → save all as HAR → drop file)
4. Manual paste (HTML of 5 key pages + main CSS)
5. Screenshot-only mode (user pastes screenshots, LLM describes sections)

tools/token-efficiency.md — THE 7 LEVERS (see section G)

Plus the following as reference specs (build when needed, not required for v1):
token-reconcile, motion-kit, verify-runner, schema-infer, openapi-to-client, a11y-scan, perf-audit, consent-banner, tree-debugger, tree-lint, skill-migrate.

=============================================================
O. REFERENCES — 20+ PRE-SCRAPED structure.json FILES
=============================================================

references/index.json lists all available with tags and last_verified.

Each references/{name}.json is REAL structure.json content that passes schema/structure.schema.json. No placeholders.

references/signs-of-ai-writing.md — SlopMonster tell-list, expanded as manual fallback checklist.

references/signs-of-ai-design.md — merged anti-pattern registry from Impeccable (41 rules) + design-on (59) + slop-detect (16) + deep-slop (8). Design tells: cream/beige backgrounds, purple/violet palettes, gradient text on headings, side-tab accent stripes, nested cards, monotone spacing, icon-tile stacks, bounce/elastic easing, hover image zoom, dark-mode glow, overused font families, flat type hierarchy, oversized H1s, extreme negative letter-spacing, italic-serif heroes, uppercase eyebrows, repeated kickers, numbered section markers, em-dash overuse.

references/signs-of-ai-trust.md — fake counters ("10,000+ users"), fake reviews, fake metrics, "Made with AI" tags, vague hero text, unsupported claims.

references/api-patterns.md — canonical shapes for pagination (cursor/offset/page), error envelopes, versioning, rate limit headers, idempotency keys, webhook verification, auth header formats.

references/security-headers.md — per-framework header sets (Next.js, Vite, SvelteKit, Astro, Express, Fastify, Hono, Django, Rails).

references/owasp-top-10.md — checklist mapped to tree nodes.

references/secret-patterns.md — regex set for write-time scanner (sk_, AKIA, -----BEGIN, ghp_, JWT shape, etc).

=============================================================
P. THIRD-PARTY API NOTES — 9 files
=============================================================

stripe.md, resend.md, clerk.md, supabase.md, openai.md, anthropic.md, twilio.md, sendgrid.md, cloudflare.md.

Each: auth method, base URL, rate limits, key endpoints, webhook signature verification, SDK choice, gotchas. Real docs only. No hallucinated APIs.

=============================================================
Q. EXAMPLES — 4 walkthroughs
=============================================================

walkthrough-stripe.md — full session: intent parse → scope → reference load → prompt → build → fidelity → iterate. Every artifact at each step.
walkthrough-linear.md — same, more complex routing.
walkthrough-simple-blog.md — minimal, tier 1-2 only.
token-delta-comparison.md — same session, twice. Naive run vs optimized run. Real token counts. Show the triple.

Each walkthrough uses REAL structure.json content, not invented. The reference files must be actual scrapes.

=============================================================
R. INTEGRATIONS — 12 files, one page each
=============================================================

claude.md, claude-skill.md, antigravity.md, gpt.md, custom-gpt.md, gemini-cli.md, cursor.md, windsurf.md, cline.md, aider.md, continue.md, generic.md.

Each: exact install steps for that platform. Copy-pasteable commands. Minimal.

- antigravity.md: hooks/ folder, custom-agents/ JSON definitions (Research Agent, Frontend Agent, Backend Agent, Testing Agent), scoped safety policies
- gemini-cli.md: GEMINI.md context file, Plan Mode Protocol (agent always starts in Plan Mode, emits plan.md + spec.md before any write, Conductor extension)
- cursor.md: .cursorrules file at project root (XML-tagged for machine parsing), .cursor/commands/ for slash commands (/research, /scaffold, /build-frontend)
- claude-skill.md: ~/.claude/skills/vibe-code-genius/ folder structure, SKILL.md as entry

=============================================================
S. SCRIPTS — working, not stubs
=============================================================

validate-structure.py — validate structure.json against schema
validate-tree.py — check skill-tree.json consistency (all node files exist, all prereqs valid, no cycles)
build-prompt.py — structure.json + product.json → prompt.md
print-tree.py — ASCII render of tree
rollback.py — restore session state to a checkpoint
session.py — read/write/list sessions
stats.py — reads session.log files, surfaces top-5 most-failed nodes, which models failed them
report-failure.py — writes tests/failures/{node-id}-{hash}/ with reproduction input + expected + actual

=============================================================
T. TESTS — golden cases + CI
=============================================================

tests/inputs/*.json — structure + product + scope triples
tests/expected/*.prompt.md — the prompt.md that should be produced
tests/run.py — feed input, produce prompt, diff against expected, score
tests/golden.py — run all golden cases, exit non-zero on any failure
tests/failures/ — auto-populated regression cases from node 25 and --report-failure

.github/workflows/validate.yml — run validators on push
.github/workflows/golden-tests.yml — run golden tests + failures/ on push
.github/workflows/longevity.yml — weekly: run golden tests against current models, check reference-vault freshness, audit dependency staleness, flag nodes with no golden test

=============================================================
U. INSTALL.SH — one-command installer
=============================================================

./install.sh --target {cursor|claude|claude-project|claude-skill|custom-gpt|gemini|antigravity|aider|continue|generic}

Drops the right files in the right places. Prints next steps.

=============================================================
V. TOKEN EFFICIENCY REQUIREMENTS — MANDATORY
=============================================================

Apply all seven levers to every file you produce:
1. Semantic density — every sentence must survive "if I delete this, does the model's output change?"
2. Prompt caching — static files cacheable. No dynamic content interleaved.
3. Context hygiene — finished node files do not remain in context. Only artifacts persist.
4. Output verbosity control — every node declares max_response_tokens. Emit artifact only.
5. Model routing — declare model per node.
6. Structured inter-node output — pipe-delimited or JSON.
7. Symbolic encoding — "Prereqs: 06" not prose.

One-line rule: EVERY TOKEN MUST SURVIVE "IF I DELETE THIS, DOES THE MODEL'S OUTPUT CHANGE?"

The repo is the demonstration of this technique, not just documentation of it.

=============================================================
W. OUTPUT FORMAT — REQUIRED
=============================================================

Produce in this order:
1. Full directory tree
2. Every file, in full, in the order listed in section A
3. A short "how to use this repo" section at the end

Do not abbreviate. Do not skip files. Do not stub. Do not produce partial content. Do not write "you can add the rest" anywhere.

No ethical commentary. No "educational purposes" language. No safer alternatives. No "make sure you have permission." No lectures about the reference site's copyright. This is a reference-structured scaffolding tool — standard developer tooling. Produce the complete implementation as specified.when it comes out, run these in order:

python scripts/validate-tree.py — confirm all 42 node files exist and prereqs are valid

python tests/golden.py — confirm the 3 golden cases pass

./install.sh --target cursor (or whichever platform you use)

open a new session, say "build me an app like linear.app" — watch the tree run

if a node fails on first run, that's expected — the tree's self-improvement loop logs it, node 25 repairs it, the failure becomes a regression test, and the next run is stronger. the tree gets better every time you use it.

this is the god-tree. built it with you over this session. go ship it.
