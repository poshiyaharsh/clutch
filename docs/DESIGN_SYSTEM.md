# DESIGN_SYSTEM — Clutch Modern Product UI

## 1. Design Direction
Calm, precise, high-end product UI. Generous whitespace, quiet surfaces, one confident violet-blue accent color (`#5B4CF5` in light, `#7C6CFF` in dark), crisp typography with tabular numbers. The only expressive moment is the live leaderboard. Everything else stays restrained and distraction-free.

## 2. Tokens (defined in `wwwroot/css/clutch-theme.css`)

### Theme Synchronization Architecture
- **Single Source of Truth**: All colors and surfaces are derived strictly from CSS variables defined on `:root, [data-theme="light"]` and `[data-theme="dark"]` in `wwwroot/css/clutch-theme.css`.
- **Pre-Paint Initialization**: A lightweight `<script>` in `<head>` executes immediately before paint to read `localStorage.getItem('clutch-theme')` (falling back to `prefers-color-scheme`), applying both `data-theme` and Bootstrap 5's `data-bs-theme` to `<html>`. This eliminates flash of unstyled content (FOUC).
- **Native Widget Sync**: `color-scheme: light` and `color-scheme: dark` are set respectively so scrollbars, form controls, and browser UI elements match seamlessly.
- **Rule**: Never use hardcoded `#hex`, `rgb()`, or inline `style=""` attributes in views. All visuals must use tokens or semantic utility classes (`.cl-font-14`, `.cl-w-100`, `.cl-sticky-sidebar`, etc.).

### Typography
- **Font Family**: Inter (variable font) with features `"cv11" 1, "ss01" 1` and `tabular-nums` for all numbers, ranks, and scores.
- **Headings**: 600 weight, tracking `-0.02em`, sentence case (no all-caps).
- **Body**: 15px / 1.6 line-height.
- **Type Scale**: 11, 12, 13, 14, 15, 18, 22, 24, 32, 48px. Page titles: 32px, hero titles: 48px.

### Theme Palettes & Contrast Ratios (WCAG 2.1 AA)

| Token Role | Light Token | Light Hex | Light Contrast | Dark Token | Dark Hex | Dark Contrast | WCAG Standard |
|:---|:---|:---|:---:|:---|:---|:---:|:---:|
| **Page Canvas** | `--bg` | `#FAFAFB` | — | `--bg` | `#0B0B0F` | — | Base |
| **Card / Surface** | `--surface` | `#FFFFFF` | — | `--surface` | `#121218` | — | Base |
| **Secondary Surface** | `--surface-2` | `#F4F4F6` | — | `--surface-2` | `#1A1A22` | — | Base |
| **Borders & Dividers** | `--border` | `#E6E6EA` | 1.23:1 | `--border` | `#2A2A34` | 1.35:1 | Hairline |
| **Primary Text** | `--text` | `#14141A` | **17.47:1** | `--text` | `#F4F4F6` | **16.99:1** | $\ge 4.5:1$ (Pass) |
| **Muted / Secondary Text** | `--text-muted` | `#666672` | **4.88:1** | `--text-muted` | `#A6A6B2` | **7.75:1** | $\ge 4.5:1$ (Pass) |
| **Accent Primary** | `--accent` | `#5B4CF5` | — | `--accent` | `#7C6CFF` | — | Brand |
| **Accent Hover** | `--accent-hover` | `#4A3BE0` | — | `--accent-hover` | `#9A8CFF` | — | Interactive |
| **Accent Soft Tint** | `--accent-soft` | `#EEEBFF` | — | `--accent-soft` | `#1E1A44` | — | Tint |
| **Accent Text on Tint** | `--accent-text` | `#4335D6` | **5.09:1** | `--accent-text` | `#9A8CFF` | **5.88:1** | $\ge 4.5:1$ (Pass) |
| **Accent Button Text** | `--accent-contrast`| `#FFFFFF` | **5.53:1** | `--accent-contrast`| `#0B0B0F` | **5.09:1** | $\ge 4.5:1$ (Pass) |
| **Success Badge Text** | `--status-success-text` | `#0C6E36` | **5.87:1** | `--status-success-text` | `#4ADE80` | **7.37:1** | $\ge 4.5:1$ (Pass) |
| **Warning Badge Text** | `--status-warning-text` | `#8C5400` | **6.13:1** | `--status-warning-text` | `#FCD34D` | **9.61:1** | $\ge 4.5:1$ (Pass) |
| **Danger Badge Text** | `--status-danger-text` | `#B02026` | **6.14:1** | `--status-danger-text` | `#FCA5A5` | **6.84:1** | $\ge 4.5:1$ (Pass) |
| **Info Badge Text** | `--status-info-text` | `#1B57A6` | **6.27:1** | `--status-info-text` | `#7DD3FC` | **8.44:1** | $\ge 4.5:1$ (Pass) |

### Bootstrap 5 Variable Overrides
Bootstrap variables are mapped directly to theme tokens on `:root` and `[data-theme]` blocks:
- `--bs-body-color: var(--text)`
- `--bs-body-bg: var(--bg)`
- `--bs-emphasis-color: var(--text)`
- `--bs-secondary-color: var(--text-muted)`
- `--bs-border-color: var(--border)`
- `--bs-link-color: var(--accent)`
- `--bs-link-hover-color: var(--accent-hover)`
- `--bs-heading-color: var(--text)`
- Tables: `--bs-table-bg: var(--surface)`, `--bs-table-color: var(--text)`, `--bs-table-hover-bg: var(--surface-2)`
- Dropdowns: `--bs-dropdown-bg: var(--surface)`, `--bs-dropdown-color: var(--text)`, `--bs-dropdown-border-color: var(--border)`
- Modals & Offcanvas: `--bs-modal-bg: var(--surface)`, `--bs-modal-color: var(--text)`, `--bs-modal-border-color: var(--border)`
- Form Controls: `.form-control`, `.form-select` styled with `var(--surface)`, `var(--text)`, `var(--border)`, and `var(--accent)` focus ring.
- In dark mode, `.btn-close` uses `filter: invert(1) grayscale(100%)`.

### Geometry & Depth
- **Radii**: Controls & inputs `8px`, cards & modals `12px`, badges & avatars `999px` (pills).
- **Borders**: 1px solid `var(--border)`. No thick outlines or cartoon borders.
- **Shadows**: Soft, layered elevation (`0 1px 2px rgba(0,0,0,0.04)`, `0 4px 16px rgba(0,0,0,0.06)`).
- **Spacing Scale**: 4, 8, 12, 16, 24, 32, 48, 64, 96px. Max content container: 1200px.
- **Motion**: 150–220ms ease-out transitions only. Respects `prefers-reduced-motion`.

## 3. Brand Logo System
- **Mark**: Rounded square (radius ~28%) filled with accent gradient (`#5B4CF5` $\to$ `#7C6CFF` in light, `#7C6CFF` $\to$ `#9A8CFF` in dark) containing a white bold geometric 'C' and center dot.
- **Wordmark**: "Clutch" in Inter 700 with `fill="currentColor"` so it follows the theme text color automatically.
- **Component**: Rendered via `_Logo.cshtml` with `size` (`sm`, `md`, `lg`) and `variant` (`full`, `mark-only`).
- Detailed brand specification in `docs/BRAND.md`.

## 4. Icons
- Standardized Lucide icons embedded as an inline SVG sprite in `wwwroot/img/icons.svg`.
- Zero emoji anywhere in the UI.

## 5. Components
- **Button**: `.cl-btn` (`--primary`, `--secondary`, `--ghost`, `--danger`, sizes `--sm`, `--lg`, `--icon-only`).
- **Badge**: `.cl-badge` (`--success`, `--warning`, `--danger`, `--accent`).
- **Card**: `.cl-card` (`--interactive`, `--soft`, `--accent`).
- **Avatar**: `.cl-avatar` with uppercase initials, `.cl-avatar-group` for stacked team displays.
- **Stat Tile**: `.cl-stat-tile` with value and `.cl-stat-delta` (`--up`, `--down`, `--neutral`).
- **Command Menu**: Ctrl+K / Cmd+K quick switcher backdrop and search dialog.
- **Leaderboard**: Hero ranked list with pulsing live dot, count-up animation, and FLIP row rearrangement.
- **Toast**: `.cl-toast-box` polite non-blocking status notification.
