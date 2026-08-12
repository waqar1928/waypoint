FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production

# Run as a non-root user rather than node:22-alpine's default root —
# standard container hardening (see docs/PRODUCTION_READINESS_AUDIT.md
# section 19). node:22-alpine already ships a "node" user/group (uid/gid
# 1000), so this reuses it rather than creating a new one.
COPY --from=build /app/public ./public
COPY --from=build --chown=node:node /app/.next/standalone ./
COPY --from=build --chown=node:node /app/.next/static ./.next/static
USER node

EXPOSE 3000

# busybox's wget ships in node:*-alpine by default, so this needs no extra
# package install just for a healthcheck. Hits the root route rather than a
# dedicated /health path since this is a Next.js app, not the API — there's
# no existing health endpoint here to reuse.
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:3000/ || exit 1

CMD ["node", "server.js"]
