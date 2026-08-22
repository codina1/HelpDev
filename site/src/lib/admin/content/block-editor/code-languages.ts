export const CODE_LANGUAGES = [
  { id: "csharp", label: "C#" },
  { id: "javascript", label: "JavaScript" },
  { id: "typescript", label: "TypeScript" },
  { id: "xml", label: "HTML" },
  { id: "css", label: "CSS" },
  { id: "json", label: "JSON" },
  { id: "sql", label: "SQL" },
  { id: "python", label: "Python" },
  { id: "java", label: "Java" },
  { id: "kotlin", label: "Kotlin" },
  { id: "dart", label: "Dart" },
  { id: "bash", label: "Bash" },
  { id: "powershell", label: "PowerShell" },
] as const;

export type CodeLanguageId = (typeof CODE_LANGUAGES)[number]["id"];

export function labelForCodeLanguage(id: string | null | undefined): string {
  const match = CODE_LANGUAGES.find((item) => item.id === id);
  return match?.label ?? id ?? "Plain text";
}
