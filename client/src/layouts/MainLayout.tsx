import { Outlet } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";

// export default function MainLayout() {
//   return (
//     <div className="flex">
//       <Sidebar />

//       <div className="flex-1">
//         <Topbar />

//         <main className="p-8 bg-slate-100 min-h-screen">
//           <Outlet />
//         </main>
//       </div>
//     </div>
//   );
// }

export default function MainLayout() {
  return (
    <div className="flex min-h-screen">
      <Sidebar />

      <div className="ml-64 flex flex-1 flex-col">
        <Topbar />

        <main className="flex-1 p-8 bg-slate-100">
          <Outlet />
        </main>
      </div>
    </div>
  );
}