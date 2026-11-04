import { useEffect, useMemo, useState } from "react";
import { Table, Tag, Spin, Select, Space } from "antd";
import axios from "axios";
import { useNavigate, useSearchParams } from "react-router-dom";
import { wasNotRun } from "../utils/resultStatus";
import { formatDuration } from "../utils/duration";

type TestRunResult = {
  id: string;
  runId: string;
  suiteName: string;
  className: string;
  displayName: string;
  status: string;
  output: string;
  error: string;
  executedAt: string;
  durationMs?: number;
  installerVersion?: string;
  projectName?: string;
  version?: string;
};

// Same label the Reports API uses for results saved before installer version was captured
const UNSPECIFIED = "(unspecified)";

const installerOf = (r: { installerVersion?: string }) => r.installerVersion?.trim() || UNSPECIFIED;

type ExecutionGroup = {
  runId: string;
  suiteName: string;
  installerVersion: string;
  projectName: string;
  version: string;
  executedAt: string;
  // Sum of the run's test durations
  durationMs: number;
  total: number;
  passed: number;
  failed: number;
  notRun: number;
};

export default function Executions() {
  const navigate = useNavigate();

  const [loading, setLoading] = useState(false);
  const [groups, setGroups] = useState<ExecutionGroup[]>([]);

  // Installer filter lives in the URL so the Reports page can link here (?installer=1.0)
  const [searchParams, setSearchParams] = useSearchParams();
  const installerFilter = searchParams.get("installer") ?? undefined;

  const setInstallerFilter = (value?: string) => {
    setSearchParams(value ? { installer: value } : {});
  };

  const installers = useMemo(
    () => [...new Set(groups.map(g => g.installerVersion))].sort(),
    [groups]
  );

  const visibleGroups = installerFilter
    ? groups.filter(g => g.installerVersion === installerFilter)
    : groups;

  useEffect(() => {
    loadExecutions();
  }, []);

  const loadExecutions = async () => {
    setLoading(true);

    try {
      const res = await axios.get<TestRunResult[]>(
        "http://localhost:5072/api/results"
      );

      const data = res.data;

      // 🔥 GROUP BY runId
      const map = new Map<string, ExecutionGroup>();

      data.forEach((r) => {
        if (!map.has(r.runId)) {
          map.set(r.runId, {
            runId: r.runId,
            suiteName: r.suiteName,
            installerVersion: installerOf(r),
            projectName: r.projectName ?? "",
            version: r.version ?? "",
            executedAt: r.executedAt,
            durationMs: 0,
            total: 0,
            passed: 0,
            failed: 0,
            notRun: 0,
          });
        }

        const g = map.get(r.runId)!;

        g.total += 1;
        g.durationMs += r.durationMs ?? 0;

        if (r.status === "Passed") g.passed += 1;
        else if (wasNotRun(r.status)) g.notRun += 1;
        else g.failed += 1;

        // keep latest time
        if (new Date(r.executedAt) > new Date(g.executedAt)) {
          g.executedAt = r.executedAt;
        }
      });

      // Newest run first
      setGroups(
        Array.from(map.values()).sort(
          (a, b) => new Date(b.executedAt).getTime() - new Date(a.executedAt).getTime()
        )
      );
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: 20 }}>

      {/* HEADER */}
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: 16,
        }}
      >
        <h2 style={{ margin: 0 }}>Executions</h2>

        <Space>
          <span>Installer</span>
          <Select
            style={{ width: 180 }}
            allowClear
            placeholder="All"
            value={installerFilter}
            onChange={setInstallerFilter}
            options={installers.map(i => ({ value: i, label: i }))}
          />
        </Space>

    </div>

      {loading ? (
        <Spin />
      ) : (
        <Table
          rowKey="runId"
          dataSource={visibleGroups}
          onRow={(record) => ({
            onClick: () => {
              navigate(`/executions/${record.runId}`);
            },
            // Rows open the run, so make them look and act like links
            onKeyDown: (e) => {
              if (e.key === "Enter") navigate(`/executions/${record.runId}`);
            },
            tabIndex: 0,
            title: "View execution details",
            style: { cursor: "pointer" },
          })}
        >
          {/* Suite */}
          <Table.Column title="Suite" dataIndex="suiteName" />

          {/* Release / project under test */}
          <Table.Column title="Installer" dataIndex="installerVersion" />
          <Table.Column
            title="Project / Version"
            render={(_, r: ExecutionGroup) =>
              r.projectName ? `${r.projectName} / ${r.version}` : "-"
            }
          />

          {/* Time */}
          <Table.Column
            title="Executed At"
            render={(_, r: ExecutionGroup) =>
              new Date(r.executedAt).toLocaleString()
            }
          />

          {/* How long the whole run took */}
          <Table.Column
            title="Duration"
            render={(_, r: ExecutionGroup) => formatDuration(r.durationMs)}
          />

          {/* Total */}
          <Table.Column title="Total" dataIndex="total" />

          {/* Passed */}
          <Table.Column
            title="Passed"
            render={(_, r: ExecutionGroup) => (
              <Tag color="green">{r.passed}</Tag>
            )}
          />

          {/* Failed */}
          <Table.Column
            title="Failed"
            render={(_, r: ExecutionGroup) => (
              <Tag color="red">{r.failed}</Tag>
            )}
          />

          {/* Not Run */}
          <Table.Column
            title="Not Run"
            render={(_, r: ExecutionGroup) => (
              <Tag>{r.notRun}</Tag>
            )}
          />

          {/* Status */}
          <Table.Column
            title="Status"
            render={(_, r: ExecutionGroup) => {
              if (r.notRun > 0)
                return <Tag color="default">Stopped</Tag>;

              const allPassed = r.failed === 0;

              return (
                <Tag color={allPassed ? "green" : "orange"}>
                  {allPassed ? "Passed" : "Failed"}
                </Tag>
              );
            }}
          />

          {/* Open hint */}
          <Table.Column
            width={40}
            align="center"
            render={() => (
              <span style={{ fontSize: 18, color: "#999" }}>›</span>
            )}
          />
        </Table>
      )}
    </div>
  );
}

// import { useEffect, useState } from "react";
// import { Table, Tag, Collapse } from "antd";
// import axios from "axios";

// type TestRunResult = {
//   id: string;
//   suiteName: string;
//   className: string;
//   displayName: string;
//   status: string;
//   output: string;
//   error: string;
//   executedAt: string;
// };

// export default function Executions() {
//   const [data, setData] = useState<TestRunResult[]>([]);

//   useEffect(() => {
//     axios
//       .get("http://localhost:5072/api/results")
//       .then((res) => setData(res.data));
//   }, []);

//   return (
//     <div style={{ padding: 20 }}>
//       <h2>Executions</h2>

//       <Table
//         rowKey="id"
//         dataSource={data}
//         pagination={{ pageSize: 10 }}
//       >
//         <Table.Column title="Suite" dataIndex="suiteName" />

//         <Table.Column title="Test" dataIndex="displayName" />

//         <Table.Column
//           title="Status"
//           render={(_, r: TestRunResult) => (
//             <Tag color={r.status === "Passed" ? "green" : "red"}>
//               {r.status}
//             </Tag>
//           )}
//         />

//         <Table.Column
//           title="Time"
//           render={(_, r: TestRunResult) =>
//             new Date(r.executedAt).toLocaleString()
//           }
//         />

//         <Table.Column
//           title="Log"
//           render={(_, r: TestRunResult) => (
//             <Collapse
//               items={[
//                 {
//                   key: "1",
//                   label: "View Output",
//                   children: (
//                     <pre>
// {r.output}

// {r.error}
//                     </pre>
//                   ),
//                 },
//               ]}
//             />
//           )}
//         />
//       </Table>
//     </div>
//   );
// }