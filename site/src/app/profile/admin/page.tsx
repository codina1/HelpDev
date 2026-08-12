import { redirect } from "next/navigation";

// The admin experience now lives in the dedicated Admin shell at /admin.
export default function ProfileAdminRedirectPage() {
  redirect("/admin");
}
