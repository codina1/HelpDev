import type { Metadata } from "next";
import { PublicPromptLabPage } from "@/components/public/prompt-lab/public-prompt-lab-page";
import {
  PROMPT_LAB_HERO_SUBTITLE,
  PROMPT_LAB_HERO_TITLE,
} from "@/lib/public/prompt-lab-routes";

export const metadata: Metadata = {
  title: PROMPT_LAB_HERO_TITLE,
  description: PROMPT_LAB_HERO_SUBTITLE,
};

export default function PromptLabPage() {
  return <PublicPromptLabPage />;
}
