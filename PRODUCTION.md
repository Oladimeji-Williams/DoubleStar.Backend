## DoubleStar.Backend, before going live
- [ ] Set real `Admin__Email` / `Admin__Password` — never the defaults.
- [ ] Generate a fresh `Jwt__SecretKey` — never reuse a development one.
- [ ] Set `Cors__AllowedOrigins__0` to the real frontend domain.
- [ ] Point `ConnectionStrings__DefaultConnection` at production Postgres, its own credentials.
- [ ] `ASPNETCORE_ENVIRONMENT=Production`.
- [ ] Terminate TLS in front of the API (reverse proxy/load balancer).
- [ ] Live keys for `Email__ApiKey`, `Sms__ApiKey`, `Paystack__SecretKey` — not test keys.
- [ ] Rotate every secret that has ever appeared in a chat, screenshot, or commit — treat today's dev secrets as burned.
- [ ] Confirm `.env` was never committed to either repo.