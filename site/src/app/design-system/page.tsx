import type { Metadata } from "next";
import DesignSystemClient from "./design-system-client";

export const metadata: Metadata = {
  title: "Design System",
  description: "HelpDev Premium Design System Foundation",
};

export default function DesignSystemPage() {
  return <DesignSystemClient />;
}
