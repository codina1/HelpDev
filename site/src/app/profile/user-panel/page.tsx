import { redirect } from "next/navigation";

export default function UserPanelAliasPage() {
  redirect("/profile?tab=profile");
}
