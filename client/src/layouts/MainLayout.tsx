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

      <div className="ml-64 print:ml-0 flex flex-1 flex-col min-w-0">
        <Topbar />

        <main className="flex-1 min-w-0 p-8 bg-slate-100 print:p-0 print:bg-white">
          <Outlet />
        </main>
      </div>
    </div>
  );
}