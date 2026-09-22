import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import axios from "axios";
import { Button, Table, message, Space, InputNumber } from "antd";
import type { ColumnsType } from "antd/es/table";

type SuiteTest = {
  order: number;
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

  // const moveUp = async (index: number) => {

  //   // if (index === 0)
  //   if (!suite || index === 0)
  //     return;

  //   const updated = [...suite.tests];

  //   [updated[index - 1], updated[index]] =
  //     [updated[index], updated[index - 1]];

  //   updated.forEach((t, i) => {
  //     t.order = i + 1;
  //   });

  //   // setSuite({
  //   //     ...suite,
  //   //     tests: updated
  //   // });
  //   const updatedSuite = {
  //     ...suite,
  //     tests: updated
  //   };

  //   setSuite(updatedSuite);

  //   try {

  //     await axios.put(
  //       `http://localhost:5072/api/suite/${suite.id}`,
  //       updatedSuite
  //     );

  //   }
  //   catch {

  //     message.error("Failed to save order");
  //   }
  // };

  // const moveDown = async (index: number) => {

  //   if (!suite || index === suite.tests.length - 1)
  //     return;

  //   const updated = [...suite.tests];

  //   [updated[index], updated[index + 1]] =
  //     [updated[index + 1], updated[index]];

  //   updated.forEach((t, i) => {
  //     t.order = i + 1;
  //   });

  //   // setSuite({
  //   //     ...suite,
  //   //     tests: updated
  //   // });
  //   const updatedSuite = {
  //     ...suite,
  //     tests: updated
  //   };

  //   setSuite(updatedSuite);

  //   try {

  //     await axios.put(
  //       `http://localhost:5072/api/suite/${suite.id}`,
  //       updatedSuite
  //     );

  //   }
  //   catch {

  //     message.error("Failed to save order");

  //   }

  // };

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
    // {
    //   title: "Move",
    //   width: 100,
    //   render: (_, record, index) => (
    //     <>
    //       <Button
    //         size="small"
    //         disabled={index === 0}
    //         onClick={() => moveUp(index)}
    //       >
    //         ↑
    //       </Button>

    //       <Button
    //         size="small"
    //         // disabled={index === suite.tests.length - 1}
    //         disabled={index >= (suite?.tests.length ?? 0) - 1}
    //         style={{ marginLeft: 5 }}
    //         onClick={() => moveDown(index)}
    //       >
    //         ↓
    //       </Button>
    //     </>
    //   )
    // },
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