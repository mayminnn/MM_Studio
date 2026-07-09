import { UserCircle } from "lucide-react";

export default function Topbar() {
  return (
    <div className="h-16 bg-white border-b flex justify-between items-center px-8">
      <h2 className="font-semibold text-xl">
        MM Studio
      </h2>

      <div className="flex items-center gap-4">
        {/* <Bell /> */}

        <UserCircle size={30} />
      </div>
    </div>
  );
}