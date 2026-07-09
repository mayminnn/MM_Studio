import {
  LayoutDashboard,
  FolderKanban,
  ClipboardList,
  PlayCircle,
  BarChart3,
  Settings,
} from "lucide-react";

import { NavLink } from "react-router-dom";

const menus = [
  {
    name: "Dashboard",
    icon: LayoutDashboard,
    path: "/",
  },
  {
    name: "Test Cases",
    icon: ClipboardList,
    path: "/testcases",
  },
  {
    name: "Test Suites",
    icon: FolderKanban,
    path: "/testsuites",
  },
  {
    name: "Executions",
    icon: PlayCircle,
    path: "/execution",
  },
  // {
  //   name: "Datalog",
  //   icon: FileText,
  //   path: "/datalog",
  // },
  {
    name: "Reports",
    icon: BarChart3,
    path: "/reports",
  },
  {
    name: "Settings",
    icon: Settings,
    path: "/settings",
  },
];

export default function Sidebar() {
  return (
    // <div className="w-64 h-screen bg-slate-900 text-white p-5">
    <div className="fixed left-0 top-0 w-64 h-screen bg-slate-900 text-white p-5">
      <h1 className="text-2xl font-bold mb-8">
        MM Studio
      </h1>

      <div className="space-y-2">
        {menus.map((menu) => {
          const Icon = menu.icon;

          return (
            <NavLink
              key={menu.path}
              to={menu.path}
              className={({ isActive }) =>
                `flex items-center gap-3 p-3 rounded-lg ${
                  isActive
                    ? "bg-blue-600"
                    : "hover:bg-slate-800"
                }`
              }
            >
              <Icon size={18} />
              {menu.name}
            </NavLink>
          );
        })}
      </div>
    </div>
  );
}