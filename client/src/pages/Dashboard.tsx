import { useEffect, useState } from "react";
import { Button, Card, Col, Empty, Row, Segmented, Space, Table, Tag, message } from "antd";
import { ArrowRightOutlined, WarningOutlined } from "@ant-design/icons";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import StackedColumnChart, { type ColumnDatum } from "../components/charts/StackedColumnChart";
import HorizontalBarChart from "../components/charts/HorizontalBarChart";
import ResultBar from "../components/charts/ResultBar";
import { CHART_INK, RESULT_COLORS, RESULT_SERIES } from "../components/charts/chartTheme";

const API = "http://localhost:5072/api/dashboard";

const RANGES = [7, 14, 30, 90] as const;
type Range = typeof RANGES[number];

type PeriodStats = {
  passed: number;
  failed: number;
  notRun: number;
  executions: number;
  runs: number;
  passRate: number | null;
};

type DashboardData = {
  days: number;
  current: PeriodStats;
  previous: PeriodStats;
  coverage: { automatedTests: number; testLinkIds: number };
  totalSuites: number;
  trend: { date: string; passed: number; failed: number; notRun: number }[];
  topFailures: { className: string; displayName: string; failures: number; executions: number }[];
  recentRuns: {
    runId: string;
    suiteName: string;
    installerVersion: string;
    startedAt: string;
    passed: number;
    failed: number;
    notRun: number;
  }[];
  latestRelease: {
    installerVersion: string;
    comparedTo: string | null;
    passRate: number;
    total: number;
    passed: number;
    failed: number;
    notExecuted: number;
    regressions: number;
    lastRun: string;
  } | null;
};

// "2026-10-09" -> local date without timezone shifts
const parseDay = (iso: string) => {
  const [y, m, d] = iso.split("-").map(Number);
  return new Date(y, m - 1, d);
};

const compact = (n: number) => new Intl.NumberFormat(undefined, { notation: "compact" }).format(n);

// ---------- Stat tiles ----------

type Delta = { text: string; color: string } | null;

// Signed change vs the previous period; coloured only when up/down clearly means good/bad
const delta = (current: number | null, previous: number | null, unit: "pts" | "", upIsGood?: boolean): Delta => {
  if (current === null || previous === null) return null;
  const diff = Math.round((current - previous) * 10) / 10;
  if (diff === 0) return { text: "No change", color: CHART_INK.secondary };
  const arrow = diff > 0 ? "▲" : "▼";
  const color = upIsGood === undefined
    ? CHART_INK.secondary
    : (diff > 0) === upIsGood ? CHART_INK.successText : CHART_INK.critical;
  return { text: `${arrow} ${Math.abs(diff).toLocaleString()}${unit ? ` ${unit}` : ""}`, color };
};

function StatTile({ label, value, suffix, change, note }: {
  label: string;
  value: string;
  suffix?: string;
  change?: Delta;
  note: string;
}) {
  return (
    <Card style={{ height: "100%" }}>
      <div style={{ color: CHART_INK.secondary, fontSize: 13 }}>{label}</div>
      <div style={{ fontSize: 28, fontWeight: 600, color: CHART_INK.primary, lineHeight: "40px" }}>
        {value}
        {suffix && <span style={{ fontSize: 14, fontWeight: 400, color: CHART_INK.secondary, marginLeft: 4 }}>{suffix}</span>}
      </div>
      <div style={{ fontSize: 12, color: CHART_INK.muted }}>
        {change && <span style={{ color: change.color, marginRight: 6 }}>{change.text}</span>}
        {note}
      </div>
    </Card>
  );
}

function Legend() {
  return (
    <Space size={16} wrap style={{ fontSize: 12, color: CHART_INK.secondary }}>
      {[...RESULT_SERIES].reverse().map(s => (
        <span key={s.key} style={{ display: "inline-flex", alignItems: "center", gap: 6 }}>
          <span style={{ width: 10, height: 10, borderRadius: 2, background: s.color }} />
          {s.label}
        </span>
      ))}
    </Space>
  );
}

// ---------- Page ----------

export default function Dashboard() {
  const navigate = useNavigate();

  const [days, setDays] = useState<Range>(14);
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [trendView, setTrendView] = useState<"Chart" | "Table">("Chart");

  useEffect(() => {
    setLoading(true);
    axios
      .get<DashboardData>(API, { params: { days } })
      .then(res => setData(res.data))
      .catch(() => message.error("Failed to load dashboard"))
      .finally(() => setLoading(false));
  }, [days]);

  // First load only; later refetches keep the previous render (dimmed) to avoid layout jumps
  if (!data)
    return <div style={{ padding: 20 }}><Card loading /></div>;

  const { current, previous } = data;
  const prevLabel = `vs previous ${data.days} days`;
  const hadPrevious = previous.runs > 0;

  const trendData: ColumnDatum[] = data.trend.map(t => {
    const date = parseDay(t.date);
    return {
      label: date.toLocaleDateString(undefined, { weekday: "short", day: "numeric", month: "short" }),
      shortLabel: date.toLocaleDateString(undefined, { day: "numeric", month: "short" }),
      values: { passed: t.passed, failed: t.failed, notRun: t.notRun },
    };
  });

  const hasTrend = data.trend.some(t => t.passed + t.failed + t.notRun > 0);
  const release = data.latestRelease;
  const passRateChange = delta(current.passRate, previous.passRate, "pts", true);

  return (
    <div style={{ padding: 20 }}>
      {/* HEADER + the one filter row, scoping everything below */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16, flexWrap: "wrap", gap: 12 }}>
        <h2 style={{ margin: 0 }}>Dashboard</h2>
        <Segmented<Range>
          value={days}
          onChange={setDays}
          options={RANGES.map(r => ({ value: r, label: `Last ${r} days` }))}
        />
      </div>

      <div style={{ opacity: loading ? 0.5 : 1, transition: "opacity 150ms" }}>
        {/* KPI ROW - pass rate is the hero figure */}
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col xs={24} lg={6}>
            <Card style={{ height: "100%" }}>
              <div style={{ color: CHART_INK.secondary, fontSize: 13 }}>Pass rate</div>
              <div style={{ fontSize: 48, fontWeight: 600, color: CHART_INK.primary, lineHeight: "60px" }}>
                {current.passRate === null ? "–" : `${current.passRate}%`}
              </div>
              <div style={{ fontSize: 12, color: CHART_INK.muted }}>
                {passRateChange
                  ? <><span style={{ color: passRateChange.color, marginRight: 6 }}>{passRateChange.text}</span>{prevLabel}</>
                  : current.passRate === null
                    ? "No tests ran in this period"
                    : `No runs in the previous ${data.days} days`}
              </div>
            </Card>
          </Col>
          <Col xs={12} lg={6}>
            <StatTile
              label="Test executions"
              value={compact(current.executions)}
              change={hadPrevious ? delta(current.executions, previous.executions, "") : null}
              note={hadPrevious ? prevLabel : `${current.failed} failed`}
            />
          </Col>
          <Col xs={12} lg={6}>
            <StatTile
              label="Runs"
              value={compact(current.runs)}
              change={hadPrevious ? delta(current.runs, previous.runs, "") : null}
              note={hadPrevious ? prevLabel : `${data.totalSuites} saved suite(s)`}
            />
          </Col>
          <Col xs={24} lg={6}>
            <StatTile
              label="Automated coverage"
              value={compact(data.coverage.testLinkIds)}
              suffix="TestLink IDs"
              note={`${data.coverage.automatedTests} automated tests`}
            />
          </Col>
        </Row>

        {/* TREND + LATEST RELEASE */}
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col xs={24} xl={16}>
            <Card
              title="Daily results"
              extra={<Segmented size="small" value={trendView} onChange={v => setTrendView(v as "Chart" | "Table")} options={["Chart", "Table"]} />}
              style={{ height: "100%" }}
            >
              {!hasTrend ? (
                <Empty description={`No tests ran in the last ${data.days} days`} />
              ) : trendView === "Chart" ? (
                <>
                  <div style={{ marginBottom: 8 }}><Legend /></div>
                  <StackedColumnChart
                    data={trendData}
                    series={RESULT_SERIES}
                    ariaLabel={`Test results per day for the last ${data.days} days`}
                  />
                </>
              ) : (
                <Table
                  size="small"
                  rowKey="date"
                  pagination={false}
                  scroll={{ y: 260 }}
                  dataSource={[...data.trend].reverse().filter(t => t.passed + t.failed + t.notRun > 0)}
                  columns={[
                    { title: "Date", dataIndex: "date", render: (d: string) => parseDay(d).toLocaleDateString() },
                    { title: "Passed", dataIndex: "passed", align: "right" },
                    { title: "Failed", dataIndex: "failed", align: "right" },
                    { title: "Not run", dataIndex: "notRun", align: "right" },
                  ]}
                />
              )}
            </Card>
          </Col>

          <Col xs={24} xl={8}>
            <Card
              title="Latest release"
              extra={release && (
                <Button type="link" size="small" onClick={() => navigate("/reports")}>
                  Report <ArrowRightOutlined />
                </Button>
              )}
              style={{ height: "100%" }}
            >
              {!release ? (
                <Empty description="No releases tested yet" />
              ) : (
                <>
                  <div style={{ color: CHART_INK.secondary, fontSize: 13 }}>Installer</div>
                  <div style={{ fontSize: 28, fontWeight: 600, color: CHART_INK.primary, lineHeight: "40px" }}>
                    {release.installerVersion}
                  </div>
                  <div style={{ fontSize: 12, color: CHART_INK.muted, marginBottom: 16 }}>
                    Last tested {new Date(release.lastRun).toLocaleString()}
                  </div>

                  <div style={{ display: "flex", justifyContent: "space-between", fontSize: 13, marginBottom: 6 }}>
                    <span style={{ color: CHART_INK.secondary }}>TestLink IDs</span>
                    <span style={{ color: CHART_INK.primary }}>
                      <b>{release.passed}</b> / {release.total} passed
                    </span>
                  </div>
                  <ResultBar
                    height={10}
                    values={{ passed: release.passed, failed: release.failed, notRun: release.notExecuted }}
                  />
                  <div style={{ display: "flex", gap: 16, marginTop: 8, fontSize: 12, color: CHART_INK.secondary, flexWrap: "wrap" }}>
                    <span>{release.failed} failed</span>
                    <span>{release.notExecuted} not executed</span>
                    <span>{release.passRate}% pass rate</span>
                  </div>

                  <div style={{ marginTop: 16 }}>
                    {release.comparedTo === null ? (
                      <span style={{ fontSize: 12, color: CHART_INK.muted }}>No earlier release to compare with</span>
                    ) : release.regressions > 0 ? (
                      <Tag icon={<WarningOutlined />} color="error">
                        {release.regressions} regression(s) since {release.comparedTo}
                      </Tag>
                    ) : (
                      <Tag color="success">No regressions since {release.comparedTo}</Tag>
                    )}
                  </div>
                </>
              )}
            </Card>
          </Col>
        </Row>

        {/* FAILURES + RECENT RUNS */}
        <Row gutter={[16, 16]}>
          <Col xs={24} xl={12}>
            <Card title="Most failing tests" style={{ height: "100%" }}>
              {data.topFailures.length === 0 ? (
                <Empty description={`No failures in the last ${data.days} days`} />
              ) : (
                <HorizontalBarChart
                  data={data.topFailures.map(f => ({
                    key: f.className,
                    label: f.className,
                    sublabel: f.displayName,
                    value: f.failures,
                    tooltip: `${f.failures} of ${f.executions} executions failed`,
                  }))}
                  color={RESULT_COLORS.failed}
                  valueLabel="failures"
                  ariaLabel={`Tests with the most failures in the last ${data.days} days`}
                />
              )}
            </Card>
          </Col>

          <Col xs={24} xl={12}>
            <Card
              title="Recent runs"
              extra={<Button type="link" size="small" onClick={() => navigate("/execution")}>All runs <ArrowRightOutlined /></Button>}
              style={{ height: "100%" }}
            >
              {data.recentRuns.length === 0 ? (
                <Empty description={`No runs in the last ${data.days} days`} />
              ) : (
                <Table
                  size="small"
                  rowKey="runId"
                  pagination={false}
                  dataSource={data.recentRuns}
                  onRow={r => ({ onClick: () => navigate(`/executions/${r.runId}`), style: { cursor: "pointer" } })}
                  columns={[
                    {
                      title: "Run",
                      render: (_, r) => (
                        <>
                          <div style={{ color: CHART_INK.primary }}>{r.suiteName}</div>
                          <div style={{ fontSize: 12, color: CHART_INK.muted }}>
                            {new Date(r.startedAt).toLocaleString()}
                            {r.installerVersion && ` · Installer ${r.installerVersion}`}
                          </div>
                        </>
                      ),
                    },
                    {
                      title: "Results",
                      width: 200,
                      render: (_, r) => (
                        <>
                          <ResultBar values={r} />
                          <div style={{ fontSize: 12, color: CHART_INK.secondary, marginTop: 4, fontVariantNumeric: "tabular-nums" }}>
                            {r.passed} passed · {r.failed} failed{r.notRun ? ` · ${r.notRun} not run` : ""}
                          </div>
                        </>
                      ),
                    },
                  ]}
                />
              )}
            </Card>
          </Col>
        </Row>
      </div>
    </div>
  );
}
