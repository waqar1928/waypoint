import { defineConfig } from "vitest/config";
import path from "node:path";

// Unit tests only (pure logic — validation schemas, small helpers), no DOM/component
// tests, so this deliberately stays minimal: no jsdom environment, no React plugin.
export default defineConfig({
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
  },
  test: {
    include: ["src/**/*.test.ts"],
  },
});
