import { useParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import axios from "axios";
import { Button, Table, message, Space, InputNumber, Tag, Modal, Input } from "antd";
import type { ColumnsType } from "antd/es/table";
import BackButton from "../components/BackButton";
import { getTestCases } from "../services/testcaseService";
import { testLinkUrl } from "../utils/testLink";

type SuiteTest = {
  order: number;
  className: string;
  displayName: string;
  executionName: string;
};

// A test from the test case library
type TestCase = {
  className: string;
  methodName: string;
  tags: string[];
  testLinkIds: string[];
};

const renderTestLinkIds = (ids: string[] = []) => (
  <>
    {ids.map(id => (
      <Tag key={id}>
        <a
          href={testLinkUrl(id)}
          target="_blank"
          rel="noopener noreferrer"
          title="Open in TestLink"
        >
          {id}
        </a>
      </Tag>
    ))}
  </>
);

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

  // Test case library: TestLink IDs for the table, and the tests that can be added
  const [library, setLibrary] = useState<TestCase[]>([]);
  const [addOpen, setAddOpen] = useState(false);
  const [addSearch, setAddSearch] = useState("");
  const [addKeys, setAddKeys] = useState<React.Key[]>([]);
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    getTestCases()
      .then(setLibrary)
      .catch(() => message.error("Failed to load test cases"));
  }, []);

  const testLinkIds = useMemo(
    () => Object.fromEntries(library.map(t => [t.className.toLowerCase(), t.testLinkIds ?? []])),
    [library]
  );

  // Library tests not in the suite yet, filtered by the search box
  const addable = useMemo(() => {
    const inSuite = new Set((suite?.tests ?? []).map(t => t.className.toLowerCase()));
    const search = addSearch.toLowerCase();

    return library.filter(t =>
      !inSuite.has(t.className.toLowerCase()) &&
      `${t.className} ${t.methodName} ${(t.testLinkIds ?? []).join(" ")} ${t.tags.join(" ")}`
        .toLowerCase()
        .includes(search)
    );
  }, [library, suite, addSearch]);

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
  // Add tests from the library
  // ----------------------------
  const closeAdd = () => {
    setAddOpen(false);
    setAddKeys([]);
    setAddSearch("");
  };

  const addSelected = async () => {
    const tests = library
      .filter(t => addKeys.includes(t.className))
      .map(t => ({
        className: t.className,
        displayName: t.methodName,
        executionName: t.methodName.replace(/\s+/g, "_")
      }));

    setAdding(true);

    try {
      // The server puts them at the end of the order and skips any already in the suite
      const res = await axios.post(
        `http://localhost:5072/api/suite/${id}/tests/add`,
        tests
      );

      setSuite(res.data);
      message.success(`${tests.length} test(s) added`);
      closeAdd();
    } catch (err) {
      console.error(err);
      message.error("Failed to add tests");
    } finally {
      setAdding(false);
    }
  };

  const libraryColumns: ColumnsType<TestCase> = [
    { title: "Test ID", dataIndex: "className", width: 140 },
    { title: "Test Name", dataIndex: "methodName" },
    {
      title: "TestLink IDs",
      dataIndex: "testLinkIds",
      width: 200,
      render: renderTestLinkIds
    },
    {
      title: "Tags",
      dataIndex: "tags",
      render: (tags: string[]) => (
        <>
          {tags.map(t => <Tag key={t}>{t}</Tag>)}
        </>
      )
    }
  ];

  // ----------------------------
  // Row selection
  // ----------------------------
  const rowSelection = {
    selectedRowKeys,
    onChange: (keys: React.Key[]) => {
      setSelectedRowKeys(keys);
    },
  };

  const moveToPosition = async (
    oldOrder: number,
    newOrder: number
  ) => {

    if (!suite)
      return;

    if (newOrder < 1)
      newOrder = 1;

    if (newOrder > suite.tests.length)
      newOrder = suite.tests.length;

    if (oldOrder === newOrder)
      return;

    const updated = [...suite.tests];

    // Remove the item
    const item = updated.splice(oldOrder - 1, 1)[0];

    // Insert into new position
    updated.splice(newOrder - 1, 0, item);

    // Renumber everything
    updated.forEach((t, i) => {
      t.order = i + 1;
    });

    const updatedSuite = {
      ...suite,
      tests: updated
    };

    setSuite(updatedSuite);

    try {

      await axios.put(
        `http://localhost:5072/api/suite/${suite.id}`,
        updatedSuite
      );

    }
    catch {

      message.error("Failed to save order");

    }

  };

  // ----------------------------
  // Table columns
  // ----------------------------
  const columns: ColumnsType<SuiteTest> = [
    {
      title: "Order",
      width: 60,

      render: (_, record) => (

        <InputNumber
          min={1}
          max={suite?.tests.length ?? 1}
          value={record.order}

          onPressEnter={(e) => {

            const value = Number(
              (e.target as HTMLInputElement).value
            );

            moveToPosition(record.order, value);

          }}

          onBlur={(e) => {

            const value = Number(
              (e.target as HTMLInputElement).value
            );

            moveToPosition(record.order, value);

          }}
        />

      )
    },
    {
      title: "Test ID",
      dataIndex: "className",
      width: 150,
    },
    {
      title: "TestLink IDs",
      width: 200,
      render: (_, record) => renderTestLinkIds(testLinkIds[record.className.toLowerCase()]),
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
      <BackButton to="/testsuites" label="Back" />

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
            type="primary"
            onClick={() => setAddOpen(true)}
          >
            Add Test Cases
          </Button>

          <Button
            danger
            disabled={selectedRowKeys.length === 0}
            onClick={removeSelected}
          >
            Remove Selected
          </Button>
        </Space>
      </div>

      <Modal
        title="Add Test Cases"
        open={addOpen}
        width={1000}
        onCancel={closeAdd}
        onOk={addSelected}
        okText={`Add (${addKeys.length})`}
        okButtonProps={{ disabled: addKeys.length === 0, loading: adding }}
      >
        <Input
          placeholder="Search test ID, name, TestLink ID or tag..."
          value={addSearch}
          onChange={e => setAddSearch(e.target.value)}
          style={{ marginBottom: 12 }}
        />

        <Table
          rowKey="className"
          size="small"
          columns={libraryColumns}
          dataSource={addable}
          rowSelection={{
            selectedRowKeys: addKeys,
            onChange: setAddKeys,
            preserveSelectedRowKeys: true
          }}
          pagination={{ pageSize: 8 }}
          locale={{
            emptyText: addSearch
              ? "No test cases match the search"
              : "Every test case is already in this suite"
          }}
        />
      </Modal>

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