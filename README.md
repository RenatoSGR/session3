# Session 3 — .NET App Modernization with GitHub Copilot

> **Duration:** ~30 minutes  
> **Goal:** Modernize a legacy .NET Framework application using GitHub Copilot. Pick **one** of the four approaches below (or try multiple if time permits).

---

## Prerequisites

- Visual Studio Code with [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) extension
- .NET SDK 8.0+ installed
- Git configured and authenticated with GitHub
- The sample project cloned locally (see `dotnet-migration-copilot-samples/` in this repo)

---

## Option A — VS Code Extension (Guided Wizard)

Use the built-in **GitHub Copilot App Modernization** extension to walk through the upgrade step by step.

📖 **Docs:** [Quickstart: Modernize a .NET app with GitHub Copilot](https://learn.microsoft.com/en-us/dotnet/azure/migration/appmod/quickstart?toc=%2Fdotnet%2Fcore%2Fporting%2Fgithub-copilot-app-modernization%2Ftoc.json&bc=%2Fdotnet%2Fbreadcrumb%2Ftoc.json&pivots=vscode)

### Steps

1. Open the sample project in VS Code.
2. Open the Command Palette (`Ctrl+Shift+P`) and run **GitHub Copilot: Modernize .NET Application**.
3. Follow the wizard — it will:
   - Analyze the project structure and dependencies.
   - Generate an upgrade plan.
   - Apply changes incrementally (project file, NuGet packages, code fixes).
4. Build the project (`dotnet build`) and fix any remaining issues with Copilot Chat.
5. Run and validate the application.

> **Hint:** If the wizard stalls on a step, open Copilot Chat and ask _"Why is the build failing after the migration?"_ — it usually spots the issue quickly.

---

## Option B — Custom Agents & Skills (awesome-copilot)

Leverage pre-built agents and skills from the **awesome-copilot** catalog to drive the modernization conversationally.

📖 **Catalog:** [awesome-copilot.github.com](https://awesome-copilot.github.com/)

### Steps

1. Browse the catalog and install the **modernize-dotnet** agent/skill if not already available.
2. Open Copilot Chat and invoke the agent:
   ```
   @modernize-dotnet Analyze this .NET Framework project and create an upgrade plan to .NET 8
   ```
3. Review the generated plan — the agent will list projects, dependencies, and breaking changes.
4. Ask the agent to execute the plan step by step:
   ```
   @modernize-dotnet Execute step 1 of the upgrade plan
   ```
5. After each step, build (`dotnet build`) and let the agent fix errors.
6. Repeat until all steps are complete and the app runs on .NET 8.

> **Hint:** You can combine agents — use `@modernize-dotnet` for the heavy lifting and regular Copilot Chat for quick code fixes.

---

## Option C — Plan Agent + Custom Agents (Two-Phase Approach)

Use the **Plan agent** first to generate a comprehensive migration plan, then hand off execution to specialized agents.

### Steps

1. Open Copilot Chat and select the **Plan** agent from the agent picker (dropdown at the top of the chat panel).
2. Ask it to assess the project:
   ```
   Create a detailed migration plan to upgrade this .NET Framework app to .NET 8. 
   Include: project files, NuGet packages, API changes, and test strategy.
   ```
3. Review and refine the plan — ask follow-up questions if needed.
4. Once satisfied, switch to a custom agent (e.g., **modernize-dotnet**) to execute:
   ```
   Execute this migration plan: [paste or reference the plan]
   ```
5. Work through each phase: project file conversion → package updates → code changes → build fixes.
6. Build and test after each phase.

> **Hint:** The Plan agent excels at identifying risks and ordering tasks. Let it think before you act — it often catches dependency conflicts that manual approaches miss.

---

## Option D — Spec-to-Cloud with Custom Skills (Full Pipeline)

Follow the end-to-end **Spec2Cloud** approach: from legacy .NET 4.x to .NET 10 + Azure deployment, using the Contoso University sample. This option leverages **custom Copilot skills** (instruction files) that act as domain-specific agents to guide each phase of the migration.

📖 **Contoso University Lab:** [https://github.com/EmeaAppGbb/AppModLab-dotnet-4to10-contosouniversity-spec2cloud](https://github.com/EmeaAppGbb/AppModLab-dotnet-4to10-contosouniversity-spec2cloud)  
📖 **Spec2Cloud Skills:** [https://github.com/EmeaAppGbb/spec2cloud/tree/vNext/.github/skills](https://github.com/EmeaAppGbb/spec2cloud/tree/vNext/.github/skills)

### What Are Custom Skills?

Skills are `.md` instruction files placed in your repo (e.g., `.github/skills/`) that GitHub Copilot loads as context. Each skill encodes expert knowledge for a specific task — assessment, migration, deployment, etc. — effectively turning Copilot into a specialized agent for that phase.

### Steps

1. Clone the Contoso University lab repo and explore its structure.
2. Browse the **Spec2Cloud skills** repo linked above — each skill file targets a phase of the migration pipeline:
   - **Assessment skill** — Instructs Copilot to analyze the legacy codebase, identify frameworks, dependencies, and blockers.
   - **Specification skill** — Guides Copilot to produce a structured modernization spec from the assessment output.
   - **Migration skill** — Drives Copilot through the actual code conversion (project files, NuGet, API replacements).
   - **Cloud deployment skill** — Helps Copilot generate Azure infrastructure and deployment artifacts.
3. Copy the relevant skill files into your project's `.github/skills/` folder (or reference them via `instructions` in your Copilot config).
4. Open Copilot Chat and work through each phase — Copilot will automatically pick up the skill context:
   ```
   Assess this .NET Framework project for migration to .NET 10
   ```
5. After assessment, move to the next phase:
   ```
   Generate a modernization specification based on the assessment
   ```
6. Execute the migration:
   ```
   Apply the migration spec to convert this project to .NET 10
   ```
7. Build, test, and iterate: `dotnet build` → fix with Copilot → repeat.
8. Optionally, use the deployment skill to push to Azure.

> **Hint:** The power of this approach is that skills make Copilot **repeatable and consistent** across projects. Once you have good skill files, every migration follows the same proven playbook.  
> **Hint:** You can combine skills — load both the assessment and migration skills at once so Copilot has full context of the pipeline.

---

## Tips for the 30-Minute Session

| Time | Activity |
|------|----------|
| 0–5 min | Pick your option, clone/open the project, verify prerequisites |
| 5–10 min | Run the initial assessment or plan generation |
| 10–25 min | Execute the migration (build → fix → repeat cycle) |
| 25–30 min | Final build, run the app, review what changed |

### General Hints

- **Don't fight Copilot** — if a suggestion looks wrong, ask _"Why did you make this change?"_ before reverting.
- **Build often** — run `dotnet build` after every major change. Small feedback loops are faster than big-bang migrations.
- **Use chat for debugging** — paste build errors directly into Copilot Chat: _"Fix this error: [error message]"_.
- **Check NuGet packages** — most migration issues come from packages that don't support the target framework. Ask Copilot: _"What's the .NET 8 equivalent of [package]?"_.
- **Commit between steps** — so you can rollback if something goes sideways: `git add -A && git commit -m "step X done"`.

### Advanced: Scaling with GitHub CLI & Orchestration

- **Fleet mode (`gh copilot`)** — Use the GitHub CLI to run Copilot in **fleet mode** across multiple repos or projects at once. Great for batch-migrating a portfolio of .NET apps:
  ```bash
  gh copilot # Start an interactive Copilot session in your terminal
  ```
  Fleet mode lets you script and parallelize migration tasks — ideal when you have 5, 10, or 50 projects to modernize.

- **Orchestrator agent** — Build or use an **orchestrator agent** that coordinates multiple Copilot agents in sequence. For example, an orchestrator can:
  1. Run the assessment agent on every project in a solution.
  2. Feed results into the migration agent.
  3. Trigger build validation automatically.
  4. Report a summary of what succeeded and what needs manual attention.

- **Generate agents & skills with `gh copilot`** — You can use GitHub CLI to scaffold custom agents and skills for your specific migration needs:
  ```bash
  gh copilot generate --agent "dotnet-migration-validator" --description "Validates .NET migration output"
  gh copilot generate --skill "check-ef-core-compat" --description "Checks Entity Framework Core compatibility"
  ```
  This lets you encode your team's migration playbook as reusable, shareable Copilot extensions.

> **Hint:** Combining fleet mode + custom skills is the fastest path for organizations migrating many apps. Write the skills once, run them everywhere.
