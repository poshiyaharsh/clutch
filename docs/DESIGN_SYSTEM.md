# DESIGN_SYSTEM — Clutch

## Concept
A sticker-pack scoreboard. The site feels like a hackathon wall: chunky outlines, hard offset shadows, bright sticker badges, loud type. The ONE memorable thing is the **live leaderboard**: ranks are tilted stickers, scores tick up when judges save, top 3 get colored medals. Everything else stays calm so that moment lands.

## Tokens (see design/theme.css)
| Name | Hex | Use |
|---|---|---|
| Paper | #F2EDFF | Page background (lavender white) |
| Ink | #1A1033 | Text, borders, shadows (deep violet, not neutral black) |
| Electric | #4B4BFF | Primary actions, links |
| Bubblegum | #FF5CA8 | Highlights, rank 3, live indicator |
| Lemon | #FFD83D | Rank 1, warnings, "new" stickers |
| Mint | #27E0A3 | Success, rank 2, "submitted" |
Dark mode: Paper -> #14102B, Ink -> #F2EDFF, accents slightly lighter. Handled by tokens only.

## Type
- Display: **Bricolage Grotesque** (700/800), tight tracking, used big for page titles, ranks and scores.
- Body/UI: **Figtree** (400/500/700).
- Scale (rem): 0.8125, 1, 1.125, 1.5, 2, 3, 4.5. Body line-height 1.55, max line 70ch.
- Sentence case everywhere. No all-caps labels, no tracked-out eyebrows.

## Shape and depth
- Borders: 2.5px solid Ink on interactive and card surfaces.
- Shadow: hard offset `4px 4px 0 Ink` (no blur). Pressed buttons move 2px and shrink the shadow.
- Radii by role: inputs 8px, cards 16px, buttons and badges 999px, stickers rotate -3deg to 3deg.
- Spacing scale: 4, 8, 12, 16, 24, 32, 48, 72.

## Motion (sparing)
- One orchestrated moment: leaderboard score tick + row reorder on live update (300–500ms).
- Responses to action only: button press, toast in, drawer open.
- No scroll-triggered fades on every section. Respect `prefers-reduced-motion` (swap to instant).

## Components
Button (primary/secondary/ghost/danger), Input/Select/Textarea, Card, Sticker badge (status), Rank sticker, Leaderboard row, Toast, Empty state, Notification bell + drawer, Stepper (only for real sequences: create event steps), Table (admin), Tabs, Modal.

## Screens (wireframes)
**Home**
```
[Clutch]                        [Events] [Sign in]
BIG LINE: Build it. Ship it. Get scored.
[Find a hackathon]   <- primary     [Host one] <- secondary
Live now: stack of event cards, each with a status sticker
```
**Event detail**
```
Banner + title + status sticker (Open / Closing in 2d / Judging)
Left: about, tracks, rubric summary, schedule   Right: sticky card
Right card: deadline countdown, team size, [Create team] [Join with code]
```
**Leaderboard (hero screen)**
```
Event title            (o) Live
#1 sticker  Team name   track chip        92.4 (ticks up)
#2 sticker  ...                           88.1
#3 sticker  ...                           85.7
Rows 4+ : smaller, same structure. "Pending" chip if not fully judged.
```
**Judge scoring**
```
Team header (name, track, links)  |  Progress: 3 of 8 teams scored
One card per criterion: slider + number input, 1-line comment
[Save scores] sticky bottom bar on mobile
```
**Organizer dashboard**: stat tiles (registrations, submissions, judged %), tabs: Overview / Teams / Judges / Rubric / Announcements.

## Copy rules
- Buttons say what happens: "Create team", "Save scores", "Publish event". Toast repeats it: "Team created".
- Errors: what went wrong + fix. "That team name is taken in this event. Try another."
- Empty states invite: "No teams yet. Create the first one."
- No apologizing in errors, no filler, no emoji spam (max one per empty state).

## Quality floor
375px first, 44px touch targets, 4.5:1 contrast, visible focus ring (3px Electric outline + offset), form labels always visible, skeleton or spinner under 200ms rule, no layout shift on live updates.
