import { useEffect, useState } from "react";
import { Table, Tag, Collapse, Spin } from "antd";
import axios from "axios";
import { useNavigate } from "react-router-dom";

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
};

type ExecutionGroup = {
  runId: string;
  suiteName: string;
  executedAt: string;
  total: number;
  passed: number;
  failed: number;
};

export default function Executions() {
  const navigate = useNavigate();

  const [loading, setLoading] = useState(false);
  const [groups, setGroups] = useState<ExecutionGroup[]>([]);

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
            executedAt: r.executedAt,
            total: 0,
            passed: 0,
            failed: 0,
          });
        }

        const g = map.get(r.runId)!;

        g.total += 1;

        if (r.status === "Passed") g.passed += 1;
        else g.failed += 1;

        // keep latest time
        if (new Date(r.executedAt) > new Date(g.executedAt)) {
          g.executedAt = r.executedAt;
        }
      });

      setGroups(Array.from(map.values()));
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

    </div>

      {loading ? (
        <Spin />
      ) : (
        <Table
          rowKey="runId"
          dataSource={groups}
          onRow={(record) => ({
            onClick: () => {
              navigate(`/executions/${record.runId}`);
            },

          })}
        >
          {/* Suite */}
          <Table.Column title="Suite" dataIndex="suiteName" />

          {/* Time */}
          <Table.Column
            title="Executed At"
            render={(_, r: ExecutionGroup) =>
              new Date(r.executedAt).toLocaleString()
            }
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

          {/* Status */}
          <Table.Column
            title="Status"
            render={(_, r: ExecutionGroup) => {
              const allPassed = r.failed === 0;

              return (
                <Tag color={allPassed ? "green" : "orange"}>
                  {allPassed ? "Passed" : "Failed"}
                </Tag>
              );
            }}
          />

          {/* Logs preview */}
          <Table.Column
            title="Details"
            render={() => (
              <Collapse
                items={[
                  {
                    key: "1",
                    label: "View details",
                    children: (
                      <div>
                        Click row to view full execution details
                      </div>
                    ),
                  },
                ]}
              />
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