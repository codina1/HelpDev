const LANGUAGE_MAP: Record<string, string> = {
  "c#": "C#",
  csharp: "C#",
  "c++": "C++",
  cpp: "C++",
  js: "JavaScript",
  javascript: "JavaScript",
  ts: "TypeScript",
  typescript: "TypeScript",
  py: "Python",
  python: "Python",
  go: "Go",
  golang: "Go",
  java: "Java",
  rust: "Rust",
  php: "PHP",
  ruby: "Ruby",
  kotlin: "Kotlin",
  swift: "Swift",
  sql: "SQL",
};

const FRAMEWORK_MAP: Record<string, string> = {
  react: "React",
  nextjs: "Next.js",
  "next.js": "Next.js",
  angular: "Angular",
  vue: "Vue",
  django: "Django",
  flask: "Flask",
  spring: "Spring",
  "asp.net": "ASP.NET",
  aspnet: "ASP.NET",
  express: "Express",
  nestjs: "NestJS",
  laravel: "Laravel",
};

const TASK_HINTS: Array<{ match: RegExp; label: string }> = [
  { match: /\blogin\b|\bauth(entication)?\b|\bsign[\s-]?in\b/i, label: "authentication / login" },
  { match: /\bapi\b|\bendpoint\b|\brest\b/i, label: "API design and implementation" },
  { match: /\bdb\b|\bdatabase\b|\bschema\b/i, label: "database design" },
  { match: /\bui\b|\bfrontend\b|\bcomponent\b/i, label: "user interface implementation" },
  { match: /\btest(ing|s)?\b|\bunit\b|\be2e\b/i, label: "testing strategy" },
  { match: /\bdeploy(ment)?\b|\bci\b|\bcd\b/i, label: "deployment and CI/CD" },
  { match: /\brefactor\b/i, label: "code refactoring" },
  { match: /\bbug\b|\bfix\b|\bdebug\b/i, label: "bug investigation and fix" },
];

function detectLanguages(tokens: string[]): string[] {
  const found = new Set<string>();

  for (const token of tokens) {
    const language = LANGUAGE_MAP[token];
    if (language) found.add(language);
  }

  // Handle multi-word tokens like "c#" already split; also check joined input pieces
  return [...found];
}

function detectFrameworks(tokens: string[]): string[] {
  const found = new Set<string>();

  for (const token of tokens) {
    const framework = FRAMEWORK_MAP[token];
    if (framework) found.add(framework);
  }

  return [...found];
}

function detectTasks(input: string): string[] {
  const tasks = TASK_HINTS.filter((hint) => hint.match.test(input)).map(
    (hint) => hint.label,
  );

  return tasks.length > 0 ? tasks : ["software implementation"];
}

function tokenize(input: string): string[] {
  return input
    .toLowerCase()
    .split(/[^a-z0-9+#.]+/i)
    .map((token) => token.trim())
    .filter(Boolean);
}

export function enhancePrompt(rawInput: string): string {
  const input = rawInput.trim();

  if (!input) {
    return "";
  }

  const tokens = tokenize(input);
  const languages = detectLanguages(tokens);
  const frameworks = detectFrameworks(tokens);
  const tasks = detectTasks(input);
  const stack = [...languages, ...frameworks];

  const specialty =
    stack.length > 0
      ? `specializing in ${stack.join(", ")}`
      : "with strong full-stack experience";

  const taskLine =
    tasks.length === 1
      ? tasks[0]
      : tasks.slice(0, -1).join(", ") + `, and ${tasks[tasks.length - 1]}`;

  const techRequirements =
    stack.length > 0
      ? stack.map((item) => `- Use ${item} idioms and current best practices`).join("\n")
      : "- Prefer clear, maintainable, production-ready code";

  return `You are a senior software engineer ${specialty}.

## Goal
Help me with: ${input}

## Task
Design and implement a solution focused on ${taskLine}.

## Requirements
${techRequirements}
- Follow clean architecture and clear naming
- Include validation, error handling, and security considerations
- Keep the solution simple, readable, and production-ready
- Call out assumptions when requirements are incomplete

## Constraints
- Do not invent unnecessary features
- Prefer standard libraries and proven patterns
- Optimize for maintainability over cleverness

## Output format
1. Brief approach
2. Implementation (code or steps)
3. Edge cases and failure modes
4. Testing checklist`;
}
