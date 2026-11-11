# BRAND — Clutch Product Identity & Brand Guide

## 1. Brand Concept & Philosophy
Clutch represents high performance, focus, and coming through when it matters most: delivering under tight deadlines, building production software, and competing on a live rubric-evaluated leaderboard.

The identity is calm, precise, and architectural. It avoids cartoon decorations, stickers, or playful tilts in favor of quiet surfaces, balanced proportions, and a confident violet-blue accent.

---

## 2. The Mark
The Clutch mark is an engineered glyph combining precision and motion.

- **Geometry**: A squircle container with ~28% corner radius (`rx: 28%`).
- **Internal Glyph**:
  - A bold geometric arc **"C"** (`stroke-width: ~12%` of container width, round caps).
  - A centered circular **dot** positioned inside the right opening of the "C".
- **Color Gradients**:
  - **Light Theme**: Linear gradient from `--accent` (`#5B4CF5`) to `--accent-hover` (`#4A3BE0`) at $135^\circ$.
  - **Dark Theme**: Linear gradient from `--accent` (`#7C6CFF`) to `--accent-hover` (`#9A8CFF`) at $135^\circ$.
  - **Internal Glyph**: Pure white (`#FFFFFF`) with 100% opacity.
  - **Monochrome Variant**: Solid `currentColor` for single-color print or embedded monochrome contexts.
- **Minimum Size**:
  - Digital screens: `24px × 24px` minimum (`16px` only in favicon).
  - Print / merchandise: `10mm × 10mm` minimum.
- **Clear Space**: Half the width of the mark ($0.5 \times X$, where $X$ is the mark dimension) on all four sides. No typography, icons, or borders may enter this exclusion zone.

---

## 3. The Wordmark
The wordmark accompanies the mark in public navigation, marketing headers, and authentication portals.

- **Typeface**: Inter, bold weight (`700`), sentence case ("Clutch", never "CLUTCH" or "clutch").
- **Letter Spacing**: `-0.03em` optical kerning.
- **Color**: `fill="currentColor"`. It dynamically tracks the parent text token (`var(--text)`), guaranteeing contrast across light mode (`#14141A`) and dark mode (`#F4F4F6`).
- **Proportions**:
  - `sm`: 24px mark height, 16px text size, 8px mark-to-text gap. Total width: 96px.
  - `md` (standard): 32px mark height, 22px text size, 10px mark-to-text gap. Total width: 128px.
  - `lg`: 44px mark height, 28px text size, 14px mark-to-text gap. Total width: 172px.

---

## 4. Logo Usage Guidelines

### Correct Usage
- Render through the Razor partial `<partial name="_Logo" model='("md", "full")' />` in views to ensure inline SVG currentColor tracking and gradient definitions.
- Use `variant="mark-only"` for collapsed sidebars, browser favicons, avatars, and small badges.
- Use `variant="full"` for navbar headers, auth hero cards, certificates, and footers.
- Always provide clear space around the logo.

### Prohibited Usage
- **Do not stretch or skew**: Maintain 1:1 aspect ratio on the mark.
- **Do not rotate**: The mark must always be rendered upright without tilts.
- **Do not outline**: Never add hard black or colored borders around the mark.
- **Do not alter colors**: Do not apply arbitrary colors, neon glow, or dropshadows to the glyph.
- **Do not place on busy backgrounds**: Always position over clean canvas (`--bg`, `--surface`, or `--surface-2`).
- **Do not use emoji**: Do not pair with emojis or replacement symbols.

---

## 5. Asset Suite Inventory (`wwwroot/img/brand/`)

| File | Purpose | Dimensions / Format |
|---|---|---|
| `clutch-mark.svg` | Primary standalone brand mark | Vector SVG with gradient |
| `clutch-mark-mono.svg` | Monochrome mark using `currentColor` | Vector SVG |
| `clutch-wordmark.svg` | Full wordmark (mark + text) | Vector SVG |
| `favicon.svg` | Scalable vector favicon for modern browsers | Vector SVG |
| `favicon-32.png` | Fallback browser tab favicon | 32 × 32 PNG |
| `apple-touch-icon.png` | iOS Home Screen icon | 180 × 180 PNG |
| `icon-192.png` | PWA standard app launcher icon | 192 × 192 PNG |
| `icon-512.png` | PWA high-res app splash icon | 512 × 512 PNG |
| `og-image.png` | Social sharing image (Open Graph / Twitter) | 1200 × 630 PNG |
| `site.webmanifest` | PWA web app manifest specifying icons and theme color | JSON Manifest |

---

## 6. How to Use `_Logo.cshtml` in Razor Views

The `_Logo.cshtml` partial accepts a tuple `(string Size, string Variant)`:

```razor
@* Default standard navbar wordmark *@
<partial name="_Logo" model='("md", "full")' />

@* Small mark-only for collapsed sidebars or table icons *@
<partial name="_Logo" model='("sm", "mark-only")' />

@* Large full wordmark for Auth login/register pages and certificates *@
<partial name="_Logo" model='("lg", "full")' />
```

Because the SVG wordmark uses `fill="currentColor"`, wrap it in an `<a>` or `<div>` with `.text-body` or styling to have it inherit text colors cleanly.
