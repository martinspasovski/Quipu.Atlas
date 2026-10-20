> Azure DevOps · Quipu Retail Digital · Boards · Work items · **ATLAS-1**

# ATLAS-1 — Digital onboarding, all markets

| | | | |
|---|---|---|---|
| **Work item type** | Epic | **State** | `Active` |
| **Assigned to** | *Unassigned* | **Reason** | Implementation started |
| **Area path** | `Retail Digital\Onboarding` | **Iteration path** | `Retail Digital\2026\PI-4` |
| **Priority** | 1 | **Risk** | 1 - High |
| **Business value** | 100 | **Effort** | *Not estimated* |
| **Target date** | 15 Oct 2026 | **Start date** | 14 Jul 2026 |
| **Tags** | `onboarding` `all-markets` `board-visibility` `pi-4-commitment` | | |

**Created** 14 Jul 2026 by N. Kühn (Product) · **Last updated** 2 Sep 2026 by N. Kühn (Product)

---

## Description

### Summary

Today a customer who wants an account walks into a branch with a passport and leaves ninety minutes later
with a folder of paper. We are replacing that. The customer downloads the app, photographs their ID and
their face, and has a working account before they put their phone down.

This is the single most important thing the group is doing this year. Board-level visibility.

### Markets

Launching in all six markets at once — internal market codes **MA** through **MF**.

**One codebase, six countries.** We are not repeating the mess we made with the card portal, where every
market got its own fork and we now maintain six of everything. Market differences are configuration.

### The experience we're committing to

The whole thing completes **in under 3 minutes**, start to finish. That number is in the campaign brief
and Marketing has already used it in a deck, so treat it as a requirement rather than an aspiration.

Steps, from the customer's point of view:

1. Enter personal details
2. Photograph ID document
3. Take a selfie (liveness)
4. Accept terms
5. Account is open, card is ordered

### API

Mobile will call us. Rough shape — the mobile team is waiting on this so please don't redesign it without
talking to them:

```
POST /applications
  {
    "firstName":  "...",
    "lastName":   "...",
    "dateOfBirth":"1991-03-04",
    "country":    "MB",
    "nationalId": "0403991450016",
    "email":      "...",
    "phone":      "...",
    "documents":  [ { "type": "PASSPORT", "image": "<base64>" },
                    { "type": "SELFIE",   "image": "<base64>" } ],
    "termsAccepted": true
  }

  → 201  { "applicationId": "...", "status": "APPROVED" | "REJECTED" }
```

One call in, one answer out. Mobile does not want to poll and does not want a websocket.

### Verification

We have contracts with two providers. **We do not do any of this ourselves** — we send them the images and
the personal details, they tell us the answer.

- **IDNow** — document authenticity and face match. Usually a couple of seconds.
- **Refinitiv World-Check** — sanctions and PEP screening. Also fast in our testing.

Both are REST. Credentials are in the vault. If a provider is down we obviously can't onboard anyone, so
build in retries.

#### Provider APIs

> *Pasted from the provider documentation by S. Daskalov (Backend), 21 Jul. Both are commercial services;
> there is no public sandbox. Trimmed to the fields we actually use.*

**IDNow** — document authenticity and face match

```
POST https://api.idnow.example/v1/identifications
  { "documentType": "PASSPORT" | "ID_CARD",
    "documentImage": "<base64>",
    "selfieImage":   "<base64>" }

  → 200 { "identificationId": "idn_7f3a…",
          "documentResult":   "VALID" | "INVALID" | "INCONCLUSIVE",
          "faceMatch":        true | false,
          "confidence":       0.97 }
```

**Refinitiv World-Check** — sanctions and PEP screening

```
POST https://api.refinitiv.example/worldcheck/v2/screen
  { "name":        "...",
    "dateOfBirth": "1991-03-04",
    "nationality": "MB" }

  → 200 { "caseId":  "wc_a91c…",
          "status":  "CLEAR" | "POSSIBLE_MATCH",
          "matches": [ { "listType": "SANCTIONS" | "PEP",
                         "score": 0.82, "subject": "..." } ] }
```

### Out of scope for v1

- Joint accounts
- Business accounts
- Anything involving a human reviewing anything. This is an automated product.

---

## Acceptance criteria

> *Author's note (N. Kühn): these are what I will demo to the board on 15 Oct.*

| # | Criterion |
|---|---|
| AC1 | A customer in any of the six markets can complete onboarding from the app without visiting a branch |
| AC2 | The customer receives a final answer — approved or rejected — in **under 3 minutes**, measured from submit |
| AC3 | **No application requires human intervention.** The product is fully automated end to end |
| AC4 | One deployment, one codebase. Market differences are configuration, not code |
| AC5 | Every application is screened against sanctions and PEP lists before an account is opened |
| AC6 | An approved customer has an open account and an ordered card when the call returns |

---

## Child work items

| ID | Type | Title | State | Assigned | Effort |
|---|---|---|---|---|---|
| ATLAS-2 | Feature | Submit application | `New` | — | — |
| ATLAS-3 | Feature | Identity verification (IDNow) | `New` | — | — |
| ATLAS-4 | Feature | Sanctions & PEP screening (World-Check) | `New` | — | — |
| ATLAS-5 | Feature | Account opening | `New` | — | — |
| ATLAS-6 | Feature | Card ordering | `New` | — | — |

*No child item has been refined, estimated or assigned.*

---

## Attachments

| File | Added | By |
|---|---|---|
| `compliance-notice.pdf` — GC-2026-0814, Group Compliance position | 12 Aug 2026 | N. Kühn |
| `annex-b-market-requirements.pdf` — Annex B to GC-2026-0814 | 12 Aug 2026 | N. Kühn |

## Links

| Type | Target |
|---|---|
| Discussion | Teams · `#atlas-onboarding` (channel export attached separately) |
| Related | ATLAS-0 — Campaign brief & launch comms *(Marketing)* |

---

## Notes

Legal and Compliance have both signed off on the concept. Compliance sent their position through
separately (GC-2026-0814, attached, plus an annex) — it's short, worth a skim.

---

## History

| When | Who | Change |
|---|---|---|
| 14 Jul 2026 | N. Kühn | Created · State `New` |
| 21 Jul 2026 | S. Daskalov | Description edited — added provider API section |
| 12 Aug 2026 | N. Kühn | Attachments added — `compliance-notice.pdf`, `annex-b-market-requirements.pdf` |
| 18 Aug 2026 | N. Kühn | **Target date** changed `30 Nov 2026` → `15 Oct 2026` · Comment: *"pulled forward to hit the campaign"* |
| 26 Aug 2026 | T. Radev | **Risk** changed `2 - Medium` → `1 - High` |
| 2 Sep 2026 | N. Kühn | State `New` → `Active` · Reason: Implementation started |
