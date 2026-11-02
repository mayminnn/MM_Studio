import { useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  Col,
  Empty,
  Row,
  Segmented,
  Select,
  Space,
  Spin,
  Statistic,
  Table,
  Tag,
  Tooltip,
  message,
} from "antd";
import { DownloadOutlined, PrinterOutlined, UnorderedListOutlined } from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { statusColor } from "../utils/resultStatus";
import { testLinkUrl } from "../utils/testLink";

const API = "http://localhost:5072/api/reports";

// Release readiness by TestLink ID. Run-level detail (runs, errors, logs) lives on the Executions page.

type ReleaseSummary = {
  installerVersion: string;
  firstRun: string;
  lastRun: string;
  runs: number;
};

type Coverage = {
  className: string;
  status: string;
};

type TestLinkRow = {
  testLinkId: string;
  name: string;
  status: string;
  previousStatus: string | null;
  flaky: boolean;
  automatedBy: Coverage[];
};

type ReleaseReport = {
  installerVersion: string;
  comparedTo: string | null;
  firstRun: string;
  lastRun: string;
  projects: string[];
  runs: number;
  total: number;
  passed: number;
  failed: number;
  notExecuted: number;
  flaky: number;
  regressions: number;
  fixed: number;
  passRate: number;
  testCases: TestLinkRow[];
};

type RowFilter = "All" | "Failed" | "Regressions" | "Fixed" | "Flaky" | "Not Executed";

// "none" tells the API not to compare; undefined lets it pick the previous release
const NO_COMPARISON = "none";

const isRegression = (t: TestLinkRow) => t.previousStatus === "Passed" && t.status === "Failed";
const isFixed = (t: TestLinkRow) => t.previousStatus === "Failed" && t.status === "Passed";

const formatDate = (value: string) => new Date(value).toLocaleString();

const csvCell = (value: unknown) => `"${String(value ?? "").replace(/"/g, '""')}"`;

const downloadCsv = (report: ReleaseReport) => {
  const header = [
    "TestLink ID", "Name", "Status", `Previous (${report.comparedTo ?? "-"})`,
    "Regression", "Fixed", "Flaky", "Automated By",
  ];

  const rows = report.testCases.map(t => [
    t.testLinkId,
    t.name,
    t.status,
    t.previousStatus ?? "",
    isRegression(t) ? "Yes" : "",
    isFixed(t) ? "Yes" : "",
    t.flaky ? "Yes" : "",
    t.automatedBy.map(c => `${c.className} (${c.status})`).join("; "),
  ]);

  const csv = [header, ...rows].map(r => r.map(csvCell).join(",")).join("\r\n");

  // BOM so Excel opens it as UTF-8
  const blob = new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8" });
  const url = URL.createObjectURL(blob);

  const link = document.createElement("a");
  link.href = url;
  link.download = `release-report-${report.installerVersion}.csv`;
  link.click();

  URL.revokeObjectURL(url);
};

export default function Reports() {
  const navigate = useNavigate();

  const [releases, setReleases] = useState<ReleaseSummary[]>([]);
  const [installer, setInstaller] = useState<string>();
  const [compareTo, setCompareTo] = useState<string>();
  const [report, setReport] = useState<ReleaseReport | null>(null);
  const [loadingReleases, setLoadingReleases] = useState(true);
  const [loadingReport, setLoadingReport] = useState(false);
  const [filter, setFilter] = useState<RowFilter>("All");

  useEffect(() => {
    axios
      .get<ReleaseSummary[]>(`${API}/releases`)
      .then(res => {
        setReleases(res.data);
        if (res.data.length > 0)
          setInstaller(res.data[0].installerVersion);
      })
      .catch(() => message.error("Failed to load releases"))
      .finally(() => setLoadingReleases(false));
  }, []);

  useEffect(() => {
    if (!installer)
      return;

    setLoadingReport(true);

    axios
      .get<ReleaseReport>(`${API}/release`, { params: { installer, compareTo } })
      .then(res => setReport(res.data))
      .catch(() => {
        setReport(null);
        message.error("Failed to load report");
      })
      .finally(() => setLoadingReport(false));
  }, [installer, compareTo]);

  const filteredRows = useMemo(() => {
    const rows = report?.testCases ?? [];

    switch (filter) {
      case "Failed": return rows.filter(t => t.status === "Failed");
      case "Regressions": return rows.filter(isRegression);
      case "Fixed": return rows.filter(isFixed);
      case "Flaky": return rows.filter(t => t.flaky);
      case "Not Executed": return rows.filter(t => t.status === "Not Executed");
      default: return rows;
    }
  }, [report, filter]);

  const columns: ColumnsType<TestLinkRow> = [
    {
      title: "TestLink ID",
      dataIndex: "testLinkId",
      width: 120,
      render: (id: string) => (
        <a href={testLinkUrl(id)} target="_blank" rel="noopener noreferrer">{id}</a>
      ),
    },
    { title: "Name", dataIndex: "name" },
    {
      title: "Status",
      dataIndex: "status",
      width: 210,
      render: (status: string, t) => (
        <Space size={4} wrap>
          <Tag color={statusColor(status)}>{status}</Tag>
          {isRegression(t) && <Tag color="volcano">Regression</Tag>}
          {isFixed(t) && <Tag color="cyan">Fixed</Tag>}
          {t.flaky && (
            <Tooltip title="An automated test for this case both passed and failed on this release">
              <Tag color="gold">Flaky</Tag>
            </Tooltip>
          )}
        </Space>
      ),
    },
    ...(report?.comparedTo
      ? [{
          title: `Previous (${report.comparedTo})`,
          dataIndex: "previousStatus",
          width: 140,
          render: (status: string | null) => status ? <Tag color={statusColor(status)}>{status}</Tag> : "-",
        }]
      : []),
    {
      title: "Automated By",
      dataIndex: "automatedBy",
      width: 200,
      render: (coverage: Coverage[]) => coverage.map(c => (
        <Tooltip key={c.className} title={c.status}>
          <Tag color={statusColor(c.status)}>{c.className}</Tag>
        </Tooltip>
      )),
    },
  ];

  if (loadingReleases)
    return <div style={{ padding: 20 }}><Spin /></div>;

  if (releases.length === 0)
    return (
      <div style={{ padding: 20 }}>
        <h2>Release Report</h2>
        <Empty description="No results yet. Run a suite with an installer version to generate a report." />
      </div>
    );

  return (
    <div style={{ padding: 20 }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16, flexWrap: "wrap", gap: 12 }}>
        <h2 style={{ margin: 0 }}>
          Release Report{report ? ` — Installer ${report.installerVersion}` : ""}
        </h2>

        <Space wrap className="print:hidden">
          <span>Installer</span>
          <Select
            style={{ width: 200 }}
            value={installer}
            onChange={value => { setInstaller(value); setCompareTo(undefined); setFilter("All"); }}
            options={releases.map(r => ({
              value: r.installerVersion,
              label: `${r.installerVersion} · ${r.runs} run(s)`,
            }))}
          />

          <span>Compare to</span>
          <Select
            style={{ width: 160 }}
            value={report?.comparedTo ?? NO_COMPARISON}
            onChange={setCompareTo}
            options={[
              { value: NO_COMPARISON, label: "None" },
              ...releases
                .filter(r => r.installerVersion !== installer)
                .map(r => ({ value: r.installerVersion, label: r.installerVersion })),
            ]}
          />

          <Button icon={<DownloadOutlined />} disabled={!report} onClick={() => report && downloadCsv(report)}>
            Export CSV
          </Button>
          <Button icon={<PrinterOutlined />} disabled={!report} onClick={() => window.print()}>
            Print / PDF
          </Button>
        </Space>
      </div>

      <Spin spinning={loadingReport}>
        {report && (
          <>
            <div style={{ color: "#888", marginBottom: 16, display: "flex", alignItems: "center", gap: 8, flexWrap: "wrap" }}>
              <span>
                Tested {formatDate(report.firstRun)} – {formatDate(report.lastRun)} · {report.runs} run(s) ·
                Project(s): {report.projects.join(", ")}
                {report.comparedTo && <> · Compared to installer {report.comparedTo}</>}
              </span>
              <Button
                type="link"
                size="small"
                className="print:hidden"
                icon={<UnorderedListOutlined />}
                onClick={() => navigate(`/execution?installer=${encodeURIComponent(report.installerVersion)}`)}
              >
                View runs
              </Button>
            </div>

            {/* SUMMARY (per TestLink ID) */}
            <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
              <Col xs={12} md={4}>
                <Card>
                  <Tooltip title="Passed / (Passed + Failed) TestLink IDs">
                    <Statistic title="Pass Rate" value={report.passRate} suffix="%" />
                  </Tooltip>
                </Card>
              </Col>
              <Col xs={12} md={4}>
                <Card><Statistic title="Passed" value={report.passed} suffix={`/ ${report.total}`} styles={{ content: { color: "#389e0d" } }} /></Card>
              </Col>
              <Col xs={12} md={4}>
                <Card><Statistic title="Failed" value={report.failed} styles={{ content: { color: "#cf1322" } }} /></Card>
              </Col>
              <Col xs={12} md={4}>
                <Card>
                  <Tooltip title="Not every automated test for the case has a pass/fail on this release">
                    <Statistic title="Not Executed" value={report.notExecuted} />
                  </Tooltip>
                </Card>
              </Col>
              <Col xs={12} md={4}>
                <Card>
                  <Statistic
                    title="Regressions"
                    value={report.comparedTo ? report.regressions : "-"}
                    styles={{ content: report.regressions ? { color: "#d4380d" } : {} }}
                  />
                </Card>
              </Col>
              <Col xs={12} md={4}>
                <Card><Statistic title="Flaky" value={report.flaky} styles={{ content: report.flaky ? { color: "#d48806" } : {} }} /></Card>
              </Col>
            </Row>

            {/* CHANGES VS PREVIOUS RELEASE */}
            {report.comparedTo && (
              <Card title={`Changes since installer ${report.comparedTo}`} size="small" style={{ marginBottom: 16 }}>
                <Row gutter={16}>
                  <Col span={12}>
                    <b style={{ color: "#cf1322" }}>Regressions ({report.regressions})</b>
                    <div style={{ marginTop: 8 }}>
                      {report.testCases.filter(isRegression).map(t => <Tag key={t.testLinkId} color="volcano">{t.testLinkId} · {t.name}</Tag>)}
                      {report.regressions === 0 && <span style={{ color: "#888" }}>None</span>}
                    </div>
                  </Col>
                  <Col span={12}>
                    <b style={{ color: "#389e0d" }}>Fixed ({report.fixed})</b>
                    <div style={{ marginTop: 8 }}>
                      {report.testCases.filter(isFixed).map(t => <Tag key={t.testLinkId} color="cyan">{t.testLinkId} · {t.name}</Tag>)}
                      {report.fixed === 0 && <span style={{ color: "#888" }}>None</span>}
                    </div>
                  </Col>
                </Row>
              </Card>
            )}

            {/* PER TESTLINK ID */}
            <Card
              title="Test Cases"
              size="small"
              extra={
                <Segmented<RowFilter>
                  className="print:hidden"
                  value={filter}
                  onChange={setFilter}
                  options={["All", "Failed", "Regressions", "Fixed", "Flaky", "Not Executed"]}
                />
              }
            >
              <Table
                rowKey="testLinkId"
                size="small"
                columns={columns}
                dataSource={filteredRows}
                pagination={false}
              />
            </Card>
          </>
        )}
      </Spin>
    </div>
  );
}
