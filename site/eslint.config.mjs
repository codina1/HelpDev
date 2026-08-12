import { dirname } from "path";
import { fileURLToPath } from "url";
import { FlatCompat } from "@eslint/eslintrc";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const compat = new FlatCompat({
  baseDirectory: __dirname,
});

const eslintConfig = [
  ...compat.extends("next/core-web-vitals", "next/typescript"),
  {
    ignores: [
      "node_modules/**",
      ".next/**",
      "out/**",
      "build/**",
      "next-env.d.ts",
    ],
  },
  {
    // Enforce canonical, versioned API routes. New frontend code must call the
    // typed client (which targets the canonical /api/v1 base) rather than
    // hardcoding unversioned /api/... URLs. Health probes and the API base
    // configuration are the only allowed exceptions.
    files: ["src/**/*.{ts,tsx}"],
    ignores: [
      "src/lib/config.ts",
      "src/**/*.test.{ts,tsx}",
      "src/**/__tests__/**",
    ],
    rules: {
      "no-restricted-syntax": [
        "error",
        {
          selector: "Literal[value=/^\\/api\\/(?!v1)/]",
          message:
            "Use the canonical /api/v1 route via the typed API client. Unversioned /api/... routes are not allowed (health probes use /health/*).",
        },
        {
          selector: "TemplateElement[value.cooked=/^\\/api\\/(?!v1)/]",
          message:
            "Use the canonical /api/v1 route via the typed API client. Unversioned /api/... routes are not allowed (health probes use /health/*).",
        },
      ],
    },
  },
];

export default eslintConfig;
