import {
  BrowserRouter,
  Routes,
  Route,
} from "react-router-dom";

import MainLayout from "./layouts/MainLayout";

import Dashboard from "./pages/Dashboard";
import TestSuites from "./pages/TestSuites";
import SuiteDetails from "./pages/SuiteDetails";
import TestCases from "./pages/TestCases";
import Executions from "./pages/Execution";
import ExecutionDetail from "./pages/ExecutionDetail";
import ExecutionResult from "./pages/ExecutionResult";
// import Datalog from "./pages/Datalog";
import Reports from "./pages/Reports";
import Settings from "./pages/Settings";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/testsuites" element={<TestSuites />} />
          <Route path="/testsuites/:id" element={<SuiteDetails />} />
          <Route path="/testcases" element={<TestCases />} />
          <Route path="/execution" element={<Executions />} />
          <Route path="/executions/:runId" element={<ExecutionDetail />} />
          <Route path="/executions/result/:id" element={<ExecutionResult />} />
          {/* <Route path="/datalog" element={<Datalog />} /> */}
          <Route path="/reports" element={<Reports />} />
          <Route path="/settings" element={<Settings />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;