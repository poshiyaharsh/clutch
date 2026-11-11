# DESIGN_SYSTEM — Clutch Modern Product UI

## 1. Design Direction
Calm, precise, high-end product UI. Generous whitespace, quiet surfaces, one confident violet-blue accent color (`#5B4CF5`), crisp typography with tabular numbers. The only expressive moment is the live leaderboard. Everything else stays restrained and distraction-free.

## 2. Tokens (defined in `wwwroot/css/clutch-theme.css`)

### Typography
- **Font Family**: Inter (variable font) with features `"cv11" 1, "ss01" 1` and `tabular-nums` for all numbers, ranks, and scores.
- **Headings**: 600 weight, tracking `-0.02em`, sentence case (no all-caps).
- **Body**: 15px / 1.6 line-height.
- **Type Scale**: 12, 13, 14, 15, 18, 24, 32, 48px. Page titles: 32px, hero titles: 48px.

### Neutrals (Light Mode)
| Token | Hex | Role |
|---|---|---|
| `--bg` | `#FAFAFB` | Canvas / page background |
| `--surface` | `#FFFFFF` | Cards, navbar, modal surfaces |
| `--surface-2` | `#F4F4F6` | Secondary wells, hover states, table headers |
| `--border` | `#E6E6EA` | 1px clean hairline borders |
| `--text` | `#14141A` | High-contrast primary copy |
| `--text-muted` | `#6B6B76` | Secondary labels, descriptions |
| `--text-subtle` | `#8E8E99` | Placeholders, disabled text |

### Neutrals (Dark Mode)
| Token | Hex | Role |
|---|---|---|
| `--bg` | `#0B0B0F` | Deep dark page canvas |
| `--surface` | `#121218` | Elevated card surfaces |
| `--surface-2` | `#1A1A22` | Secondary containers |
| `--border` | `#26262F` | Subtle dark borders |
| `--text` | `#F4F4F6` | Primary crisp text |
| `--text-muted` | `#9A9AA6` | Secondary body text |

### Brand & Status Colors
| Token | Light | Dark | Purpose |
|---|---|---|---|
| `--accent` | `#5B4CF5` | `#5B4CF5` | Primary violet-blue brand accent |
| `--accent-hover` | `#4A3BE0` | `#7367F7` | Interactive hover states |
| `--accent-soft` | `#EEEBFF` | `#1E1A44` | Soft tint backgrounds & top-3 highlight |
| `--success` | `#12A150` | `#12A150` | Open registration, submitted projects |
| `--warning` | `#E5A00D` | `#E5A00D` | Judging phases, pending scores |
| `--danger` | `#E5484D` | `#E5484D` | Destructive actions, closed events, errors |
| `--info` | `#2F80ED` | `#2F80ED` | Announcements, informational callouts |

### Geometry & Depth
- **Radii**: Controls & inputs `8px`, cards & modals `12px`, badges & avatars `999px` (pills).
- **Borders**: 1px solid `var(--border)`. No thick outlines or cartoon borders.
- **Shadows**: Soft, multi-layered elevation (e.g. `0 1px 2px rgba(0,0,0,0.04)`, `0 4px 16px rgba(0,0,0,0.06)`).
- **Spacing Scale**: 4, 8, 12, 16, 24, 32, 48, 64, 96px. Max content container: 1200px.
- **Motion**: 150–220ms ease-out transitions only. Respects `prefers-reduced-motion`.

## 3. Icons
- Standardized Lucide icons embedded as an inline SVG sprite in `wwwroot/img/icons.svg`.
- Zero emoji anywhere in the UI.

## 4. Components
- **Button**: `.cl-btn` (`--primary`, `--secondary`, `--ghost`, `--danger`, sizes `--sm`, `--lg`, `--icon-only`).
- **Badge**: `.cl-badge` (`--success`, `--warning`, `--danger`, `--accent`).
- **Card**: `.cl-card` (`--interactive`, `--soft`, `--accent`).
- **Avatar**: `.cl-avatar` with uppercase initials, `.cl-avatar-group` for stacked team displays.
- **Stat Tile**: `.cl-stat-tile` with value and `.cl-stat-delta` (`--up`, `--down`, `--neutral`).
- **Command Menu**: Ctrl+K / Cmd+K quick switcher backdrop and search dialog.
- **Leaderboard**: Hero ranked list with pulsing live dot, count-up animation, and FLIP row rearrangement.
- **Toast**: `.cl-toast-box` polite non-blocking status notification.

## 5. Screen Specifications
1. **Home**: Hero with headline, 2 CTA buttons, subtle radial accent tint gradient, live now strip, event cards with search and filter, 3-step "How Clutch works" section, and clean footer.
2. **Event Detail**: Status badges, key facts strip (duration, team size, prize pool, location), tabs (Overview, Tracks, Rubric, Schedule, Announcements), sticky registration countdown card.
3. **Team Management**: Team header, member roster with initials avatars and lead role badge, 8-character join code card with copy action, submission card with live demo and repository links.
4. **Judge Scoring**: Split layout with project repository and demo preview on the left; criteria cards with range sliders, number outputs, feedback inputs, live weighted total calculation, and sticky save bar on the right.
5. **Leaderboard**: Real-time scoreboard with rank numbers, team initials avatars, track badges, delta movement indicators, top-3 restrained accent-soft highlights, pulsing live indicator, and SignalR live update integration.
6. **Organizer Dashboard**: Split sidebar layout, 4 stat tiles with deltas, pure inline SVG registration velocity sparkline, tabbed navigation, and data tables with search and CSV export actions.
7. **Authentication**: Centered cards with clean input fields, password show/hide eye toggles, clear validation feedback, and links to registration/login.
8. **Error & System Pages**: Friendly, helpful 403, 404, and 500 error cards with clear actions to return home.
