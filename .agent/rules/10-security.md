# Security rules
- Every POST/PUT/DELETE: [ValidateAntiForgeryToken] + authorization attribute.
- Check ownership/assignment on every resource id (no IDOR): organizer owns hackathon, judge is assigned, user is team member.
- Validate on the server even if the client validates.
- Never log or commit secrets. Use user-secrets locally, env vars in Docker.
- Passwords: Identity defaults (PBKDF2), lockout enabled, rate limit login/register.
- Add security headers (CSP, X-Content-Type-Options, Referrer-Policy, frame protection).
- Sanitize any user-provided URLs (only http/https).
