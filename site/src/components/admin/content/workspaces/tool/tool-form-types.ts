export const TOOL_PRICING = ["Free", "Freemium", "Paid", "OpenSource"] as const;
export const TOOL_LICENSES = ["Commercial", "OpenSource", "Community"] as const;
export const TOOL_PLATFORMS = ["Windows", "Linux", "MacOS", "Web"] as const;

export type ToolFormState = {
  toolName: string;
  officialWebsiteUrl: string;
  githubUrl: string;
  companyName: string;
  pricingModel: (typeof TOOL_PRICING)[number];
  toolCategory: string;
  platforms: string[];
  licenseType: (typeof TOOL_LICENSES)[number];
  alternatives: Array<{ alternativeToolContentId: string; order: number }>;
};

export const EMPTY_TOOL_FORM: ToolFormState = {
  toolName: "",
  officialWebsiteUrl: "",
  githubUrl: "",
  companyName: "",
  pricingModel: "Freemium",
  toolCategory: "",
  platforms: ["Web"],
  licenseType: "Commercial",
  alternatives: [],
};
