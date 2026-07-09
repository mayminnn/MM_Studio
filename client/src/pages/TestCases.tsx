import { useEffect, useState } from "react";
import { Table, Input, Button, Modal } from "antd";
import type { ColumnsType } from "antd/es/table";
import { getTestCases } from "../services/testcaseService";
import axios from "axios";
import { useNavigate } from "react-router-dom";

type TestCase = {
  className: string;
  methodName: string;
};

type SuiteTest = {
  className: string;
  displayName: string;
  executionName: string;
};

export default function TestCases() {
  const [data, setData] = useState<TestCase[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState("");

  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [suiteName, setSuiteName] = useState("Regression");

  const navigate = useNavigate();

  useEffect(() => {
    getTestCases()
      .then((res) => setData(res))
      .finally(() => setLoading(false));
  }, []);

    // Filter
  const filteredData = data.filter((item) =>
    `${item.className} ${item.methodName}`
      .toLowerCase()
      .includes(searchText.toLowerCase())
  );

  // Row selection (IMPORTANT FIX)
  const rowSelection = {
    selectedRowKeys,
    onChange: (keys: React.Key[]) => {
      setSelectedRowKeys(keys);
    },
  };

  // Table columns
  const columns: ColumnsType<TestCase> = [
    {
      title: "Test ID",
      dataIndex: "className",
      sorter: (a, b) => a.className.localeCompare(b.className),
    },
    {
      title: "Test Name",
      dataIndex: "methodName",
      sorter: (a, b) => a.methodName.localeCompare(b.methodName),
    },
  ];

  // Open modal
  const handleCreateSuiteClick = () => {
    setIsModalOpen(true);
  };

  // API call
  const handleCreateSuiteConfirm = async () => {
  try {
    const selectedTests: SuiteTest[] = data
    .filter((t) => selectedRowKeys.includes(t.className))
    .map((t) => ({
      className: t.className,
      displayName: t.methodName,
      executionName: t.methodName.replace(/\s+/g, "_"),
    }));

  await axios.post(
    "http://localhost:5072/api/suite",
    {
      name: suiteName,
      tests: selectedTests,
    }
  );

    alert("Suite created successfully!");

    setIsModalOpen(false);
    setSelectedRowKeys([]);

    navigate("/testsuites");

  } catch (err) {
    console.error(err);
    alert("Failed to create suite.");
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
        <h2 style={{ margin: 0 }}>Test Cases</h2>

        <span>Total: {filteredData.length}</span>
      </div>

      {/* SEARCH + BUTTON */}
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          marginBottom: 16,
        }}
      >
        <Input
          placeholder="Search test cases..."
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          style={{ width: 300 }}
        />

        <Button
          type="primary"
          disabled={selectedRowKeys.length === 0}
          onClick={handleCreateSuiteClick}
        >
          Create Suite
        </Button>
      </div>

      {/* TABLE */}
      <Table
        rowSelection={rowSelection}
        columns={columns}
        dataSource={filteredData}
        loading={loading}
        rowKey={(record) => record.className}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
        }}
      />

      {/* MODAL (MUST BE INSIDE RETURN) */}
      <Modal
        title="Create Test Suite"
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        onOk={handleCreateSuiteConfirm}
        okText="Create"
      >
        <div style={{ marginBottom: 12 }}>
          <label>Suite Name</label>
          <Input
            value={suiteName}
            onChange={(e) => setSuiteName(e.target.value)}
          />
        </div>

        <div>
          <h4>Selected Tests:</h4>
          <ul>
            {selectedRowKeys.map((key) => (
              <li key={key.toString()}>{key}</li>
            ))}
          </ul>
        </div>
      </Modal>

    </div>
  );
}