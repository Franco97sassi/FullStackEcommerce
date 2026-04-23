import { AdminDashboard } from "@/modules/admin/components/admin-dashboard";
import { RequireAdmin } from "@/modules/admin/components/require-admin";

export default function AdminPage() {
  return (
    <RequireAdmin>
      <AdminDashboard />
    </RequireAdmin>
  );
}
