# Technical Task — Atlas Onboarding

**Role:** Backend / .NET microservices
**Effort:** 3 days.
**Delivery:** a public GitHub repository — send us the link when you are done. If you would rather not have
your work public, say so and we will take a private repo with us invited instead.

> ### ⚠️ Use of AI tools
>
> **You are encouraged to use AI tools (e.g. Claude, Copilot, ChatGPT) throughout the whole development process: design, coding, tests, documentation and the written material we ask for below.**
>
> **However, you must be able to explain every part of your solution yourself: the chosen approach, the service boundaries, the data model, the failure handling and every other implementation decision. You must also be able to say which alternatives you considered and why you rejected them.**
>
> **The follow-up interview will focus on these explanations, and part of it is spent in your own repository with your hands on the keyboard (see *After you submit* below). A solution that you cannot explain will not be accepted, however good the code is.**

---

## Context

**Atlas** is a digital customer-onboarding platform for a banking group operating in six countries. A
prospective customer downloads the app, submits identity documents, passes verification and sanctions
screening, and ends up with an activated account.

None of it is built yet. What exists is the material in `/requirements/` — and that material is where this
task really lives.

---

## What is in the box

```
/requirements/
  ticket-ATLAS-1.md                  The epic, written by our product manager
  teams-01.png … teams-03.png        #atlas-onboarding, six weeks of discussion
  compliance-notice.pdf              Group Compliance, position GC-2026-0814
  annex-b-market-requirements.pdf    Annex B to the above
  core-banking-accounts-integration.pdf
                                     The core banking team's integration page for
                                     the ACCOUNTS service — the system that actually
                                     opens the account
  customer-identifier-formats.pdf    Group Data Office reference sheet — identifier
                                     formats and check algorithms per market

/platform/
  docker-compose.yml                 SQL Server and Seq — so you don't spend
                                     your first hour on docker pull
```

Four documents, about twenty-five minutes of reading. **This is not a specification** — it is the material
as it stands. Parts of it are vague, and parts assume knowledge you do not have.

Read all of it before you start. The requirements documents are not preamble to the task — working out what
Atlas should actually be is most of the task.

`/platform/` is infrastructure only. There are no application services in it and no project structure,
because what services exist and where the boundaries fall is yours to decide. Extend it, replace it, or
ignore it — just tell us in your README what we should run.

---

## What we would like you to deliver

**Four things. The first is code, and it is where most of your time should go.**

If you think something in the requirements should not be built as described, do not build it as described.
Tell us what you did instead.

### 1. The code

Build the path from `POST /applications` to an answer: the request comes in, both providers are called,
their results are brought together, and the caller gets a response. **That path is mandatory**, and it has
to genuinely work end to end against the infrastructure in `/platform/`.

It must handle these three situations correctly — we will test each of them:

- a provider is slow to respond,
- a provider returns a possible match instead of a clean result,
- the same application is submitted twice.

Anything beyond that path — account opening, card ordering, the other markets, whatever else you find in
the requirements — is yours to include or leave out. Leaving things out is fine. Say what you left out, and
why.

Two hard requirements on how it is built:

- **Integration tests that run against the real SQL Server from `/platform/`** (and the broker, if you use
  one) — not in-memory substitutes.
- **EF Core migrations**, not `EnsureCreated()`.

> **We are not looking for volume.** A narrow path built properly beats a broad one built thinly. One
> integration test that reproduces a failure you are genuinely worried about is worth more than forty that
> check a mapper maps.

### 2. The design note

**Two pages at most.** What you built and what you deliberately did not. The order you would do the rest
in, and why that order. For each of the two or three decisions you consider structural, what it would cost
to reverse once there is production data behind it.

And one honest line: which part of this would you least want someone else to change as it stands?

### 3. Notes for QA

QA have an automated API test suite and will write the tests for your service from what you give them.
They have not read the requirements as closely as you have. **Two pages at most.**

- **The scenarios you want covered**, starting with the ones you would be most uncomfortable shipping
  untested. For each: the starting state, the request or sequence of requests, and what must be true
  afterwards — the response, the stored state, and anything that should or should not have happened as a
  side effect.
- **How to drive your service into each scenario** — in particular, how to make the provider stand-ins
  return each of their possible responses, and how to set up data for a specific market.
- **What cannot be tested through the API**, and how you would test it instead.

### 4. The API contract

Whatever you want mobile to build against — OpenAPI, or just the shapes written down. Plus the note you
would send the mobile team, who are waiting on it and were told it would not change.

---

## What you are *not* being asked to build

The requirements describe a product. You are building a backend, in three days, on your own. So, to save
you worrying about it:

- **You are not implementing identity verification.** Document authenticity, OCR, face matching and
  liveness detection are what IDNow is paid for. Same for sanctions screening and World-Check. Your job is
  to decide when to call them, what to do with the answer, and what happens when the answer is slow,
  unhelpful or doesn't arrive.
- **You will not have provider credentials, and there is no sandbox.** Both are commercial services behind
  signed contracts. Stand in for them however suits you — in-process, a local stub service, whatever. What
  we will look at is whether we can drive the three cases in (1) through your stand-in without editing
  your code.
- **No UI, no mobile app.** Assume the mobile team exists and will call you.
- **No real core-banking or card integration.** If your design opens an account or orders a card, a
  plausible seam is enough.

Everything else — what the services are, where the boundaries fall, what the API ends up looking like,
which parts of the product you build at all — is yours to decide. That part is deliberate.

## Technical expectations

- **.NET 8, 9 or 10.** We run .NET 8 in production, so it is the closest match and the safest default —
  but any of the three is fine and we can build and run all of them. Nothing older, please.
- **A service-oriented backend.** How many services, and where the boundaries fall, is one of the decisions
  we are interested in.
- **It has to run**, in a few minutes, from your README. If we have to guess at a manual step, we will get
  it wrong.

## How we will run it

**1. Your services run on the host**, started with `dotnet run` / `dotnet test` or a script you provide —
not built into containers by us. You are welcome to include Dockerfiles and we will read them as part of
your design.

**2. Infrastructure runs in Docker, from this list only:**

| Image | |
|---|---|
| `mcr.microsoft.com/mssql/server:2022-latest` | running in `/platform` |
| `datalust/seq:latest` | running in `/platform` |
| `redis:7-alpine` | if you want a cache |
| `rabbitmq:3-management` | if your design calls for a broker |
| `mcr.microsoft.com/azure-storage/azurite:latest` | if your design calls for blob storage |

Add whichever of the last three you need to `/platform/docker-compose.yml`. If your design genuinely needs
something not on this list, use it and say so plainly in your README — we will pull the image before we
review. Don't let it block you, and don't wait on us.

---

## What to send us

1. **The repository.** Real commit history, please — no squashing. We read it. It should contain your work;
   there is no need to commit the material we sent you.
2. **A README** covering how to run it, what you built, and what you left out.
3. **The three documents above** — design note, notes for QA, contract. Anywhere in the repo;
   tell us where in `WHERE-TO-LOOK.md`.
4. **Your reasoning.** Alongside the code, tell us what you decided and why: what you found unclear, the
   calls you made, and the alternatives you rejected.

   Format, length and structure are entirely up to you — a markdown file, comments in the README, notes in
   the commits, whatever fits how you work. We care about the thinking, not the packaging.
5. **Your open questions**, if you have any.
6. **Known limitations.** What does not work, what you did not get to, what you would do next.
7. **`WHERE-TO-LOOK.md`** — half a page at most. The three or four **things** you most want us to read —
   a file, the design note, the QA notes, whichever — the one decision you most want to be asked about, and
   anything you would rather we ignored. We read this first.

---

## After you submit

We will read everything, run your code, and then book **90 minutes with you**.

Part of that session is spent in your own repository, screen shared, making a change we have not told you
about in advance. Bring the machine you built it on and whatever tools you used to build it — including
your agent, if you used one. We are not testing whether you can work without it.

We mention this now so you can take it into account while you work. A codebase you can move around in
quickly in three weeks' time is worth more here than one more feature.

---

## Questions

We don't answer questions about the requirements while the exercise is running — working out what Atlas
should be from this material is the exercise. If you have questions, document them and submit them with
the task.

If something practical blocks you — a file won't open, the delivery mechanics are unclear — email us and
we'll sort it out straight away.
