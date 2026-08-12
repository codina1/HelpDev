import { redirect } from "next/navigation";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

/**
 * Legacy generic create → Article workspace.
 * Keeps the old URL working without a type selector.
 */
export default function AdminContentNewRedirectPage() {
  redirect(ADMIN_ROUTES.contentArticlesNew);
}
