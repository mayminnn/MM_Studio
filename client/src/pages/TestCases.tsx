import { useEffect, useMemo, useState } from "react";
import { Table, Input, Button, Modal, Tag, Select, Card,
  Row, Col, Statistic, Space } from "antd";
import type { ColumnsType } from "antd/es/table";
import { getTestCases } from "../services/testcaseService";
import axios from "axios";
import { useNavigate } from "react-router-dom";

type TestCase = {
  className: string;
  methodName: string;
  tags: string[];
};

type SuiteTest = {
  className: string;
  displayName: string;
  executionName: string;
};

export default function TestCases() {
  const navigate = useNavigate();

  const [data, setData] = useState<TestCase[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState("");
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [suiteName, setSuiteName] = useState("Regression");

  useEffect(() => {
    getTestCases().then(setData).finally(() => setLoading(false));
  }, []);

  const tagColor = (tag: string) => {
    switch (tag) {
      case "Smoke": return "green";
      case "Regression": return "blue";
      case "Sanity": return "purple";
      case "Negative": return "red";
      case "Positive": return "cyan";
      case "P0": return "volcano";
      case "P1": return "gold";
      case "Project_Explorer": return "geekblue";
      case "Automated": return "magenta";
      // case "Channel_Maps": return "orange";
      // case "Site": return "lime";
      default: return "default";
    }
  };

  const allTags = useMemo(
    () => [...new Set(data.flatMap(x => x.tags))].sort(),
    [data]
  );

  const filteredData = data.filter(t => {
    const s = `${t.className} ${t.methodName}`.toLowerCase()
      .includes(searchText.toLowerCase());
    const tag = selectedTags.length === 0 ||
      selectedTags.every(x => t.tags.includes(x));
    return s && tag;
  });

  const smokeCount = data.filter(x=>x.tags.includes("Smoke")).length;
  const regCount = data.filter(x=>x.tags.includes("Regression")).length;

  const columns: ColumnsType<TestCase> = [
    { title:"Test ID", dataIndex:"className", width:140 },
    { title:"Test Name", dataIndex:"methodName" },
    {
      title:"Tags",
      dataIndex:"tags",
      render:(tags:string[])=>(
        <>
          {tags.map(t=><Tag color={tagColor(t)} key={t}>{t}</Tag>)}
        </>
      )
    }
  ];

  const createSuite = async()=>{
    const tests:SuiteTest[]=data
      .filter(x=>selectedRowKeys.includes(x.className))
      .map(x=>({
        className:x.className,
        displayName:x.methodName,
        executionName:x.methodName.replace(/\s+/g,"_")
      }));

    await axios.post("http://localhost:5072/api/suite",{
      name:suiteName,
      tests
    });

    setIsModalOpen(false);
    setSelectedRowKeys([]);
    navigate("/testsuites");
  };

  const handleRunSelected = async () => {

    // const selectedTests = data
    //     .filter(t => selectedRowKeys.includes(t.className))
    //     .map(t => ({
    //         className: t.className,
    //         displayName: t.methodName,
    //         executionName: t.methodName.replace(/\s+/g, "_")
    //     }));

    const selectedTests: SuiteTest[] =
    selectedRowKeys.map((key, index) => {

        const t = data.find(x => x.className === key)!;

        return {
            order: index + 1,
            className: t.className,
            displayName: t.methodName,
            executionName: t.methodName.replace(/\s+/g, "_")
        };

    });

    const res = await axios.post(
      "http://localhost:5072/api/execution/run",
      {
          suiteName: "Ad Hoc Run",
          tests: selectedTests
      }
    );

    navigate(`/executions/${res.data.runId}`);
};

  return (
    <div style={{padding:24}}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          marginBottom: 16,
        }}
      >
        <h2>Test Case Library</h2>
      </div>

      <Row gutter={16} style={{marginBottom:20}}>
        <Col span={6}><Card><Statistic title="Total Tests" value={data.length}/></Card></Col>
        <Col span={6}><Card><Statistic title="Selected" value={selectedRowKeys.length}/></Card></Col>
        <Col span={6}><Card><Statistic title="Smoke" value={smokeCount}/></Card></Col>
        <Col span={6}><Card><Statistic title="Regression" value={regCount}/></Card></Col>
      </Row>

      <Card style={{marginBottom:20}}>
        <Space wrap style={{width:"100%",justifyContent:"space-between"}}>
          <Space wrap>
            <Input
              placeholder="Search..."
              value={searchText}
              onChange={e=>setSearchText(e.target.value)}
              style={{width:260}}
            />
            <Select
              mode="multiple"
              allowClear
              placeholder="Filter Tags"
              style={{width:320}}
              value={selectedTags}
              onChange={setSelectedTags}
              options={allTags.map(t=>({label:t,value:t}))}
            />
          </Space>
          <Space wrap>
            <Button
              type="primary"
              disabled={selectedRowKeys.length===0}
              onClick={()=>setIsModalOpen(true)}
            >
              Create Suite ({selectedRowKeys.length})
            </Button>

            <Button
              type="primary"
              disabled={selectedRowKeys.length === 0}
              onClick={handleRunSelected}
            >
                ▶ Run Selected
            </Button>
          </Space>
        </Space>
      </Card>

      <Table
        rowKey="className"
        loading={loading}
        columns={columns}
        dataSource={filteredData}
        rowSelection={{
          selectedRowKeys,
          onChange:setSelectedRowKeys
        }}
      />

      <Modal
        title="Create Test Suite"
        open={isModalOpen}
        onCancel={()=>setIsModalOpen(false)}
        onOk={createSuite}
        okText="Create Suite"
      >
        <Input
          value={suiteName}
          onChange={e=>setSuiteName(e.target.value)}
          placeholder="Suite Name"
        />

        <p style={{marginTop:20}}>
          <b>{selectedRowKeys.length}</b> test(s) selected.
        </p>
      </Modal>
    </div>
  );
}
