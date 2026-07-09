import { useEffect, useState } from "react";
import {
  Table,
  Button,
  Input,
  Space,
  Popconfirm,
  message,
  Tag,
  Collapse,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import axios from "axios";
import { useNavigate } from "react-router-dom";

type SuiteTest = {
  className: string;
  displayName: string;
  executionName: string;
};

type TestSuite = {
  id: string;
  name: string;
  createdAt: string;
  tests: SuiteTest[];
};

type RunResult = {
  className: string;
  displayName: string;
  status: string;
  output: string;
  error: string;
};

export default function TestSuites() {
  const navigate = useNavigate();

  const [data, setData] = useState<TestSuite[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchText, setSearchText] = useState("");
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [runResults, setRunResults] = useState<RunResult[]>([]);

  const loadSuites = async () => {
    setLoading(true);

    try {
      const res = await axios.get("http://localhost:5072/api/suite");
      setData(res.data);
    } catch (err) {
      console.error(err);
      message.error("Failed to load test suites.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSuites();
  }, []);

  const deleteSuite = async (id: string) => {
    try {
      await axios.delete(`http://localhost:5072/api/suite/${id}`);

      message.success("Suite deleted");

      loadSuites();
    } catch (err) {
      console.error(err);
      message.error("Delete failed.");
    }
  };

  const runSelectedSuites = async () => {
    try {
      const selectedSuites = data.filter((s) =>
        selectedRowKeys.includes(s.id)
      );

      let allResults: RunResult[] = [];

      for (const suite of selectedSuites) {
        const payload = {
          suiteName: suite.name,
          tests: suite.tests,
        };

        console.log("Running Suite:", payload);

        const res = await axios.post(
          "http://localhost:5072/api/execution/run",
          payload
        );

        allResults = [...allResults, ...res.data];
      }

      setRunResults(allResults);

      message.success("Execution completed");
    } catch (err) {
      console.error(err);
      message.error("Execution failed");
    }
  };

  const filteredData = data.filter((suite) =>
    suite.name.toLowerCase().includes(searchText.toLowerCase())
  );

  const columns: ColumnsType<TestSuite> = [
    {
      title: "Suite Name",
      dataIndex: "name",
      sorter: (a, b) => a.name.localeCompare(b.name),
    },
    {
      title: "Tests",
      render: (_, record) => record.tests.length,
    },
    {
      title: "Created",
      render: (_, record) =>
        new Date(record.createdAt).toLocaleDateString(),
    },
    {
      title: "Actions",
      render: (_, record) => (
        <Space>
          <Button
            size="small"
            onClick={() => navigate(`/testsuites/${record.id}`)}
          >
            View
          </Button>

          <Popconfirm
            title="Delete this suite?"
            onConfirm={() => deleteSuite(record.id)}
          >
            <Button danger size="small">
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: 20 }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          marginBottom: 16,
        }}
      >
        <h2>Test Suites</h2>

        <Button
          type="primary"
          disabled={selectedRowKeys.length === 0}
          onClick={runSelectedSuites}
        >
          Run Selected ({selectedRowKeys.length})
        </Button>
      </div>

      <Space style={{ marginBottom: 16 }}>
        <Input
          placeholder="Search suite..."
          value={searchText}
          style={{ width: 300 }}
          onChange={(e) => setSearchText(e.target.value)}
        />
      </Space>

      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={filteredData}
        rowSelection={{
          selectedRowKeys,
          onChange: (keys) => setSelectedRowKeys(keys),
        }}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
        }}
      />

      {runResults.length > 0 && (
        <>
          <h2 style={{ marginTop: 40 }}>Execution Results</h2>

          <Table
            rowKey={(record) =>
              `${record.className}-${record.displayName}`
            }
            dataSource={runResults}
            pagination={false}
          >
            <Table.Column
              title="Test"
              dataIndex="displayName"
            />

            <Table.Column
              title="Status"
              render={(_, record: RunResult) => (
                <Tag
                  color={
                    record.status === "Passed"
                      ? "green"
                      : "red"
                  }
                >
                  {record.status}
                </Tag>
              )}
            />

            <Table.Column
              title="Execution Log"
              render={(_, record: RunResult) => (
                <Collapse
                  ghost
                  items={[
                    {
                      key: "1",
                      label: "View Log",
                      children: (
                        <pre
                          style={{
                            whiteSpace: "pre-wrap",
                            margin: 0,
                          }}
                        >
                          {record.output}

                          {record.error}
                        </pre>
                      ),
                    },
                  ]}
                />
              )}
            />
          </Table>
        </>
      )}
    </div>
  );
}

// import { useEffect, useState } from "react";
// import { Table, Button, Input, Space, Popconfirm, message, Tag, Collapse} from "antd";
// import type { ColumnsType } from "antd/es/table";
// import axios from "axios";
// import { useNavigate } from "react-router-dom";

// type SuiteTest = {
//   className: string;
//   displayName: string;
//   executionName: string;
// };

// type TestSuite = {
//   id: string;
//   name: string;
//   createdAt: string;
//   tests: SuiteTest[];
// };

// export default function TestSuites() {
//   const navigate = useNavigate();
//   const [data, setData] = useState<TestSuite[]>([]);
//   const [loading, setLoading] = useState(false);
//   const [searchText, setSearchText] = useState("");
//   const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);

//   // ---------------------------
//   // Load suites
//   // ---------------------------
//   const loadSuites = async () => {
//     setLoading(true);
//     try {
//       const res = await axios.get("http://localhost:5072/api/suite");
//       setData(res.data);
//     } catch (err) {
//       console.error(err);
//     } finally {
//       setLoading(false);
//     }
//   };

//   useEffect(() => {
//     loadSuites();
//   }, []);

//   // ---------------------------
//   // Delete suite
//   // ---------------------------
//   const deleteSuite = async (id: string) => {
//     try {
//       await axios.delete(`http://localhost:5072/api/suite/${id}`);
//       message.success("Suite deleted");
//       loadSuites();
//     } catch (err) {
//       console.error(err);
//     }
//   };

//   // ---------------------------
//   // Run selected suites
//   // ---------------------------
//   // const runSelectedSuites = async () => {
//   // try {
//   //   const selectedSuites = data.filter((s) =>
//   //     selectedRowKeys.includes(s.id)
//   //   );

//   //   for (const suite of selectedSuites) {
//   //     const res = await axios.post(
//   //       "http://localhost:5072/api/execution/run",
//   //       {
//   //         suiteName: suite.name,
//   //       }
//   //     );

//   //     console.log("OUTPUT:", res.data.output);
//   //   }

//   //   message.success("Execution completed");
//   // } catch (err) {
//   //     console.error(err);
//   //     message.error("Execution failed");
//   //   }
//   // };

//   const runSelectedSuites = async () => {
//     try {
//       const selectedSuites = data.filter((s) =>
//         selectedRowKeys.includes(s.id)
//       );

//       for (const suite of selectedSuites) {

//         const payload = {
//           suiteName: suite.name,
//           tests: suite.tests
//         };

//         console.log("PAYLOAD");
//         console.log(payload);

//         const res = await axios.post(
//           "http://localhost:5072/api/execution/run",
//           payload
//         );

//         console.log(res.data);
//       }

//       message.success("Execution completed");

//     } catch (err) {
//       console.error(err);
//       message.error("Execution failed");
//     }
//   };

//   // ---------------------------
//   // Filter UI
//   // ---------------------------
//   const filteredData = data.filter((s) =>
//     s.name.toLowerCase().includes(searchText.toLowerCase())
//   );

//   // ---------------------------
//   // Table columns
//   // ---------------------------
//   const columns: ColumnsType<TestSuite> = [
//     {
//       title: "Suite Name",
//       dataIndex: "name",
//       sorter: (a, b) => a.name.localeCompare(b.name),
//     },
//     {
//       title: "Test Count",
//       render: (_, record) => record.tests?.length ?? 0,
//     },
//     {
//       title: "Created",
//       render: (_, record) =>
//         new Date(record.createdAt).toLocaleDateString(),
//     },
//     {
//       title: "Actions",
//       render: (_, record) => (
//         <Space>
//           <Popconfirm
//             title="Delete this suite?"
//             onConfirm={() => deleteSuite(record.id)}
//           >
//             <Button danger size="small">
//               Delete
//             </Button>
//           </Popconfirm>

//           <Button size="small" onClick={() => navigate(`/testsuites/${record.id}`)}>
//             View
//           </Button>
//         </Space>
//       ),
//     },
//   ];

//   // ---------------------------
//   // Row selection (IMPORTANT)
//   // ---------------------------
//   const rowSelection = {
//     selectedRowKeys,
//     onChange: (keys: React.Key[]) => {
//       setSelectedRowKeys(keys);
//     },
//   };

//   return (
//     <div style={{ padding: 20 }}>
//       {/* Header */}
//       <div
//         style={{
//           display: "flex",
//           justifyContent: "space-between",
//           marginBottom: 16,
//         }}
//       >
//         <h2>Test Suites</h2>

//         <Button
//           type="primary"
//           disabled={selectedRowKeys.length === 0}
//           onClick={runSelectedSuites}
//         >
//           Run Selected ({selectedRowKeys.length})
//         </Button>
//       </div>

//       {/* Search */}
//       <Space style={{ marginBottom: 16 }}>
//         <Input
//           placeholder="Search suites..."
//           value={searchText}
//           onChange={(e) => setSearchText(e.target.value)}
//           style={{ width: 300 }}
//         />
//       </Space>

//       {/* Table */}
//       <Table
//         rowKey={(record) => record.id}
//         rowSelection={rowSelection}
//         columns={columns}
//         dataSource={filteredData}
//         loading={loading}
//         pagination={{
//           pageSize: 10,
//           showSizeChanger: true,
//         }}
//       />
//     </div>
//   );
// }