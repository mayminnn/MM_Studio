import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import axios from "axios";
import { Button, Table, message, Space } from "antd";
import type { ColumnsType } from "antd/es/table";

type SuiteTest = {
  className: string;
  displayName: string;
  executionName: string;
};

type Suite = {
  id: string;
  name: string;
  createdAt: string;
  tests: SuiteTest[];
};

export default function SuiteDetails() {
  const { id } = useParams();

  const [suite, setSuite] = useState<Suite | null>(null);
  const [loading, setLoading] = useState(false);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);

  // ----------------------------
  // Load suite
  // ----------------------------
  const loadSuite = async () => {
    setLoading(true);

    try {
      const res = await axios.get(
        `http://localhost:5072/api/suite/${id}`
      );

      setSuite(res.data);
    } catch (err) {
      console.error(err);
      message.error("Failed to load suite");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSuite();
  }, [id]);

  // ----------------------------
  // Remove selected tests
  // ----------------------------
  const removeSelected = async () => {
    try {
        await axios.post(
        `http://localhost:5072/api/suite/${id}/tests/remove`,
        selectedRowKeys
        );

        message.success("Tests removed");
        setSelectedRowKeys([]);
        loadSuite();
    } catch (err) {
        console.error(err);
        message.error("Failed to remove tests");
    }
  };

  // ----------------------------
  // Row selection
  // ----------------------------
  const rowSelection = {
    selectedRowKeys,
    onChange: (keys: React.Key[]) => {
      setSelectedRowKeys(keys);
    },
  };

  // ----------------------------
  // Table columns
  // ----------------------------
  const columns: ColumnsType<SuiteTest> = [
    {
      title: "Test ID",
      dataIndex: "className",
      width: 150,
    },
    {
      title: "Test Name",
      dataIndex: "displayName",
    },
    {
      title: "Execution Name",
      dataIndex: "executionName",
    },
  ];

  // ----------------------------
  // UI
  // ----------------------------
  if (!suite) return <div style={{ padding: 20 }}>Loading...</div>;

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
        <div>
          <h2 style={{ marginBottom: 4 }}>{suite.name}</h2>
          <div style={{ color: "#888" }}>
            {suite.tests?.length ?? 0} Test(s)
          </div>
        </div>

        <Space>
          {/* <Button type="primary">
            Run Suite
          </Button> */}

          <Button
            danger
            disabled={selectedRowKeys.length === 0}
            onClick={removeSelected}
          >
            Remove Selected
          </Button>
        </Space>
      </div>

      {/* TABLE */}
      <Table
        loading={loading}
        rowKey={(r) => r.className}
        rowSelection={rowSelection}
        columns={columns}
        dataSource={suite.tests}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
        }}
      />
    </div>
  );
}